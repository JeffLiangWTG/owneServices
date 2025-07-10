using System;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class ConsoleService : IConsoleService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "needed to print form builder debug message")]
		public void Log(object obj)
		{
			if (!isConsoleAllocated)
			{
				isConsoleAllocated = NativeMethods.AllocConsole();
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine((NoResString)"Hello from Form Builder debug console."); 
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine((NoResString)"Please note that:");
				Console.WriteLine((NoResString)"   1. it should be only used for diagnostic purposes.");
				Console.WriteLine((NoResString)"   2. closing this consol will terminate CW1 process, to close the console only press Ctrl+C");
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine((NoResString)"Listening for diagnostic messages...");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine();
			}

			Console.WriteLine(obj);
		}

		bool isConsoleAllocated;
	}
}
