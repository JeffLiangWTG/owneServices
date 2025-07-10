using System;

namespace LoadSolution
{
	class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "error message for developer users")]
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("please give filename and linenumber");
				Environment.Exit(-1);
			}

			var filename = args[0];
			if (!int.TryParse(args[1], out var lineNumber))
			{
				lineNumber = 1;
			}

			var ls = new LoadSolution();

			Environment.Exit(ls.Run(filename, lineNumber));
		}
	}
}
