using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserConvertNormalColumnToComputedTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[]
			{
				@"
CREATE TABLE TableConvertNormalColumnToComputed
(
	Col1     int NOT NULL,
	CompCol1 AS ISNULL(Col1 % 2, 0),
	CompCol2 AS ISNULL(Col1 % 2, 0) PERSISTED NOT NULL,
);
"
			});
		}

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[]
			{
				@"
CREATE TABLE TableConvertNormalColumnToComputed
(
	Col1     int NOT NULL,
	CompCol1 int NOT NULL,
	CompCol2 AS ISNULL(Col1 % 2, 0),
);
"
			});
		}

		public void TestConversion()
		{
			var expectedDefinition = "CompCol1 int NOT NULL NOT PERSISTED AS (isnull([Col1]%(2),(0)))";

			AssertComputedColumnDefinition(mockTemplateDb, "dbo", "TableConvertNormalColumnToComputed", "CompCol1", expectedDefinition);
			AssertComputedColumnDefinition(mockMainDb, "dbo", "TableConvertNormalColumnToComputed", "CompCol1", expected: "");

			AssertComputedColumnDefinition(mockTemplateDb, "dbo", "TableConvertNormalColumnToComputed", "CompCol2", "CompCol2 int NOT NULL PERSISTED AS (isnull([Col1]%(2),(0)))");
			AssertComputedColumnDefinition(mockMainDb, "dbo", "TableConvertNormalColumnToComputed", "CompCol2", "CompCol2 int NOT NULL NOT PERSISTED AS (isnull([Col1]%(2),(0)))");

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			AssertComputedColumnDefinition(mockTemplateDb, "dbo", "TableConvertNormalColumnToComputed", "CompCol1", expectedDefinition);
			AssertComputedColumnDefinition(mockMainDb, "dbo", "TableConvertNormalColumnToComputed", "CompCol1", expectedDefinition);

			AssertComputedColumnDefinition(mockTemplateDb, "dbo", "TableConvertNormalColumnToComputed", "CompCol2", "CompCol2 int NOT NULL PERSISTED AS (isnull([Col1]%(2),(0)))");
			AssertComputedColumnDefinition(mockMainDb, "dbo", "TableConvertNormalColumnToComputed", "CompCol2", "CompCol2 int NOT NULL PERSISTED AS (isnull([Col1]%(2),(0)))");
		}
	}
}
