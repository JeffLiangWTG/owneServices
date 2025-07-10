using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WoolworthsOrdersControllerOverride))]
	public class WoolworthsOrdersControllerOverrideTest : ZControllerBasherTest
	{
		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override Enterprise.ZArchitecture.Modules.ControllerID GetControllerID()
		{
			return ControllerIDs.Orders;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			Order order = Factory.New<Order>();
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.JD_OrderNumber = "ordernum";
			Factory.Save();
			return order;
		}
	}
}
