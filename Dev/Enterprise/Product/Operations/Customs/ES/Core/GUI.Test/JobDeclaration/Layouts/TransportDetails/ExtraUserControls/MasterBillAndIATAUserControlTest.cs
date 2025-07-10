using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class MasterBillAndIATAUserControlTest : TestCaseWithFactory
	{
		public void TestMasterBillTextBox()
		{
			var masterBillTextBox = control.MasterBillTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), masterBillTextBox.Location);
				AssertEquals("Tab", 0, masterBillTextBox.TabIndex);
				AssertEquals("Binding", "JE_MasterBill", masterBillTextBox.BindTo);
			});
		}
		public void TestIATALoadPortCodeFindBox()
		{
			var iataLoadPortCodeFindBox = control.IATALoadPortCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 0, true), iataLoadPortCodeFindBox.Location);
				AssertEquals("Tab", 1, iataLoadPortCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_IATALoadPort", iataLoadPortCodeFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new MasterBillAndIATAUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		MasterBillAndIATAUserControl control;
	}
}
