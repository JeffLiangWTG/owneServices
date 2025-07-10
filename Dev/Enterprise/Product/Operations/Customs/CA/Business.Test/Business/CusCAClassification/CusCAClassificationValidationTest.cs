using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAClassificationValidationTest : TestCaseWithFactory
	{
		#region TestCheckCCA_RN_NKOrigin

		public void TestCheckCCA_RN_NKOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_RN_NKOriginInfo, "??", Core.Constants.CountryCodes.Australia);
		}

		#endregion

		#region TestCheckCCA_ProvinceOfOrigin

		public void TestCheckCCA_ProvinceOfOriginForExports()
		{
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			cAClassification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ProvinceOfOriginInfo, "??", CanadianProvinceList.Codes.Alberta);
			cAClassification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ProvinceOfOriginInfo, "??", CanadianProvinceList.Codes.BritishColumbia);
			cAClassification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Australia;
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ProvinceOfOriginInfo, "??", CanadianProvinceList.Codes.Manitoba);
		}

		public void TestCheckCCA_ProvinceOfOriginForImports()
		{
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			cAClassification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ProvinceOfOriginInfo, "??", CanadianProvinceList.Codes.Alberta);
			cAClassification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ProvinceOfOriginInfo, "??", USStatesList.Codes.Alabama);
			cAClassification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Australia;
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ProvinceOfOriginInfo, "??", ZString.Empty);
		}

		#endregion

		#region TestCheckCCA_99TariffCode

		public void TestCheckCCA_99TariffCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_99TariffCodeInfo, "9956", "9955", "Tariff code 9956 not found in the customs tariff code list.");
		}

		#endregion

		#region TestCheckCCA_ValueForDutyCode

		public void TestCheckCCA_ValueForDutyCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ValueForDutyCodeInfo, "99", "28");
		}

		#endregion

		#region TestCheckCCA_DestinationProvince

		public void TestCheckCCA_DestinationProvince()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_DestinationProvinceInfo, "??", CanadianProvinceList.Codes.Alberta);
		}

		#endregion

		#region TestCheckCCA_RN_NKCFIAOrigin

		public void TestCheckCCA_RN_NKCFIAOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_RN_NKCFIAOriginInfo, "??", Core.Constants.CountryCodes.Australia);
		}

		#endregion

		#region TestCheckCCA_CFIAUSStateOfOrigin

		public void TestCheckCCA_CFIAUSStateOfOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_CFIAUSStateOfOriginInfo, "??", USStatesList.Codes.Alabama);
		}

		#endregion

		#region TestCheckCCA_ImportReasonCode

		public void TestCheckCCA_ImportReasonCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ImportReasonCodeInfo, "99", "02");
		}

		#endregion

		#region TestCheckCCA_EndUse

		public void TestCheckCCA_EndUse()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_EndUseInfo, "??", "01");
		}

		#endregion

		#region TestCheckCCA_MiscID

		public void TestCheckCA_MiscID()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CFIAM", "CFIA Misc Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CFIAM", "1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_MiscIDInfo, "XX", "1");
		}

		#endregion

		#region TestCheckCCA_GSTStatusCode

		public void TestCheckCCA_GSTStatusCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_GSTStatusCodeInfo, "??", "48");
		}

		#endregion

		#region TestCheckCCA_ETExemption

		public void TestCheckCCA_ETExemption()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ETExemptionInfo, "??", "85");
		}

		#endregion

		#region TestCheckCCA_ETRateCode

		public void TestCheckCCA_ETRateCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.ExciseTax);
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "E07", refCusRateType.PK);

			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_ETRateCodeInfo, "??", "E07");
		}

		#endregion

		#region TestCheckCCA_TreatmentCode

		public void TestCheckCCA_TreatmentCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry("01", "01", Core.Constants.CountryCodes.Canada);
			ValidationTestHelper.AssertInvalidCodeMessageError(cAClassification.CCA_TreatmentCodeInfo, "99", "01");
		}

		#endregion

		#region CheckCCA_AuthorityNumber

		public void TestCheckCCA_AuthorityNumber()
		{
			var warning = "Special Authority Number not found in Rulings table. Either add a new Ruling (F3) or select a valid Ruling (F4).";
			var error = "Special Authority Number is found but is associated with another organization and cannot be used with this organization.";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling1 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C001", "2", ZDate.Today, ZDate.Today.AddDays(1));
			cusRuling1.ZZX_OA_AppliesTo = org1.MainAddress.PK;
			var cusRuling2 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C002", "2", ZDate.Today, ZDate.Today.AddDays(1));
			cusRuling2.ZZX_OA_AppliesTo = org2.MainAddress.PK;

			Factory.Save();

			var part = Factory.New<OrgSupplierPart>();
			part.RelatedOrganisations.AddOwner(org1);

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CCA_AuthorityNumber = "~";
			AssertHasWarning(pivot.CCA_AuthorityNumberInfo, warning);

			pivot.CCA_AuthorityNumber = "C002";
			AssertNoWarning(pivot.CCA_AuthorityNumberInfo, warning);
			AssertHasError(pivot.CCA_AuthorityNumberInfo, error);

			pivot.CCA_AuthorityNumber = "C001";
			AssertNoError(pivot.CCA_AuthorityNumberInfo, error);

			var classification = Factory.New<CusClassification>();
			classification.CCA_AuthorityNumber = "~";
			AssertHasWarning(classification.CCA_AuthorityNumberInfo, warning);

			classification.CCA_AuthorityNumber = "C001";
			AssertNoWarning(classification.CCA_AuthorityNumberInfo, warning);
		}

		#endregion

		public void TestThereIsNoExceptionWhenParentIsCusClassification()
		{
			var classification = Factory.New<CusClassification>();
			var caClassification = classification.Details;
			AssertNoExceptionThrown(() =>
			{
				caClassification.Validation.ValidateCCA_AuthorityNumber();
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			cAClassification = pivot.Details;
		}
		CusClassPartPivot pivot;
		CusCAClassification cAClassification;

		#endregion
	}
}
