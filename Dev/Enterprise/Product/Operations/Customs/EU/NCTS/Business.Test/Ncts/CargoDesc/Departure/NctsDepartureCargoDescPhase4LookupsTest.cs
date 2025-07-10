using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsDepartureCargoDescPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryOfDispatchList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfDispatchList = lookups.CountryOfDispatchList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes From List", "AU, DE, FR", countryOfDispatchList.CodesAsString);
				AssertSame("Cached", countryOfDispatchList, lookups.CountryOfDispatchList);
			});
		}

		public void TestCountryOfDestinationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfDestinationList = lookups.CountryOfDestinationList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes From List", "AU, DE, FR", countryOfDestinationList.CodesAsString);
				AssertSame("Cached", countryOfDestinationList, lookups.CountryOfDestinationList);
			});
		}

		public void TestDeclarationTypeList()
		{
			var declarationTypeCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType;
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(declarationTypeCode, "NCTS Declaration Type (Box 1)");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T1", "T1", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T2", "T2", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T-", "T-", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "TIR", "TIR", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T2SM", "T2SM", startDate, endDate);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("No Header", ZString.Empty, new NctsDepartureCargoDescPhase4Lookups(Factory.New<NctsDepartureCargoDesc>()).DeclarationTypeList.CodesAsString);
				var list = lookups.DeclarationTypeList;
				AssertEquals("Has a Header", "T1, T2", list.CodesAsString);
				AssertSame("Cached", list, lookups.DeclarationTypeList);
			});
		}

		public void TestTransportChargesModeOfPaymentList()
		{
			CombineAssertions(() =>
			{
				var transportChargesModeOfPaymentList = lookups.TransportChargesModeOfPaymentList;
				AssertEquals("Codes", "A, B, C, D, H, Y, Z", transportChargesModeOfPaymentList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<TransportChargesModeOfPayment>(), transportChargesModeOfPaymentList);
			});
		}

		public void TestTaxOrFeeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("LV1", 0.1, Core.Constants.CountryCodes.Latvia, 0.1, 0.1, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTaxOrFee("LV2", 0.2, Core.Constants.CountryCodes.Latvia, 0.2, 0.2, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTaxOrFee("LV3", 0.3, Core.Constants.CountryCodes.Latvia, 0.3, 0.3, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTaxOrFee("IT", 0.3, Core.Constants.CountryCodes.Italy, 0.3, 0.3, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			AssertEquals("LV1, LV2", lookups.TaxOrFeeCodeList.CodesAsString);
		}

		public void TestConsigneeList()
		{
			AssertType<ConsigneeCollection>(lookups.ConsigneeList);
		}

		public void TestBondedWhsUnitQtyList()
		{
			AssertNullOrEmpty(lookups.BondedWhsUnitQtyList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var line = nctsHeader.MovementHeader.GoodsItems.AddNew();
			lookups = new NctsDepartureCargoDescPhase4Lookups(line);
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDescPhase4Lookups lookups;
	}
}
