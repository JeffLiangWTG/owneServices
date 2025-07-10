using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE044CommodityTypeProviderTest : Customs.Business.Testing.DataProviderTestCase<IE044CommodityTypeProvider>
	{
		protected override IE044CommodityTypeProvider GetProvider() => new IE044CommodityTypeProvider((NctsArrivalCargoDesc)consignmentItem);

		public void TestGoodsDescription()
		{
			consignmentItem.BY_Description = "Test Item";
			var provider = GetProvider();
			AssertEquals("Goods Description", "Test Item", provider.GoodsDescription);
		}

		public void TestCUSCode()
		{
			consignmentItem.BY_CusC4Number = "1";
			var provider = GetProvider();
			AssertEquals("CUS Code", "1", provider.CUSCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: euGroup);
			helper.CreateCusCodeType(EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL010");
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "ZZ", "EU 1", yesterday, tomorrow);
			Factory.Save();

			CodeDescriptionPairList cl112Codes = Factory.GetCountryCodesCTC();
			AssertEquals("Code successfully added", true, cl112Codes.ContainsCode("ZZ"));

			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.CusAuthorizationUsages.RemoveAndDeleteAll();
			header.ArrivalMovementHeader.CustomsOfficesForDeparture.RemoveAll();
			var bill = header.Bills.AddNew();
			consignmentItem.BY_FormattedHarmonisedTariff = "690.3 201564";
			bill.ArrivalGoodsItems.Add(consignmentItem);

			var provider = GetProvider();
			AssertEquals("Combined Nomenclature Code with no customs office", "15", provider.CombinedNomenclatureCode);

			header.ArrivalMovementHeader.CustomsOfficesForDeparture.AddNew("DEP", "ZZ1");
			provider = GetProvider();
			AssertEquals("Combined Nomenclature Code with customs office in CL112 list", null, provider.CombinedNomenclatureCode);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			consignmentItem.BY_FormattedHarmonisedTariff = "690.3 201564";
			var provider = GetProvider();
			AssertEquals("Harmonized System Sub Heading Code", "690320", provider.HarmonizedSystemSubHeadingCode);
		}

		public void TestGrossMass()
		{
			consignmentItem.BY_GrossWeight = 12.34m;
			consignmentItem.BY_GrossWeightUnit = "KG";

			var provider = GetProvider();
			AssertEquals("Gross Mass", 12.34m, provider.GrossMass);
		}

		public void TestNetMass()
		{
			consignmentItem.BY_NetWeight = 12.34m;
			consignmentItem.BY_NetWeightUnit = "KG";

			var provider = GetProvider();
			AssertEquals("Nett Mass", 12.34m, provider.NetMass);
		}

		public void TestSupplementaryUnits()
		{
			// TODO in a future work item
			Assert(true);
		}

		protected override void SetUp()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.CusAuthorizationUsages.RemoveAndDeleteAll();
			var bill = header.Bills.AddNew();
			consignmentItem = bill.ArrivalGoodsItems.AddNew();
			consignmentItem.BY_LineNo = 1;
		}

		NctsCommonCargoDesc consignmentItem;
	}

	class IE044CommodityTypeProvider_UnloadedGoodsItemTest : Customs.Business.Testing.DataProviderTestCase<IE044CommodityTypeProvider>
	{
		protected override IE044CommodityTypeProvider GetProvider() => new IE044CommodityTypeProvider(consignmentItem);

		public void TestGoodsDescription()
		{
			unloadedGoodsItem.BY_Description = "Test Item";
			var provider = GetProvider();
			AssertEquals("Goods Description", "Test Item", provider.GoodsDescription);
		}

		public void TestCUSCode()
		{
			unloadedGoodsItem.BY_CusC4Number = "1";
			var provider = GetProvider();
			AssertEquals("CUS Code", "1", provider.CUSCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: euGroup);
			helper.CreateCusCodeType(EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL010");
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "ZZ", "EU 1", yesterday, tomorrow);
			Factory.Save();

			var cl112Codes = Factory.GetCountryCodesCTC();
			AssertEquals("Code successfully added", true, cl112Codes.ContainsCode("ZZ"));

			header.ArrivalMovementHeader.CustomsOfficesForDeparture.RemoveAll();
			unloadedGoodsItem.BY_FormattedHarmonisedTariff = "690.3 201564";

			var provider = GetProvider();
			AssertEquals("Combined Nomenclature Code with no customs office", "15", provider.CombinedNomenclatureCode);

			header.ArrivalMovementHeader.CustomsOfficesForDeparture.AddNew("DEP", "ZZ1");
			provider = GetProvider();
			AssertEquals("Combined Nomenclature Code with customs office in CL112 list", null, provider.CombinedNomenclatureCode);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			unloadedGoodsItem.BY_FormattedHarmonisedTariff = "690.3 201564";
			var provider = GetProvider();
			AssertEquals("Harmonized System Sub Heading Code", "690320", provider.HarmonizedSystemSubHeadingCode);
		}

		public void TestGrossMass()
		{
			unloadedGoodsItem.BY_GrossWeight = 12.34m;
			unloadedGoodsItem.BY_GrossWeightUnit = "KG";

			var provider = GetProvider();
			AssertEquals("Gross Mass", 12.34m, provider.GrossMass);
		}

		public void TestNetMass()
		{
			unloadedGoodsItem.BY_NetWeight = 12.34m;
			unloadedGoodsItem.BY_NetWeightUnit = "KG";

			var provider = GetProvider();
			AssertEquals("Nett Mass", 12.34m, provider.NetMass);
		}

		public void TestSupplementaryUnits()
		{
			// TODO in a future work item
			Assert(true);
		}

		protected override void SetUp()
		{
			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.CusAuthorizationUsages.RemoveAndDeleteAll();
			var bill = header.Bills.AddNew();
			consignmentItem = bill.ArrivalGoodsItems.AddNew();
			consignmentItem.BY_LineNo = 1;
			consignmentItem.BY_UnloadedState = "DIF";
			unloadedGoodsItem = consignmentItem.UnloadedGoodsItem;
		}
		NctsHeader header;
		NctsArrivalCargoDesc consignmentItem;
		NctsUnloadedCargoDesc unloadedGoodsItem;
	}
}
