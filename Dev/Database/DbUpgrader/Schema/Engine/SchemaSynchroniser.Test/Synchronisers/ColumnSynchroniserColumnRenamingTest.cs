using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing;

sealed class ColumnSynchroniserColumnRenamingTest : TestCaseWithMockMainDbAndTemplateDbTransactional
{
	protected override IAuxiliaryDbCreator GetMockMainDbCreator()
	{
		return new AuxiliaryDbCreatorForTesting(
			mockMainDb,
			[
				@"
CREATE TABLE dbo.TableConvertCharFlagsToBitTest (
	PK uniqueidentifier NOT NULL, 
	Flag char(1) NOT NULL DEFAULT 'N',
	[_2_Flag] bit SPARSE,
	CONSTRAINT PK_TableConvertCharFlagsToBitTest PRIMARY KEY NONCLUSTERED (PK)
);
"
			]);
	}

	protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
	{
		return new AuxiliaryDbCreatorForTesting(
			mockTemplateDb,
			[
				@"
CREATE TABLE dbo.TableConvertCharFlagsToBitTest (
	PK uniqueidentifier NOT NULL,
	Flag bit SPARSE,
	CONSTRAINT PK_TableConvertCharFlagsToBitTest PRIMARY KEY NONCLUSTERED (PK)
);
"
			]);
	}

	// Use char-to-bit column synchroniser to check if renaming function supports sparse columns
	public void TestSupportToRenameSparseColumn()
	{
		// Arrange
		var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);

		// Act
		RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

		// Assert
		using var useMockMainDb = ((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb);
		var columns = ColumnChangeMetadataHelper.GetTableColumnMetadata(TestConnection, "dbo", "TableConvertCharFlagsToBitTest").Select(x => x.ColumnDeclaration);
		AssertContainsExactElementsInAnyOrder(
			[
				"[PK] uniqueidentifier NOT NULL",
				"[Flag] bit SPARSE NULL",
			],
			columns);
	}
}
