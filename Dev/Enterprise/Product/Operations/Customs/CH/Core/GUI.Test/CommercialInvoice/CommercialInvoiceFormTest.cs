using Enterprise.Customs.CH.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CommercialInvoiceForm))]
sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
{
	public void TestInvoiceLinesUserControl()
	{
		var invoice = Factory.New<JobComInvoiceHeader>();
		using (var testForm = new CommercialInvoiceForm(invoice))
		{
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testForm.LoadLinesTabPage();
			AssertEquals(typeof(ExportInvoiceLineUserControl), testForm.InvoiceLineUserControl.GetType());

			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testForm.LoadLinesTabPage();
			AssertEquals(typeof(ImportInvoiceLineUserControl), testForm.InvoiceLineUserControl.GetType());
		}
	}

	protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());
}
