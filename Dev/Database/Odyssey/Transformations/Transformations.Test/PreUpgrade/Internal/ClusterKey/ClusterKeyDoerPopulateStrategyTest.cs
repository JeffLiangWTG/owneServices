using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	class ClusterKeyDoerPopulateStrategyTest : TransactionedTestCase
	{
		public void TestGetClusterKeyWorkerPopulateStrategy()
		{
			var definition = new KeyDefinition(DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0);
			var offlineCkDoer = new ClusterKeyDoerForPopulateStrategyTesting(UpgradeMode.Offline);
			var onlineCkDoer = new ClusterKeyDoerForPopulateStrategyTesting(UpgradeMode.Online);

			CombineAssertions("Cluster Key Worker Populate Strategy Type", () =>
			{
				AssertType<SimpleClusterKeyPopulateStrategy>("Offline Mode", offlineCkDoer.GetClusterKeyWorkerPopulateStrategy_Exposed(definition));
				AssertType<OnlineClusterKeyWorkerPopulateStrategy>("Online Mode", onlineCkDoer.GetClusterKeyWorkerPopulateStrategy_Exposed(definition));
			});
		}

		protected class ClusterKeyDoerForPopulateStrategyTesting : ClusterKeyDoer
		{
			public ClusterKeyDoerForPopulateStrategyTesting(UpgradeMode upgMode) : base(new DummyUpgradeManager())
			{
				this.upgMode = upgMode;
			}

			protected override UpgradeMode UpgMode => upgMode;
			readonly UpgradeMode upgMode;

			public IClusterKeyWorkerPopulateStrategy GetClusterKeyWorkerPopulateStrategy_Exposed(KeyDefinition definition)
			{
				return GetClusterKeyWorkerPopulateStrategy(definition);
			}
		}
	}
}
