using CargoWise.EntityFramework.Testing;

namespace ZClientEDI.Business.Test
{
	public class WTGActiveDirectoryCredentialsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDomainName()
		{
			var credentials = new WTGActiveDirectoryCredentials();
			AssertNoErrors(credentials.DomainNameInfo);
			credentials.IsEnabled = true;
			credentials.Validation.ValidateDomainName();
			AssertHasError(credentials.DomainNameInfo, "Please enter a value.");

			credentials.DomainName = "fake.domain";
			AssertNoErrors(credentials.DomainNameInfo);
		}

		public void TestValidateDomainUserName()
		{
			var credentials = new WTGActiveDirectoryCredentials();
			AssertNoErrors(credentials.DomainUserNameInfo);
			credentials.IsEnabled = true;
			credentials.Validation.ValidateDomainUserName();
			AssertHasError(credentials.DomainUserNameInfo, "Please enter a value.");

			credentials.DomainUserName = "Dexter";
			AssertNoErrors(credentials.DomainUserNameInfo);
		}

		public void TestValidateDomainPassword()
		{
			var credentials = new WTGActiveDirectoryCredentials();
			AssertNoErrors(credentials.DomainUserPasswordInfo);
			credentials.IsEnabled = true;
			credentials.Validation.ValidateDomainUserPassword();
			AssertHasError(credentials.DomainUserPasswordInfo, "Please enter a value.");

			credentials.DomainUserPassword = "DasIstEinPasswort";
			AssertNoErrors(credentials.DomainUserPasswordInfo);
		}

		public void TestValidateOrganizationalUnitPath()
		{
			var credentials = new WTGActiveDirectoryCredentials();
			credentials.IsEnabled = true;
			credentials.Validation.ValidateOrganizationalUnitPath();
			AssertHasError(credentials.OrganizationalUnitPathInfo, "Please enter a value.");

			credentials.OrganizationalUnitPath = "root/Accounts/Token Based Authentication";
			AssertNoErrors(credentials.OrganizationalUnitPathInfo);
		}
	}
}
