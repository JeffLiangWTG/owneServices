namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.MasterFiles.Business;

	internal class EdiCustomerUserAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateEUA_IsEmailVerificationRequired()
		{
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			var contact = Factory.NewWithValidTestData<OrgContact>();

			userAccount.EUA_IsEmailVerificationRequired = true;
			AssertEquals("Precondition", true, userAccount.EUA_IsEmailVerificationRequired);
			AssertNull(userAccount.WebAccessContact);
			AssertNoErrors("Should have no errors since no web access contact exists", userAccount.EUA_IsEmailVerificationRequiredInfo);

			userAccount.EUA_OC_WebAccessContact = contact.PK;
			userAccount.EUA_IsEmailVerificationRequired = true;
			AssertEquals("Precondition", true, userAccount.EUA_IsEmailVerificationRequired);
			AssertNotNull(userAccount.WebAccessContact);
			AssertHasError("Should have error since web access contact exists", userAccount.EUA_IsEmailVerificationRequiredInfo, "User Accounts with a linked Web Access Contact should not require Email Verification");
		}

		public void TestCountryValidation()
		{
			var acc = Factory.New<EdiCustomerUserAccount>();
			acc.EUA_FullName = "name";
			acc.EUA_Email = "email@gmail.com";
			acc.EUA_LD = Factory.New<LicenceDatabase>().PK;
			acc.Validation.ValidateAll();

			AssertNoErrors(acc);

			acc.EUA_RN_NKCountry = "99";
			AssertHasErrors(acc.EUA_RN_NKCountryInfo);

			acc.EUA_RN_NKCountry = "AU";
			AssertNoErrors(acc.EUA_RN_NKCountryInfo);
		}
	}
}
