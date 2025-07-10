using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class GoodsItemDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
	}

	public void TestCustomsStatusUserControl()
	{
		var customsStatusUserControl = control.CustomsStatusUserControl;
		AssertType<GoodsItemCustomsStatusUserControl>("Status", customsStatusUserControl);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new GoodsItemDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	GoodsItemDetailsUserControl control;
}
