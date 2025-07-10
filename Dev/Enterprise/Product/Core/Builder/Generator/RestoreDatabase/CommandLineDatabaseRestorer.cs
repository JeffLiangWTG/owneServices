using System;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.Builder.Generator.RestoreDatabase
{
	static class CommandLineDatabaseRestorer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Command line application")]
		public static bool Run()
		{
			ConsoleActivator.EnsureConsole();

			Console.WriteLine("Server: {0}, Database: {1}", Db.ServerName, Db.DatabaseName);

			var restorer = new DatabaseRestorer();
			try
			{
				restorer.Restore();
				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Console.WriteLine("Unhandled {0}: {1}", ex.GetType().Name, ex.Message);
				return false;
			}
		}
	}
}
