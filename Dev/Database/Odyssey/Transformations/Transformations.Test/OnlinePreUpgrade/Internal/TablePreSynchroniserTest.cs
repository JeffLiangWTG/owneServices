using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[UseSnapshotProtection]
	sealed class TablePreSynchroniserTest : TestCase
	{
		public void TestGetSystemLastEditColumns_ColumnInclusion()
		{
			var tableName1 = "TblNoAudit";
			var cols1 = new[]
			{
				"TT1_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
				"TT1_SomeData varchar(50) NULL"
			};
			var tableName2 = "TblWithAudit";
			var cols2 = new[]
			{
				"TT2_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
				"TT2_SomeData varchar(50) NULL",
				"TT2_SystemLastEditTimeUtc smalldatetime",
				"TT2_SystemLastEditUser varchar(3) NULL"
			};

			var tableData = new (string, IEnumerable<string>, IEnumerable<string>)[] { (tableName1, cols1, cols1), (tableName2, cols2, cols2) };

			RunTestWithCleanup(tableData, () =>
			{
				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				
				var table = new PopulatedTable(Db.DatabaseName, "dbo", tableName1, TablePreSynchroniser.PreAddDb_Exposed, tableName1) { TargetTablePKColumnName = "TT1_PK" };
				table.ColumnList.Add(new PopulatedColumn { ColumnName = "TT1_SomeData", ColumnType = "varchar" });
				var columns = synchroniser.GetSystemLastEditColumns(table, new List<string> { "[TT1_SomeData] = 'Some Update'" });

				AssertEquals("Should not contain TT1_SystemLastEditTimeUtc", false, columns.Any(x => x.Contains("TT1_SystemLastEditTimeUtc")));
				AssertEquals("Should not contain TT1_SystemLastEditUser", false, columns.Any(x => x.Contains("TT1_SystemLastEditUser")));

				table = new PopulatedTable(Db.DatabaseName, "dbo", tableName2, TablePreSynchroniser.PreAddDb_Exposed, tableName2) { TargetTablePKColumnName = "TT2_PK" };
				table.ColumnList.Add(new PopulatedColumn { ColumnName = "TT2_SomeData", ColumnType = "varchar" });
				columns = synchroniser.GetSystemLastEditColumns(table, new List<string> { "[TT2_SomeData] = 'Some Update'" });

				AssertEquals("Should now contain TT2_SystemLastEditTimeUtc", true, columns.Any(x => x.Contains("TT2_SystemLastEditTimeUtc")));
				AssertEquals("Should now contain TT2_SystemLastEditUser", true, columns.Any(x => x.Contains("TT2_SystemLastEditUser")));

				table.ColumnList.Add(new PopulatedColumn { ColumnName = "TT2_SystemLastEditTimeUtc", ColumnType = "smalldatetime" });
				table.ColumnList.Add(new PopulatedColumn { ColumnName = "TT2_SystemLastEditUser", ColumnType = "varchar" });
				columns = synchroniser.GetSystemLastEditColumns(table, new List<string> { "[TT2_SystemLastEditTimeUtc] = '2021-01-01'", "[TT2_SystemLastEditUser] = 'BOB'" });

				AssertEquals("Should not contain TT2_SystemLastEditTimeUtc", false, columns.Any(x => x.Contains("TT2_SystemLastEditTimeUtc")));
				AssertEquals("Should not contain TT2_SystemLastEditUser", false, columns.Any(x => x.Contains("TT2_SystemLastEditUser")));
			});
		}

		public void TestGetSystemLastEditColumns_NoComparison()
		{
			var tableName1 = "TblNoComparisonSupport";
			var cols1 = new[]
			{
				"TT3_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
				"TT3_SomeData varchar(50) NULL",
				"TT3_SystemLastEditTimeUtc smalldatetime",
				"TT3_SystemLastEditUser varchar(3) NULL"
			};
			var tableData = new (string, IEnumerable<string>, IEnumerable<string>)[] { (tableName1, cols1, cols1) };

			RunTestWithCleanup(tableData, () =>
			{
				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				
				var table = new PopulatedTable(Db.DatabaseName, "dbo", tableName1, TablePreSynchroniser.PreAddDb_Exposed, tableName1) { TargetTablePKColumnName = "TT3_PK" };

				foreach (var nonComparableType in new[] { "image", "ntext", "text", "xml" })
				{
					table.ColumnList.Add(new PopulatedColumn { ColumnName = "TT3_SomeData", ColumnType = nonComparableType });
					var columns = synchroniser.GetSystemLastEditColumns(table, new List<string> { "[TT3_SomeData] = 'Some Update'" });

					AssertEquals($"{nonComparableType} - Should contain default for TT3_SystemLastEditTimeUtc", "[TT3_SystemLastEditTimeUtc] = GETUTCDATE()", columns.FirstOrDefault(x => x.Contains("TT3_SystemLastEditTimeUtc")));
					AssertEquals($"{nonComparableType} - Should contain default for TT3_SystemLastEditUser", "[TT3_SystemLastEditUser] = '~BP'", columns.FirstOrDefault(x => x.Contains("TT3_SystemLastEditUser")));
				}
			});
		}

		public void TestGetSystemLastEditColumns_WithComparison()
		{
			var tableName1 = "TblComparison";
			var cols1 = new[]
			{
				"TT4_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
				"TT4_SomeData1 varchar(50) NULL",
				"TT4_SomeData2 varchar(50) NULL",
				"TT4_SystemLastEditTimeUtc smalldatetime",
				"TT4_SystemLastEditUser varchar(3) NULL"
			};
			var tableData = new (string, IEnumerable<string>, IEnumerable<string>)[] { (tableName1, cols1, cols1) };

			RunTestWithCleanup(tableData, () =>
			{
				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				var table = new PopulatedTable(Db.DatabaseName, "dbo", tableName1, TablePreSynchroniser.PreAddDb_Exposed, tableName1) { TargetTablePKColumnName = "TT4_PK" };

				table.ColumnList.Add(new PopulatedColumn { ColumnName = "TT4_SomeData1", ColumnType = "varchar" });
				table.ColumnList.Add(new PopulatedColumn { ColumnName = "TT4_SomeData2", ColumnType = "varchar" });
				var columns = synchroniser.GetSystemLastEditColumns(table, new List<string>
				{
					"[TT4_SomeData1] = RTRIM(CONVERT(nvarchar(50), [TT4_SomeData1]))",
					"[TT4_SomeData2] = ISNULL(RTRIM(CONVERT(varchar(3), [TT4_SomeData2])), (''))"
				});

				AssertEquals($"CheckSum for TT4_SystemLastEditTimeUtc", "[TT4_SystemLastEditTimeUtc] = (CASE WHEN ((target.[TT4_SomeData1] = RTRIM(CONVERT(nvarchar(50), [TT4_SomeData1]))) AND (target.[TT4_SomeData2] = ISNULL(RTRIM(CONVERT(varchar(3), [TT4_SomeData2])), ('')))) THEN ISNULL([TT4_SystemLastEditTimeUtc], GETUTCDATE()) ELSE GETUTCDATE() END)", columns.FirstOrDefault(x => x.Contains("TT4_SystemLastEditTimeUtc")));
				AssertEquals($"CheckSum for TT4_SystemLastEditUser", "[TT4_SystemLastEditUser] = (CASE WHEN ((target.[TT4_SomeData1] = RTRIM(CONVERT(nvarchar(50), [TT4_SomeData1]))) AND (target.[TT4_SomeData2] = ISNULL(RTRIM(CONVERT(varchar(3), [TT4_SomeData2])), ('')))) THEN ISNULL(NULLIF([TT4_SystemLastEditUser], ''), '~BP') ELSE '~BP' END)", columns.FirstOrDefault(x => x.Contains("TT4_SystemLastEditUser")));
			});
		}

		public void TestSystemLastEditTimeUpdatedWhenRequired()
		{
			var tableName1 = "TblProcessing";
			var commonCols = new[]
			{
				"TT5_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
				"TT5_SystemLastEditTimeUtc smalldatetime",
				"TT5_SystemLastEditUser varchar(3) NULL"
			};

			var originalCols1 = new[]
			{
				"TT5_SomeData1 varchar(50) NULL",
				"TT5_SomeData2 nvarchar(50) NULL",
			};

			var templateCols1 = new[]
			{
				"TT5_SomeData1 nvarchar(50) NULL",
				"TT5_SomeData2 varchar(50) NULL",
			};

			var tableData = new (string, IEnumerable<string>, IEnumerable<string>)[] { (tableName1, commonCols.Concat(originalCols1), commonCols.Concat(templateCols1)) };

			RunTestWithCleanup(tableData, () =>
			{
				var pkNoChange = Guid.NewGuid();
				var pkWithChange = Guid.NewGuid();
				var pkWithNull = Guid.NewGuid();
				var pkWithEmptyUser = Guid.NewGuid();
				var lastEditDate = new DateTime(2025, 2, 1, 13, 14, 0);
				var lastEditUser = "E";

				Db.Connection.ExecuteNonQuery($@"--Setup TestSystemLastEditTimeUpdatedWhenRequired
				INSERT INTO {tableName1} (TT5_PK, TT5_SomeData1, TT5_SomeData2, TT5_SystemLastEditTimeUtc, TT5_SystemLastEditUser)
				VALUES
					('{pkNoChange}', 'No Change', N'Expected', '{lastEditDate:yyyy-MM-dd HH:mm:00}', '{lastEditUser}'),
					('{pkWithChange}', 'NVarchar', N'Non-ascii should be replaced: 你好', '{lastEditDate:yyyy-MM-dd HH:mm:00}', '{lastEditUser}'),
					('{pkWithNull}', 'Nulls', N'Update', NULL, NULL),
					('{pkWithEmptyUser}', 'Empty User', N'Update', NULL, '')
			");

				using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
				{
					var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
					var populatedTableList = synchroniser.GetCharToCharColumnsToPopulate();
					var testTable = populatedTableList.First();

					synchroniser.DoPopulateForTest(populatedTableList, 1, testTable.TargetTableSize, string.Empty, string.Empty, TablePreSynchroniser.CharToCharStatusNameForTest, TablePreSynchroniser.CharToCharWatermarkNameForTest);

					CombineAssertions(() =>
					{
						var rowCount = 0;
						Db.Connection.ExecuteReader($"SELECT TT5_PK, TT5_SystemLastEditTimeUtc, TT5_SystemLastEditUser FROM {tableName1}", (dataRecord) =>
						{
							rowCount++;

							AssertNotNull("SystemLastEditTime", dataRecord[1]);
							AssertNotNull("SystemLastEditUser", dataRecord[2]);

							var pk = dataRecord.GetGuid(0);
							var dt = dataRecord.GetDateTime(1);
							var user = dataRecord.GetString(2);

							if (pk == pkNoChange)
							{
								AssertEquals("No Change Expected - SystemLastEditTime", lastEditDate, dt);
								AssertEquals("No Change Expected - TT5_SystemLastEditUser", lastEditUser, user);
							}
							else if (pk == pkWithChange || pk == pkWithNull || pk == pkWithEmptyUser)
							{
								AssertNotEquals("Change Expected - SystemLastEditTime", lastEditDate, dt);
								AssertEquals("Change Expected - TT5_SystemLastEditUser", "~BP", user);
							}
							else
							{
								Assert($"Unexpected Guid: {pk}", false);
							}
						});
						AssertEquals("4 Records should have been read", 4, rowCount);
					});
				}
			});
		}

		public void TestConvertCharFlagsToBitResumesAfterCrash()
		{
			const string targetTable = "_TargetTable";
			var tableData = new[] { (targetTable, pk.Union(targetPrerequisiteColumnList), pk.Union(templatePrerequisiteColumnList).Union(newColumnList)) };
			RunTestWithCleanup(tableData, () =>
			{
				Db.Connection.ExecuteNonQuery(String.Format(CultureInfo.InvariantCulture, @"
				INSERT {0} (TST_SomeColumn, TST_Flag_1) VALUES
				-- (TST_SomeColumn, TST_Flag_1)
					('1'           , 'Y'       ),
					('2'           , 'Y'       ),
					('3'           , 'N'       ),
					('4'           , 'N'       ),
					('5'           , 'N'       )
					;
				", targetTable));

				using ((Db.Instance as IDbUpgradeSupport).ElevateToAdminConnectionForUpgrade())
				{
					AssertEquals("PRECONDITION", "TST_Flag_1, TST_Flag_2, TST_Flag_3", GetFlagNames(targetTable, isBit: false));
					AssertEquals("PRECONDITION", String.Empty, GetFlagNames(targetTable, isBit: true));
					var tableSize = 5;

					var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
					synchroniser.TopRowCount_ForTest = 1;
					synchroniser.PopulateTargetColumnsBreak = true;
					synchroniser.ConvertCharFlagsToBit();

					int updatedCount = (int)Db.Connection.ExecuteScalar(String.Format("SELECT COUNT(*) FROM {0} WHERE {1}TST_Flag_1 = 1", targetTable, ColumnSynchroniser.ConvertCharToBitColumnPrefix));
					Assert("Some rows should be populated with the default value", updatedCount > 0);
					Assert("Not all rows should be populated with the default value", updatedCount < tableSize);

					synchroniser.PopulateTargetColumnsBreak = false;
					synchroniser.ConvertCharFlagsToBit();
					var expected = String.Format("{0}TST_Flag_1, {0}TST_Flag_2, {0}TST_Flag_3", ColumnSynchroniser.ConvertCharToBitColumnPrefix);
					AssertEquals("Bit flags have been created", expected, GetFlagNames(targetTable, isBit: true));

					var expectedFalseCount = 3;
					var expectedTrueCount = tableSize - expectedFalseCount;
					AssertEquals("Char flags have been converted to bit", expectedFalseCount, (int)Db.Connection.ExecuteScalar(String.Format("SELECT COUNT(*) FROM {0} WHERE {1}TST_Flag_1 = 0", targetTable, ColumnSynchroniser.ConvertCharToBitColumnPrefix)));
					AssertEquals("Char flags have been converted to bit", expectedTrueCount, (int)Db.Connection.ExecuteScalar(String.Format("SELECT COUNT(*) FROM {0} WHERE {1}TST_Flag_1 = 1", targetTable, ColumnSynchroniser.ConvertCharToBitColumnPrefix)));
					AssertEquals("Char flags have been converted to bit", tableSize, (int)Db.Connection.ExecuteScalar(String.Format("SELECT COUNT(*) FROM {0} WHERE {1}TST_Flag_2 = 1", targetTable, ColumnSynchroniser.ConvertCharToBitColumnPrefix)));
					AssertEquals("Char flags have been converted to bit", tableSize, (int)Db.Connection.ExecuteScalar(String.Format("SELECT COUNT(*) FROM {0} WHERE {1}TST_Flag_3 = 1", targetTable, ColumnSynchroniser.ConvertCharToBitColumnPrefix)));

					AssertEquals("Column TST_ToBeOrNotToBe exists?", false, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM sys.columns WHERE name = 'TST_ToBeOrNotToBe'") > 0);
				}
			});
		}

		string GetFlagNames(string targetTable, bool isBit)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
DECLARE @ColumnNames varchar(1000);
SELECT
	@ColumnNames = ISNULL(@ColumnNames + ', ', '') + c.name
FROM
	sys.columns                       AS c
	JOIN sys.types                    AS t ON t.user_type_id = c.user_type_id
	LEFT JOIN sys.default_constraints AS d ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
WHERE
	c.object_id = OBJECT_ID(N'{0}', N'U')
	AND
	(
		@isBit = 0 AND t.name in ('char', 'varchar') AND c.max_length = 1 AND d.definition in ('(''Y'')', '(''N'')', '('''')')
		OR @isBit = 1 AND t.name = 'bit'
	)
ORDER BY
	c.name;

SELECT ColumnNames = ISNULL(@ColumnNames, '');
",
				targetTable
				);

			string columnNames = String.Empty;
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@isBit", SqlDbType.Bit, isBit);
				columnNames = (string)cmd.ExecuteScalar();
			}

			return columnNames;
		}

		readonly UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();

		readonly IEnumerable<string> pk = new string[] { "TST_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED" };
		readonly IEnumerable<string> templatePrerequisiteColumnList = new string[]
			{
				"TST_SomeColumn varchar(300) NOT NULL",
				"TST_Flag_1     bit          NOT NULL CONSTRAINT DF_TST_Flag_1 DEFAULT (0)",
				"TST_Flag_2     bit          NOT NULL CONSTRAINT DF_TST_Flag_2 DEFAULT (0)",
				"TST_Flag_3     bit              NULL",
			};
		readonly IEnumerable<string> targetPrerequisiteColumnList = new string[]
			{
				"TST_SomeColumn varchar(300) NOT NULL",
				"TST_Flag_1     varchar(1)   NOT NULL CONSTRAINT DF_TST_Flag_1 DEFAULT ('Y')",
				"TST_Flag_2     char(1)      NOT NULL CONSTRAINT DF_TST_Flag_2 DEFAULT ('Y')",
				"TST_Flag_3     char(1)      NOT NULL CONSTRAINT DF_TST_Flag_3 DEFAULT ('Y')",
			};
		readonly IEnumerable<string> newColumnList = new string[]
			{
				"TST_ToBeOrNotToBe char(1) NOT NULL DEFAULT ('N')"
			};

		const string templateDb = "TestPreSynchroniser_TemplateDb";

		void RunTestWithCleanup((string targetTable, IEnumerable<string> targetColumnList, IEnumerable<string> templateColumnList)[] tables, Action runAssertions)
		{
			IAuxiliaryDbCreator templateDbCreator = null;

			try
			{
				TablePreSynchroniser.CreatePreAddDb_ForTest();
				templateDbCreator = PrepareTables(tables);

				runAssertions();
			}
			finally
			{
				CleanupTables(tables.Select(x => x.targetTable).ToArray());
				templateDbCreator.Drop();
				TablePreSynchroniser.DropPreAddDb_ForTest();
			}
		}

		IAuxiliaryDbCreator PrepareTables((string targetTable, IEnumerable<string> targetColumnList, IEnumerable<string> templateColumnList)[] tables)
		{
			var templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(templateDb, tables.Select(x => (x.targetTable, x.templateColumnList)).ToArray());// targetTable, pk.Union(templatePrerequisiteColumnList).Union(newColumnList));
			templateDbCreator.CreateDropExisting();

			foreach (var tbl in tables)
			{
				DropTestTable(tbl.targetTable);
				CreateTestTable(tbl.targetTable, tbl.targetColumnList);
			}

			return templateDbCreator;
		}
		void CleanupTables(string[] targetTables)
		{
			foreach (var targetTable in targetTables)
			{
				DropTestTable(targetTable);
			}
		}

		void DropTestTable(string targetTable)
		{
			Db.Connection.ExecuteNonQuery($"if (OBJECT_ID(N'[dbo].[{targetTable}]', N'U') is NOT NULL) DROP TABLE [dbo].[{targetTable}];");
		}

		void CreateTestTable(string targetTable, IEnumerable<string> targetColumnList)
		{
			var sql = $@"CREATE TABLE [dbo].[{targetTable}]
			(
				{(string.Join(",\r\n\t", targetColumnList))}
			);";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
