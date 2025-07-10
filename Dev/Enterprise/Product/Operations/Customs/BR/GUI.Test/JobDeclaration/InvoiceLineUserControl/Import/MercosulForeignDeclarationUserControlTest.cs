using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class MercosulForeignDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestColumnsForMercosulForeignDeclarationImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.MercosulForeignDeclarationTabPage;
					var mercosulForeignDeclaration = invoiceLineUserControl.MercosulForeignDeclarationUserControl;
					var mercosulForeignGrid = mercosulForeignDeclaration.MercosulForeignDeclarationGrid;

					AssertEquals(2, mercosulForeignGrid.Columns.Count);
					Assert("Should have CSI_Code", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Code));
					Assert("Should have CSI_Quantity3", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Quantity3));
				}
			}
		}

		public void TestColumnsForMercosulForeignDeclarationImportSiscomex()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportSiscomexInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.MercosulForeignDeclarationTabPage;
					var mercosulForeignDeclaration = invoiceLineUserControl.MercosulForeignDeclarationUserControl;
					var mercosulForeignGrid = mercosulForeignDeclaration.MercosulForeignDeclarationGrid;

					AssertEquals(7, mercosulForeignGrid.Columns.Count);
					Assert("Should have CSI_Description", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Description));
					Assert("Should have CSI_ReferenceNumber", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber));
					Assert("Should have CSI_ReferenceNumber2", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber2));
					Assert("Should have CSI_RN_NKCountryCode", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_RN_NKCountryCode));
					Assert("Should have CSI_Code", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Code));
					Assert("Should have CSI_ItemNumber", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ItemNumber));
					Assert("Should have CSI_Quantity3", mercosulForeignGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Quantity3));
				}
			}
		}
	}
}
