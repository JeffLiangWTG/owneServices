using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class CommercialInvoiceEDIMenuTest : TestCaseWithFactory
	{
		public void TestImportNFEMenuItem()
		{
			using (var commercialInvoiceEDIMenu = CreateCommercialInvoiceEDIMenu())
			{
				var importNfeMenuItem = commercialInvoiceEDIMenu.MenuItems.FindByText("Import NF-e");
				AssertNotNull("There should be a Import NF-e menu", importNfeMenuItem);

				commercialInvoiceEDIMenu.InvoiceHeader.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
				commercialInvoiceEDIMenu.RefreshMenu();
				AssertEquals("Import NF-e menu should be visible for export declarations.", true, importNfeMenuItem.Visible);

				commercialInvoiceEDIMenu.InvoiceHeader.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
				commercialInvoiceEDIMenu.RefreshMenu();
				AssertEquals("Import NF-e menu should be invisible for export declarations.", false, importNfeMenuItem.Visible);
			}
		}

		public void TestImportNfeMenuItem_Click()
		{
			using (var form = new ZForm())
			using (var menu = CreateCommercialInvoiceEDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();

				var importNfeMenuItem = menu.MenuItems.FindByText("Import NF-e");
				AssertNotNull(importNfeMenuItem);
				AssertEquals(true, importNfeMenuItem.Visible);

				menu.InvoiceHeader.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
				importNfeMenuItem.PerformClick();
				AssertNull("Nothing happened for Import", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Nothing happened for Import", ZFormModaliser.LastFormShownDialogForTest);

				menu.InvoiceHeader.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
				importNfeMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType<NFEImportForm>("NFEImportForm popped up", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		CommercialInvoiceEDIMenu CreateCommercialInvoiceEDIMenu()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var declaration = new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData as JobDeclaration;

			return new CommercialInvoiceEDIMenu { Declaration = declaration };
		}
	}
}
