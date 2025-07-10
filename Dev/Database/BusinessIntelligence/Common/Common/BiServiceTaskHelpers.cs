using System;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;

namespace CargoWise.Bi.Common
{
	public static class BiServiceTaskHelpers
	{
		public static string IsBusinessIntelligenceEnabled()
		{
			var result = new StringBuilder();

			var isAuditEnabledMessage = CheckAuditEnabledForHostedServiceRequirements();
			var isEdwEnabledMessage = CheckEdwEnabledForHostedServiceRequirements();

			if (!string.IsNullOrWhiteSpace(isAuditEnabledMessage) && !string.IsNullOrWhiteSpace(isEdwEnabledMessage))
			{
				result.AppendLine(isAuditEnabledMessage);
				result.AppendLine(isEdwEnabledMessage);
			}

			return result.ToString();
		}

		public enum ServerType
		{
			Audit,
			DataWarehouse
		}

		public static string CheckAuditEnabledForHostedServiceRequirements()
		{
			return AssertServerWithoutCache(ServerType.Audit);
		}

		public static string CheckEdwEnabledForHostedServiceRequirements()
		{
			return AssertServerWithoutCache(ServerType.DataWarehouse);
		}

		public static string IsAuditEnabled()
		{
			return AssertServerExistsSafe(ServerType.Audit);
		}

		public static string IsEdwEnabled()
		{
			return AssertServerExistsSafe(ServerType.DataWarehouse);
		}

		static string AssertServerExistsSafe(ServerType serverType)
		{
			try
			{
				return GetServerValueAndAssertDatabaseExists(serverType);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var errorMessage = $"An exception was thrown when trying to connect to the {serverType} server:\r\n{ex.Message}";
				return errorMessage;
			}
		}

		static string AssertServerWithoutCache(ServerType serverType)
		{
			try
			{
				return GetServerValueAndAssertDatabaseExistsWithoutCache(serverType);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var errorMessage = $"An exception was thrown when trying to connect to the {serverType} server:\r\n{ex.Message}";
				return errorMessage;
			}
		}

		static string GetServerValueAndAssertDatabaseExists(ServerType serverType)
		{
			string serverName = null;
			if (serverType == ServerType.Audit)
			{
				serverName = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
			}
			else if (serverType == ServerType.DataWarehouse)
			{
				serverName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			}

			if (!string.IsNullOrWhiteSpace(serverName))
			{
				if (serverType == ServerType.Audit)
				{
					return CheckBiDatabaseExistsAndIsUpdated(serverName, Db.AuditDatabaseName);
				}
				else if (serverType == ServerType.DataWarehouse)
				{
					return CheckBiDatabaseExistsAndIsUpdated(serverName, Db.EdwDatabaseName);
				}
			}
			return $"No {serverType} server set in the registry."; // information for logging only.
		}

		static string GetServerValueAndAssertDatabaseExistsWithoutCache(ServerType serverType)
		{
			string serverName = null;
			if (serverType == ServerType.Audit)
			{
				serverName = BiServers.LoadAuditServerWithoutCache(Db.Connection);
			}
			else if (serverType == ServerType.DataWarehouse)
			{
				serverName = BiServers.LoadDataWarehouseServerWithoutCache(Db.Connection);
			}

			if (!string.IsNullOrWhiteSpace(serverName))
			{
				if (serverType == ServerType.Audit)
				{
					return CheckBiDatabaseExistsAndIsUpdated(serverName, Db.AuditDatabaseName);
				}
				else if (serverType == ServerType.DataWarehouse)
				{
					return CheckBiDatabaseExistsAndIsUpdated(serverName, Db.EdwDatabaseName);
				}
			}
			return $"No {serverType} server set in the registry."; // information for logging only.
		}

		static string CheckBiDatabaseExistsAndIsUpdated(string serverName, string dbName)
		{
			var result = string.Empty;

			if (!string.IsNullOrEmpty(serverName))
			{
				using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(serverName, Db.SqlMasterDb))
				{
					if (biConnection.DatabaseExists(dbName))
					{
						var majorDbVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection);
						var minorDbVersion = DbRegistry.DatabaseMinorSchemaVersion.LoadValue(Db.Connection);
						var mainDbVersion = new VersionLabel(majorDbVersion, minorDbVersion).ToString();

						var biDbVersion = BiMasterState.GetBiDatabaseExtPty(biConnection, dbName, BiConstants.MainDbSchemaVersionExtPtyName);
						if (mainDbVersion != biDbVersion)
						{
							result = $"[{dbName}] is not updated. Expected version: {mainDbVersion}, Actual version: {biDbVersion}";
						}
					}
					else
					{
						result = $"[{dbName}] does not exist in the server [{serverName}].";
					}
				}
			}

			return result;
		}

		public static string IsAnalysisServerSet()
		{
			var asServer = BiServers.LoadAnalysisServerUsingCacheIfPossible(Db.Connection);
			if (!string.IsNullOrWhiteSpace(asServer))
			{
				return string.Empty;
			}
			else
			{
				return "No Analysis server set in the registry."; // information for logging only.
			}
		}

		public static string IsCdcDisableFlagTrue()
		{
			var disableCdcFlag = DbRegistry.BiDisableChangeDataCapture.LoadValue(Db.Connection);
			if (disableCdcFlag)
			{
				return "CDC Disabled flag is true"; // information for logging only.
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
