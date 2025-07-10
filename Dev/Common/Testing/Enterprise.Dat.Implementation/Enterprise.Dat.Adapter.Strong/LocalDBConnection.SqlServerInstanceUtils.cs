using System;
using System.Linq;
using Microsoft.Management.Infrastructure;

namespace Enterprise.Dat.Implementation
{
	public static partial class LocalDBConnection
	{
		sealed class SqlServerInstanceUtils : IDisposable
		{
			readonly CimSession session;
			readonly string rootNamespace;
			const string QueryDialect = "WQL";

			const string ParentNameSpace = @"ROOT\Microsoft\SqlServer";
			const int SqlServiceTypeSqlServer = 1;
			const int SqlServiceStateRunning = 4;
			const string InstanceIdPropertyName = "INSTANCEID";

			public SqlServerInstanceUtils()
			{
				session = CimSession.Create(null);
				rootNamespace = GetRootNamespace(session);
			}

			// See https://learn.microsoft.com/en-us/sql/relational-databases/wmi-provider-configuration-classes/sqlservice-class/sqlservice-class?view=sql-server-ver16
			public string[] GetRunningSqlServerInstances()
			{
				return GetRunningSqlServices(session, rootNamespace)
					.Select(x => GetInstanceNameFromInstanceId(GetSqlServerServiceAdvancedProperty<string>(x, InstanceIdPropertyName)))
					.ToArray();
			}

			T GetSqlServerServiceAdvancedProperty<T>(string serviceName, string propertyName)
			{
				var query = $@"
SELECT PropertyNumValue, PropertyStrValue
FROM SqlServiceAdvancedProperty
WHERE
	SqlServiceType      = {SqlServiceTypeSqlServer}
	AND ServiceName     = '{serviceName}'
	AND PropertyName    = '{propertyName}'
";
				return session.QueryInstances(rootNamespace, QueryDialect, query)
					.Select(x => GetPropertyValue(x))
					.FirstOrDefault();

				T GetPropertyValue(CimInstance cimInstance)
				{
					return typeof(T).IsClass
						? (T)cimInstance.CimInstanceProperties["PropertyStrValue"].Value
						: (T)cimInstance.CimInstanceProperties["PropertyNumValue"].Value;
				}
			}

			static string GetInstanceNameFromInstanceId(string instanceId)
			{
				if (string.IsNullOrEmpty(instanceId))
				{
					throw new LoginFailedException($"Could not find expected {InstanceIdPropertyName} in SQL Server service's AdvancedProperties.");
				}

				// instance id = MSSQL16.MSSQLSERVER22, MSSQL15.MSSQLSERVER, etc.
				return instanceId.Substring(instanceId.IndexOf(".") + 1);
			}

			static string[] GetRunningSqlServices(CimSession session, string rootNamespace)
			{
				// SqlServiceType = 1 - MSSQLSERVER is the SQL Server service.
				// State          = 4 - Running. The service is running.
				var query = $"SELECT ServiceName FROM SqlService WHERE SqlServiceType = {SqlServiceTypeSqlServer} AND State = {SqlServiceStateRunning}";
				return session.QueryInstances(rootNamespace, QueryDialect, query)
					.Select(x => (string)x.CimInstanceProperties["ServiceName"].Value)
					.ToArray();
			}

			static string GetRootNamespace(CimSession session)
			{
				var subNamespace = GetLatestWmiProviderSubNamespace();
				if (string.IsNullOrEmpty(subNamespace))
				{
					throw new LoginFailedException("Found no WMI provider for SQL Server installed on current machine.");
				}

				return $@"{ParentNameSpace}\{subNamespace}";

				string GetLatestWmiProviderSubNamespace()
				{
					const string ComputerManagementNameSpace = "ComputerManagement";
					var query = $"SELECT Name FROM __NAMESPACE WHERE Name LIKE '{ComputerManagementNameSpace}%'";

					return session.QueryInstances(ParentNameSpace, QueryDialect, query)
						.Select(x => (string)x.CimInstanceProperties["Name"].Value)
						.OrderByDescending(x => GetIndexFromNamespace(x))
						.FirstOrDefault();

					int GetIndexFromNamespace(string name)
					{
						// name == "ComputerManagement"
						if (ComputerManagementNameSpace.Length == name.Length)
						{
							return 0;
						}

						// ComputerManagement15, ComputerManagement16, etc.
						if (!int.TryParse(name.Substring(ComputerManagementNameSpace.Length), out var index))
						{
							throw new LoginFailedException($"Unexpected WMI provider namespace '{name}'");
						}

						return index;
					}
				}
			}

			public void Dispose()
			{
				session.Dispose();
			}
		}
	}
}
