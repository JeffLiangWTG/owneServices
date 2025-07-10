using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class BaseInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestTariffColumnNotExists_UniversalTariffFalse()
		{
			using (var control = new BaseInvoiceLineUserControl())
			{
				AssertNull(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsVisibility_JobDeclarationForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var grid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
				var columns = grid.Columns;
				AssertEquals("CustomsInvoiceLinesBoundGrid should have JI_CEI column", true, columns.Contains(JobComInvoiceLine.Schema.JI_CEI));
				AssertEquals("JI_CEI column should only show code", ZDropEdit.ShowInDropDownList.OnlyShowCode, (grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI) as ZGuidDropEditColumnStyleInfo).ShowInDropDown);
				AssertEquals("CustomsInvoiceLinesBoundGrid should have Observations column", true, columns.Contains(JobComInvoiceLine.Schema.Observations));
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsVisibility_CommercialInvoiceForm()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				AssertEquals("CustomsInvoiceLinesBoundGrid Shouldn't have JI_CEI column", false, form.InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_CEI));
				AssertEquals("CustomsInvoiceLinesBoundGrid Should have JI_Calc_MergedLineNumber column", true, form.InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber));
				AssertEquals("CustomsInvoiceLinesBoundGrid Shouldn't have Observations column", true, form.InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.Observations));
			}
		}
		public void TestTabPages()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.IdentifiersTabPage;
				Assert(invoiceLineUserControl.IdentifiersUserControl.Visible);
				AssertType<IdentifiersUserControl>(invoiceLineUserControl.IdentifiersUserControl);
			}
		}

		public abstract class BaseInvoiceLineUserControlAbstractTest : TestCaseWithFactory
		{
			public void TestCustomsInvoiceLinesBoundGridColumnsOrder()
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageType;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
					customsBrokerageUserControl.InvoiceGroupingTabPage.TabVisible = true;
					customsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					var customsInvoiceLinesBoundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					customsInvoiceLinesBoundGrid.ResetColumns();
					AssertContainsExactElementsInExactOrder(ExpectedColumnNamesListInOrder, customsInvoiceLinesBoundGrid.Columns.Where(c => c.IsVisible).Select(c => c.ColumnStyle.MappingName));
				}
			}

			protected abstract string JobMessageType { get; }

			protected abstract List<ZString> ExpectedColumnNamesListInOrder { get; }
		}
	}
}
