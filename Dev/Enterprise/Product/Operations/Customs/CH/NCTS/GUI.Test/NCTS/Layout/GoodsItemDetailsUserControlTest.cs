using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class GoodsItemDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType() => AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);

	public void TestHarmonisedTariffFindBox()
	{
		AssertType<NctsTariffFindBox>(control.HarmonisedTariffFindBox);
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
