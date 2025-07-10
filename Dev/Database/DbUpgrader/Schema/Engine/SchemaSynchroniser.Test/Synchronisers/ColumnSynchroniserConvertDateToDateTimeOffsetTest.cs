using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserConvertDateToDateTimeOffsetTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[] {
				"CREATE SCHEMA hrm",
				@"CREATE TABLE dbo.DboTableDateToTimeOffsetTest (
					PK uniqueidentifier NOT NULL, 
					EffectiveDate date NOT NULL DEFAULT GETDATE(),
					CONSTRAINT PK_DboTableDateToTimeOffsetTest PRIMARY KEY NONCLUSTERED (PK)
				);

				CREATE TABLE hrm.HrmTableDateToTimeOffsetTest (
					PK uniqueidentifier NOT NULL, 
					EffectiveDate date NOT NULL DEFAULT GETDATE(),
					CONSTRAINT PK_HrmTableDateToTimeOffsetTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			});
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[] {
				"CREATE SCHEMA hrm;",
				@"CREATE TABLE dbo.DboTableDateToTimeOffsetTest (
					PK uniqueidentifier NOT NULL, 
					EffectiveDate datetimeoffset(0) NOT NULL DEFAULT GETDATE(),
					CONSTRAINT PK_DboTableDateToTimeOffsetTest PRIMARY KEY NONCLUSTERED (PK)
				);

				CREATE TABLE hrm.HrmTableDateToTimeOffsetTest (
					PK uniqueidentifier NOT NULL, 
					EffectiveDate datetimeoffset(0) NOT NULL DEFAULT GETDATE(),
					CONSTRAINT PK_HrmTableDateToTimeOffsetTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			});
		}

		public void TestConversion()
		{
			AssertColumnExistInMockMainDb("DboTableDateToTimeOffsetTest", "EffectiveDate", "DATE", "NO", null);
			AssertColumnExistInMockMainDb("HrmTableDateToTimeOffsetTest", "EffectiveDate", "DATE", "NO", null);

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			AssertColumnExistInMockMainDb("DboTableDateToTimeOffsetTest", "EffectiveDate", "DATETIMEOFFSET", "NO", null);
			AssertColumnExistInMockMainDb("HrmTableDateToTimeOffsetTest", "EffectiveDate", "DATETIMEOFFSET", "NO", null);
		}
	}
}
