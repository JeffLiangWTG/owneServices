using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class InvoiceLineDetailsAdditionalInfoUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new InvoiceLineDetailsAdditionalInfoUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobDeclaration), control.DataSourceType);
			}
		}

		public void TestBRJustificationExportIteVisibility_JobDeclarationForm()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRAdditionalInformationTab;
				AssertEquals("BRJustificationExportTextBox should be visible", true, invoiceLineUserControl.BRAdditionalInformationUserControl.BRJustificationExportTextBox.Visible);
			}
		}

		public void TestBRJustificationExportGroupBoxVisibility_CommercialInvoiceForm()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
				form.MainTabControl.SelectedTab = form.LinesTabPage;

				var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.InvoiceLineUserControl;
				form.InvoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRAdditionalInformationTab;
				AssertEquals("BRJustificationExportTextBox should be visible", false, invoiceLineUserControl.BRAdditionalInformationUserControl.BRJustificationExportTextBox.Visible);
			}
		}
	}
}
