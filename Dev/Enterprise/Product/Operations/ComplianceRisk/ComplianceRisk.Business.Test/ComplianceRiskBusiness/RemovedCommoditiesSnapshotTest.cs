using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(RemovedCommoditiesSnapshot))]
	public class RemovedCommoditiesSnapshotTest : TestCase
	{
		public void TestInitializeProperties()
		{
			var snapshot = new RemovedCommoditiesSnapshot();
			CombineAssertions(() =>
			{
				AssertNotNull(snapshot.Commodities);
				AssertEquals(0, snapshot.Commodities.Count);
			});
		}
	}
}
