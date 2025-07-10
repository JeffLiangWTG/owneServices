using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserConvertCharFlagsToBitTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[] {
				@"CREATE TABLE TableConvertCharFlagsToBitTest (
					PK uniqueidentifier NOT NULL, 
					Flag char(1) NOT NULL DEFAULT 'N',
					[_2_Flag] bit NOT NULL CONSTRAINT [_2_DF_Flag] DEFAULT 0,
					CONSTRAINT PK_TableConvertCharFlagsToBitTest PRIMARY KEY NONCLUSTERED (PK));"
			});
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[] {
				@"CREATE TABLE TableConvertCharFlagsToBitTest (
					PK uniqueidentifier NOT NULL,
					Flag bit NOT NULL DEFAULT 0,
					CONSTRAINT PK_TableConvertCharFlagsToBitTest PRIMARY KEY NONCLUSTERED (PK));"
			});
		}

		public void TestConversion()
		{
			AssertColumnExistInMockMainDb("TableConvertCharFlagsToBitTest", "Flag", "CHAR", "NO", "1");
			AssertColumnExistInMockMainDb("TableConvertCharFlagsToBitTest", "_2_Flag", "BIT", "NO", null);

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			AssertColumnExistInMockMainDb("TableConvertCharFlagsToBitTest", "Flag", "BIT", "NO", null);
			AssertColumnDoesNotExistInMockMainDb("TableConvertCharFlagsToBitTest", "_2_Flag");
		}
	}
}
