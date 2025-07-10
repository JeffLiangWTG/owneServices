using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserConvertCharToCharTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[] {
				"CREATE SCHEMA hrm",
				@"CREATE TABLE dbo.DboTableCharToCharTest (
					PK uniqueidentifier NOT NULL, 
					Text varchar(3) NOT NULL DEFAULT '',
					CONSTRAINT PK_DboTableCharToCharTest PRIMARY KEY NONCLUSTERED (PK)
				);

				CREATE TABLE hrm.HrmTableCharToCharTest (
					PK uniqueidentifier NOT NULL, 
					Text varchar(3) NOT NULL DEFAULT '',
					CONSTRAINT PK_HrmTableCharToCharTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			});
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[] {
				"CREATE SCHEMA hrm;",
				@"CREATE TABLE dbo.DboTableCharToCharTest (
					PK uniqueidentifier NOT NULL, 
					Text char(3) NOT NULL DEFAULT '',
					CONSTRAINT PK_DboTableCharToCharTest PRIMARY KEY NONCLUSTERED (PK)
				);

				CREATE TABLE hrm.HrmTableCharToCharTest (
					PK uniqueidentifier NOT NULL, 
					Text char(3) NOT NULL DEFAULT '',
					CONSTRAINT PK_HrmTableCharToCharTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			});
		}

		public void TestConversion()
		{
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "Text", "VARCHAR", "NO", "3");
			AssertColumnExistInMockMainDb("HrmTableCharToCharTest", "Text", "VARCHAR", "NO", "3");

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "Text", "CHAR", "NO", "3");
			AssertColumnExistInMockMainDb("HrmTableCharToCharTest", "Text", "CHAR", "NO", "3");
		}
	}
}
