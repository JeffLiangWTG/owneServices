using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Startup;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dat.Implementation
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	public static partial class LocalDBConnection
	{
		const string ConnectionStringWithoutMachineName = "persist security info=no;pooling=no;application name=Dat.Client;connect timeout=300;";
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static string connectionStringCache;
		static readonly object syncRoot = new object();

		static bool CanLogOnAsSysAdminWithConnectionString(string connectionString)
		{
			int retryCount = 0;

			while (retryCount < 5)
			{
				try
				{
					using (SqlConnection sqlCon = new SqlConnection(connectionString))
					{
						sqlCon.Open();
						using (var isSysAdmin = new SqlCommand("SELECT IS_SRVROLEMEMBER('sysadmin')", sqlCon))
						{
							return (int)isSysAdmin.ExecuteScalar() == 1;
						}
					}
				}
				catch (SqlException ex)
				{
					var dbErrorMatch = new DbErrorMatch(ex);

					if ((dbErrorMatch.ExceptionType == DbErrorType.GeneralNetworkError || dbErrorMatch.ExceptionType == DbErrorType.TimeoutExpired) && retryCount < 4)
					{
						retryCount++;
					}
					else
					{
						break;
					}
				}

				Thread.Sleep(5000);
			}

			return false;
		}

		public static bool CheckIsLatestRequiredSqlServerVersion(SqlConnection connection, out string failedRequirementMessage)
		{
			SqlServerVersionNumber serverVersionNumber;
			using (var cmd = new SqlCommand(DbConnection.SelectProductVersionCommandText, connection))
			{
				serverVersionNumber = new SqlServerVersionNumber(DbConnection.CheckedProductVersionString(cmd.ExecuteScalar()?.ToString()));
			}

			string serverFullVersionText;
			using (var cmd = new SqlCommand(DbConnection.SelectServerFullVersionTextCommandText, connection))
			{
				serverFullVersionText = cmd.ExecuteScalar().ToString();
			}

			try
			{
				return RequirementChecker.CheckServerVersion(serverVersionNumber, serverFullVersionText, out failedRequirementMessage);
			}
			catch (ServerRequirementsNotMetException e)
			{
				failedRequirementMessage = e.Message;
				return false;
			}
		}

		public static SqlConnection GetConnection()
		{
			return new SqlConnection(GetConnectionString());
		}

		static string GetConnectionString()
		{
			lock (syncRoot)
			{
				if (connectionStringCache == null)
				{
					var instanceName = GetSqlServerInstanceName();
					connectionStringCache = GetConnectionStringCore(instanceName);
					serverName = instanceName; // set serverName only when instanceName can be used to connect
				}

				return connectionStringCache;
			}

			string GetConnectionStringCore(string instanceName)
			{
				var connectionString = ConnectionStringWithoutMachineName + "server=" + instanceName + ";Integrated Security=true;TrustServerCertificate=True";
				if (CanLogOnAsSysAdminWithConnectionString(connectionString))
				{
					return connectionString;
				}

				throw new LoginFailedException("Was not able to login to the SQL Server.");
			}

			// Assuming that we have and only have one SQL Server instance running on one DAT machine,
			// but on developer's local machine, there might be multiple sql instances, which is allowed,
			// in which case we choose one available SQL instance (default instance is the first option if there is).
			string GetSqlServerInstanceName()
			{
				using (var utils = new SqlServerInstanceUtils())
				{
					var runningSqInstanceNames = utils.GetRunningSqlServerInstances()
						.Select(x => ConvertToUsableInstanceName(x))
						.OrderBy(x => x, StringComparer.OrdinalIgnoreCase) // make sure default instance is at first
						.ToArray();

					if (runningSqInstanceNames.Length == 0)
					{
						throw new LoginFailedException("No SQL Server instance was found.");
					}

					if (DatIsTesting && runningSqInstanceNames.Length > 1)
					{
						throw new LoginFailedException($"More than one SQL Server instance was found, cannot determine which to use. Running instances are {string.Join(", ", runningSqInstanceNames)}");
					}

					return runningSqInstanceNames[0];
				}

				string ConvertToUsableInstanceName(string instanceName)
				{
					return instanceName.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
						? Environment.MachineName
						: $"{Environment.MachineName}\\{instanceName}";
				}
			}
		}

		public static string GetServerName()
		{
			if (serverName == null)
			{
				using (var connection = GetConnection())
				{
				}
			}
			return serverName;
		}
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static string serverName;

		public static string GetServiceName()
		{
			if (serviceName == null)
			{
				try
				{
					using (var connection = GetConnection())
					{
						connection.Open();

						using (SqlCommand cmd = new SqlCommand("SELECT @@servicename", connection))
						{
							serviceName = (string)cmd.ExecuteScalar();
						}
					}
				}
				catch
				{
					return string.Empty;
				}
			}
			return serviceName;
		}
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static string serviceName;

		// Value of DatIsTesting is supposed to be fixed in one environment, which should be generally initialized at the very beginning.
		// If value is changed, all the caches should be cleared as they might have already been invalid.
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static bool datIsTesting = true;
		public static bool DatIsTesting
		{
			get => datIsTesting;
			set
			{
				if (datIsTesting == value)
				{
					return;
				}

				lock (syncRoot)
				{
					if (datIsTesting == value)
					{
						return;
					}

					connectionStringCache = null;
					serverName = null;
					serviceName = null;

					datIsTesting = value;
				}
			}
		}
	}
}
