using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportLicenseManufacturerDetailsTest : TestCaseWithFactory
	{
		public void TestComponentsAreVisible()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = ((ImportLicenseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.ManufacturerDetailsTab;
				Assert(invoiceLineUserControl.ManufacturerDetailsUserControl.ManufacturerIndicatorDropEdit.Visible);
				Assert(invoiceLineUserControl.ManufacturerDetailsUserControl.ManufacturerContactDetailDocAddress.Visible);
				Assert(invoiceLineUserControl.ManufacturerDetailsUserControl.JI_CountryOfOriginBoundFindBox.Visible);
			}
		}
	}
}
