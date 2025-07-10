using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	public class FROrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJE_DeltaMode_AuthorizationOwnerWithoutZO_DeltaG1SubProcedure()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = org.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount.CZ_Account = "12345678";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = org.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_RepresentativeID = "123456";

			var orgImpAddInfo = FROrgImpAddInfo.Get(org);
			orgImpAddInfo.ZO_DeltaG1SubProcedure = ZString.Empty;
			AssertHasMessageErrorContaining(orgImpAddInfo.ZO_DeltaG1SubProcedureInfo, "This value is mandatory when Delta Mode G1 is selected.");

			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgImpAddInfo.ZO_DeltaG1SubProcedure = ZString.Empty;
			AssertNoMessageErrorContaining(orgImpAddInfo.ZO_DeltaG1SubProcedureInfo, "This value is mandatory when Delta Mode G1 is selected.");

			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgImpAddInfo.ZO_DeltaG1SubProcedure = "C";
			AssertNoMessageErrorContaining(orgImpAddInfo.ZO_DeltaG1SubProcedureInfo, "This value is mandatory when Delta Mode G1 is selected.");
		}

		public void TestCheckZO_VatProcedureDateLimit()
		{
			var org = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(org);
			frOrgImpAddInfo.ZO_VATProcedureDateLimit = ZDate.Empty;

			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.S;
			frOrgImpAddInfo.Validation.ValidateZO_VATProcedureDateLimit();
			AssertNoMessageErrorContaining(frOrgImpAddInfo.ZO_VATProcedureDateLimitInfo, MandatoryValidation.YouHaveNotEntered);

			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.L;
			frOrgImpAddInfo.Validation.ValidateZO_VATProcedureDateLimit();
			AssertHasMessageErrorContaining(frOrgImpAddInfo.ZO_VATProcedureDateLimitInfo, MandatoryValidation.YouHaveNotEntered);

			var dateShouldBeInPast = "Date should be in the past.";
			frOrgImpAddInfo.ZO_VATProcedureDateLimit = ZDate.Today.AddYears(-1);
			AssertNoMessageError(frOrgImpAddInfo.ZO_VATProcedureDateLimitInfo, dateShouldBeInPast);

			frOrgImpAddInfo.ZO_VATProcedureDateLimit = ZDate.Today;
			AssertNoMessageError(frOrgImpAddInfo.ZO_VATProcedureDateLimitInfo, dateShouldBeInPast);

			frOrgImpAddInfo.ZO_VATProcedureDateLimit = ZDate.Today.AddYears(1);
			AssertHasMessageError(frOrgImpAddInfo.ZO_VATProcedureDateLimitInfo, dateShouldBeInPast);
		}

		public void TestCheckZO_DeltaG1SubProcedure()
		{
			var orgHeader = Factory.New<OrgHeader>();

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(orgHeader);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = "X";
			AssertHasMessageErrorContaining(frOrgImpAddInfo.ZO_DeltaG1SubProcedureInfo, ListValidation.InvalidCodeMessageError);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;
			AssertNoMessageErrors(frOrgImpAddInfo.ZO_DeltaG1SubProcedureInfo);
		}

		public void TestCheckZO_VATDeferType()
		{
			var org = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(org);
			frOrgImpAddInfo.ZO_VATDeferType = "X";
			AssertHasMessageErrorContaining(frOrgImpAddInfo.ZO_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.S;
			AssertNoMessageErrors(frOrgImpAddInfo.ZO_VATDeferTypeInfo);
		}
	}
}
