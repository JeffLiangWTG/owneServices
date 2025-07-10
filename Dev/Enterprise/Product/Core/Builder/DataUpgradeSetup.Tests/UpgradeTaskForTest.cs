using Enterprise.DbUpgrader.Data;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	[CodeAlive("Used Code")]
	public class UpgradeTaskForTest : EmbeddedUpgradeTask
	{
		public UpgradeTaskForTest()
				: base(new DataFileForTest())
		{
		}
	}
}
