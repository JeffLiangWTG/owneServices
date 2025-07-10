using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ConsolRevenue.Testing
{
	using Enterprise.Accounting.Integration;
	using Enterprise.Freight.Forwarding.Business;
	using NUnit.Framework;

	[TestedType(typeof(RevenueSplitChargeColllection))]
	public class RevenueSplitChargeColllectionTest : ActiveBusinessObjectCollectionTestCase<RevenueSplitChargeColllection>
	{
		protected override RevenueSplitChargeColllection GetCollectionToTest()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, Factory);
			RevenueSplitChargeColllection col = new RevenueSplitChargeColllection(Factory);
			return col;
		}

		public void TestRelation()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, Factory);
			RevenueSplitChargeColllection col = new RevenueSplitChargeColllection(Factory);
			AssertEquals(true, col.Relationship is AdhocCollectionRelationship);
		}
	}
}
