using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ZArchitecture.GUI.ZDropEdit;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestEntryInstructionGuidDropEdit()
		{
			var entryDropEdt = control.EntryInstructionGuidDropEdit;
			AssertType<ZGuidDropEdit>(entryDropEdt);
			AssertEquals("ShowInDropDown", ShowInDropDownList.OnlyShowCode, entryDropEdt.ShowInDropDown);
			AssertEquals("ShowDescriptionBox", false, entryDropEdt.ShowDescriptionBox);
			AssertEquals("UseFullWidthForCodeBox", true, entryDropEdt.UseFullWidthForCodeBox);
		}

		public void TestObservationsTextBox()
		{
			AssertType<LongTextControl>(control.ObservationsTextBox);
		}

		public void TestVehicleUserControl()
		{
			AssertType<VehicleDetailsUserControl>(control.VehicleDetailsUserControl);
		}

		InvoiceLineDetailsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
