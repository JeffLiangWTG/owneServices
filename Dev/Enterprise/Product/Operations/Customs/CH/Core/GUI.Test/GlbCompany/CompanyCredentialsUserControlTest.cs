using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.GUI.Testing;

sealed class CompanyCredentialsUserControlTest : TestCaseWithFactory
{
	public void TestCompanyBrokerageDetailsUserControl()
	{
		AssertType<CompanyCredentialsDetailsUserControl>(control.CompanyCredentialsDetailsUserControl);
	}

	CompanyCredentialsUserControl control;
	protected override void SetUp()
	{
		base.SetUp();
		control = new CompanyCredentialsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
