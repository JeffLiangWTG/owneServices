using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoDepotShipment))]
	sealed class SeaCargoDepotShipmentTest : SeaCargoDepotNonPersistantBusineesObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			SeaCargoDepotShipment result = SeaCargoDepotShipment.Load(shipment);
			return result;
		}

		CFSShipment shipment;
		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<CFSShipment>();
			shipment.JS_TotalPackageCount = 20;
			CommonContainer shipmentsContainer = Factory.New<CommonContainer>();
			shipmentsContainer.JC_ContainerNum = "CTRL0000022";
			shipmentsContainer.JC_OH_CFSClient = SomeForwarder().PK;
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].SetContainer(shipmentsContainer.PK);
		}
	}
}
