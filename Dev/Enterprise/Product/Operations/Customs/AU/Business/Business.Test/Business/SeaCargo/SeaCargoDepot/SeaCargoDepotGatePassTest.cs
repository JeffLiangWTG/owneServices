using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoDepotGatePass))]
	sealed class SeaCargoDepotGatePassTest : SeaCargoDepotNonPersistantBusineesObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			SeaCargoDepotGatePass result = SeaCargoDepotGatePass.Load(shipment);
			return result;
		}

		#region Implementation
		const string ContainerNum1 = "CTRL0000022";
		GatePassShipment shipment;
		CommonContainer shipmentsContainer;
		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<GatePassShipment>();
			shipment.JS_TotalPackageCount = 20;
			shipmentsContainer = Factory.New<GatePassContainer>();
			shipmentsContainer.JC_ContainerNum = ContainerNum1;
			shipmentsContainer.JC_OH_CFSClient = SomeForwarder().PK;
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].SetContainer(shipmentsContainer.PK);
		}

		#endregion
	}
}
