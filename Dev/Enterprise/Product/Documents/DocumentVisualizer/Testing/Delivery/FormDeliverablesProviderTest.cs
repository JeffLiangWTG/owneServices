using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class FormDeliverablesProviderTest : TestCaseWithFactory
	{
		#region TestGetDeliverables

		public void TestGetDeliverables()
		{
			var shipment = CreateShipment();
			Factory.Save();

			var billOfLadingMenuItem = GetIAUBillOfLadingMenuItem();

			var provider = new FormDeliverablesProvider();
			var deliverables = provider.GetDeliverables(billOfLadingMenuItem, (IDocumentSupportable)shipment);

			AssertEquals("created bill of lading deliverables", 4, deliverables?.Count);
		}

		#endregion

		#region TestAccessibleViaObjectFactory

		public void TestAccessibleViaObjectFactory()
		{
			Assert("FormDeliverablesProvider has been registered in ObjectFactory", ObjectFactory.Get<IFormDeliverablesProvider>() is FormDeliverablesProvider);
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
