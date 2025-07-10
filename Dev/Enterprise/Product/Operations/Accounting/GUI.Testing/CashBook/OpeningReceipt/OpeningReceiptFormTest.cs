using System.Windows.Forms;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.Testing
{
	[TestedType(typeof(OpeningReceiptForm))]
	public class OpeningReceiptFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OpeningReceipt openingRec = Factory.New<OpeningReceipt>();
			return new OpeningReceiptForm(openingRec);
		}

		protected override bool ShouldHaveAuditPlugIn => true;
	}
}
