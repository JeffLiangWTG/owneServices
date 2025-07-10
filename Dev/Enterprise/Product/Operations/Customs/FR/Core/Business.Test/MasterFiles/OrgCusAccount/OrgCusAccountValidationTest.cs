using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	class OrgCusAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCZ_Account()
		{
			CombineAssertions(() =>
			{
				orgCusAccount.CZ_Account = ZString.Empty.PadRight(7, '9');
				AssertHasMessageError(orgCusAccount.CZ_AccountInfo, "Entered Account must have 8 numeric digits");
				orgCusAccount.CZ_Account = ZString.Empty.PadRight(8, '9');
				AssertNoMessageErrors(orgCusAccount.CZ_AccountInfo);
			});
		}

		public void TestDECOForDeltaIE_CZ_Account_AllowsMaximum35AlphaNumericChars()
		{
			CombineAssertions(() =>
			{
				var orgCusAccount1 = Factory.NewWithValidTestData<OrgCusAccount>();
				orgCusAccount1.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				orgCusAccount1.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
				orgCusAccount1.CZ_OH = orgHeader.PK;
				AssertNoMessageError("Zero characters are allowed", orgCusAccount1.CZ_AccountInfo, "Entered DECO for Delta IE Account cannot have more than 35 alphanumeric characters");
				orgCusAccount1.CZ_Account = "Non_ALPHANUMERIC_CHARACTERS$";
				AssertHasMessageError("Non alphanumeric characters are not allowed", orgCusAccount1.CZ_AccountInfo, "Entered DECO for Delta IE Account cannot have more than 35 alphanumeric characters");
				AssertEquals("Max length should be 35", 35, orgCusAccount1.CZ_AccountInfo.MaxLength);
				orgCusAccount1.CZ_Account = "35ALPHANUMERICCHARACTERSxxxxxxxxxxx";
				AssertNoMessageError("Should allow 35 alphanumeric characters", orgCusAccount1.CZ_AccountInfo, "Entered DECO for Delta IE Account cannot have more than 35 alphanumeric characters");
			});
		}

		public void TestDECForDeltaIE_CZ_TypeIsInvalidCodeOrEmpty()
		{
			var message = "Enter a valid selection";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount.CZ_Type = string.Empty;
			orgCusAccount.CZ_Account = "0000186";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = orgHeader.PK;

			AssertHasMessageErrorContaining(orgCusAccount.CZ_TypeInfo, "You have not entered");
			orgCusAccount.CZ_Type = "XX";
			orgCusAccount.Validation.ValidateCZ_Type();
			AssertHasErrorContaining(orgCusAccount.CZ_TypeInfo, message);
			AssertNoMessageError(orgCusAccount.CZ_TypeInfo, "You have not entered");
			orgCusAccount.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			AssertNoMessageErrorContaining(orgCusAccount.CZ_TypeInfo, "You have not entered");
			AssertNoMessageErrorContaining("DCN is valid DEC sub type", orgCusAccount.CZ_TypeInfo, message);
		}

		public void TestDECOForDeltaIE_MustHaveUniqueAccountNumber()
		{
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount.CZ_Type = string.Empty;

			var orgCusAccount2 = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount2.CZ_Type = string.Empty;
			orgCusAccount2.CZ_OH = orgHeader.PK;

			orgCusAccount.CZ_Account = "111";
			orgCusAccount2.CZ_Account = "111";
			AssertHasErrorContaining("Only one non DCN account is allowed", orgCusAccount2.CZ_AccountInfo, "Entered DECO for Delta IE Account already exists for this organization");

			orgCusAccount.CZ_Type = string.Empty;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount.CZ_Account = "222";
			orgCusAccount2.CZ_Account = "222";
			AssertHasErrorContaining(" A DCN account and a non-DCN account with the same account number is not allowed", orgCusAccount2.CZ_AccountInfo, "Entered DECO for Delta IE Account already exists for this organization");

			orgCusAccount.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount.CZ_Account = "333";
			orgCusAccount2.CZ_Account = "333";
			AssertHasErrorContaining("non unique DCN accounts are not allowed", orgCusAccount2.CZ_AccountInfo, "Entered DECO for Delta IE Account already exists for this organization");

			orgCusAccount2.CZ_Account = "0000186";
			AssertNoErrorContaining("multiple unique DCN accounts are allowed", orgCusAccount2.CZ_AccountInfo, "Entered DECO for Delta IE Account already exists for this organization");
		}

		public void TestCheckCZ_Issuer()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000001", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(orgCusAccount.CZ_IssuerInfo, "~", "FR000001");
		}

		public void TestCheckCZ_Code()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(orgCusAccount.CZ_CodeInfo, "~", OrgCusAccountCodeList.Codes.DGE);

			var message = "This combination already exists for this organization";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = orgHeader.PK;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;

			var orgCusAccount2 = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_OH = orgHeader.PK;
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGI;

			AssertHasMessageError(orgCusAccount2.CZ_CodeInfo, message);

			orgCusAccount.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DEC;

			AssertNoMessageError("You can have multiple DeltaIE DEC accounts", orgCusAccount2.CZ_CodeInfo, message);
		}

		public void TestCheckCZ_TypeAgainstG1SubType()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(orgHeader);

			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_Account = "0000186";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = orgHeader.PK;
			AssertHasMessageErrorContaining("Sub procedure not set", orgCusAccount.CZ_TypeInfo, "Delta G1 sub procedure type in consignee tab of current organization is not set.");

			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;
			orgCusAccount.RunPreSaveValidation();
			AssertNoMessageErrorContaining("Sub procedure set", orgCusAccount.CZ_TypeInfo, "Delta G1 sub procedure type in consignee tab of current organization is not set.");
		}

		public void TestCheckCZ_Type()
		{
			var message = "This combination already exists for this organization";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(orgHeader);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;

			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_Account = "0000186";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = orgHeader.PK;

			var orgCusAccount2 = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount2.CZ_OH = orgHeader.PK;
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			AssertNoMessageError(orgCusAccount2.CZ_CodeInfo, message);
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertHasMessageError(orgCusAccount2.CZ_TypeInfo, message);

			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertNoMessageError(orgCusAccount2.CZ_TypeInfo, message);
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			AssertHasMessageError(orgCusAccount2.CZ_CodeInfo, message);

			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;

			AssertNoMessageError(orgCusAccount2.CZ_TypeInfo, message);

			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			AssertNoMessageError(orgCusAccount2.CZ_TypeInfo, message);

			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			AssertNoMessageError(orgCusAccount2.CZ_TypeInfo, message);
		}

		public void TestCheckOnlyOneCoupleCZ_TypeCZ_CodeIsAuthorized()
		{
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			ValidationTestHelper.AssertErrorIfInvalidCode(orgCusAccount.CZ_TypeInfo, "~", OrgCusAccountDeltaTTypeList.Codes.TR);
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			ValidationTestHelper.AssertErrorIfInvalidCode(orgCusAccount.CZ_TypeInfo, "~", OrgCusAccountDeltaGTypeList.Codes.G1);
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			ValidationTestHelper.AssertErrorIfInvalidCode(orgCusAccount.CZ_TypeInfo, "~", OrgCusAccountDeltaGTypeList.Codes.G1);

			AssertMandatoryValidationError(orgCusAccount.CZ_TypeInfo, false);
			orgCusAccount.CZ_Type = ZString.Empty;
			AssertMandatoryValidationError(orgCusAccount.CZ_TypeInfo, true);
		}

		public void TestCheckCZ_RepresentativeID()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(orgCusAccount.CZ_RepresentativeIDInfo);
		}

		protected override void SetUp()
		{
			orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgCusAccount.CZ_OH = orgHeader.PK;
		}
		OrgCusAccount orgCusAccount;
		OrgHeader orgHeader;
	}
}
