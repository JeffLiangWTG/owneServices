using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(PartPaymentForm))]
	internal class PartPaymentFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (PartPaymentForm form = new PartPaymentForm(CalloutPartPayment))
			{
				AssertEquals("Enter Part Payment Details", form.FormHeading);
			}
		}

		public void TestOkButtonClick()
		{
			using (PartPaymentForm form = new PartPaymentForm(CalloutPartPayment))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals("Information All errors must be corrected before you can proceed.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(false, CalloutPartPayment.MustAddNote);
				CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.BRK;
				CalloutPartPayment.Remarks = "Test";
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(true, CalloutPartPayment.MustAddNote);
			}
		}

		public void TestCancelButtonClick()
		{
			using (PartPaymentForm form = new PartPaymentForm(CalloutPartPayment))
			{
				form.Show();
				form.cancelButton.PerformClick();
				AssertEquals(false, CalloutPartPayment.MustAddNote);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new PartPaymentForm(CalloutPartPayment);
		}

		CalloutPartPayment CalloutPartPayment
		{
			get
			{
				if (fCalloutPartPayment == null)
				{
					fCalloutPartPayment = new CalloutPartPayment(Factory);
				}

				return fCalloutPartPayment;
			}
		}

		CalloutPartPayment fCalloutPartPayment;
	}
}
