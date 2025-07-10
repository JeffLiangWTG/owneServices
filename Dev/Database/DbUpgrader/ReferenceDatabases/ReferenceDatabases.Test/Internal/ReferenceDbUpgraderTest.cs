using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	[TestsSubclassesOf(typeof(ReferenceDbUpgrader), (Type)null, excludedTypesAndTheirDescendants: new[] { typeof(ReferenceDbUpgraderForVersionTesting), typeof(ReferenceDbUpgraderForTestGetColumnNames) })]
	abstract class ReferenceDbUpgraderTest<TUpgrader> : RefDbTransactionedTestCase<TUpgrader> where TUpgrader : ReferenceDbUpgrader
	{
		public void TestGetColumnNames()
		{
			var counter = new CSVFileHitCounter();
			refDbUpgrader.CsvFilePopulationStatisticAction += (csvFileName) => counter.CountHit(csvFileName);
			var dbName = refDbUpgrader.DbName;
			if (ExistingDataRequired)
			{
				CreateReferenceDbIfNotExists();

				using (((ICurrentDbControl)testConnection).UseDatabase(dbName))
				{
					PrepareExistingDataBeforeUpgrade(testConnection);
				}
			}
			refDbUpgrader.CreateAndUpgradeIfRequired();

			using (((ICurrentDbControl)testConnection).UseDatabase(dbName))
			{
				var tableName = ExpectedReferenceTables.FirstOrDefault();
				if (!tableName.IsNullOrEmpty())
				{
					AssertNoExceptionThrown(() =>
					{
						var columnNames = ReferenceDbUpgraderForTestGetColumnNames.GetColumnNames_Exposed(dbName, dbName, testConnection, tableName);
						Assert(columnNames.Count > 1);
					});
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestUpgradeReferenceDb()
		{
			var counter = new CSVFileHitCounter();
			refDbUpgrader.CsvFilePopulationStatisticAction += (csvFileName) => counter.CountHit(csvFileName);
			if (ExistingDataRequired)
			{
				CreateReferenceDbIfNotExists();

				using (((ICurrentDbControl)testConnection).UseDatabase(refDbUpgrader.DbName))
				{
					PrepareExistingDataBeforeUpgrade(testConnection);
				}
			}

			AssertEquals("VersionBeforeUpgrade", refDbUpgrader.DbPreparationStrategy.GetVersionFromDatabase(), refDbUpgrader.VersionBeforeUpgrade);

			refDbUpgrader.CreateAndUpgradeIfRequired();

			AssertEquals("VersionBeforeUpgrade is reset after upgrading", refDbUpgrader.LatestVersion, refDbUpgrader.VersionBeforeUpgrade);

			AssertEquals("DB should exist - " + refDbUpgrader.DbName, true, DbObjectCreator.DatabaseExists(testConnection, refDbUpgrader.DbName));

			using (((ICurrentDbControl)testConnection).UseDatabase(refDbUpgrader.DbName))
			{
				AssertTablesExist(testConnection);
				AssertVersionNumber();
				AssertDefaultConstraints(testConnection);
				AssertCSVHitCount(counter.Dict);
				PerformExtraAssertsAfterUpgrade(testConnection);
			}

			// Ref DB version = Latest => Running the upgrade shouldn't do anything, and shouldn't fail either.
			refDbUpgrader.CreateAndUpgradeIfRequired();
		}

		void AssertCSVHitCount(Dictionary<string, List<string>> hitDictionary)
		{
			CombineAssertions("Below CSV files has been hit more than once in the upgrade process, if they are updated in the latest version upgrade code, please double check the occurance of that in the previous version and remove them accordingly. This means you have probably told the system to read and import the same CSV file more than once, likely for different version stamps, meaning the last import clobbers the previous, thus all but the last are pointless.", () =>
			{
				foreach (var csvFile in hitDictionary.Keys)
				{
					var stacks = hitDictionary[csvFile];
					if (stacks.Count > 1)
					{
						Assert(string.Format(CultureInfo.CurrentCulture, "Expected to see exactly one CSV hit for file {0} but saw {1}. Stacks:\r\n {2}", csvFile, stacks.Count, string.Join("\r\n\r\n\r\n", stacks.ToArray())), false);
					}
				}
				Assert(true);
			});
		}

		void CreateReferenceDbIfNotExists()
		{
			using (var adminConn = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(adminConn, refDbUpgrader.DbName);
			}
		}

		void AssertTablesExist(DbConnection refDbConn)
		{
			string sqlText = "SELECT name FROM sys.tables";
			var allTables = DataUtils.GetDataTableFromQuery(refDbConn, sqlText);
			var expectedTableList = new List<string>(ExpectedReferenceTables);

			var newTables = new List<string>();
			foreach (DataRow row in allTables.Rows)
			{
				string tableName = row[0].ToString();
				if (expectedTableList.Contains(tableName))
				{
					expectedTableList.Remove(tableName);
				}
				else
				{
					newTables.Add(tableName);
				}
			}

			var result = new StringBuilder();
			if (newTables.Count > 0)
			{
				result.AppendLine("The following tables are not in the expected list:");
				newTables.ForEach(x => result.AppendLine(x));
				result.AppendLine();
			}
			if (expectedTableList.Count > 0)
			{
				result.AppendLine("The following tables are expected to exists but are missing:");
				expectedTableList.ForEach(x => result.AppendLine(x));
				result.AppendLine();
			}
			if (result.Length > 0)
			{
				Fail(result.ToString());
			}
		}

		void AssertVersionNumber()
		{
			AssertEquals("Version After Upgrade:", refDbUpgrader.LatestVersion, refDbUpgrader.DbPreparationStrategy.GetVersionFromDatabase());
		}

		void AssertDefaultConstraints(DbConnection refDbConn)
		{
			const string sqlText = @"
				SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.columns
				WHERE (COLUMN_NAME LIKE '__[_]%' OR COLUMN_NAME LIKE '___[_]%') AND COLUMN_DEFAULT IS NULL AND IS_NULLABLE = 'NO' 
				AND DATA_TYPE <> 'uniqueidentifier' AND DATA_TYPE <> 'datetime' AND DATA_TYPE <> 'smalldatetime'
				AND TABLE_NAME in (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.Tables WHERE TABLE_TYPE = 'BASE TABLE')";

			using (var reader = refDbConn.Command(sqlText).ExecuteReader())
			{
				var builder = new StringBuilder();
				while (reader.Read())
				{
					builder.AppendLine(reader["COLUMN_NAME"].ToString());
				}

				string assertMsg = string.Format("The following columns in '{0}' don't have default values:\r\n\r\n", refDbConn.CurrentDatabase);
				Assert(assertMsg + builder.ToString(), builder.Length == 0);
			}
		}

		protected virtual bool ExistingDataRequired
		{
			get { return false; }
		}

		protected virtual void PrepareExistingDataBeforeUpgrade(DbConnection refDbConn)
		{
		}

		protected virtual void PerformExtraAssertsAfterUpgrade(DbConnection refDbConn)
		{
			AssertFormatNonDeterminsticDefaultConstraintNames(refDbConn);
		}

		protected void AssertTableRowCount(DbConnection refDbConn, string tableName, int expectedCount)
		{
			AssertEquals(tableName + " row count", expectedCount, (int)refDbConn.ExecuteScalar("SELECT count(*) FROM " + tableName));
		}

		protected void AssertTableRowValue(DbConnection refDbConn, string tableName, string columnName, string whereClauses, object expectedValue)
		{
			AssertEquals(tableName + "." + columnName, expectedValue, refDbConn.ExecuteScalar("SELECT TOP 1 " + columnName + " FROM " + tableName + " WHERE " + whereClauses));
		}

		protected void AssertTableRowValues(DbConnection refDbConn, string tableName, string columnName, string whereClauses, object expectedValue, string prefixData = "")
		{
			AssertEquals(tableName + "." + columnName, expectedValue, refDbConn.ExecuteScalar(prefixData + "\r\nSELECT " + columnName + " FROM " + tableName + " WHERE " + whereClauses));
		}

		protected void AssertPKsAreNotNull(DbConnection refDbConn, string tableName, string pK)
		{
			AssertEquals(pK + " in " + tableName + " should not have any null values", 0, (int)refDbConn.ExecuteScalar("SELECT count(*) FROM " + tableName + " WHERE " + pK + " IS NULL"));
		}

		protected void AssertFormatNonDeterminsticDefaultConstraintNames(DbConnection refDbConn)
		{
			string sqlText = $@"
SELECT  dc.name as ConstraintName, t.name as TableName, c.name as ColumnName
FROM
	sys.default_constraints dc
	INNER JOIN sys.tables t ON t.object_id = dc.parent_object_id
	INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
	INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE
	t.is_ms_shipped = 0
	AND dc.name <> 'DF_' + t.name + '_' + c.name
ORDER BY
	t.name, c.name
";
			var messageBuilder = new StringBuilder();
			using (var reader = refDbConn.Command(sqlText).ExecuteReader())
			{
				var builder = new StringBuilder();
				while (reader.Read())
				{
					var tableName = reader["TableName"].ToString();
					var columnName = reader["ConstraintName"].ToString();
					builder.AppendLine(tableName + " : " + columnName);
				}
				if (builder.Length != 0)
				{
					messageBuilder.AppendLine("The following constraint(s) should be in standard format(DF_[TableName]_[ColumnName]):\r\n\r\n");
					messageBuilder.AppendLine(builder.ToString());
				}
			}
			Assert(messageBuilder.ToString(), messageBuilder.Length == 0);
		}

		protected abstract string[] ExpectedReferenceTables { get; }
	}

	class CSVFileHitCounter
	{
		internal Dictionary<string, List<string>> Dict => dict ?? (dict = new Dictionary<string, List<string>>());
		Dictionary<string, List<string>> dict;

		internal void CountHit(string fileName)
		{
			var stack = System.Environment.StackTrace;
			var partToChop = stack.IndexOf("ReferenceDbUpgrader.UpgradeDatabase(", StringComparison.OrdinalIgnoreCase);
			var stackMinusBoringPart = partToChop > 0 ? stack.Substring(0, partToChop) : stack;
			if (Dict.ContainsKey(fileName))
			{
				var currentValues = Dict[fileName];
				currentValues.Add(stackMinusBoringPart);
				Dict[fileName] = currentValues;
			}
			else
			{
				Dict.Add(fileName, new List<string>() { stackMinusBoringPart });
			}
		}
	}

	class UpgradeTaskWorkflowLoggerTestClass : IUpgradeTaskWorkflowLogger
	{
		public void ActivateSubtaskProgress(int numOfSubtasks)
		{
			NumOfSubtasksForTesting = NumOfSubtasksForTesting ?? new List<int>();
			NumOfSubtasksForTesting.Add(numOfSubtasks);
		}
		public List<int> NumOfSubtasksForTesting;

		public void ActivateTaskProgress(int numOfTasks)
		{
			NumOfTasksForTesting = NumOfTasksForTesting ?? new List<int>();
			NumOfTasksForTesting.Add(numOfTasks);
		}
		public List<int> NumOfTasksForTesting;

		public void ShowInfoMessage(string infoMessage)
		{
			InfoMessageForTesting = InfoMessageForTesting ?? new List<string>();
			InfoMessageForTesting.Add(infoMessage);
		}
		public List<string> InfoMessageForTesting;

		public void ShowTaskError(string errorMessage)
		{
			ErrorMessageForTesting = ErrorMessageForTesting ?? new List<string>();
			ErrorMessageForTesting.Add(errorMessage);
		}
		public List<string> ErrorMessageForTesting;

		public void StartSubtask(string subtask)
		{
			SubtaskForTesting = SubtaskForTesting ?? new List<string>();
			SubtaskForTesting.Add(subtask);
		}
		public List<string> SubtaskForTesting;

		public void StartTask(string task)
		{
			TaskForTesting = TaskForTesting ?? new List<string>();
			TaskForTesting.Add(task);
		}
		public List<string> TaskForTesting;
	}

	class ReferenceDbUpgraderForTestGetColumnNames : ReferenceDbUpgrader
	{
		public ReferenceDbUpgraderForTestGetColumnNames(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public override string ReferenceName => throw new NotImplementedException();

		public override RefDbTypeEnum DatabaseType => throw new NotImplementedException();

		public override string CountryCode => throw new NotImplementedException();

		public override int LatestVersion => throw new NotImplementedException();

		protected override void DoDataUpgrade(DbConnection conn, int versionBeforeUpgrade)
		{
			throw new NotImplementedException();
		}

		public static List<string> GetColumnNames_Exposed(string currentRefDbName, string exclusiveRefDbName, DbConnection conn, string tableName)
		{
			return GetColumnNames(currentRefDbName, exclusiveRefDbName, conn, tableName);
		}
	}
}
