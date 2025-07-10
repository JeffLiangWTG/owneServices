using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class InvoiceLineCopyDocumentsUserControlTest : TestCase
	{
		public void TestInvoiceNumberDropEdit()
		{
			CombineAssertions(() =>
			{
				using var control = new InvoiceLineCopyDocumentsUserControl();
				control.InvoiceNumberDropEditGroupBox.Controls.Contains(control.InvoiceNumberDropEdit);
				AssertEquals("Caption", "Copy to lines of Invoice:", control.InvoiceNumberDropEditGroupBox.CaptionResourceString.Caption);
				AssertEquals("No description box", expected: false, control.InvoiceNumberDropEdit.ShowDescriptionBox);
				AssertEquals("No resizing", expected: false, control.InvoiceNumberDropEdit.ShouldResizeByMaxLength);
				AssertEquals("PreBoundMaxLength", 35, control.InvoiceNumberDropEdit.PreBoundMaxLength);
				AssertEquals("Lookup", "Lookups.Invoices", control.InvoiceNumberDropEdit.BindToList);
				AssertEquals("Binding", "InvoiceNumber", control.BindingSource.GetBindingMember(control.InvoiceNumberDropEdit));
			});
		}
	}
}
