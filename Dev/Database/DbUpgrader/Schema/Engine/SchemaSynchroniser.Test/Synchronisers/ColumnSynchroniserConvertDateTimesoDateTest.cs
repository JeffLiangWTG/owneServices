using System.Data;
using System.Linq;
using Enterprise.DbUpgrader.Schema.Testing;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Synchroniser.Test.Synchronisers
{
	sealed class ColumnSynchroniserConvertDateTimesoDateTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[] {
				@"CREATE TABLE dbo.DboTableDateTimesToDateTest (
					PK uniqueidentifier NOT NULL, 
					ExpiryDate SMALLDATETIME NOT NULL DEFAULT GETDATE(),
					[_DTD_ExpiryDate] DATE NOT NULL DEFAULT GETDATE(),
					StartDate DATETIME NOT NULL DEFAULT GETDATE(),
					[_DTD_StartDate] DATE NOT NULL DEFAULT GETDATE(),
					EndDate DATETIME2 NOT NULL DEFAULT GETDATE(),
					[_DTD_EndDate] DATE NOT NULL DEFAULT GETDATE(),
					CONSTRAINT PK_DboTableDateTimesToDateTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			});
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[] {
				@"CREATE TABLE dbo.DboTableDateTimesToDateTest (
					PK uniqueidentifier NOT NULL, 
					ExpiryDate DATE NOT NULL DEFAULT GETDATE(),
					StartDate DATE NOT NULL DEFAULT GETDATE(),
					EndDate DATE NOT NULL DEFAULT GETDATE(),
					CONSTRAINT PK_DboTableDateTimesToDateTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			});
		}

		public void TestConversion()
		{
			AssertColumnExistInMockMainDb("DboTableDateTimesToDateTest", "ExpiryDate", "SMALLDATETIME", "NO", null);
			AssertColumnExistInMockMainDb("DboTableDateTimesToDateTest", "StartDate", "DATETIME", "NO", null);
			AssertColumnExistInMockMainDb("DboTableDateTimesToDateTest", "EndDate", "DATETIME2", "NO", null);

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			AssertColumnExistInMockMainDb("DboTableDateTimesToDateTest", "ExpiryDate", "DATE", "NO", null);
			AssertColumnExistInMockMainDb("DboTableDateTimesToDateTest", "StartDate", "DATE", "NO", null);
			AssertColumnExistInMockMainDb("DboTableDateTimesToDateTest", "EndDate", "DATE", "NO", null);
		}

		public void TestGetColumnsToGetColumnsToConvertDateTimesToDate()
		{
			var columns = new ColumnChangeRetriever(TestConnection, mockMainDb, mockTemplateDb).GetColumnsToConvertDateTimesToDate();
			var columnChangeMetadataRows = columns.Rows.Cast<DataRow>()
				.Select(row => new ColumnChangeMetadata(row)).ToArray();
			AssertEquals(3, columnChangeMetadataRows.Length);

			var smallDateTimeColumn = columnChangeMetadataRows.Single(c => c.ColumnName == "ExpiryDate");
			var dateTimeColumn = columnChangeMetadataRows.Single(c => c.ColumnName == "StartDate");
			var dateTime2Column = columnChangeMetadataRows.Single(c => c.ColumnName == "EndDate");
			AssertEquals("dbo", smallDateTimeColumn.TableSchema);
			AssertEquals("DboTableDateTimesToDateTest", smallDateTimeColumn.TableName);
			AssertEquals("ExpiryDate", smallDateTimeColumn.ColumnName);
			AssertEquals("StartDate", dateTimeColumn.ColumnName);
			AssertEquals("EndDate", dateTime2Column.ColumnName);
		}
	}
}
