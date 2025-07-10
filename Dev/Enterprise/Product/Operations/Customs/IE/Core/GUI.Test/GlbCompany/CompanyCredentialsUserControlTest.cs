using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class CompanyCredentialsUserControlTest : TestCaseWithFactory
	{
		public void TestCompanyCredentialsDetailsUserControl()
		{
			using (var control = new CompanyCredentialsUserControl())
			{
				AssertType<CompanyCredentialsDetailsUserControl>(control.CompanyCredentialsDetailsUserControl);
			}
		}
	}
}
