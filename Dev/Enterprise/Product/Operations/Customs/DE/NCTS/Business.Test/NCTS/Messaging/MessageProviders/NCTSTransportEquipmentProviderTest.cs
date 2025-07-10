using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSTransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSTransportEquipmentProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(NCTSTransportEquipmentProvider.NewOrNull(null, null));
		}

		public void TestIdentificationNumber_CNT()
		{
			headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("CN001", Provider.IdentificationNumber);
		}

		public void TestIdentificationNumber_NCT()
		{
			headerContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			AssertNull(Provider.IdentificationNumber);
		}

		public void TestGoodsReferences_CNT()
		{
			headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			headerContainer2.BC_Mode = Core.Constants.ContainerModes.Containerised;
			var bill1 = nctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = item1.Packages.AddNew();
			package1.ContainersPivot.AddPivotFor(headerContainer);
			var item2 = bill1.GoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 2;
			var package2 = item2.Packages.AddNew();
			package2.ContainersPivot.AddPivotFor(headerContainer);
			var item3 = bill1.GoodsItems.AddNew();
			item3.BY_DeclarationGoodsItemNumber = 3;
			item3.Packages.AddNew();

			var bill2 = nctsHeader.Bills.AddNew();
			var item4 = bill2.GoodsItems.AddNew();
			item4.BY_DeclarationGoodsItemNumber = 4;
			var package4 = item4.Packages.AddNew();
			package4.ContainersPivot.AddPivotFor(headerContainer);

			AssertSequencesEqual(new[] { 1, 2, 4 }, Provider.GoodsReferences);
		}

		public void TestGoodsReferences_NCT()
		{
			headerContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			var bill1 = nctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = item1.Packages.AddNew();
			package1.ContainersPivot.AddPivotFor(headerContainer);
			var item2 = bill1.GoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 2;
			var package2 = item2.Packages.AddNew();
			package2.ContainersPivot.AddPivotFor(headerContainer);
			var item3 = bill1.GoodsItems.AddNew();
			item3.BY_DeclarationGoodsItemNumber = 3;
			item3.Packages.AddNew();

			var bill2 = nctsHeader.Bills.AddNew();
			var item4 = bill2.GoodsItems.AddNew();
			item4.BY_DeclarationGoodsItemNumber = 4;
			var package4 = item4.Packages.AddNew();
			package4.ContainersPivot.AddPivotFor(headerContainer);

			AssertEquals(false, Provider.GoodsReferences.Any());
		}

		public void TestSeals_00()
		{
			declarationType = "00";
			headerContainer.BC_Seal1 = "111";
			AssertNull(Provider.Seals);
		}

		public void TestSeals_10()
		{
			declarationType = "10";
			headerContainer.BC_Seal1 = "111";
			headerContainer.BC_Seal2 = "333";
			var additionalSeal = headerContainer.AdditionalSeals.AddNew();
			additionalSeal.BK_SealNumber = "222";
			var additionalSeal2 = headerContainer.AdditionalSeals.AddNew();
			additionalSeal2.BK_SealNumber = "444";
			AssertSequencesEqual(new[] { "111", "222", "333", "444" }, Provider.Seals.Identities);
		}

		public void TestSeals_11()
		{
			declarationType = "11";
			headerContainer.BC_Seal1 = "111";
			AssertSequencesEqual(new[] { "111" }, Provider.Seals.Identities);
		}

		string declarationType;
		protected override NCTSTransportEquipmentProvider GetProvider() => NCTSTransportEquipmentProvider.NewOrNull(headerContainer, declarationType);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CN001";
			headerContainer2 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_ContainerNum = "CN002";
			declarationType = "00";
		}
		NctsHeader nctsHeader;
		NctsDepartureHeaderContainer headerContainer;
		NctsDepartureHeaderContainer headerContainer2;
	}
}
