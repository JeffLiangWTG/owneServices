using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Data.Testing
{
	[CodeAlive("Used Code")]
	sealed class UpgradeTaskForTest : EmbeddedUpgradeTask
	{
		public UpgradeTaskForTest()
			: base(new DataFileForTest())
		{
		}
	}
}
