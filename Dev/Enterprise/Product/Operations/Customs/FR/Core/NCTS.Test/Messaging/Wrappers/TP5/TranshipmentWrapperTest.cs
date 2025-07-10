using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class TranshipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<TranshipmentWrapper>
	{
		public void TestContainerIndicator()
		{
			AssertEquals("ContainerIndicator should be equal to true if there is at least one container with containerNumber.", true, Provider.ContainerIndicator);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var container = nctsHeader.ArrivalHeaderContainers.AddNew();
			container.BC_ContainerNum = ZString.Empty;

			var incident = nctsHeader.EnRouteIncidents.AddNew();
			incident.BN_IncidentCode = "C";
			incident.BN_Information = "info";
			var incidentContainer = incident.IncidentContainers.AddNew();
			incidentContainer.BC_ContainerNum = ZString.Empty;
			var bill = nctsHeader.Bills.AddNew();

			var item1 = bill.ArrivalGoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 7;
			var package1 = item1.Packages.AddNew();
			var containerPivot1 = package1.ContainersPivot.AddNew();
			containerPivot1.XX_Relation2ID = container.PK;
			var wrapper = TranshipmentWrapper.New(incident);

			AssertEquals("ContainerIndicator should be equal to false if there is no container with containerNumber.", false, wrapper.ContainerIndicator);
		}

		public void TestTransportMeans()
		{
			AssertType<EnRouteIncidentTransportMeansWrapper>("TransportMeans should be of type EnRouteIncidentTransportMeansWrapper.", Provider.TransportMeans);
		}

		protected override TranshipmentWrapper GetProvider()
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

			return TranshipmentWrapper.New(incident);
		}
	}
}
