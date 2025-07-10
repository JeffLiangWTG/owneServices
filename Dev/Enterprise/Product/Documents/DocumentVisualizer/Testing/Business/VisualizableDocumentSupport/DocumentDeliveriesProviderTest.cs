using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentDeliveriesProviderTest : TestCaseWithFactory
	{
		#region TestGetDocumentDeliveries

		public void TestGetDocumentDeliveries()
		{
			var shipment = CreateShipment();
			Factory.Save();

			var billOfLadingMenuItem = GetIAUBillOfLadingMenuItem();

			var provider = new DocumentDeliveriesProvider();
			var deliveries = provider.GetDocumentDeliveries(billOfLadingMenuItem, (BusinessObject)shipment);

			AssertEquals("created bills of lading", 4, deliveries.Count);
		}

		#endregion

		#region TestAccessibleViaObjectFactory

		public void TestAccessibleViaObjectFactory()
		{
			Assert("IDocumentDeliveriesProvider has been registered in ObjectFactory", ObjectFactory.Get<IDocumentDeliveriesProvider>() is DocumentDeliveriesProvider);
		}

		#endregion

		#region Implementation

		StmMenuItem GetIAUBillOfLadingMenuItem() => Factory.Load<StmMenuItemBase>(new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244"));

		IForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRRIO";
			shipment.JS_HouseBillOfLadingType = "IAU";

			return shipment;
		}

		#endregion
	}
}
