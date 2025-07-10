using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GUI.Testing
{
	sealed class CompanyCredentialsUserControlTest : TestCaseWithFactory
	{
		public void TestCredentialsDetailsUserControl()
		{
			using var control = new CompanyCredentialsUserControl();
			AssertType<CredentialsDetailsUserControl>(control.CredentialsDetailsUserControl);
		}
	}
}
