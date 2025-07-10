using System.Collections.Generic;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(BaseCustomsSupplierHeaderUserControl))]
sealed class BaseCustomsSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<BaseCustomsSupplierHeaderUserControl, JobDeclaration>
{
	protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

	public void TestAdditionalDetails()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceType = "B";
		invoice.JZ_ElectronicInvoiceReceiptNumber = "ABC123";

		using var form = new JobDeclarationForm(declaration);
		form.Show();

		var invoicesTabPage = form.CustomsBrokerageUserControl.InvoicesTabPage;
		form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = invoicesTabPage;

		var invoiceTabControl = invoicesTabPage.FindSingle<ZTemplateTabControl>("InvoiceTabControl");
		var additionalDetailsTabPage = invoiceTabControl.FindSingle<ZTabPage>("AdditionalDetailsTabPage");
		invoiceTabControl.SelectedTab = additionalDetailsTabPage;

		CombineAssertions(() =>
		{
			var invoiceTypeDropEdit = additionalDetailsTabPage.FindSingle<ZDropEdit>("InvoiceTypeDropEdit");
			Assert("InvoiceTypeDropEdit Visible", invoiceTypeDropEdit.Visible);
			AssertEquals("InvoiceTypeDropEdit Text", "B", invoiceTypeDropEdit.Text);
			var electronicInvoiceReceptionNumberTextBox = additionalDetailsTabPage.FindSingle<ZTextBox>("ElectronicInvoiceReceiptNumberTextBox");
			Assert("ElectronicInvoiceReceiptNumberTextBox Visible", electronicInvoiceReceptionNumberTextBox.Visible);
			AssertEquals("ElectronicInvoiceReceiptNumberTextBox Text", "ABC123", electronicInvoiceReceptionNumberTextBox.Text);
		});
	}
}

