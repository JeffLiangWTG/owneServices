using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class CompanyBrokerageUserControlTest : TestCaseWithFactory
{
	public void TestCompanyBrokerageDetailsUserControl()
	{
		using var control = new CompanyCredentialsUserControl();
		AssertType<CompanyCredentialsDetailsUserControl>(control.CompanyCredentialsDetailsUserControl);
	}
}
