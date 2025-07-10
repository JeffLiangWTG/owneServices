using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	[UseSnapshotProtection]
	public abstract class TestCaseForColumnConversion : TestCaseWithMockMainDbAndTemplateDb
	{
		protected abstract string CreateMainDbScript { get; }
		protected abstract string CreateTemplateDbScript { get; }

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, CreateMainDbScript);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, CreateTemplateDbScript);
		}

		protected void SynchroniseColumnsInTransaction()
		{
			TestConnection.BeginTransaction();
			new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb).DropAlterAndAddColumns();
			TestConnection.CommitTransaction();
		}
	}
}
