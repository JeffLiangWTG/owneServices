using Enterprise.DbUpgrader.Data.Test;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Data
{
	public class DataBaseUpgraderTest : BaseUpgraderTestCase
	{
		public override BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
		{
			return new DataUpgraderTest.DataUpgraderForTest(dummyUpgradeManager);
		}
	}
}
