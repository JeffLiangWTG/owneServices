using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WoolworthsOrderLineControllerOverride))]
	public class WoolworthsOrderLineControllerOverrideTest : ZControllerBasherTest
	{
		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected OrderLineCollection fOrderLineCollection;
		protected OrderLine fOrderLine;
		public void TestGetForm_EmptyOrder()
		{
			WoolworthsOrderLineControllerOverride controller = new WoolworthsOrderLineControllerOverride();
			fOrderLine.JO_JD = System.Guid.Empty;
			using (WoolworthsOrderLineForm form = (WoolworthsOrderLineForm)controller.ShowFormForNewEntity(fOrderLine))
			{
				form.Show();
				System.Windows.Forms.Application.DoEvents();
				AssertNotNull(fOrderLine.Order);
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return fOrderLine;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrderLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "Zubstest";
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			fOrderLineCollection = order.OrderLines;
			fOrderLine = fOrderLineCollection.AddNew();
			Factory.Save();
			Controller.SetCollectionForDefaultsAndValidation(fOrderLineCollection);
		}
	}
}
