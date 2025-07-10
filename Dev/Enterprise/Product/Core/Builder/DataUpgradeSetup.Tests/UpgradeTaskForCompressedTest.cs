using Enterprise.DbUpgrader.Data;

namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	public class UpgradeTaskForCompressedTest : EmbeddedUpgradeTask
	{
		public UpgradeTaskForCompressedTest()
				: base(new DataFileForCompressedTest())
		{
		}
	}
}
