using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(ExitControlMessageSendingForm))]
	class ExitControlMessageSendingFormTest : MessageSendingFormWithValidationDetailsAbstractTest
	{
		public virtual void TestSendingWithValidationErrors()
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = "AH";
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_OfficeOfExit = "ABC123";
			report.CER_Status = "REJ";
			Factory.Save();
			var parent = new ExitControlMessageSendingObjectParent(header);
			using (var form = new ExitControlMessageSendingForm(parent))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				var sendWithValidationErrorsCheckBox = (ZCheckBox)form.Controls.Find("SendWithValidationErrorsCheckBox", true).FirstOrDefault();
				var validationErrorsTextBox = (ZTextBox)form.Controls.Find("ValidationErrorsTextBox", true).FirstOrDefault();

				var sendingObject = parent.SendingObjectsCollection.First() as ExitControlMessageSendingObject;
				sendingObject.ShouldSend = true;

				CombineAssertions(() =>
				{
					Assert("Send button should be disabled", !sendButton.Enabled);
					Assert("Send with validation errors checkbox visible", sendWithValidationErrorsCheckBox.Visible);
					AssertStartsWith("Validation errors", "It is likely that your message(s) will be rejected by Customs", validationErrorsTextBox.Text);
				});

				sendWithValidationErrorsCheckBox.Checked = true;

				Assert("Send button should be enabled", sendButton.Enabled);
			}
		}

		public virtual void TestSendingWithoutValidationErrors()
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_OfficeOfExit = "ABC123";
			report.CER_Status = "REJ";
			Factory.Save();
			var parent = new ExitControlMessageSendingObjectParentForTest(header);
			using (var form = new ExitControlMessageSendingForm(parent))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				var sendWithValidationErrorsCheckBox = (ZCheckBox)form.Controls.Find("SendWithValidationErrorsCheckBox", true).FirstOrDefault();
				var validationErrorsTextBox = (ZTextBox)form.Controls.Find("ValidationErrorsTextBox", true).FirstOrDefault();

				// No objects to send
				CombineAssertions(() =>
				{
					Assert("Send button should be disabled", !sendButton.Enabled);
					Assert("No send with validation errors checkbox", !sendWithValidationErrorsCheckBox.Visible);
					AssertNullOrEmpty("No validation errors", validationErrorsTextBox.Text);
				});

				var sendingObject = parent.SendingObjectsCollection.First() as ExitControlMessageSendingObject;
				sendingObject.ShouldSend = true;

				// object without errors to send
				CombineAssertions(() =>
				{
					Assert("Send button should be enabled", sendButton.Enabled);
					Assert("No send with validation errors checkbox", !sendWithValidationErrorsCheckBox.Visible);
					AssertNullOrEmpty("No validation errors", validationErrorsTextBox.Text);
				});
			}
		}

		protected override MessageSendingFormWithValidationDetails GetFormToTestPreviewCheckbox(BaseMessageSendingObjectParent messageSendingObjectParent) =>
			new ExitControlMessageSendingForm((ExitControlMessageSendingObjectParent)messageSendingObjectParent);

		protected override BaseMessageSendingObjectParent GetMessageSendingObjectParent()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			cusExitHeader.CusExitReports.AddNew();
			cusExitHeader.CusExitReports.AddNew();
			return new ExitControlMessageSendingObjectParent(cusExitHeader);
		}

		protected override Form GetFormToBashCore()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var parent = new ExitControlMessageSendingObjectParent(exitHeader);
			return new ExitControlMessageSendingForm(parent);
		}

		sealed class ExitControlMessageSendingObjectParentForTest : ExitControlMessageSendingObjectParent
		{
			public ExitControlMessageSendingObjectParentForTest(CusExitHeader cusExitHeader) : base(cusExitHeader)
			{
			}

			public ZString BizObjValidationMessageErrorsForTest = ZString.Empty;

			protected override ZString GetBizObjValidationMessageErrors() => BizObjValidationMessageErrorsForTest;
		}
	}
}
