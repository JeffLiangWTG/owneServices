using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class InvoiceLineChargesUserControlTest : TestCaseWithFactory
	{
		public void TestChangeGSTApplicableText()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (CAImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.LineChargesTabPage.Select();
				var userControl = (InvoiceLineChargesUserControl)invoiceLineUserControl.LineChargesTabPage.Controls.Find("InvoiceLineCharges", true)[0];
				AssertEquals("Line Charges grid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, userControl.ChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name).Caption);
				AssertEquals("Line Apportioned Charges grid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, userControl.ApportionedChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name).Caption);
			}
		}
	}
}
