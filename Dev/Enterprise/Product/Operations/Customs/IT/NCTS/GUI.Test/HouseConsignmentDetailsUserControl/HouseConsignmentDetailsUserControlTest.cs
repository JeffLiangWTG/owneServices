using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class HouseConsignmentDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsBill), control.BindingSource.DataSourceType);
	}

	public void TestCustomsStatusUserControl()
	{
		var customsStatusUserControl = control.CustomsStatusUserControl;
		AssertType<HouseConsignmentCustomsStatusUserControl>("Status", customsStatusUserControl);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new HouseConsignmentDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	HouseConsignmentDetailsUserControl control;
}
