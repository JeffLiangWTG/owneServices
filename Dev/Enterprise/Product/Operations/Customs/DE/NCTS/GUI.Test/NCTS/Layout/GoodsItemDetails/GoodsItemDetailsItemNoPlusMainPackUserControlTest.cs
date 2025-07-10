using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	sealed class GoodsItemDetailsItemNoPlusMainPackUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals("Test data source", typeof(NctsDepartureCargoDesc), goodsItemDetailsItemNoPlusMainPackUserControl.BindingSource.DataSourceType);
		}

		public void TestItemNumberTextBox()
		{
			AssertType<ZTextBox>(goodsItemDetailsItemNoPlusMainPackUserControl.ItemNumberTextBox);
		}

		public void TestIsMainPackCheckBox()
		{
			AssertType<ZCheckBox>(goodsItemDetailsItemNoPlusMainPackUserControl.IsMainPackCheckBox);
		}

		public void TestItemNumberTextBoxBinding()
		{
			AssertEquals("ItemNo is bound to BY_LineNo", nameof(NctsDepartureCargoDesc.BY_LineNo), goodsItemDetailsItemNoPlusMainPackUserControl.ItemNumberTextBox.GetBindingMember());
		}

		public void TestIsMainPackCheckBoxBinding()
		{
			AssertEquals("Checkbox is bound to BY_IsMainPack", nameof(NctsDepartureCargoDesc.BY_IsMainPack), goodsItemDetailsItemNoPlusMainPackUserControl.IsMainPackCheckBox.GetBindingMember());
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItemDetailsItemNoPlusMainPackUserControl = new GoodsItemDetailsItemNoPlusMainPackUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			goodsItemDetailsItemNoPlusMainPackUserControl.Dispose();
		}
		GoodsItemDetailsItemNoPlusMainPackUserControl goodsItemDetailsItemNoPlusMainPackUserControl;
	}
}
