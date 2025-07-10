using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing.Business
{
	sealed class DocumentEngineHelperTest : TestCaseWithFactory
	{
		public void TestGetBusinessContext()
		{
			AssertEquals(BusinessContext.INVALID, new DocumentEngineHelper().GetBusinessContext(null));

			var dummy = Factory.New<DummyBusinessObject>();
			AssertEquals(BusinessContext.INVALID, new DocumentEngineHelper().GetBusinessContext(dummy));

			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as BusinessObject;
			var consolDocumentSupportable = consol as IDocumentSupportable;
			AssertEquals(consolDocumentSupportable.DocumentSupporter.BusinessContext, new DocumentEngineHelper().GetBusinessContext(consol));

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as BusinessObject;
			var shipmentDocumentSupportable = shipment as IDocumentSupportable;
			AssertEquals(shipmentDocumentSupportable.DocumentSupporter.BusinessContext, new DocumentEngineHelper().GetBusinessContext(shipment));
		}
	}
}
