using System.Diagnostics;
using Microsoft.Win32;

namespace Enterprise.Dat.Implementation.Preconditions
{
	class SqlServerChecker : IPrecondition
	{
		string errorMessage;

		public SqlServerChecker()
		{
		}

		public string ErrorMessage
		{
			get { return errorMessage; }
		}

		static bool SqlServerInstalled
		{
			get
			{
				bool result;
				using (RegistryKey localMachineRoot = Registry.LocalMachine)
				using (RegistryKey sql2000Key = localMachineRoot.OpenSubKey(@"SOFTWARE\Microsoft\MSSQLServer\MSSQLServer\CurrentVersion", false))
				{
					result = (sql2000Key != null);

					if (!result)
					{
						using (RegistryKey sql2005Key = localMachineRoot.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false))
						{
							result = (sql2005Key != null && sql2005Key.ValueCount > 0);
						}
					}
				}

				if (!result)
				{
					result = SqlServerRunning;
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Baseline")]
		static bool SqlServerRunning
		{
			get { return Process.GetProcessesByName("sqlservr").Length > 0; }
		}

		public bool CheckPreconditionMet()
		{
			bool sqlServerInstalled = SqlServerInstalled;
			bool sqlServerRunning = SqlServerRunning;

			if (!sqlServerInstalled)
			{
				errorMessage = "SQL Server does not appear to be installed on this machine.";
			}
			else if (!sqlServerRunning)
			{
				errorMessage = "SQL Server is not running on this machine.";
			}

			return sqlServerInstalled && sqlServerRunning;
		}
	}
}
