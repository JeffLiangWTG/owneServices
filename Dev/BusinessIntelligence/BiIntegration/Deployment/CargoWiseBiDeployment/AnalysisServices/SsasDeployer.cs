namespace CargoWise.Bi.Deployment.AnalysisServices
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using CargoWise.Bi.Common;
	using CargoWise.Bi.ConfigLoader;
	using CargoWise.Bi.Registration;
	using CargoWise.Bi.Registration.Common;
	using CargoWise.Common;
	using CargoWise.Data;
	using CargoWise.Types;
	using Enterprise.DbUpgrader.Resource.Version;
	using Enterprise.Integration;

	#region SuppressResourceStringsCheckRegion

	public class SsasDeployer : Deployer, IDisposable
	{
		public SsasDeployer(string analysisServer, ILogger logger = null)
		{
			this.analysisServer = analysisServer;
			this.logger = logger;
			biConnection = Db.NewExtraConnectionWithMainDbCredentials(DataWarehouseServer, Db.EdwDatabaseName);
			TabularModelsToSkip = GetTabularModelsToSkip();
			TabularModelsToRedeploy = GetTabularModelsToRedploy();
			biReportUser = new BiReportUser();
		}
		protected readonly string analysisServer;
		readonly ILogger logger;
		public readonly IEnumerable<string> TabularModelsToSkip;
		public readonly IEnumerable<string> TabularModelsToRedeploy;
		DbConnection biConnection;
		readonly BiReportUser biReportUser;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (biConnection != null)
				{
					biConnection.Dispose();
					biConnection = null;
				}
			}
		}

		void Log(LogType logType, string message)
		{
			logger?.Log(logType, message);
		}

		IEnumerable<string> GetTabularModelsToRedploy()
		{
			return DataUtils.GetListOfValuesFromQuery(biConnection, $"SELECT DISTINCT SsasModelFileName FROM [{BiConstants.BiAdminSchemaName}].[SsasCube] WHERE RedeployOnNextBID = 1");
		}

		IEnumerable<string> GetTabularModelsToSkip()
		{
			return DataUtils.GetListOfValuesFromQuery(biConnection, $"SELECT DISTINCT SsasModelFileName FROM [{BiConstants.BiAdminSchemaName}].[SsasCube] WHERE DeployToServer = 0");
		}

		SsasServer Server
		{
			get
			{
				return server ?? (server = SsasServer.New(analysisServer));
			}
		}
		SsasServer server;

		public void DeploySsasModels()
		{
			var exceptionsDict = new Dictionary<string, Exception>();
			foreach (var bimFileName in GetRegisteredBimFiles())
			{
				string bimFileContent = new DeploymentFileLoader().LoadEmbeddedResource(bimFileName);
				string ssasDatabaseName = GetDatabaseNameFromBimFile(bimFileContent);
				string ssasModelFileName = GetSsasModelFileNameFromResourceName(bimFileName);

				if (!TabularModelsToSkip.Contains(ssasModelFileName))
				{
					if (TabularModelsToRedeploy.Contains(ssasModelFileName))
					{
						DropModel(ssasModelFileName, ssasDatabaseName);
					}
					try
					{
						ExecuteDeployment(ssasModelFileName, ssasDatabaseName, bimFileContent);
						CancelModelRedeploy(ssasModelFileName);
					}
					catch (SsasException ex) when (new Regex("Item '.+?' already exists in the collection.", RegexOptions.IgnoreCase).Match(ex.Message).Success)
					{
						try
						{
							Server.DropModel(ssasDatabaseName);
							ExecuteDeployment(ssasModelFileName, ssasDatabaseName, bimFileContent);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							exceptionsDict[ssasModelFileName] = e;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						exceptionsDict[ssasModelFileName] = ex;
					}
				}
				else
				{
					DropModel(ssasModelFileName, ssasDatabaseName);
				}
			}

			try
			{
				PopulateSsasInfo();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				exceptionsDict["SSAS Info"] = ex;
			}

			if (exceptionsDict.Any())
			{
				Server.DropAndRestorePreviousModels();
				throw new SsasException(string.Format(CultureInfo.InvariantCulture,
					"Exceptions thrown while deploying SSAS cubes:\r\n{0}",
					string.Join("\r\n\r\n", exceptionsDict.Select(ex => string.Format(CultureInfo.InvariantCulture, "[{0}] {1}\r\n{2}", ex.Key, ex.Value.Message, ex.Value.StackTrace)))));
			}
		}

		public void DropModel(string ssasModelFileName, string ssasDatabaseName)
		{
			if (Server.DatabaseExists(ssasDatabaseName))
			{
				Log(LogType.Information, $"Dropping model [{ssasModelFileName}]");
				Server.DropModel(ssasDatabaseName);
				ResetModelInfo(ssasModelFileName);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void CancelModelRedeploy(string ssasModelFileName)
		{
			var cancelRedeployModelQuery = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[SsasCube]
SET RedeployOnNextBID = 0
WHERE SsasModelFileName = @SsasModelFileName", BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(cancelRedeployModelQuery))
			{
				cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, ssasModelFileName);
				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void ResetModelInfo(string ssasModelFileName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[SsasCube]
SET SsasModelVersion = NULL,
	LastResetModelInfoUTC = @LastResetModelInfoUTC,
	LastSourceLsnDateTimeUTC = NULL,
	LastProcessingStartDateTimeUTC = NULL,
	LastProcessingFinishDateTimeUTC = NULL,
	IsCubeProcessing = 0
WHERE SsasModelFileName = @SsasModelFileName", BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, ssasModelFileName);
				cmd.AddParameter("@LastResetModelInfoUTC", SqlDbType.DateTime, ZDateTime.UtcNow.ToDateTime());
				cmd.ExecuteNonQuery();
			}
		}

		public void ProcessSsasModels()
		{
			var exceptionsDict = new Dictionary<string, Exception>();
			PopulateSsasInfo();
			SetPartitionsToBeProcessed();

			var hasTabularModelsProcessed = false;

			foreach (var bimFileName in GetRegisteredBimFiles())
			{
				string bimFileContent = new DeploymentFileLoader().LoadEmbeddedResource(bimFileName);
				string ssasDatabaseName = GetDatabaseNameFromBimFile(bimFileContent);
				string ssasModelFileName = GetSsasModelFileNameFromResourceName(bimFileName);
				try
				{
					if (!TabularModelsToSkip.Contains(ssasModelFileName))
					{
						Log(LogType.Information, $"Processing model [{ssasModelFileName}]"); // SQL query

						CheckModelVersion(ssasModelFileName, ssasDatabaseName);
						var partitionCollection = GetPartitionsToBeProcessedForSpecificDatabase(ssasDatabaseName);
						ProcessSsasModel(ssasDatabaseName, partitionCollection);

						hasTabularModelsProcessed = true;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					exceptionsDict[ssasModelFileName] = ex;
				}
			}

			if (exceptionsDict.Any())
			{
				throw new SsasException(string.Format(CultureInfo.InvariantCulture,
					"Exceptions thrown while processing SSAS cubes:\r\n{0}",
					string.Join("\r\n\r\n", exceptionsDict.Select(ex => string.Format(CultureInfo.InvariantCulture, "[{0}] {1}\r\n{2}", ex.Key, ex.Value.Message, ex.Value.StackTrace)))));
			}
			else if (hasTabularModelsProcessed)
			{
				LogCubeProcessingResult();
			}
			else
			{
				Log(LogType.Debug, "No tabular models to process.");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void LogCubeProcessingResult()
		{
			var message = new StringBuilder();
			message.AppendLine("SSAS cubes processed successfully.");
			message.AppendLine("Last date processed in branch time:");

			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT SsasModelFileName, LastSourceLsnDateTimeUTC, LastProcessingStartDateTimeUTC, LastProcessingFinishDateTimeUTC FROM [{0}].[SsasCube]", BiConstants.BiAdminSchemaName);
			using (var reader = biConnection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var ssasModelFileName = reader["SsasModelFileName"].ToString();
					var lastSourceLsnDateTimeUtcObj = reader["LastSourceLsnDateTimeUTC"];
					var lastProcessingStartDateTimeUtcObj = reader["LastProcessingStartDateTimeUTC"];
					var lastProcessingFinishDateTimeUtcObj = reader["LastProcessingFinishDateTimeUTC"];

					if (!TabularModelsToSkip.Contains(ssasModelFileName))
					{
						message.AppendLine(string.Format(CultureInfo.InvariantCulture,
							"{0}\r\nLastSourceLsnDateTimeUTC: {1}\r\nLastProcessingStartDateTimeUTC: {2}\r\nLastProcessingFinishDateTimeUTC: {3}",
							ssasModelFileName,
							(lastSourceLsnDateTimeUtcObj == DBNull.Value) ? "NULL" : Convert.ToDateTime(lastSourceLsnDateTimeUtcObj, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
							(lastProcessingStartDateTimeUtcObj == DBNull.Value) ? "NULL" : Convert.ToDateTime(lastProcessingStartDateTimeUtcObj, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
							(lastProcessingFinishDateTimeUtcObj == DBNull.Value) ? "NULL" : Convert.ToDateTime(lastProcessingFinishDateTimeUtcObj, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
						));
					}
				}
			}

			Log(LogType.Debug, message.ToString());
		}

		protected virtual string GetSsasModelFileNameFromResourceName(string resourceName)
		{
			var ssasCube = BiAutomationConfigLoader.Instance.ConfigData.SsasCubes.Where(c => new Regex(string.Format(CultureInfo.InvariantCulture, @"\.{0}\.bim", c.SsasModelFileName)).Match(resourceName).Success).FirstOrDefault();
			if (ssasCube != null)
			{
				return ssasCube.SsasModelFileName;
			}
			else
			{
				throw new SsasException(string.Format(CultureInfo.InvariantCulture, "Resource '{0}' is not registered as a valid cube for CW1.", resourceName));
			}
		}

		protected virtual ReadOnlyDictionary<string, VersionLabel> ModelVersions
		{
			get
			{
				return SsasProjectVersion.ModelVersions;
			}
		}

		protected virtual IEnumerable<string> GetRegisteredBimFiles()
		{
			return new DeploymentFileLoader().GetSsasResourceFileNames();
		}

		#region Deployment

		protected virtual void ExecuteDeployment(string ssasModelFileName, string ssasDatabaseName, string bimFileContent)
		{
			if (!Server.DatabaseExists(ssasDatabaseName) || !IsCubeUpdated(ssasModelFileName, ssasDatabaseName))
			{
				Log(LogType.Information, $"Deploying model [{ssasModelFileName}]"); // SQL query
				var executeCommand = PrepareDeploymentCommand(ssasDatabaseName, bimFileContent);

				Server.BackupModel(ssasDatabaseName);
				Server.DropModel(ssasDatabaseName);
				Server.ExecuteBatchCommand(executeCommand);

				if (Server.DatabaseExists(ssasDatabaseName))
				{
					VersionLabel modelVersion;
					if (ModelVersions.TryGetValue(ssasModelFileName, out modelVersion))
					{
						Server.SetDatabaseVersion(ssasDatabaseName, modelVersion.ToString());
					}

					if (Server.DataSourceExists(ssasDatabaseName, "EDW"))
					{
						var alterCommand = PrepareAlterEdwDataSourceCommand(ssasDatabaseName, Provider.MSOLEDBSQL);
						Server.ExecuteBatchCommand(alterCommand);
					}
				}
			}

			if (Server.DatabaseExists(ssasDatabaseName))
			{
				AddReaderRole(ssasDatabaseName);
			}
		}

		bool IsCubeUpdated(string ssasModelFileName, string ssasDatabaseName)
		{
			var result = true;
			VersionLabel expectedVersion;
			if (ModelVersions.TryGetValue(ssasModelFileName, out expectedVersion))
			{
				var ssasVersion = Server.GetDatabaseVersion(ssasDatabaseName);
				if (ssasVersion != expectedVersion.ToString())
				{
					result = false;
				}
			}
			else
			{
				throw new SsasException(string.Format(CultureInfo.InvariantCulture, "Version for analysis cube [{0}] was not found.", ssasModelFileName));
			}

			return result;
		}

		void AddReaderRole(string ssasDatabaseName)
		{
			try
			{
				if (!biReportUser.IsNull)
				{
					Server.TryAddOrUpdateDatabaseReaderRole(ssasDatabaseName, biReportUser.DomainAndUsername);
				}

				if (!string.IsNullOrEmpty(ClientSystemGroup))
				{
					Server.TryAddOrUpdateDatabaseReaderRole(ssasDatabaseName, ClientSystemGroup);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "[{0}] - Failed to assign Report user as reader.\r\n{1}", ssasDatabaseName, ex.Message));
			}
		}

		public bool CheckReaderRoleHasCompleteMembers(string ssasDatabaseName)
		{
			var result = true;
			if (!biReportUser.IsNull)
			{
				result &= Server.CheckUserHasReaderRole(ssasDatabaseName, biReportUser.DomainAndUsername);
			}

			if (!string.IsNullOrWhiteSpace(ClientSystemGroup))
			{
				result &= Server.CheckUserHasReaderRole(ssasDatabaseName, ClientSystemGroup);
			}

			return result;
		}

		void PopulateSsasInfo()
		{
			if (!ssasInfoPopulated)
			{
				using (var transactionManager = biConnection.BeginTransactionWithManager())
				{
					try
					{
						PopulateSsasInfoSafe();
						ssasInfoPopulated = true;
					}
					finally
					{
						transactionManager.CommitTransaction();
					}
				}
			}
		}

		bool ssasInfoPopulated;

		void PopulateSsasInfoSafe()
		{
			var ssasInfoPopulator = new SsasInfoPopulator(biConnection);
			ssasInfoPopulator.PopulateSsasInfo();

			var mapper = new EdwModelTableToSsasTableMapper(BiAutomationConfigLoader.Instance.ConfigData, biConnection);
			mapper.PopulateMapping();
		}

		string PrepareDeploymentCommand(string ssasDatabaseName, string bimFileContent)
		{
			ReplaceDatabaseFields(ref bimFileContent, ssasDatabaseName);

			return string.Format(CultureInfo.InvariantCulture, @"
{{
	""createOrReplace"": {{
		""object"": {{
				""database"": ""{0}""
		}},
		""database"": {1}
	}}
}}",
			ssasDatabaseName,
			bimFileContent);
		}

		string PrepareAlterEdwDataSourceCommand(string ssasDatabaseName, Provider provider)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
{{
	""alter"": {{
		""object"": {{
			""database"": ""{0}"",
			""dataSource"": ""EDW""
		}},
		""dataSource"": {{
			""name"": ""EDW"",
			""connectionString"": ""Provider={1};Data Source={2};Persist Security Info=True;Initial Catalog={3};Integrated Security=True;Application Name={6}"",
			""impersonationMode"": ""impersonateServiceAccount""
		}}
	}}
}}",
				ssasDatabaseName,
				(provider == Provider.MSOLEDBSQL) ? "MSOLEDBSQL" : "SQLOLEDB.1",
				BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection).Replace("\\", "\\\\"),
				Db.EdwDatabaseName,
				"SSAS " + ssasDatabaseName
#if DEBUG
					+ (Enterprise.ZArchitecture.Environment.Globals.IsTest ? " (" + Guid.NewGuid() + ")" : "")
#endif
			);
		}

		enum Provider
		{
			MSOLEDBSQL = 0,
			NativeClient
		}

		void ReplaceDatabaseFields(ref string bimFileContent, string ssasDatabaseName)
		{
			Regex databaseNameFieldRegex = new Regex(@"(?<databaseNameField>""name""\s*:\s*"".+"")", RegexOptions.IgnoreCase);
			Match match = databaseNameFieldRegex.Match(bimFileContent);
			if (match.Success)
			{
				var matchGroup = match.Groups["databaseNameField"];
				if (matchGroup != null)
				{
					bimFileContent = bimFileContent.Replace(match.Groups["databaseNameField"].Value, string.Format(CultureInfo.InvariantCulture, @"""name"": ""{0}""", ssasDatabaseName));
				}
				else
				{
					throw new SsasException("Cannot find database name in script.");
				}
			}

			Regex databaseIdFieldRegex = new Regex(@"(?<databaseIdField>""id""\s*:\s*"".+"")", RegexOptions.IgnoreCase);
			match = databaseIdFieldRegex.Match(bimFileContent);
			if (match.Success)
			{
				var matchGroup = match.Groups["databaseIdField"];
				if (matchGroup != null)
				{
					bimFileContent = bimFileContent.Replace(match.Groups["databaseIdField"].Value, string.Format(CultureInfo.InvariantCulture, @"""id"": ""{0}""", ssasDatabaseName));
				}
				else
				{
					throw new SsasException("Cannot find database id in script.");
				}
			}
		}

		string GetDatabaseNameFromBimFile(string bimFileContent)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", GetClientName(), GetSsasModelLogicalNameFromBimFile(bimFileContent));
		}

		protected string GetSsasModelLogicalNameFromBimFile(string bimFileContent)
		{
			Regex ssasModelLogicalNameRegex = new Regex(@"""name""\s*:\s*""(?<logicalName>.+)""", RegexOptions.IgnoreCase);
			Match match = ssasModelLogicalNameRegex.Match(bimFileContent);
			string ssasModelLogicalName = "";

			if (match.Success)
			{
				var matchGroup = match.Groups["logicalName"];

				if (matchGroup == null)
				{
					throw new SsasException("Cannot find name in BIM file.");
				}
				else
				{
					ssasModelLogicalName = match.Groups["logicalName"].Value;
				}
			}

			return ssasModelLogicalName;
		}

		protected virtual string GetClientName()
		{
			return Db.DatabaseName;
		}

		string DataWarehouseServer
		{
			get
			{
				return dataWarehouseServer ?? (dataWarehouseServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string dataWarehouseServer;

		#endregion

		#region Cube processing

		void CheckModelVersion(string ssasModelFileName, string ssasDatabaseName)
		{
			if (Server.DatabaseExists(ssasDatabaseName))
			{
				VersionLabel ssasVersion;
				if (ModelVersions.TryGetValue(ssasModelFileName, out ssasVersion))
				{
					if (Server.GetDatabaseVersion(ssasDatabaseName) != ssasVersion.ToString())
					{
						throw new SsasException(string.Format(CultureInfo.InvariantCulture, "[{0}] is incompatible with the current schema. Restore it with version {1} or drop the database.", ssasDatabaseName, ssasVersion));
					}
				}
				else
				{
					throw new SsasException(string.Format(CultureInfo.InvariantCulture, "Version for analysis cube [{0}] was not found.", ssasModelFileName));
				}
			}
			else
			{
				throw new SsasException(string.Format(CultureInfo.InvariantCulture, "Analysis cube [{0}] does not exist in server [{1}].", ssasDatabaseName, analysisServer));
			}
		}

		void SetPartitionsToBeProcessed()
		{
			using (var transactionManager = biConnection.BeginTransactionWithManager())
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[SsasPartition]
SET ProcessingRequired = 1
WHERE SsasPartitionID IN
(
	SELECT p.SsasPartitionID
	FROM [{0}].SsasPartition p
	INNER JOIN [{0}].EdwModelTableToSsasTableMapping m
		ON p.SsasTableID = m.SsasTableID
	INNER JOIN [{0}].SsasPartitionUnprocessedDate ud
		ON ud.SchemaName = m.EdwModelSchema AND ud.TableName = m.EdwModelTable
	WHERE
		ud.CreateDate IS NULL OR
		((p.FromValue IS NULL OR p.FromValue <= ud.CreateDate) AND
		 (p.ToValue IS NULL OR p.ToValue > ud.CreateDate))
)

TRUNCATE TABLE [{0}].[SsasPartitionUnprocessedDate]",
					BiConstants.BiAdminSchemaName);

				biConnection.ExecuteNonQuery(sqlText);
				transactionManager.CommitTransaction();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		PartitionCollection GetPartitionsToBeProcessedForSpecificDatabase(string ssasDatabaseName)
		{
			PartitionCollection result = null;

			bool cubeHasLastProcessingFinishDateTime;
			string ssasModelFileName = GetSsasModelFileNameFromDatabaseName(ssasDatabaseName);
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"IF EXISTS (SELECT NULL FROM [{0}].[SsasCube] WHERE SsasModelFileName = @SsasModelFileName AND LastProcessingFinishDateTimeUTC IS NOT NULL AND IsCubeProcessing = 2) SELECT 1 ELSE SELECT 0",
				BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, ssasModelFileName);
				cubeHasLastProcessingFinishDateTime = Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}

			if (cubeHasLastProcessingFinishDateTime)
			{
				result = new PartitionCollection();
				sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT t.TableName, p.PartitionName
FROM [{0}].[SsasCube] c
INNER JOIN [{0}].[SsasTable] t
	ON c.SsasCubeID = t.SsasCubeID
INNER JOIN [{0}].[SsasPartition] p
	ON t.SsasTableID = p.SsasTableID
WHERE p.ProcessingRequired = 1 AND c.SsasModelFileName = @SsasModelFileName",
					BiConstants.BiAdminSchemaName);

				using (var cmd = biConnection.Command(sqlText))
				{
					cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, ssasModelFileName);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var tableName = reader["TableName"].ToString();
							var partitionName = reader["partitionName"].ToString();

							result.Add(ssasDatabaseName, tableName, partitionName);
						}
					}
				}
			}

			return result;
		}

		public void ProcessSsasModel(string ssasDatabaseName, PartitionCollection partitionCollection = null, int attempts = 0, int sleepDuration = 10)
		{
			SetCubeProcessingFlagStartForModel(ssasDatabaseName);

			try
			{
				ProcessSsasModelUnsafe(ssasDatabaseName, partitionCollection);
			}
			catch (SsasException ex) when (ex.Message.Contains("The provider 'MSOLEDBSQL' is not registered."))
			{
				if (Server.DataSourceExists(ssasDatabaseName, "EDW"))
				{
					var alterCommand = PrepareAlterEdwDataSourceCommand(ssasDatabaseName, Provider.NativeClient);
					Server.ExecuteBatchCommand(alterCommand);
					ProcessSsasModelUnsafe(ssasDatabaseName, partitionCollection);
				}
				else
				{
					throw;
				}
			}
			catch (SsasException ex) when (ex.Message.ToLower().Contains("connection was forcibly closed by the remote host"))
			{
				if (Db.GetLockoutReason() == LockoutReason.Upgrade)
				{
					Log(LogType.Warning, $"Connection was closed due to database upgrade. Skipping processing of [{ssasDatabaseName}].");
					return;
				}
				else if (attempts < 5)
				{
					Log(LogType.Warning, "Connection was closed unexpectedly. SSAS Model will process again shortly");
					var sleepInterval = TimeSpan.FromSeconds(sleepDuration);
					Thread.Sleep(sleepInterval);

					ProcessSsasModel(ssasDatabaseName, partitionCollection, attempts + 1);
				}
				else
				{
					throw;
				}
			}
		}

		protected virtual void ProcessSsasModelUnsafe(string ssasDatabaseName, PartitionCollection partitionCollection)
		{
			var executeCommand = PrepareRefreshCommand(ssasDatabaseName, partitionCollection);
			Server.ExecuteBatchCommand(executeCommand);

			if (partitionCollection != null && partitionCollection.Count > 0)
			{
				executeCommand = PrepareRefreshRecalcCommand(ssasDatabaseName);
				Server.ExecuteBatchCommand(executeCommand);
			}

			SetLastSourceLsnDateTimeUTCForModel(ssasDatabaseName);

			if (Server.TableExists(ssasDatabaseName, "Cube Info"))
			{
				var ssasCubePartition = new PartitionCollection(new List<Partition> { new Partition(ssasDatabaseName, "Cube Info") });
				executeCommand = PrepareRefreshCommand(ssasDatabaseName, ssasCubePartition);
				Server.ExecuteBatchCommand(executeCommand);
			}
		}

		string PrepareRefreshCommand(string ssasDatabaseName, PartitionCollection partitionCollection)
		{
			var objectList = new List<string>();

			if (partitionCollection == null)
			{
				objectList.Add(string.Format(CultureInfo.InvariantCulture, @"
			{{
				""database"": ""{0}""
			}}",
					ssasDatabaseName));
			}
			else
			{
				foreach (var partition in partitionCollection)
				{
					objectList.Add(string.Format(CultureInfo.InvariantCulture, @"
			{{
				""database"": ""{0}"",
				""table"": ""{1}""{2}
			}}",
						ssasDatabaseName,
						partition.TableName,
						!string.IsNullOrEmpty(partition.PartitionName) ?
							string.Format(CultureInfo.InvariantCulture, ",\r\n\t\t\t\t\"partition\" : \"{0}\"", partition.PartitionName) :
							"")
					);
				}
			}

			return objectList.Any() ? string.Format(CultureInfo.InvariantCulture, @"
{{
	""refresh"": {{
		""type"": ""full"",
		""objects"": [
{0}
		]
	}}
}}",
			string.Join(",", objectList)) :
			null;
		}

		string PrepareRefreshRecalcCommand(string ssasModelName)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
{{
	""refresh"": {{
		""type"": ""calculate"",
		""objects"": [
			{{
				""database"": ""{0}""
			}}
		]
	}}
}}", // Refresh command for JSON scripting TMSL
			ssasModelName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void SetLastSourceLsnDateTimeUTCForModel(string ssasDatabaseName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"
DECLARE @LastSourceLsnDateTimeUTC VARCHAR(100)
EXEC [{0}].usp_GetMasterStateParameter @ParamName = 'LAST_LSN_TRANSFORMED_UTC', @ParamValue = @LastSourceLsnDateTimeUTC OUTPUT

IF @LastSourceLsnDateTimeUTC IS NOT NULL
	UPDATE [{0}].[SsasCube]
	SET
		LastSourceLsnDateTimeUTC = CAST(@LastSourceLsnDateTimeUTC AS DATETIME)
	WHERE SsasModelFileName = @SsasModelFileName

UPDATE [{0}].[SsasCube]
SET
	LastProcessingFinishDateTimeUTC = GETUTCDATE(),
	IsCubeProcessing = 2 --Cube processed
WHERE SsasModelFileName = @SsasModelFileName

UPDATE [{0}].[SsasPartition]
SET ProcessingRequired = 0
WHERE SsasPartitionID IN
(
	SELECT p.SsasPartitionID
	FROM [{0}].[SsasPartition] p
	INNER JOIN [{0}].[SsasTable] t ON p.SsasTableID = t.SsasTableID
	INNER JOIN [{0}].[SsasCube] c ON t.SsasCubeID = c.SsasCubeID
	WHERE c.SsasModelFileName = @SsasModelFileName
)",
				BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, GetSsasModelFileNameFromDatabaseName(ssasDatabaseName));
				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void SetCubeProcessingFlagStartForModel(string ssasModelName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"
UPDATE [{0}].[SsasCube]
SET 
LastProcessingStartDateTimeUTC = GETUTCDATE(),
IsCubeProcessing = 1 --Cube is processing
WHERE SsasModelFileName = @SsasModelFileName
",
				BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, GetSsasModelFileNameFromDatabaseName(ssasModelName));
				cmd.ExecuteNonQuery();
			}
		}

		protected virtual string GetSsasModelFileNameFromDatabaseName(string ssasDatabaseName)
		{
			var ssasModelLogicalName = BiAutomationConfigLoader.Instance.ConfigData.SsasCubes
				.Where(c => ssasDatabaseName == string.Format(CultureInfo.InvariantCulture, "{0}_{1}", GetClientName(), c.SsasModelLogicalName))
				.Select(c => c.SsasModelFileName).FirstOrDefault();
			return ssasModelLogicalName;
		}

		#endregion
	}

	#endregion
}
