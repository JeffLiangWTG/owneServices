using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.Testing;
sealed class CargoIdentifierWrapperTest : Customs.Business.Testing.DataProviderTestCase<ICargoIdentifier>
{
	public void TestNewOrNull()
	{
		AssertNull("When deliveryOrderDocData is null", CargoIdentifierWrapper.NewOrNull(null));
		AssertNotNull("When deliveryOrderDocData is not null", Provider);
	}

	public void TestCargoIdentifierKey1()
	{
		AssertEquals("2", Provider.CargoIdentifierKey1);
	}

	public void TestCargoIdentifierKey2()
	{
		AssertEquals("3", Provider.CargoIdentifierKey2);
	}

	public void TestCargoIdentifierType()
	{
		AssertEquals(1, Provider.CargoIdentifierType);
	}

	protected override ICargoIdentifier GetProvider()
		=> CargoIdentifierWrapper.NewOrNull(deliveryOrderDocData);

	protected override void SetUp()
	{
		base.SetUp();
		var shipment = Factory.New<ForwardingShipment>();
		deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
		deliveryOrderDocData.CargoIdentifierType = new CodeDescription(new CodeDescriptionPairList());
		deliveryOrderDocData.CargoIdentifierType.Code = "1";
		deliveryOrderDocData.ManifestNumber = "2";
		deliveryOrderDocData.DealNumber = "3";
	}

	DeliveryOrderDocDataObject deliveryOrderDocData;
}

