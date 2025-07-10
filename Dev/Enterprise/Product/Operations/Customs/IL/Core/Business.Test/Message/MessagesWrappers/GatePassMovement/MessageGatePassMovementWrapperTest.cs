using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.GPM;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageGatePassMovementWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageGatePassMovement>
	{
		public void TestNewOrNull()
		{
			AssertNull("When gatePassMovement is null", MessageGatePassMovementWrapper.NewOrNull(null, false));
			AssertNotNull("When gatePassMovement s not null", Provider);
		}

		public void TestGatepassRequestMessageDetailsList()
		{
			AssertNotNull(Provider.GatepassRequestMessageDetailsList);
			AssertType<MessageGatePassMovementDetailsWrapper>(Provider.GatepassRequestMessageDetailsList.Single());
		}

		public void TestRequestContentHeader()
		{
			AssertNotNull(Provider.RequestContentHeader);
			AssertType<RequestContentHeaderWrapper>(Provider.RequestContentHeader);
		}

		protected override IMessageGatePassMovement GetProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gatePassMovement = new GatePassMovementDocDataObject("ForwardingShipment", "S0001001", Factory);
			gatePassMovement.CargoType = new CodeDescription(new CodeDescriptionPairList());
			gatePassMovement.CargoType.Code = "3";
			gatePassMovement.OriginSite = new CodeDescription(new CodeDescriptionPairList());
			gatePassMovement.OriginSite.Code = "ITMIL";
			gatePassMovement.ProcessType = "1";

			return MessageGatePassMovementWrapper.NewOrNull(gatePassMovement, false);
		}
	}
}
