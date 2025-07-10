using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5GoodsItemDifferencesDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsArrivalCargoDesc), userControl.BindingSource.DataSourceType);
		}

		public void TestSequenceNo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.BY_LineNo), userControl.SequenceNumberTextBox.BindTo);
				AssertEquals("Visible", true, userControl.SequenceNumberTextBox.Visible);
			});
		}

		public void TestItemNo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.BY_DeclarationGoodsItemNumber), userControl.ItemNumberTextBox.BindTo);
				AssertEquals("Visible", true, userControl.ItemNumberTextBox.Visible);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemDifferencesDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		Phase5GoodsItemDifferencesDetailsUserControl userControl;
	}
}
