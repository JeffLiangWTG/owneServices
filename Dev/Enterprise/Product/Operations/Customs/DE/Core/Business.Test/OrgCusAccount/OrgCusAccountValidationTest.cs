using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class OrgCusAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCZ_Account()
		{
			CombineAssertions(() =>
			{
				orgCusAccount.CZ_Account = ZString.Empty.PadRight(5, '9');
				AssertHasMessageError(orgCusAccount.CZ_AccountInfo, "Entered Account must have 6 numeric digits");
				orgCusAccount.CZ_Account = ZString.Empty.PadRight(6, 'X');
				AssertHasMessageError(orgCusAccount.CZ_AccountInfo, "Entered Account must have 6 numeric digits");
				orgCusAccount.CZ_Account = ZString.Empty.PadRight(6, '9');
				AssertNoMessageErrors(orgCusAccount.CZ_AccountInfo);
			});
		}

		public void TestCheckDecryptedPassword()
		{
			CombineAssertions(() =>
			{
				orgCusAccount.DecryptedPassword = ZString.Empty.PadRight(24, '9');
				AssertHasMessageError(orgCusAccount.DecryptedPasswordInfo, "Entered BIN must have 25 numeric digits");
				orgCusAccount.DecryptedPassword = ZString.Empty.PadRight(25, 'X');
				AssertHasMessageError(orgCusAccount.DecryptedPasswordInfo, "Entered BIN must have 25 numeric digits");
				orgCusAccount.DecryptedPassword = ZString.Empty.PadRight(25, '9');
				AssertNoMessageErrors(orgCusAccount.DecryptedPasswordInfo);
			});
		}

		public void TestCheckCZ_Issuer()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(orgCusAccount.CZ_IssuerInfo, "~", OrgCusAccountIssuerList.Codes.Berlin);
		}

		#region CheckCZ_Code
		public void TestCheckCZ_Code()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(orgCusAccount.CZ_CodeInfo, "~", OrgCusAccountCodeList.Codes.ImportDutiesOneMonth);
		}

		public void TestCheckCZ_CodeDuplicate()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgCusAccount1 = orgHeader.DefermentAccountNumberCollection.AddNew();
			var orgCusAccount2 = orgHeader.DefermentAccountNumberCollection.AddNew();
			orgCusAccount2.CZ_Code = orgCusAccount1.CZ_Code = orgCusAccount1.Lookups.CodeList[0].Code;
			AssertHasMessageError(orgCusAccount2.CZ_CodeInfo, "Account is already present");
		}

		public void TestCheckCZ_Code_Eori()
		{
			const string errorMessage = "A Registration Number/Code of Type 'EOR' is required for Deferment Accounts";
			var orgHeader = orgCusAccount.Header;

			CombineAssertions(() =>
			{
				orgCusAccount.CZ_OH = ZGuid.Empty;
				orgCusAccount.Validation.ValidateCZ_Code();
				AssertNoMessageError("No orgHeader", orgCusAccount.CZ_CodeInfo, errorMessage);

				orgCusAccount.CZ_OH = orgHeader.PK;
				orgCusAccount.Validation.ValidateCZ_Code();
				AssertHasMessageError("No registrations", orgCusAccount.CZ_CodeInfo, errorMessage);

				var orgCusCode1 = orgHeader.CustomsCodes.AddNew();
				orgCusCode1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
				orgCusAccount.Validation.ValidateCZ_Code();
				AssertHasMessageError("No EOR registration", orgCusAccount.CZ_CodeInfo, errorMessage);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Greece);
				orgCusAccount.Validation.ValidateCZ_Code();
				AssertNoMessageError("Has EOR registration", orgCusAccount.CZ_CodeInfo, errorMessage);
			});
		}
		#endregion

		public void TestCheckCZ_Type()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(orgCusAccount.CZ_TypeInfo, "~", OrgCusAccountTypeList.Codes.E);
		}

		protected override void SetUp()
		{
			orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		}
		OrgCusAccount orgCusAccount;
	}
}
