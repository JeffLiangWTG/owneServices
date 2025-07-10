using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class LinkedDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new LinkedDocumentUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobDeclaration), control.DataSourceType);
			}
		}

		public void TestColumnsForLinkedDocumentImportSiscomex()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				using (var invoiceLineUserControl = (ImportSiscomexInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LinkedDocumentTab;
					var linkedDocument = invoiceLineUserControl.LinkedDocumentUserControl;
					var linkedGrid = linkedDocument.LinkedDocumentGrid;

					AssertEquals(2, linkedGrid.Columns.Count);
					Assert("Should have CSI_Code", linkedGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Code));
					Assert("Should have CSI_ReferenceNumber", linkedGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber));
				}
			}
		}

		public void TestColumnsForLinkedDocumentImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				using (var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LinkedDocumentTab;
					var linkedDocument = invoiceLineUserControl.LinkedDocumentUserControl;
					var linkedGrid = linkedDocument.LinkedDocumentGrid;

					AssertEquals(3, linkedGrid.Columns.Count);
					Assert("Should have CSI_Code", linkedGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Code));
					Assert("Should have CSI_ReferenceNumber", linkedGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber));
					Assert("Should have CSI_ItemNumber", linkedGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ItemNumber));
				}
			}
		}
	}
}
