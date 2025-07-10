using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CAExportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestGridLayoutContext()
		{
			using (CAExportSupplierHeaderUserControl control = new CAExportSupplierHeaderUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestColumnNamesInSortOrder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CustomsBrokerageUserControl brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				CAExportSupplierHeaderUserControl invoiceControl = (CAExportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
				ZGrid grid = invoiceControl.JobComInvoiceHeadersBoundGrid.InnerGrid;
				grid.ResetColumns();
				Assert("invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.Count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= grid.Columns.Count);
				for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
				{
					ZGridColumn column = grid.Columns[i];
					AssertNotNull(column);
					ZString expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			}
		}

		public void TestColumnWidths()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var found = false;
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				if (brokerageControl != null)
				{
					found = true;
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
					var invoiceControl = (CAExportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
					var grid = invoiceControl.JobComInvoiceHeadersBoundGrid.InnerGrid;
					grid.ResetColumns();
					AssertEquals("JobComInvoiceHeader.Schema.JZ_InvoiceNumber", 110, grid.GetColumnWidth(JobComInvoiceHeader.Schema.JZ_InvoiceNumber));
					AssertEquals("JobComInvoiceHeader.Schema.JZ_IncoTerm", 60, grid.GetColumnWidth(JobComInvoiceHeader.Schema.JZ_IncoTerm));
					AssertEquals("JobComInvoiceHeader.Schema.JZ_InvoiceAmount", 100, grid.GetColumnWidth(JobComInvoiceHeader.Schema.JZ_InvoiceAmount));
					AssertEquals("JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin", 100, grid.GetColumnWidth(JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin));
					AssertEquals("JobComInvoiceHeader.Schema.JZ_RW_NKOriginState", 100, grid.GetColumnWidth(JobComInvoiceHeader.Schema.JZ_RW_NKOriginState));
				}
				Assert("The form has no Brokerage Control", found);
			}
		}

		public void TestInvoiceHeadersGridAvailability()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CustomsBrokerageUserControl brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				CAExportSupplierHeaderUserControl invoiceControl = (CAExportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
				AssertColumnIsNotAvailable(JobComInvoiceHeader.Schema.JZ_OH_Supplier, invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles);
				AssertColumnIsNotAvailable(JobComInvoiceHeader.Schema.SupplierName, invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles);
				AssertColumnIsNotAvailable(JobComInvoiceHeader.Schema.JZ_OH_Buyer, invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles);
				AssertColumnIsNotAvailable(JobComInvoiceHeader.Schema.JZ_InvoiceCurrLandedCostExRate, invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles);
				AssertColumnIsNotAvailable(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice, invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles);
				AssertColumnIsNotAvailable(JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill, invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles);
				AssertColumnIsNotAvailable(JobComInvoiceHeader.Schema.JZ_NetWeight, invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles);
				AssertColumnIsNotAvailable(JobComInvoiceHeader.Schema.JZ_NetWeightUQ, invoiceControl.JobComInvoiceHeadersBoundGrid.ColumnStyles);
			}
		}

		void AssertColumnIsNotAvailable(string columnName, System.Collections.ArrayList arrayList)
		{
			bool found = false;
			foreach (ZGridColumnInfo columnInfo in arrayList)
			{
				if (columnInfo.ColumnName == columnName)
				{
					found = true;
					AssertEquals(columnName + "'s IsUnavailable", true, columnInfo.IsUnavailable);
				}
			}
			AssertEquals(columnName + " was not found", true, found);
		}

		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (fExpectedColumnNamesInSortOrderList == null)
				{
					fExpectedColumnNamesInSortOrderList = new List<ZString>();
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceNumber);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_IncoTerm);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_IncoTermPlace);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceAmount);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.InvoiceLineTotal);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_RW_NKOriginState);
					fExpectedColumnNamesInSortOrderList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceDate);
				}
				return fExpectedColumnNamesInSortOrderList;
			}
		}
		List<ZString> fExpectedColumnNamesInSortOrderList;
	}
}
