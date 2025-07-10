using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class GuaranteeReferenceWrapperTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeReferenceWrapper>
	{
		public void TestAlternateGrn()
		{
			AssertEquals("Grn should be mapped to application specific reference of guarantee when able.", "ABC", GetAlternateProvider().Grn);
		}

		public void TestGrn()
		{
			AssertEquals("Grn should be mapped to PW_BondNumber.", "GRN1", Provider.Grn);
		}

		public void TestAccessCode()
		{
			AssertEquals("AccessCode should be mapped to PW_Password.", "1234", Provider.AccessCode);
		}

		public void TestAmountToBeCovered()
		{
			AssertEquals("AmountToBeCovered should be mapped to PW_BondAmount.", 999.99m, Provider.AmountToBeCovered);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency should be mapped to PW_RX_NKCurrency.", "USD", Provider.Currency);
		}

		protected override GuaranteeReferenceWrapper GetProvider()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GRN1";
			guarantee.PW_Password = "1234";
			guarantee.PW_BondAmount = 999.99m;
			guarantee.PW_RX_NKCurrency = "USD";
			return GuaranteeReferenceWrapper.New(guarantee);
		}

		protected GuaranteeReferenceWrapper GetAlternateProvider()
		{
			EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DEC001";

			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "DeclarantAddress";
			declarantAddress.Address1 = "Declarant Address";

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = declarantAddress.PK;
			nctsHeader.Declarant.E2_OA_Address = declarantAddress.PK;

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = declarant.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guaranteeHeader.CPH_Number = "GRN1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;

			var additionalReference = guaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			additionalReference.CY_Data = "ABC";
			additionalReference.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
			Factory.Save();
			return GuaranteeReferenceWrapper.New(guarantee);
		}
	}
}
