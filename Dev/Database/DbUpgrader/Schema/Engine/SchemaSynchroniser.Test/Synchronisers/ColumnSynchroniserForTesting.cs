using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserForTesting : ColumnSynchroniser
	{
		public ColumnSynchroniserForTesting(DbConnection upgConnection, string dbBeingUpgraded, string templateDb)
			: base(upgConnection, new DummyUpgradeManager(), dbBeingUpgraded, templateDb)
		{
		}
	}
}
