using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AR.Manifest.GUI.Testing
{
	sealed class CompanyCredentialsUserControlTest : TestCaseWithFactory
	{
		public void TestCompanyCredentialsDetailsUserControl()
		{
			using var control = new CompanyCredentialsUserControl();
			AssertType<CompanyCredentialsDetailsUserControl>(control.CompanyCredentialsDetailsUserControl);
		}
	}
}
