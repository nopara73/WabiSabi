using System;
using System.Collections.Generic;
using System.Linq;
using AmountOrganization;
using AmountOrganization.DescPow2;

var inputCount = 50;
var userCount = 40;
var remixRatio = 0.3;

var efficiency = new List<decimal>();
for (int i = 0; i < 1; i++)
{
    var preRandomAmounts = Sample.Amounts.DeterministicRandomElements(inputCount);
    var preGroups = preRandomAmounts.DeterministicRandomGroups(userCount);

    var preMixer = new DescPow2Mixer();
    var preMix = (preMixer as IMixer).CompleteMix(preGroups);

    var remixCount = (int)(inputCount * remixRatio);
    var randomAmounts = Sample.Amounts.DeterministicRandomElements(inputCount - remixCount).Concat(preMix.SelectMany(x => x).DeterministicRandomElements(remixCount));
    var inputGroups = randomAmounts.DeterministicRandomGroups(userCount).ToArray();
    var mixer = new DescPow2Mixer();
    var outputGroups = (mixer as IMixer).CompleteMix(inputGroups).Select(x => x.ToArray()).ToArray();

    if (inputGroups.SelectMany(x => x).Sum() <= outputGroups.SelectMany(x => x).Sum())
    {
        throw new InvalidOperationException("Bug. Transaction doesn't pay fees.");
    }

    var outputCount = outputGroups.Sum(x => x.Length);
    var inputAmount = inputGroups.SelectMany(x => x).Sum();
    var outputAmount = outputGroups.SelectMany(x => x).Sum();
    var fee = inputAmount - outputAmount;
    var size = inputCount * mixer.InputSize + outputCount * mixer.OutputSize;
    var feeRate = (fee / size).ToSats();

    Console.WriteLine();

    foreach (var (value, count, unique) in inputGroups
        .GetIndistinguishable()
        .OrderBy(x => x.value))
    {
        if (count == 1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        var displayResult = count.ToString();
        if (count != unique)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            displayResult = $"{unique}/{count} unique/total";
        }
        Console.WriteLine($"There are {displayResult} occurrences of\t{value} BTC input.");
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    Console.WriteLine();

    foreach (var (value, count, unique) in outputGroups
        .GetIndistinguishable()
        .OrderBy(x => x.value))
    {
        if (count == 1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        var displayResult = count.ToString();
        if (count != unique)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            displayResult = $"{unique}/{count} unique/total";
        }
        Console.WriteLine($"There are {displayResult} occurrences of\t{value} BTC output.");
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    var avgAnon = Analyzer.AverageAnonsetGain(inputGroups, outputGroups);
    Console.WriteLine();
    Console.WriteLine($"Number of users:\t{userCount}");
    Console.WriteLine($"Number of inputs:\t{inputCount}");
    Console.WriteLine($"Number of outputs:\t{outputCount}");
    Console.WriteLine($"Total in:\t\t{inputAmount} BTC");
    Console.WriteLine($"Fee paid:\t\t{fee} BTC");
    Console.WriteLine($"Size:\t\t\t{size} vbyte");
    Console.WriteLine($"Fee rate:\t\t{feeRate} sats/vbyte");
    Console.WriteLine($"Average anonset:\t{avgAnon:0.##}");
    Console.WriteLine($"Average input anonset:\t{Analyzer.AverageAnonsetGain(inputGroups):0.##}");
    Console.WriteLine($"Average output anonset:\t{Analyzer.AverageAnonsetGain(outputGroups):0.##}");
    var eff = avgAnon / (size / 1000);
    efficiency.Add(eff);
    Console.WriteLine($"Blockspace efficiency:\t{eff:0.##}");
    Console.WriteLine($"foo:\t{Analyzer.AverageAnonsetGain(outputGroups) / outputCount:0.######}");
}

Console.WriteLine(string.Join(", ", efficiency.Select(x => $"{x:0.##}")));
Console.WriteLine($"Avg: {efficiency.Average():0.##}");
Console.WriteLine($"Med: {efficiency.Median():0.##}");

Console.ReadLine();
