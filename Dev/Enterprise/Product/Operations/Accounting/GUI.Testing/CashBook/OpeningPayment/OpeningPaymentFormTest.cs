using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.Testing
{
	[TestedType(typeof(OpeningPaymentForm))]
	public class OpeningPaymentFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OpeningPayment opy = Factory.New<OpeningPayment>();
			return new OpeningPaymentForm(opy);
		}

		public void TestPostDateEditCaption()
		{
			using (OpeningPaymentForm form = new OpeningPaymentForm(Factory.NewWithValidTestData<OpeningPayment>()))
			{
				var postDateEdit = (ZDateEdit)form.Controls.Find("PostDateEdit", true).FirstOrDefault();
				AssertEquals("Expecting the caption of PostDateEdit Control is 'Post Date'", "Post Date", postDateEdit.CaptionResourceString.Caption);
			}
		}

		protected override bool ShouldHaveAuditPlugIn => true;
	}
}
