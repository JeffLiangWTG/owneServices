using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;
using static CargoWise.Database.ExtendedProperties.ExtProperty;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Test
{
	[UseSnapshotProtection]
	internal class TestDoPopulate : TestCase
	{
		readonly UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();

		public void TestPopulateTargetColumns_LastExecuteRowsLessThanBatchSize()
		{
			TestPopulateTargetColumnsCore(7, 6, false);
			TestPopulateTargetColumnsCore(5, 6, false);
		}

		public void TestPopulateTargetColumns_LastExecuteRowsEqualToBatchSize()
		{
			TestPopulateTargetColumnsCore(6, 6, false);
			TestPopulateTargetColumnsCore(3, 6, false);
		}

		public void TestPopulateTargetColumns_BreakAfterLastExecute()
		{
			const int tableRows = 6;

			TargetTableCreateValues(tableRows);

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, TemplateDb)
				{
					TopRowCount_ForTest = tableRows - 2,
					TimeConsumingThresholdForTest = TimeSpan.Zero,
					PopulateTargetColumnsBreak = true
				};
				var populatedTableList = synchroniser.GetCharToCharColumnsToPopulate();
				var testTable = populatedTableList.First();

				synchroniser.DoPopulateForTest(populatedTableList, 1, testTable.TargetTableSize, string.Empty, string.Empty, TablePreSynchroniser.CharToCharStatusNameForTest, TablePreSynchroniser.CharToCharWatermarkNameForTest);

				AssertEquals("1", GetActualWatermarkValue(testTable, TablePreSynchroniser.CharToCharWatermarkNameForTest));
				AssertEquals("Populating", GetActualColumnStatusValue(testTable, TablePreSynchroniser.CharToCharStatusNameForTest));
				AssertSequencesEqual(new [] { GetExpectedRecordsRemainingMessage(tableRows), GetExpectedRecordsRemainingMessage(tableRows) }, GetActualRecordsRemainingMessages(manager.OutputTextCollection.Cast<string>()));
			}
		}

		public void TestPopulateTargetColumns_TableIsEmpty()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, TemplateDb) { TimeConsumingThresholdForTest = TimeSpan.Zero };
				var populatedTableList = synchroniser.GetCharToCharColumnsToPopulate();
				var testTable = populatedTableList.First();
				synchroniser.DoPopulateForTest(populatedTableList, 1, testTable.TargetTableSize, string.Empty, string.Empty, ColumnStatusName, WatermarkName);

				AssertNull(GetActualWatermarkValue(testTable, WatermarkName));
				AssertNull(GetActualColumnStatusValue(testTable, ColumnStatusName));
				AssertSequencesEqual(new[] { GetExpectedRecordsRemainingMessage(0) }, GetActualRecordsRemainingMessages(manager.OutputTextCollection.Cast<string>()));
			}
		}

		public void TestPopulateTargetColumns_RowsCheckedButNotUpdated()
		{
			TestPopulateTargetColumnsCore(5, 13, true);
			TestPopulateTargetColumnsCore(5, 5, true);
			TestPopulateTargetColumnsCore(13, 5, true);
		}

		void TestPopulateTargetColumnsCore(int topRowCount, int tableRows, bool userAction)
		{
			CreateTestTable(TargetTable, targetPrerequisiteColumnList);
			TargetTableCreateValues(tableRows);
			manager.OutputTextCollection.Clear();

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, TemplateDb)
				{
					TopRowCount_ForTest = topRowCount,
					TimeConsumingThresholdForTest = TimeSpan.Zero
				};
				var populatedTableList = synchroniser.GetCharToCharColumnsToPopulate();
				var testTable = populatedTableList.First();

				if (userAction)
				{
					synchroniser.UserAction_ForTest.Value = (tableFullName) =>
					{
						Db.Connection.ExecuteNonQuery($"UPDATE TOP(3) {tableFullName} SET _3_TST_Col = 'Dummy';");
					};
				}

				synchroniser.DoPopulateForTest(populatedTableList, 1, testTable.TargetTableSize, string.Empty, string.Empty, ColumnStatusName, WatermarkName);

				AssertNull(GetActualWatermarkValue(testTable, WatermarkName));
				AssertNull(GetActualColumnStatusValue(testTable, ColumnStatusName));
				var expectedRecordsRemainingMessages = new List<string>();
				var totalRecordCount = tableRows;
				expectedRecordsRemainingMessages.Add(GetExpectedRecordsRemainingMessage(totalRecordCount));
				while (totalRecordCount > 0)
				{
					expectedRecordsRemainingMessages.Add(GetExpectedRecordsRemainingMessage(totalRecordCount));
					totalRecordCount -= topRowCount;
				}
				AssertSequencesEqual(expectedRecordsRemainingMessages, GetActualRecordsRemainingMessages(manager.OutputTextCollection.Cast<string>()));
			}
		}

		static string GetExpectedRecordsRemainingMessage(int recordsRemaining) => string.Format(CultureInfo.InvariantCulture, "\t    {0,11:N0} records remaining", recordsRemaining);

		static string[] GetActualRecordsRemainingMessages(IEnumerable<string> actualMessages) => actualMessages.Where(m => m.EndsWith("records remaining")).ToArray();

		static string GetActualWatermarkValue(PopulatedTable table, string watermarkName) => Table.Select(Db.Connection, table.TargetTableSchema, table.TargetTableName, watermarkName);

		static string GetActualColumnStatusValue(PopulatedTable table, string columnStatusName) => Column.Select(Db.Connection, table.TargetTableSchema, table.TargetTableName, table.ColumnList.First().ColumnName, columnStatusName);

		IAuxiliaryDbCreator templateDbCreator;

		static void TargetTableCreateValues(int rows)
		{
			Db.Connection.ExecuteNonQuery(string.Concat(Enumerable.Repeat($"INSERT dbo.[{TargetTable}] ({TargetColumn}) VALUES ('Dummy');", rows)));
		}

		static void CreateTestTable(string targetTable, IEnumerable<string> columnList)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"-- CreateTestTable
if (OBJECT_ID(N'[dbo].[{0}]', N'U') is NOT NULL) DROP TABLE [dbo].[{0}];
CREATE TABLE [dbo].[{0}]
(
	{1}
);",
				targetTable,
				string.Join(",\r\n\t", columnList)
			);

			Db.Connection.ExecuteNonQuery(sql);
		}

		readonly IEnumerable<string> targetPrerequisiteColumnList = new[]
		{
			TestPkColumn,
			$"{TargetColumn} char(10)  NOT NULL",
		};

		readonly IEnumerable<string> templatePrerequisiteColumnList = new[]
		{
			TestPkColumn,
			$"{TargetColumn} nchar(10)     NULL",
		};

		const string TestPkColumn = "TST_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED";

		const string TemplateDb = "_testDoPopulate_TemplateDb";
		const string TargetTable = "_testDoPopulate_TargetTable";

		const string WatermarkName = "_test_watermark";
		const string ColumnStatusName = "_test_column_status";

		const string TargetColumn = "TST_Col";

		protected override void SetUp()
		{
			base.SetUp();

			templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(TemplateDb, TargetTable, templatePrerequisiteColumnList);
			templateDbCreator.CreateDropExisting();

			TablePreSynchroniser.CreatePreAddDb_ForTest();

			CreateTestTable(TargetTable, targetPrerequisiteColumnList);
		}

		protected override void TearDown()
		{
			TablePreSynchroniser.DropPreAddDb_ForTest();

			templateDbCreator.Drop();

			base.TearDown();
		}
	}
}
