using CargoWise.EntityFramework.Testing;

namespace Enterprise.ServiceManager.Business.Testing
{
	class ServiceInstallInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckConfirmPassword()
		{
			var info = new ServiceInstallInfo(Factory);
			info.Password = "123";
			info.ConfirmPassword = "1234";
			AssertHasError(info.ConfirmPasswordInfo, "Passwords mismatch.");

			info.ConfirmPassword = "123";
			AssertNoErrors(info.ConfirmPasswordInfo);
		}
	}
}