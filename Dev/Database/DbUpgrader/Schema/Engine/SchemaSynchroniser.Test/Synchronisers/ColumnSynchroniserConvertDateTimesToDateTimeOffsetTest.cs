using System.Data;
using System.Linq;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserConvertDateTimesToDateTimeOffsetTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[] {
				@"CREATE TABLE dbo.TableConvertDateTimesToDateTimeOffset (
					PK uniqueidentifier NOT NULL, 
					TestSmallDateTime smalldatetime NOT NULL,
					[_DTO_TestSmallDateTime] datetimeoffset NOT NULL CONSTRAINT DefaultSmallDateTime DEFAULT ('1994-01-01'),
					TestDateTime datetime NOT NULL,
					[_DTO_TestDateTime] datetimeoffset NOT NULL,
					TestDateTime2 datetime2 NOT NULL,
					[_DTO_TestDateTime2] datetimeoffset NOT NULL,
					CONSTRAINT PK_TableConvertDateTimesToDateTimeOffset PRIMARY KEY NONCLUSTERED (PK));"
			});
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[] {
				@"CREATE TABLE dbo.TableConvertDateTimesToDateTimeOffset (
					PK uniqueidentifier NOT NULL,
					TestSmallDateTime datetimeoffset NOT NULL,
					TestDateTime datetimeoffset NOT NULL,
					TestDateTime2 datetimeoffset NOT NULL,
					CONSTRAINT PK_TableConvertDateTimesToDateTimeOffset PRIMARY KEY NONCLUSTERED (PK));"
			});
		}

		public void TestConversion()
		{
			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "TestSmallDateTime", "smalldatetime", "NO", null);
			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "TestDateTime", "datetime", "NO", null);
			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "TestDateTime2", "datetime2", "NO", null);
			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "_DTO_TestSmallDateTime", "datetimeoffset", "NO", null);
			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "_DTO_TestDateTime", "datetimeoffset", "NO", null);
			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "_DTO_TestDateTime2", "datetimeoffset", "NO", null);

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "TestSmallDateTime", "datetimeoffset", "NO", null);
			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "TestDateTime", "datetimeoffset", "NO", null);
			AssertColumnExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "TestDateTime2", "datetimeoffset", "NO", null);
			AssertColumnDoesNotExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "_DTO_TestSmallDateTime");
			AssertColumnDoesNotExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "_DTO_TestDateTime");
			AssertColumnDoesNotExistInMockMainDb("TableConvertDateTimesToDateTimeOffset", "_DTO_TestDateTime2");
		}

		public void TestGetColumnsToCompleteDateTimesToDateTimeOffsetTransform()
		{
			var columns = new ColumnChangeRetriever(TestConnection, mockMainDb, mockTemplateDb).GetColumnsToCompleteDateTimesToDateTimeOffsetTransform();
			var columnChangeMetadataRows = columns.Rows.Cast<DataRow>()
				.Select(row => new ColumnChangeMetadata(row)).ToArray();
			AssertEquals(3, columnChangeMetadataRows.Length);

			var smallDateTimeColumn = columnChangeMetadataRows.Single(c => c.ColumnName == "TestSmallDateTime");
			AssertEquals("dbo", smallDateTimeColumn.TableSchema);
			AssertEquals("TableConvertDateTimesToDateTimeOffset", smallDateTimeColumn.TableName);
			AssertEquals("TestSmallDateTime", smallDateTimeColumn.ColumnName);
			AssertEquals("DefaultSmallDateTime", smallDateTimeColumn.OldDefaultName);

			var dateTimeColumn = columnChangeMetadataRows.Single(c => c.ColumnName == "TestDateTime");
			AssertEquals("dbo", dateTimeColumn.TableSchema);
			AssertEquals("TableConvertDateTimesToDateTimeOffset", dateTimeColumn.TableName);
			AssertEquals("TestDateTime", dateTimeColumn.ColumnName);
			AssertEquals("", dateTimeColumn.OldDefaultName);

			var dateTime2Column = columnChangeMetadataRows.Single(c => c.ColumnName == "TestDateTime2");
			AssertEquals("dbo", dateTime2Column.TableSchema);
			AssertEquals("TableConvertDateTimesToDateTimeOffset", dateTime2Column.TableName);
			AssertEquals("TestDateTime2", dateTime2Column.ColumnName);
			AssertEquals("", dateTime2Column.OldDefaultName);
		}

		public void TestGetColumnsToCompleteDateTimesToDateTimeOffsetTransform_WhenDBHasNoTempColumn()
		{
			TestConnection.ExecuteNonQuery($"ALTER TABLE [{mockMainDb}].dbo.TableConvertDateTimesToDateTimeOffset DROP Constraint DefaultSmallDateTime");
			TestConnection.ExecuteNonQuery($"ALTER TABLE [{mockMainDb}].dbo.TableConvertDateTimesToDateTimeOffset DROP COLUMN [_DTO_TestSmallDateTime]");
			TestConnection.ExecuteNonQuery($"ALTER TABLE [{mockMainDb}].dbo.TableConvertDateTimesToDateTimeOffset DROP COLUMN [_DTO_TestDateTime]");
			TestConnection.ExecuteNonQuery($"ALTER TABLE [{mockMainDb}].dbo.TableConvertDateTimesToDateTimeOffset DROP COLUMN [_DTO_TestDateTime2]");

			var columns = new ColumnChangeRetriever(TestConnection, mockMainDb, mockTemplateDb).GetColumnsToCompleteDateTimesToDateTimeOffsetTransform();
			AssertEquals(0, columns.Rows.Count);
		}
	}
}
