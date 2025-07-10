using CargoWise.Customs.IL.MessageDefinitions.GPM;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageGatePassMovementDestinationSiteWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageGatePassMovementDestinationSite>
	{
		public void TestNewOrNull()
		{
			AssertNull("When gatePassMovement is null", MessageGatePassMovementDestinationSiteWrapper.NewOrNull(null));
			AssertNotNull("When gatePassMovement s not null", Provider);
		}

		public void TestDesignateSiteCode()
		{
			AssertEquals("IL000001", Provider.DesignateSiteCode);
		}

		public void TestTransportationTypeCode()
		{
			AssertEquals(150, Provider.TransportationTypeCode);
		}

		public void TestIsFinalDestination()
		{
			AssertEquals(true, Provider.IsFinalDestination);
		}

		public void TestExportManifestNo()
		{
			AssertEquals(null, Provider.ExportManifestNo);
		}

		protected override IMessageGatePassMovementDestinationSite GetProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gatePassMovement = new GatePassMovementDocDataObject("ForwardingShipment", "S0001001", Factory);
			gatePassMovement.DestinationSite = new CodeDescription(new CodeDescriptionPairList());
			gatePassMovement.DestinationSite.Code = "IL000001";

			gatePassMovement.TransportMethod = new CodeDescription(new CodeDescriptionPairList());
			gatePassMovement.TransportMethod.Code = "150";
			return MessageGatePassMovementDestinationSiteWrapper.NewOrNull(gatePassMovement);
		}
	}
}
