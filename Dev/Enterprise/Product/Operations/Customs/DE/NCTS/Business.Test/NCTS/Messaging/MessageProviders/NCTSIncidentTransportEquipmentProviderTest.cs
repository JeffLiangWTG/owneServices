using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSIncidentTransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSIncidentTransportEquipmentProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(NCTSIncidentTransportEquipmentProvider.NewOrNull(null));
		}

		public void TestIdentificationNumber()
		{
			container.BC_ContainerNum = "CN001";
			AssertEquals("CN001", Provider.IdentificationNumber);
		}

		public void TestSeals()
		{
			container.BC_Seal1 = "111";
			container.BC_Seal2 = "333";
			var additionalSeal = container.Seals.AddNew();
			additionalSeal.BK_SealNumber = "222";
			var additionalSeal2 = container.Seals.AddNew();
			additionalSeal2.BK_SealNumber = "444";
			AssertSequencesEqual(new[] { "111", "222", "333", "444" }, Provider.Seals.Identities);
		}

		public void TestGoodsReferences()
		{
			var item1 = container.ItemNumbers.AddNew();
			item1.CY_DataNumeric = 2;
			var item2 = container.ItemNumbers.AddNew();
			item2.CY_DataNumeric = 1;

			AssertSequencesEqual(new[] { 1, 2 }, Provider.GoodsReferences);
		}

		public void TestDamagedSealsNotRetained()
		{
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			var additionalSeal1 = container.Seals.AddNew();
			additionalSeal1.BK_SealNumber = "ADD1";
			additionalSeal1.BK_UnloadingState = "NEW";
			var additionalSeal2 = container.Seals.AddNew();
			additionalSeal2.BK_SealNumber = "ADD2";
			additionalSeal2.BK_UnloadingState = "DEC";
			var additionalSeal3 = container.Seals.AddNew();
			additionalSeal3.BK_SealNumber = "ADD3";
			additionalSeal3.BK_UnloadingState = "MIS";
			var additionalSeal4 = container.Seals.AddNew();
			additionalSeal4.BK_SealNumber = "ADD4";
			additionalSeal4.BK_UnloadingState = "DAM";

			AssertSequencesEqual(new[] { "ADD1", "ADD2", "ADD3", "seal1", "seal2" }, Provider.Seals.Identities);
		}

		protected override NCTSIncidentTransportEquipmentProvider GetProvider() => NCTSIncidentTransportEquipmentProvider.NewOrNull(container);

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = nctsHeader.EnRouteIncidents.AddNew();
			container = incident.IncidentContainers.AddNew();
		}
		EnRouteIncident incident;
		NctsContainer container;
	}
}
