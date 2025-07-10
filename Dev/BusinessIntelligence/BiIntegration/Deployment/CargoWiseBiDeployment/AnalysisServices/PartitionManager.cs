using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using Enterprise.Integration;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace CargoWise.Bi.Deployment.AnalysisServices
{
	public class PartitionManager
	{
		public PartitionManager(DbConnection biConnection, string analysisServer, ILogger logger)
		{
			this.biConnection = biConnection;
			this.analysisServer = analysisServer;
			this.logger = logger;

			if (!biConnection.DatabaseExists(Db.EdwDatabaseName))
			{
				throw new SsasException(string.Format(CultureInfo.InvariantCulture, "EDW database does not exist on {0}", biConnection.ServerName));
			}
			else
			{
				tabularModelsToSkip = GetTabularModelsToSkip();
			}
		}
		readonly DbConnection biConnection;
		readonly string analysisServer;
		readonly ILogger logger;
		readonly IEnumerable<string> tabularModelsToSkip;

		void Log(LogType logType, string message)
		{
			logger?.Log(logType, message);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		protected virtual DateTime CurrentDay
		{
			get
			{
				if (currentDay == null)
				{
					currentDay = DateTime.UtcNow;
				}
				return currentDay.Value;
			}
		}
		DateTime? currentDay;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Information log")]
		public void PartitionCubes()
		{
			var ssasCubes = BiAutomationConfigLoader.Instance.ConfigData.SsasCubes.Where(c => !tabularModelsToSkip.Contains(c.SsasModelFileName));
			if (ssasCubes.Any())
			{
				foreach (var ssasCube in ssasCubes)
				{
					PartitionCube(ssasCube);
				}
			}
		}

		public void PartitionCube(SsasCubesRow ssasCube)
		{
			var analysisDbName = GetSsasDatabaseName(ssasCube.SsasModelLogicalName);
			if (Server.DatabaseExists(analysisDbName))
			{
				Log(LogType.Information, $"Partitioning model [{ssasCube.SsasModelLogicalName}]"); // SQL query
				var newPartitionCollection = CreatePartitionList();
				UpdateSsasPartitionTable(ssasCube.SsasModelFileName, newPartitionCollection);
				CreateAndMergePartitions(analysisDbName, newPartitionCollection);
				DeleteOutdatedPartitions(analysisDbName, newPartitionCollection);
			}
		}

		protected virtual string GetSsasDatabaseName(string databaseName)
		{
			return Db.DatabaseName + "_" + databaseName;
		}

		#region Partition Creation and Merging

		void CreateAndMergePartitions(string analysisDbName, PartitionCollection newPartitionList)
		{
			var partitionsFromSsasServer = Server.GetPartitionCollectionFromCube(analysisDbName);
			var ssasCube = BiAutomationConfigLoader.Instance.ConfigData.SsasCubes.Where(c => GetSsasDatabaseName(c.SsasModelLogicalName) == analysisDbName).FirstOrDefault();
			if (ssasCube != null)
			{
				foreach (var ssasTable in ssasCube.GetSsasTablesRows().Where(t => !t.IsCalculated))
				{
					var expectedPartitions = new PartitionCollection(newPartitionList.List.Where(p => p.DatabaseName == analysisDbName && p.TableName == ssasTable.TableName).OrderBy(p => p.FromValue));
					var actualPartitions = new PartitionCollection(partitionsFromSsasServer.List.Where(p => p.DatabaseName == analysisDbName && p.TableName == ssasTable.TableName).OrderBy(p => p.FromValue));
					var validatedPartitions = FixGapsAndOverlaps(actualPartitions);

					CreatePartitionsForTable(ssasCube, ssasTable, expectedPartitions, validatedPartitions);
				}
			}
		}

		const string tempPartitionName = "TempPartition";

		PartitionCollection FixGapsAndOverlaps(PartitionCollection inputCollection)
		{
			var result = new PartitionCollection();
			DateTime? expectedFromValue = null;

			foreach (var currentPartition in inputCollection)
			{
				if (expectedFromValue < currentPartition.FromValue || (expectedFromValue == null && currentPartition.FromValue != null))
				{
					result.Add(currentPartition.DatabaseName, currentPartition.TableName, tempPartitionName, expectedFromValue, currentPartition.FromValue, true);
					result.Add(currentPartition.DatabaseName, currentPartition.TableName, currentPartition.PartitionName, currentPartition.FromValue, currentPartition.ToValue, false);
				}
				else if (expectedFromValue > currentPartition.FromValue && expectedFromValue != currentPartition.ToValue)
				{
					result.Add(currentPartition.DatabaseName, currentPartition.TableName, tempPartitionName, expectedFromValue, currentPartition.ToValue, true);
				}
				else
				{
					result.Add(currentPartition.DatabaseName, currentPartition.TableName, currentPartition.PartitionName, currentPartition.FromValue, currentPartition.ToValue, false);
				}
				expectedFromValue = currentPartition.ToValue;
			}

			return result;
		}

		void CreatePartitionsForTable(SsasCubesRow ssasCube, SsasTablesRow ssasTable, PartitionCollection expectedPartitions, PartitionCollection validatedPartitions)
		{
			foreach (var partition in expectedPartitions)
			{
				var coincidingPartitions = GetCoincidingPartitions(partition, validatedPartitions);
				if (!coincidingPartitions.List.Any() ||
					HasOverlapOrPartitionsToBeProcessed(partition, coincidingPartitions))
				{
					CreatePartition(ssasCube, ssasTable, partition);
				}
				else if (coincidingPartitions.Count == 1)
				{
					var oldPartition = coincidingPartitions.List.FirstOrDefault();
					if (oldPartition != null)
					{
						if (oldPartition.PartitionName != partition.PartitionName || oldPartition.FromValue != partition.FromValue || oldPartition.ToValue != partition.ToValue)
						{
							CreatePartition(ssasCube, ssasTable, partition);
						}
					}
				}
				else
				{
					Server.MergePartitions(partition.DatabaseName, ssasTable.TableName, partition.PartitionName, coincidingPartitions.List.Select(p => p.PartitionName), "EDW", GetPartitionQuery(ssasTable, partition));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void SetProcessingRequiredForPartition(string ssasModelFileName, Partition partition)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
UPDATE [{0}].[SsasPartition]
	SET ProcessingRequired = 1
FROM [{0}].[SsasPartition] p
INNER JOIN [{0}].[SsasTable] t
	ON p.SsasTableID = t.SsasTableID
INNER JOIN [{0}].[SsasCube] c
	ON t.SsasCubeID = c.SsasCubeID
WHERE c.SsasModelFileName = @SsasModelFileName
	AND t.TableName = @TableName
	AND p.PartitionName = @PartitionName",
				BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@SsasModelFileName", SqlDbType.NVarChar, 128, ssasModelFileName);
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, 128, partition.TableName);
				cmd.AddParameter("@PartitionName", SqlDbType.NVarChar, 128, partition.PartitionName);
				cmd.ExecuteNonQuery();
			}
		}

		bool HasOverlapOrPartitionsToBeProcessed(Partition partition, PartitionCollection coincidingPartitions)
		{
			var result = false;

			var fromValue = (partition.FromValue != null) ? partition.FromValue.Value : DateTime.MinValue;
			var toValue = (partition.ToValue != null) ? partition.ToValue.Value : DateTime.MaxValue;

			if (coincidingPartitions.List.Select(p => (p.FromValue != null) ? p.FromValue.Value : DateTime.MinValue).Min() < fromValue ||
					coincidingPartitions.List.Select(p => (p.ToValue != null) ? p.ToValue.Value : DateTime.MaxValue).Max() > toValue ||
					coincidingPartitions.List.Any(p => p.NeedsProcessing))
			{
				result = true;
			}

			return result;
		}

		void CreatePartition(SsasCubesRow ssasCube, SsasTablesRow ssasTable, Partition partition)
		{
			SetProcessingRequiredForPartition(ssasCube.SsasModelFileName, partition);
			Server.CreatePartition(partition.DatabaseName, ssasTable.TableName, partition.PartitionName, GetPartitionQuery(ssasTable, partition));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Partition Query in SQL")]
		string GetPartitionQuery(SsasTablesRow ssasTable, Partition partition)
		{
			var whereClauseList = new List<string>();
			if (partition.FromValue != null)
			{
				whereClauseList.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] >= '{1}'", ssasTable.PartitionKeyName, SqlFormatInfo.ToSqlDateString(partition.FromValue.Value)));
			}
			if (partition.ToValue != null)
			{
				whereClauseList.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] < '{1}'", ssasTable.PartitionKeyName, SqlFormatInfo.ToSqlDateString(partition.ToValue.Value)));
			}
			var whereClause = string.Join(" AND ", whereClauseList);

			if (!string.IsNullOrEmpty(whereClause))
			{
				return string.Format(CultureInfo.InvariantCulture,
					@"SELECT * FROM (
{0}
) Q
{1}",
					ssasTable.Query,
					string.Format(CultureInfo.InvariantCulture, "WHERE {0}", whereClause)
					);
			}
			else
			{
				return ssasTable.Query;
			}
		}

		PartitionCollection GetCoincidingPartitions(Partition targetPartition, PartitionCollection sourcePartitions)
		{
			var result = new PartitionCollection();

			foreach (var partition in sourcePartitions)
			{
				var sourceFromValue = (partition.FromValue != null) ? partition.FromValue.Value : DateTime.MinValue;
				var sourceToValue = (partition.ToValue != null) ? partition.ToValue.Value : DateTime.MaxValue;
				var targetFromValue = (targetPartition.FromValue != null) ? targetPartition.FromValue.Value : DateTime.MinValue;
				var targetToValue = (targetPartition.ToValue != null) ? targetPartition.ToValue.Value : DateTime.MaxValue;

				if ((sourceFromValue >= targetFromValue && sourceFromValue < targetToValue) ||
					(sourceToValue > targetFromValue && sourceToValue <= targetToValue) ||
					(targetFromValue >= sourceFromValue && targetFromValue < sourceToValue) ||
					(targetToValue > sourceFromValue && targetToValue <= sourceToValue))
				{
					result.Add(partition);
				}
			}

			return result;
		}

		void DeleteOutdatedPartitions(string analysisDbName, PartitionCollection updatedPartitionList)
		{
			var partitionsFromSsasServer = Server.GetPartitionCollectionFromCube(analysisDbName);
			foreach (var outdatedPartition in partitionsFromSsasServer.List
				.Where(p => !updatedPartitionList.List.Any(up =>
					up.DatabaseName == p.DatabaseName &&
					up.TableName == p.TableName &&
					up.PartitionName == p.PartitionName &&
					up.FromValue == p.FromValue &&
					up.ToValue == p.ToValue)))
			{
				Server.DeletePartition(analysisDbName, outdatedPartition.TableName, outdatedPartition.PartitionName);
			}
		}

		#endregion

		#region Update Partition Definition

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query")]
		protected void UpdateSsasPartitionTable(string ssasModelFileName, PartitionCollection newPartitionList)
		{
			if (newPartitionList != null && newPartitionList.Count > 0)
			{
				var partitionValueList = new List<string>();
				foreach (var partition in newPartitionList)
				{
					partitionValueList.Add(string.Format(CultureInfo.InvariantCulture,
						"'{0}', '{1}', '{2}', {3}, {4}",
						ssasModelFileName,
						partition.TableName,
						partition.PartitionName,
						(partition.FromValue != null) ? string.Format(CultureInfo.InvariantCulture, "CONVERT(datetime, '{0}', 121)", partition.FromValue.Value.ToString(CultureInfo.InvariantCulture)) : "NULL",
						(partition.ToValue != null) ? string.Format(CultureInfo.InvariantCulture, "CONVERT(datetime, '{0}', 121)", partition.ToValue.Value.ToString(CultureInfo.InvariantCulture)) : "NULL"));
				}

				var sqlText = string.Format(CultureInfo.InvariantCulture,
	@"
CREATE TABLE #TempTable
(
	SsasModelFileName varchar(100) COLLATE database_default,
	TableName varchar(100) COLLATE database_default,
	PartitionName varchar(100) COLLATE database_default,
	FromValue date,
	ToValue date
)

INSERT INTO #TempTable (SsasModelFileName, TableName, PartitionName, FromValue, ToValue)
VALUES
	({0})

DELETE FROM [{1}].SsasPartition
WHERE SsasPartitionID NOT IN
(
	SELECT p.SsasPartitionID FROM [{1}].SsasPartition p
	INNER JOIN [{1}].SsasTable t ON p.SsasTableID = t.SsasTableID
	INNER JOIN [{1}].SsasCube c ON t.SsasCubeID = c.SsasCubeID
	INNER JOIN #TempTable tt ON tt.SsasModelFileName = c.SsasModelFileName AND tt.TableName = t.TableName AND tt.PartitionName = p.PartitionName
	WHERE tt.FromValue = p.FromValue AND tt.ToValue = p.ToValue
)
AND SsasPartitionID IN
(
	SELECT p.SsasPartitionID FROM [{1}].SsasPartition p
	INNER JOIN [{1}].SsasTable t ON p.SsasTableID = t.SsasTableID
	INNER JOIN [{1}].SsasCube c ON t.SsasCubeID = c.SsasCubeID
	AND c.SsasModelFileName = '{2}'
)

INSERT INTO [{1}].SsasPartition (SsasTableID, PartitionName, FromValue, ToValue, ProcessingRequired)
SELECT t.SsasTableID, tt.PartitionName, tt.FromValue, tt.ToValue, 0
FROM #TempTable tt
INNER JOIN [{1}].SsasCube c ON tt.SsasModelFileName = c.SsasModelFileName
INNER JOIN [{1}].SsasTable t ON tt.TableName = t.TableName AND c.SsasCubeID = t.SsasCubeID
LEFT JOIN  [biadmin].SsasPartition p ON p.SsasTableID = t.SsasTableID AND tt.PartitionName = p.PartitionName
WHERE p.SsasPartitionID IS NULL

DROP TABLE #TempTable
",
				string.Join("),\r\n\t(", partitionValueList),
				BiConstants.BiAdminSchemaName,
				ssasModelFileName);

				using (((ICurrentDbControl)biConnection).UseDatabase(Db.EdwDatabaseName))
				{
					biConnection.ExecuteNonQuery(sqlText);
				}
			}
		}

		#endregion

		#region Expected Partition Collection

		protected PartitionCollection CreatePartitionList()
		{
			var result = new PartitionCollection();
			var partitionDefinition = CreatePartitionDefinition();
			foreach (var ssasCube in BiAutomationConfigLoader.Instance.ConfigData.SsasCubes)
			{
				foreach (var ssasTable in ssasCube.GetSsasTablesRows())
				{
					var databaseName = GetSsasDatabaseName(ssasCube.SsasModelLogicalName);
					var tableName = ssasTable.TableName;

					if (!string.IsNullOrEmpty(ssasTable.PartitionKeyName))
					{
						foreach (var def in partitionDefinition)
						{
							var partitionName = tableName + "_" + def.Name;
							result.Add(databaseName, tableName, partitionName, def.FromValue, def.ToValue);
						}
					}
					else
					{
						result.Add(databaseName, tableName, tableName, null, null);
					}
				}
			}
			return result;
		}

		List<PartitionDefinition> CreatePartitionDefinition()
		{
			var result = new List<PartitionDefinition>();
			CreateWeeklyPartitions(result);
			CreateMonthlyPartitions(result);
			CreateYearlyPartitions(result);

			return result;
		}

		void CreateWeeklyPartitions(List<PartitionDefinition> result)
		{
			var weekCounter = 4;
			var fromValue = new DateTime(CurrentDay.Year, CurrentDay.Month, 22);
			var toValue = new DateTime(CurrentDay.AddMonths(1).Year, CurrentDay.AddMonths(1).Month, 1);
			var lastPartitionStartDate = new DateTime(CurrentDay.AddMonths(-1).Year, CurrentDay.AddMonths(-1).Month, 1);
			var isLastMonth = false;

			while (fromValue >= lastPartitionStartDate)
			{
				if (fromValue < CurrentDay)
				{
					var name = "Y" + fromValue.Year.ToString("0000", CultureInfo.InvariantCulture) + "M" + fromValue.Month.ToString("00", CultureInfo.InvariantCulture) + "W" + weekCounter.ToString(CultureInfo.InvariantCulture);
					result.Add(new PartitionDefinition(name, fromValue, toValue));
				}

				toValue = fromValue;
				fromValue = fromValue.AddDays(-7);

				weekCounter--;
				if (weekCounter == 0 && !isLastMonth)
				{
					isLastMonth = true;
					weekCounter = 4;
					fromValue = new DateTime(CurrentDay.AddMonths(-1).Year, CurrentDay.AddMonths(-1).Month, 22);
					toValue = new DateTime(CurrentDay.Year, CurrentDay.Month, 1);
				}
			}
		}

		void CreateMonthlyPartitions(List<PartitionDefinition> result)
		{
			var fromValue = new DateTime(CurrentDay.AddMonths(-2).Year, CurrentDay.AddMonths(-2).Month, 1);
			var toValue = fromValue.AddMonths(1);
			var lastPartitionStartDate = new DateTime(CurrentDay.AddYears(-1).Year, 1, 1);

			while (fromValue >= lastPartitionStartDate)
			{
				var name = "Y" + fromValue.Year.ToString("0000", CultureInfo.InvariantCulture) + "M" + fromValue.Month.ToString("00", CultureInfo.InvariantCulture);
				result.Add(new PartitionDefinition(name, fromValue, toValue));

				toValue = fromValue;
				fromValue = fromValue.AddMonths(-1);
			}
		}

		void CreateYearlyPartitions(List<PartitionDefinition> result)
		{
			var fromValue = new DateTime(CurrentDay.AddYears(-2).Year, 1, 1);
			var toValue = fromValue.AddYears(1);
			var lastPartitionStartDate = new DateTime(CurrentDay.AddYears(-6).Year, 1, 1);
			string name;

			while (fromValue >= lastPartitionStartDate)
			{
				name = "Y" + fromValue.Year.ToString("0000", CultureInfo.InvariantCulture);
				result.Add(new PartitionDefinition(name, fromValue, toValue));

				toValue = fromValue;
				fromValue = fromValue.AddYears(-1);
			}

			name = "YP" + fromValue.Year.ToString("0000", CultureInfo.InvariantCulture);
			result.Add(new PartitionDefinition(name, null, toValue));
		}

		class PartitionDefinition
		{
			public PartitionDefinition(string name, DateTime? fromValue, DateTime toValue)
			{
				Name = name;
				FromValue = fromValue;
				ToValue = toValue;
			}
			public string Name { get; private set; }
			public DateTime? FromValue { get; private set; }
			public DateTime ToValue { get; private set; }
		}

		#endregion
	}
}
