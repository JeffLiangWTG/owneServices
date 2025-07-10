using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	using Enterprise.Freight.Forwarding.Business;
	using NUnit.Framework;

	[TestedType(typeof(ConsolRevenueCollection))]
	public class ConsolRevenueCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ConsolRevenueCollection>
	{
		protected override ConsolRevenueCollection GetCollectionToTest()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, Factory);
			return new ConsolRevenueCollection(master, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, Factory);
			return new ConsolRevenue(master);
		}
	}
}
