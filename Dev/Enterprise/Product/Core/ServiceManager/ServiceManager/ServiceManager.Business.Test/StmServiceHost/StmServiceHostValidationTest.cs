using CargoWise.EntityFramework.Testing;

namespace Enterprise.ServiceManager.Business.Testing
{
	class StmServiceHostValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSH_ProxyPort()
		{
			var host = Factory.New<StmServiceHost>();
			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyPort = 0;
			host.Validation.ValidateSH_ProxyPort();
			AssertNoErrors(host.SH_ProxyPortInfo);

			host.SH_ProxyHost = "some.host";
			host.Validation.ValidateSH_ProxyPort();
			AssertHasErrors(host.SH_ProxyPortInfo);

			host.SH_ProxyPort = 65535;
			AssertNoErrors(host.SH_ProxyPortInfo);

			host.SH_ProxyPort = 65536;
			AssertHasErrors(host.SH_ProxyPortInfo);

			host.SH_ProxyPort = 1;
			AssertNoErrors(host.SH_ProxyPortInfo);
		}

		public void TestSH_ProxyUserName()
		{
			var host = Factory.New<StmServiceHost>();

			host.SH_ProxyAutoDetect = true;
			host.SH_ProxyAuthentication = true;
			host.Validation.ValidateSH_ProxyUserName();
			AssertNoErrors(host.SH_ProxyUserNameInfo);

			host.SH_ProxyAuthentication = false;
			host.Validation.ValidateSH_ProxyUserName();
			AssertNoErrors(host.SH_ProxyUserNameInfo);

			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyAuthentication = false;
			host.Validation.ValidateSH_ProxyUserName();
			AssertNoErrors(host.SH_ProxyUserNameInfo);

			host.SH_ProxyAuthentication = true;
			host.Validation.ValidateSH_ProxyUserName();
			AssertHasErrors(host.SH_ProxyUserNameInfo);

			host.SH_ProxyUserName = "some.host";
			AssertNoErrors(host.SH_ProxyUserNameInfo);
		}

		public void TestSH_ProxyPassword()
		{
			var host = Factory.New<StmServiceHost>();

			host.SH_ProxyAutoDetect = true;
			host.SH_ProxyAuthentication = true;
			host.Validation.ValidateSH_ProxyPassword();
			AssertNoErrors(host.SH_ProxyPasswordInfo);

			host.SH_ProxyAuthentication = false;
			host.Validation.ValidateSH_ProxyPassword();
			AssertNoErrors(host.SH_ProxyPasswordInfo);

			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyAuthentication = false;
			host.Validation.ValidateSH_ProxyPassword();
			AssertNoErrors(host.SH_ProxyPasswordInfo);

			host.SH_ProxyAuthentication = true;
			host.Validation.ValidateSH_ProxyPassword();
			AssertHasErrors(host.SH_ProxyPasswordInfo);

			host.SH_ProxyPassword = "some password";
			AssertNoErrors(host.SH_ProxyPasswordInfo);
		}
	}
}