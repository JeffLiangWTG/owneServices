using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	sealed class GoodsItemDetailsLayoutsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals("GoodsItemDetailsLayoutsUserControl test data source", typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestGoodsItemDetailsItemNoPlusMainPackUserControl()
		{
			AssertType<GoodsItemDetailsItemNoPlusMainPackUserControl>(control.GoodsItemDetailsItemNoPlusMainPackUserControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new GoodsItemDetailsLayoutsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		GoodsItemDetailsLayoutsUserControl control;
	}
}

