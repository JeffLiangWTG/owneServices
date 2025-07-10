using System.Windows.Forms;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.Testing
{
	[TestedType(typeof(DirectReceiptForm))]
	public class DirectReceiptFormTest : DirectCashBookBaseFormTest
	{
		protected override Form GetFormToBashCore()
		{
			DirectReceipt testReceipt = Factory.New<DirectReceipt>();
			return new DirectReceiptForm(testReceipt);
		}

		public void TestColumnLayoutContextIsSet()
		{
			DirectReceipt directReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			Factory.Save();
			using (DirectReceiptForm newForm = new DirectReceiptForm(directReceipt))
			{
				newForm.Show();

				AssertEquals("Should not change the context string now. Grid layouts persisted for this context will be lost", "Receipt", newForm.CashBookLineBoundGrid_ForTestOnly.ColumnLayoutContext);
			}
		}

		public void TestCashBookTransactionTabPage1TabCaption()
		{
			DirectReceipt directReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			Factory.Save();
			using (DirectReceiptForm newForm = new DirectReceiptForm(directReceipt))
			{
				newForm.Show();

				AssertEquals("Caption string should be Receipt", "Receipt", newForm.CashBookTransactionTabPage1_ForTestOnly.CaptionResourceString.Caption);
			}
		}
	}
}
