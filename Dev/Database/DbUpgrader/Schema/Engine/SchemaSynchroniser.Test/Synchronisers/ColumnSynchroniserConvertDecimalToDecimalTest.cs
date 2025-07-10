using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing;

sealed class ColumnSynchroniserConvertDecimalToDecimalTest : TestCaseWithMockMainDbAndTemplateDbTransactional
{
	protected override IAuxiliaryDbCreator GetMockMainDbCreator()
	{
		return new AuxiliaryDbCreatorForTesting(mockMainDb,
				"CREATE SCHEMA hrm;",
				@"CREATE TABLE dbo.DboTableDecimalToDecimalTest (
					PK uniqueidentifier NOT NULL,
					Number1 decimal(2,1) NOT NULL DEFAULT 0.0,
					Number2 decimal(2,1) NOT NULL DEFAULT 0.0,
					Number3 decimal(2,1) NOT NULL DEFAULT 0.0,
					Number4 decimal(2,1) NOT NULL DEFAULT 0.0,
					CONSTRAINT PK_DboTableDecimalToDecimalTest PRIMARY KEY NONCLUSTERED (PK)
				);

				CREATE TABLE hrm.HrmTableDecimalToDecimalTest (
					PK uniqueidentifier NOT NULL,
					Number1 decimal(2,1) NOT NULL DEFAULT 0.0,
					Number2 decimal(2,1) NOT NULL DEFAULT 0.0,
					Number3 decimal(2,1) NOT NULL DEFAULT 0.0,
					Number4 decimal(2,1) NOT NULL DEFAULT 0.0,
					CONSTRAINT PK_HrmTableDecimalToDecimalTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			);
	}

	protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
	{
		return new AuxiliaryDbCreatorForTesting(mockTemplateDb,
				"CREATE SCHEMA hrm;",
				@"CREATE TABLE dbo.DboTableDecimalToDecimalTest (
					PK uniqueidentifier NOT NULL, 
					Number1 decimal(3, 2) NOT NULL DEFAULT 0.0, -- Fine
					Number2 decimal(2, 0) NOT NULL DEFAULT 0.0, -- Loses scale
					Number3 decimal(2, 2) NOT NULL DEFAULT 0.0, -- Loses leading digit
					Number4 decimal(1, 0) NOT NULL DEFAULT 0.0, -- Loses precision
					CONSTRAINT PK_DboTableDecimalToDecimalTest PRIMARY KEY NONCLUSTERED (PK)
				);

				CREATE TABLE hrm.HrmTableDecimalToDecimalTest (
					PK uniqueidentifier NOT NULL, 
					Number1 decimal(3, 2) NOT NULL DEFAULT 0.0,
					Number2 decimal(2, 0) NOT NULL DEFAULT 0.0,
					Number3 decimal(2, 2) NOT NULL DEFAULT 0.0,
					Number4 decimal(1, 0) NOT NULL DEFAULT 0.0,
					CONSTRAINT PK_HrmTableDecimalToDecimalTest PRIMARY KEY NONCLUSTERED (PK)
				);"
			);
	}

	public void TestConversion()
	{
		for (var i = 1; i <= 4; i++)
		{
			AssertColumnExistInMockMainDb("DboTableDecimalToDecimalTest", "Number" + i, "decimal", "NO", "2,1");
			AssertColumnExistInMockMainDb("HrmTableDecimalToDecimalTest", "Number" + i, "decimal", "NO", "2,1");
		}

		var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
		RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

		AssertColumnExistInMockMainDb("DboTableDecimalToDecimalTest", "Number1", "decimal", "NO", "3,2");
		AssertColumnExistInMockMainDb("HrmTableDecimalToDecimalTest", "Number1", "decimal", "NO", "3,2");
		AssertColumnExistInMockMainDb("DboTableDecimalToDecimalTest", "Number2", "decimal", "NO", "2,0");
		AssertColumnExistInMockMainDb("HrmTableDecimalToDecimalTest", "Number2", "decimal", "NO", "2,0");
		AssertColumnExistInMockMainDb("DboTableDecimalToDecimalTest", "Number3", "decimal", "NO", "2,2");
		AssertColumnExistInMockMainDb("HrmTableDecimalToDecimalTest", "Number3", "decimal", "NO", "2,2");
		AssertColumnExistInMockMainDb("DboTableDecimalToDecimalTest", "Number4", "decimal", "NO", "1,0");
		AssertColumnExistInMockMainDb("HrmTableDecimalToDecimalTest", "Number4", "decimal", "NO", "1,0");
	}
}
