using System.Reflection;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal abstract class CalloutPaymentDetailsFormTestCase : ZFormBasherTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestOKAndCancelButtonVisible()
		{
			using (CalloutPaymentDetailsForm form = (CalloutPaymentDetailsForm)GetFormToBashCore())
			{
				ZButton oKButton = GetOKButton(form);
				ZButton cancelButton = GetCancelButton(form);
				form.Show();
				Assert(oKButton.Visible);
				Assert(cancelButton.Visible);
			}
		}

		public void TestFormHeading()
		{
			using (CalloutPaymentDetailsForm form = (CalloutPaymentDetailsForm)GetFormToBashCore())
			{
				AssertEquals("Enter Payment Details", form.FormHeading);
			}
		}

		public void TestNoteCreatedOnOkButtonClick()
		{
			using (CalloutPaymentDetailsForm form = (CalloutPaymentDetailsForm)GetFormToBashCore())
			{
				ZButton oKButton = GetOKButton(form);
				AssertEquals(0, Callout.Notes.GetAllNotes().Count);
				form.Show();
				oKButton.PerformClick();
				Application.DoEvents();
				AssertEquals("There should be a CalloutPaymentNote", 1, Callout.Notes.GetAllNotes().Count);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return CalloutPaymentDetailsForm.New(Callout, PaymentMethod);
		}

		protected abstract UPECargoPaymentMethod PaymentMethod { get; }

		Callout Callout
		{
			get
			{
				if (fCallout == null)
				{
					fCallout = Factory.New<Callout>();
				}

				return fCallout;
			}
		}

		ZButton GetOKButton(CalloutPaymentDetailsForm form)
		{
			FieldInfo oKButtonField = typeof(CalloutPaymentDetailsForm).GetField("OKButton", BindingFlags.NonPublic | BindingFlags.Instance);
			return (ZButton)oKButtonField.GetValue(form);
		}

		ZButton GetCancelButton(CalloutPaymentDetailsForm form)
		{
			FieldInfo cancelButtonField = typeof(CalloutPaymentDetailsForm).GetField("CancelButton", BindingFlags.NonPublic | BindingFlags.Instance);
			return (ZButton)cancelButtonField.GetValue(form);
		}

		Callout fCallout;
	}
}
