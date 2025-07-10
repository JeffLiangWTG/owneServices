using System;
using CargoWise.ApplicationManager.Common;
using Microsoft.Win32;

namespace Enterprise.Dat.Implementation.Preconditions
{
	public class SqlServiceAccountChecker : IPrecondition, IAppManagerInvocable
	{
		SqlServiceAccountChecker()
		{ }

		public SqlServiceAccountChecker(SqlServerServiceRestarter restarter)
		{
			this.restarter = restarter;
		}

		public string ErrorMessage
		{
			get { return "SQL Servcer Service should be running under the Local System account"; }
		}

		public bool CheckPreconditionMet()
		{
			string account;
			using (var registryKey = Registry.LocalMachine.OpenSubKey(GetKeyName(SqlServerTools.SqlServerServiceName), false))
			{
				account = registryKey.GetValue("ObjectName") as string;
			}
			if (account.StartsWith("NT "))
			{
				using (var appManagerClient = new AppManagerClient())
				{
					var type = typeof(SqlServiceAccountChecker);
					var invokeResult = appManagerClient.Invoke(type.Assembly.Location, type.FullName, SqlServerTools.SqlServerServiceName, null);
					var result = invokeResult.Status == AppManagerResultStatus.Success;
					if (!result)
					{
						return false;
					}
				}

				restarter.RequireRestart(ErrorMessage);
			}
			else if (!account.Equals("LocalSystem", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return true;
		}

		public AppManagerResult Invoke(bool waitedForMutex, object state)
		{
			using (var registryKey = Registry.LocalMachine.OpenSubKey(GetKeyName((string)state), true))
			{
				registryKey.SetValue("ObjectName", "LocalSystem");
			}
			return new AppManagerResult(AppManagerResultStatus.Success);
		}

		static string GetKeyName(string sqlServiceName)
		{
			return @"System\CurrentControlSet\Services\" + sqlServiceName;
		}

		readonly SqlServerServiceRestarter restarter;
	}
}
