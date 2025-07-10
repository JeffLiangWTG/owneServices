using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		public void TestHandleInvoiceLineUserControlVisibilityChangedCore()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertEquals(typeof(ImportInvoiceLineUserControl), form.InvoiceLineUserControl.GetType());

				form.MainTabControl.SelectedTab = form.HeaderTabPage;
				Application.DoEvents();
				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertEquals(typeof(ExportInvoiceLineUserControl), form.InvoiceLineUserControl.GetType());

				form.MainTabControl.SelectedTab = form.HeaderTabPage;
				Application.DoEvents();
				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertEquals(typeof(ImportSiscomexInvoiceLineUserControl), form.InvoiceLineUserControl.GetType());

				form.MainTabControl.SelectedTab = form.HeaderTabPage;
				Application.DoEvents();
				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.LPCO;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertEquals(typeof(ExportInvoiceLineUserControl), form.InvoiceLineUserControl.GetType());
			}
		}

		public void TestEntryInstructionDropEditVisibility()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();

				var dynamicLineDetailsPanel = form.InvoiceLineUserControl.FindSingle<DynamicLayoutPanel>("DynamicLineDetailsPanel");
				var entryInstructionGuidDropEdit = dynamicLineDetailsPanel.FindSingle<ZDropEdit>("EntryInstructionGuidDropEdit");

				AssertEquals("EntryInstructionGuidDropEdit visibility for export", false, entryInstructionGuidDropEdit.Visible);

				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals("EntryInstructionGuidDropEdit visibility for import", false, entryInstructionGuidDropEdit.Visible);
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsVisibility()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();

				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.InvoiceLineUserControl))
				{
					AssertEquals("CustomsInvoiceLinesBoundGrid Shouldn't have JI_CEI column for export", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_CEI));
					AssertEquals("CustomsInvoiceLinesBoundGrid Shouldn't have JI_JustificationInfoExport column for export ", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_ExportJustificationInfo));
				}

				invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.InvoiceLineUserControl))
				{
					AssertEquals("CustomsInvoiceLinesBoundGrid Shouldn't have JI_CEI column for import", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_CEI));
					AssertEquals("CustomsInvoiceLinesBoundGrid Shouldn't have JI_JustificationInfoExport column for import ", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_ExportJustificationInfo));
				}
			}
		}

		public void TestGetNewTopLevelMenuCore()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration1.Invoices.AddNew();
			using (var commercialInvoiceEDIMenu1 = new CommercialInvoiceEDIMenu { Declaration = declaration1 })
			{
				var importNfeMenuItem1 = commercialInvoiceEDIMenu1.MenuItems.Cast<MenuItem>().FirstOrDefault(menuItem => menuItem.Text == "Import NF-e");
				AssertNotNull("There should be a Import NF-e menu", importNfeMenuItem1);
				commercialInvoiceEDIMenu1.RefreshMenu();
				AssertEquals("Import NF-e menu should be invisible for import declarations.", false, importNfeMenuItem1.Visible);
			}

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration2.Invoices.AddNew();
			using (var commercialInvoiceEDIMenu2 = new CommercialInvoiceEDIMenu { Declaration = declaration2 })
			{
				var importNfeMenuItem2 = commercialInvoiceEDIMenu2.MenuItems.Cast<MenuItem>().FirstOrDefault(menuItem => menuItem.Text == "Import NF-e");
				AssertNotNull("There should be a Import NF-e menu", importNfeMenuItem2);
				commercialInvoiceEDIMenu2.RefreshMenu();
				AssertEquals("Import NF-e menu should be visible for export declarations.", true, importNfeMenuItem2.Visible);
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());
	}
}
