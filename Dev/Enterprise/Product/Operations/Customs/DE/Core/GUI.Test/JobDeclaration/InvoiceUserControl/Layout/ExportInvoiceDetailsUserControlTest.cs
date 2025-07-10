using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.GUI.Declaration.Testing
{
	class ExportInvoiceDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestFreeOfChargeCheckBox()
		{
			AssertType(typeof(ZArchitecture.GUI.ZCheckBox), control.FreeOfChargeCheckBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ExportInvoiceDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ExportInvoiceDetailsUserControl control;
	}
}
