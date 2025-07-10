using CargoWise.Customs.IL.MessageDefinitions.GPM;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageGatePassMovementCargoIdentifierWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageGatePassMovementCargoIdentifier>
	{
		public void TestNewOrNull()
		{
			AssertNull("When gatePassMovement is null", MessageGatePassMovementCargoIdentifierWrapper.NewOrNull(null));
			AssertNotNull("When gatePassMovement s not null", Provider);
		}

		public void TestCargoIdentifierType()
		{
			AssertEquals(11, Provider.CargoIdentifierType);
		}

		public void TestCargoIdentifierKey1()
		{
			AssertEquals("ARR123", Provider.CargoIdentifierKey1);
		}

		public void TestCargoIdentifierKey2()
		{
			AssertEquals("FDN456", Provider.CargoIdentifierKey2);
		}

		public void TestCargoIdentifierKey3()
		{
			AssertEquals(null, Provider.CargoIdentifierKey3);
		}

		protected override IMessageGatePassMovementCargoIdentifier GetProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gatePassMovement = new GatePassMovementDocDataObject("ForwardingShipment", "S0001001", Factory);
			gatePassMovement.CargoIdentifierType = new CodeDescription(new CodeDescriptionPairList());
			gatePassMovement.CargoIdentifierType.Code = "11";
			gatePassMovement.CargoIdentifierKey1 = "ARR123";
			gatePassMovement.CargoIdentifierKey2 = "FDN456";
			return MessageGatePassMovementCargoIdentifierWrapper.NewOrNull(gatePassMovement);
		}
	}
}
