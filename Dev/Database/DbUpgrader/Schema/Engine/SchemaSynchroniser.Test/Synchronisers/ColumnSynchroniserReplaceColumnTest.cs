using System.Data;
using System.Linq;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserReplaceColumnTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[] {
				"CREATE SCHEMA hrm",
				@"CREATE TABLE dbo.DboTableRenameColumnTest (
					PK uniqueidentifier NOT NULL, 
					Text varchar(3) NOT NULL DEFAULT '',
					_R_Text varchar(3) NOT NULL CONSTRAINT [DF_R_Text] DEFAULT '',
					_R_NoTarget varchar(3) NOT NULL DEFAULT '',
					OtherText varchar(3) NOT NULL DEFAULT '',
					WD_R_OtherText varchar(3) NOT NULL DEFAULT '',
					Age int NULL,
					_R_Age int NOT NULL CONSTRAINT [DF_R_Age] DEFAULT 0,
					CONSTRAINT PK_DboTableRenameColumnTest PRIMARY KEY NONCLUSTERED (PK)
				);

				CREATE TABLE hrm.HrmTableRenameColumnTest (
					PK uniqueidentifier NOT NULL, 
					Text varchar(3) NOT NULL DEFAULT '',
					_R_Text varchar(3) NOT NULL CONSTRAINT [DF_hrm_R_Text] DEFAULT '',
					CONSTRAINT PK_HrmTableRenameColumnTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			});
		}

		public void TestConversion()
		{
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "Text", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "_R_Text", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "_R_NoTarget", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "OtherText", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "WD_R_OtherText", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "Age", "INT", "YES", null);
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "_R_Age", "INT", "NO", null);
			// other schema hrm
			AssertColumnExistInMockMainDb("HrmTableRenameColumnTest", "Text", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("HrmTableRenameColumnTest", "_R_Text", "VARCHAR", "NO", "3");

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "Text", "VARCHAR", "NO", "3");
			AssertColumnDoesNotExistInMockMainDb("DboTableRenameColumnTest", "_R_Text");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "_R_NoTarget", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "OtherText", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "WD_R_OtherText", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("DboTableRenameColumnTest", "Age", "INT", "No", null);
			AssertColumnDoesNotExistInMockMainDb("DboTableRenameColumnTest", "_R_Age");
			// other schema hrm
			AssertColumnExistInMockMainDb("HrmTableRenameColumnTest", "Text", "VARCHAR", "NO", "3");
			AssertColumnDoesNotExistInMockMainDb("HrmTableRenameColumnTest", "_R_Text");
		}

		public void TestGetColumnsToReplaceWithTargetColumnsTransform()
		{
			var columns = new ColumnChangeRetriever(TestConnection, mockMainDb, mockMainDb).GetColumnsToReplaceWithTargetColumnsTransform();
			var columnChangeMetadataRows = columns.Rows.Cast<DataRow>()
				.Select(row => new ColumnChangeMetadata(row)).ToArray();
			AssertEquals(3, columnChangeMetadataRows.Length);

			var dboTextCol = columnChangeMetadataRows.Single(c => c.TableSchema == "dbo" && c.ColumnName == "Text");
			AssertEquals("dbo", dboTextCol.TableSchema);
			AssertEquals("DboTableRenameColumnTest", dboTextCol.TableName);
			AssertEquals("Text", dboTextCol.ColumnName);
			AssertEquals("DF_R_Text", dboTextCol.OldDefaultName);

			var dboTextAge = columnChangeMetadataRows.Single(c => c.TableSchema == "dbo" && c.ColumnName == "Age");
			AssertEquals("dbo", dboTextAge.TableSchema);
			AssertEquals("DboTableRenameColumnTest", dboTextAge.TableName);
			AssertEquals("Age", dboTextAge.ColumnName);
			AssertEquals("DF_R_Age", dboTextAge.OldDefaultName);

			var hrmTextCol = columnChangeMetadataRows.Single(c => c.TableSchema == "hrm" && c.ColumnName == "Text");
			AssertEquals("hrm", hrmTextCol.TableSchema);
			AssertEquals("HrmTableRenameColumnTest", hrmTextCol.TableName);
			AssertEquals("Text", hrmTextCol.ColumnName);
			AssertEquals("DF_hrm_R_Text", hrmTextCol.OldDefaultName);
		}
	}
}
