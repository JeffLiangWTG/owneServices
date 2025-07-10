using System.Linq;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class EnRouteIncidentWrapperTest : Customs.Business.Testing.DataProviderTestCase<EnRouteIncidentWrapper>
	{
		public void TestCode()
		{
			AssertEquals("Code should be equal to BN_IncidentCode.", "C", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("Text should be equal to BN_Information.", "info", Provider.Text);
		}

		public void TestEndorsement()
		{
			AssertType<EndorsementWrapper>("Endorsement should be of type EndorsmentWrapper.", Provider.Endorsement);
		}

		public void TestLocation()
		{
			AssertType<IncidentLocationWrapper>("Location should be of type LocationWrapper.", Provider.Location);
		}

		public void TestTranshipment()
		{
			AssertType<TranshipmentWrapper>("Transhipment should be of type TranshipmentWrapper.", Provider.Transhipment);
		}

		public void TestTransportEquipment()
		{
			AssertEquals("There should be 2 TransportEquipments.", 2, Provider.TransportEquipment.Count);
			AssertType<EnRouteTransportEquipmentWrapper>("Transhipment should be of type TranshipmentWrapper.", Provider.TransportEquipment.ElementAt(0));
			AssertEquals("ContainerIdentificationNumber should be equal to BC_ContainerNum.", "124", Provider.TransportEquipment.ElementAt(0).ContainerIdentificationNumber);
		}

		protected override EnRouteIncidentWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var container = nctsHeader.ArrivalHeaderContainers.AddNew();
			container.BC_ContainerNum = "124";

			var container2 = nctsHeader.ArrivalHeaderContainers.AddNew();
			container2.BC_ContainerNum = "125";

			var incident = nctsHeader.EnRouteIncidents.AddNew();
			incident.BN_IncidentCode = "C";
			incident.BN_Information = "info";
			var incidentContainer = incident.IncidentContainers.AddNew();
			incidentContainer.BC_ContainerNum = "124";
			var bill = nctsHeader.Bills.AddNew();

			var incidentContainer2 = incident.IncidentContainers.AddNew();
			incidentContainer2.BC_ContainerNum = "125";

			var item1 = bill.ArrivalGoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 7;
			var package1 = item1.Packages.AddNew();
			var containerPivot1 = package1.ContainersPivot.AddNew();
			containerPivot1.XX_Relation2ID = container.PK;

			var item2 = bill.ArrivalGoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 7;
			var package2 = item2.Packages.AddNew();
			var containerPivot2 = package2.ContainersPivot.AddNew();
			containerPivot2.XX_Relation2ID = container2.PK;

			return EnRouteIncidentWrapper.New(incident);
		}
	}
}
