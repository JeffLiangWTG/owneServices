using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class AnalysisServerInformation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
		public AnalysisServerInformation(string analysisServer, string dataWarehouseServer)
		{
			if (!string.IsNullOrEmpty(analysisServer))
			{
				ServerName = analysisServer;
				DataWarehouseServer = dataWarehouseServer;
				try
				{
					using (var server = SsasServer.New(ServerName))
					{
						ServerVersion = server.ServerVersion;
						ServerMode = server.ServerMode;
						RetrieveModelMemoryUsage(server);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException() && ex.Message.Contains("user does not have permission"))
				{
					ServerVersion = adminRightsRequired;
					ServerMode = adminRightsRequired;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServerVersion = ex.Message;
					ServerMode = ex.Message;
				}
			}
		}
		readonly string DataWarehouseServer;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message when admin rights required. Might be for CWSupport only.")]
		public const string adminRightsRequired = "(Requires administrator rights to retrieve this information)";

		public string ServerName { get; private set; } = string.Empty;
		public string ServerVersion { get; private set; } = string.Empty;
		public string ServerMode { get; private set; } = string.Empty;
		public string TotalEstimatedMemoryUsage { get; private set; } = string.Empty;

		public SsasCubeCollection SsasCubes { get; private set; }

		readonly Dictionary<string, decimal> tabularModelMemoryUsage = new Dictionary<string, decimal>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query, filter query, exception message, file size format")]
		public virtual void RetrieveModelMemoryUsage(SsasServer server)
		{
			decimal totalMemoryUsage = 0;
			var unit = "MB";

			try
			{
				var dataTable = server.GetDataTableFromQuery("", "select OBJECT_PARENT_PATH, OBJECT_ID, OBJECT_MEMORY_SHRINKABLE, OBJECT_MEMORY_NONSHRINKABLE, OBJECT_MEMORY_CHILD_SHRINKABLE, OBJECT_MEMORY_CHILD_NONSHRINKABLE from $system.Discover_object_memory_usage order by object_parent_path");
				var databaseMemoryUsage = dataTable.Select($"OBJECT_PARENT_PATH LIKE '%[.]Databases' AND OBJECT_ID LIKE '{Db.DatabaseName}[_]%'");

				if (databaseMemoryUsage != null)
				{
					foreach (DataRow database in databaseMemoryUsage)
					{
						var databaseName = database["OBJECT_ID"].ToString();
						var memoryUsageShrinkable = Convert.ToDecimal(database["OBJECT_MEMORY_SHRINKABLE"], CultureInfo.InvariantCulture) / 1048576;
						var memoryUsageNonShrinkable = Convert.ToDecimal(database["OBJECT_MEMORY_NONSHRINKABLE"], CultureInfo.InvariantCulture) / 1048576;
						var memoryUsageChildShrinkable = Convert.ToDecimal(database["OBJECT_MEMORY_CHILD_SHRINKABLE"], CultureInfo.InvariantCulture) / 1048576;
						var memoryUsageChildNonShrinkable = Convert.ToDecimal(database["OBJECT_MEMORY_CHILD_NONSHRINKABLE"], CultureInfo.InvariantCulture) / 1048576;

						var memoryUsage = memoryUsageShrinkable +
															memoryUsageNonShrinkable +
															memoryUsageChildShrinkable +
															memoryUsageChildNonShrinkable;

						totalMemoryUsage += memoryUsage;

						if (tabularModelMemoryUsage.ContainsKey(databaseName))
						{
							tabularModelMemoryUsage[databaseName] += memoryUsage;
						}
						else
						{
							tabularModelMemoryUsage[databaseName] = memoryUsage;
						}
					}
				}

				if (totalMemoryUsage > 10000)
				{
					totalMemoryUsage /= 1024;
					unit = "GB";
				}
			}
			catch (Microsoft.AnalysisServices.AdomdClient.AdomdErrorResponseException ex) when (ex.Message.Contains("does not have permission"))
			{
				throw new SsasException(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }

			TotalEstimatedMemoryUsage = string.Format(CultureInfo.InvariantCulture, "{0:n} {1}", totalMemoryUsage, unit);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SSAS server mode")]
		public void RefreshInfo()
		{
			if (ServerMode.Equals("Tabular", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(DataWarehouseServer))
			{
				using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(DataWarehouseServer, Db.EdwDatabaseName))
				{
					RetrieveSsasCubeInfo(biConnection);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveSsasCubeInfo(DbConnection biConnection)
		{
			var sqlText = $@"
SELECT SsasModelFileName,
	SsasModelVersion,
	DeployToServer,
	RedeployOnNextBID,
	LastSourceLsnDateTimeUTC,
	LastProcessingStartDateTimeUTC,
	LastProcessingFinishDateTimeUTC,
	LastResetModelInfoUTC,
	EnableEtl,
	IsCubeProcessing
FROM [{BiConstants.BiAdminSchemaName}].SsasCube";

			SsasCubes = new SsasCubeCollection();

			using (var reader = biConnection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var modelFileName = reader["SsasModelFileName"].ToString();
					var ssasCube = new SsasCube(modelFileName);

					ssasCube.ModelVersion = reader["SsasModelVersion"].ToString();
					ssasCube.DeployToServer = Convert.ToBoolean(reader["DeployToServer"], CultureInfo.InvariantCulture);
					ssasCube.RedeployOnNextBID = Convert.ToBoolean(reader["RedeployOnNextBID"], CultureInfo.InvariantCulture);
					ssasCube.EnableEtl = Convert.ToBoolean(reader["EnableEtl"], CultureInfo.InvariantCulture);

					ssasCube.LastSourceLsnDateTimeUtc = new ZDateTime(reader["LastSourceLsnDateTimeUTC"].ToString());
					if (!ssasCube.LastSourceLsnDateTimeUtc.IsEmpty)
					{
						ssasCube.LastSourceLsnDateTimeLocal = new ZDateTime(Env.Time.GetLocalTimeFromUtc(ssasCube.LastSourceLsnDateTimeUtc.ToDateTime()));
					}

					ssasCube.LastProcessingStartDateTimeUtc = new ZDateTime(reader["LastProcessingStartDateTimeUTC"].ToString());
					if (!ssasCube.LastProcessingStartDateTimeUtc.IsEmpty)
					{
						ssasCube.LastProcessingStartDateTimeLocal = new ZDateTime(Env.Time.GetLocalTimeFromUtc(ssasCube.LastProcessingStartDateTimeUtc.ToDateTime()));
					}

					ssasCube.LastProcessingFinishDateTimeUtc = new ZDateTime(reader["LastProcessingFinishDateTimeUTC"].ToString());
					if (!ssasCube.LastProcessingFinishDateTimeUtc.IsEmpty)
					{
						ssasCube.LastProcessingFinishDateTimeLocal = new ZDateTime(Env.Time.GetLocalTimeFromUtc(ssasCube.LastProcessingFinishDateTimeUtc.ToDateTime()));
					}

					ssasCube.LastResetModelInfoUTC = new ZDateTime(reader["LastResetModelInfoUTC"].ToString());
					if (!ssasCube.LastResetModelInfoUTC.IsEmpty)
					{
						ssasCube.LastResetModelInfoLocal = new ZDateTime(Env.Time.GetLocalTimeFromUtc(ssasCube.LastResetModelInfoUTC.ToDateTime()));
					}

					ssasCube.IsCubeProcessing = ((SsasCube.CubeStatus)(Convert.ToInt32(reader["IsCubeProcessing"], CultureInfo.InvariantCulture))).ToString().Replace("_", " ");

					var databaseName = $"{Db.DatabaseName}_{modelFileName.Replace(" ", "")}"; // database name
					if (tabularModelMemoryUsage.ContainsKey(databaseName))
					{
						ssasCube.MemoryUsage = tabularModelMemoryUsage[databaseName];
					}

					SsasCubes.Add(ssasCube);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query")]
		TimeSpan ServerTimeOffset
		{
			get
			{
				if (serverTimeOffset == null)
				{
					using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(DataWarehouseServer, Db.EdwDatabaseName))
					{
						serverTimeOffset = new ZDateTimeOffset(biConnection.ExecuteScalar("select SYSDATETIMEOFFSET()")).Offset;
					}
				}
				return serverTimeOffset.Value;
			}
		}
		TimeSpan? serverTimeOffset;
	}
}
