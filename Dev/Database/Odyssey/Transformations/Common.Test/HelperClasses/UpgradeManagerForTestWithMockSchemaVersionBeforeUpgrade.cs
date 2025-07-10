#if DEBUG

using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public class UpgradeManagerForTestWithMockSchemaVersionBeforeUpgrade : DummyUpgradeManager
	{
		public UpgradeManagerForTestWithMockSchemaVersionBeforeUpgrade(VersionLabel schemaVersionBeforeUpgrade)
		{
			this.schemaVersionBeforeUpgrade = schemaVersionBeforeUpgrade;
		}

		public override VersionLabel SchemaVersionBeforeUpgrade => schemaVersionBeforeUpgrade;

		readonly VersionLabel schemaVersionBeforeUpgrade;
	}
}

#endif
