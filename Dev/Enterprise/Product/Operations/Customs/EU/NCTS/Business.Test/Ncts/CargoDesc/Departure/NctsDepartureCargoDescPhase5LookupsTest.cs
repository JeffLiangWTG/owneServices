using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsDepartureCargoDescPhase5LookupsTest : BusinessObjectLookupsTestCase
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
			SetUpDeclarationTypeCodes();

			CombineAssertions(() =>
			{
				AssertEquals("No Header", ZString.Empty, new NctsDepartureCargoDescPhase5Lookups(Factory.New<NctsDepartureCargoDesc>()).DeclarationTypeList.CodesAsString);
				var list = lookups.DeclarationTypeList;
				AssertEquals("Has a Header", "T1, T2, T2F, T2SM", list.CodesAsString);
				AssertSame("Cached", list, lookups.DeclarationTypeList);
			});
		}

		public void TestDeclarationTypeList_DifferentCachedKey()
		{
			SetUpDeclarationTypeCodes();

			AssertEquals("Should not be the same", false, ReferenceEquals(GetDeclarationTypeList(CusInBondApplicationCodeList.Codes.NCTS4), GetDeclarationTypeList(CusInBondApplicationCodeList.Codes.NCTS5)));

			CodeDescriptionPairList GetDeclarationTypeList(ZString applicationCode)
			{
				if (applicationCode == CusInBondApplicationCodeList.Codes.NCTS4)
				{
					var phase4NctsHeader = Factory.New<NctsHeader>();
					phase4NctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					phase4NctsHeader.BH_ApplicationCode = applicationCode;
					var phase4GoodsItem = phase4NctsHeader.MovementHeader.GoodsItems.AddNew();
					return phase4GoodsItem.Lookups.DeclarationTypeList;
				}
				else
				{
					return lookups.DeclarationTypeList;
				}
			}
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

		public void TestParts()
		{
			AssertNotNull(lookups.Parts);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();
			lookups = (NctsDepartureCargoDescPhase5Lookups)goodsItem.Lookups;
		}
		NctsHeader nctsHeader;
		NctsDepartureCargoDescPhase5Lookups lookups;

		void SetUpDeclarationTypeCodes()
		{
			var declarationTypeCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType;
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(declarationTypeCode, "NCTS Declaration Type (Box 1)");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T1", "T1", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T2", "T2", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T", "T", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "TIR", "TIR", startDate, endDate);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, declarationTypeCode, "T2SM", "T2SM", startDate, endDate);
			Factory.Save();
		}
	}
}
