using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.GUI.Testing
{
	[TestedType(typeof(CustomerServiceEmailForm))]
	class CustomerServiceEmailFormTest : ZFormBasherTest
	{
		public void TestEmailCancellationLogIsCreated()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			try
			{
				var incident = Factory.NewWithValidTestData<SupportIncident>();

				using (var form = new DummyCustomerServiceEmailForm(SupportIncidentEmail.New(incident)))
				{
					form.Show();
					form.Reason = "because I said so";
					var expectedMessage = GlbStaff.CurrentUser.GS_FullName + " canceled the customer email notification. Reason: because I said so";

					form.CloseButton_Exposed.PerformClick();
					Assert(form.IsClosed);
					AssertEquals(true, incident.EConversation.Conversation.Messages.Any(m => m.JCM_Body == expectedMessage));
				}
			}
			finally
			{
				ZFormModaliser.ShowDialogsInTest = false;
			}
		}

		public void TestCloseButtonText()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyCustomerServiceEmailForm(new CustomerServiceEmail(emailContactObject)))
			{
				form.Show();

				AssertEquals("Cancel", form.CloseButton_Exposed.Text);
			}
		}

		public void TestCloseButtonPopup()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyCustomerServiceEmailForm(new CustomerServiceEmail(emailContactObject)))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.CloseButton_Exposed.PerformClick();
				Assert(!form.IsClosed);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CloseButton_Exposed.PerformClick();
				Assert(form.IsClosed);
			}
		}

		public void TestXCloseButtonPopup()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyCustomerServiceEmailForm(new CustomerServiceEmail(emailContactObject)))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.DialogResult = DialogResult.Cancel;
				form.Close();
				Assert(!form.IsClosed);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Close();
				Assert(form.IsClosed);
			}
		}

		public void TestCloseButtonPopup_DialogResultOK()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyCustomerServiceEmailForm(new CustomerServiceEmail(emailContactObject)))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.DialogResult = DialogResult.OK;
				form.Close();
				Assert(form.IsClosed);
			}
		}

		EmailToContactBusinessObject GetPrepopulatedEmailToContactBusinessObject(Type parentType = null)
		{
			var bizo = new EmailToContactBusinessObject(Factory.New(parentType ?? typeof(DummyBizOWithRelatedNotes)));
			bizo.Cc = "cc@cc.com";
			bizo.FromEmailAddress = "from@from.com";
			bizo.Priority = "MED";
			bizo.ToEmailAddress = "to@to.com";

			return bizo;
		}

		protected override Form GetFormToBashCore()
		{
			DummyBusinessObject businessObject = Factory.New<DummyBusinessObject>();
			return new CustomerServiceEmailForm(new CustomerServiceEmail(businessObject));
		}

		class DummyCustomerServiceEmailForm : CustomerServiceEmailForm
		{
			public DummyCustomerServiceEmailForm(CustomerServiceEmail emailToContactBusinessObject) : base(emailToContactBusinessObject)
			{
			}

			public ZButton CloseButton_Exposed
			{
				get { return base.CloseButton; }
			}

			protected override void OnClosed(EventArgs e)
			{
				base.OnClosed(e);
				IsClosed = true;
			}

			public bool IsClosed;

			public string Reason { get; set; }

			protected override CancelEmailSendConfirmationForm GetConfirmationForm(SupportIncident incident)
			{
				return new CancelEmailSendConfirmationFormForTest(new SupportIncidentCancelEmailNotificationActionForTest(incident, Reason));
			}
		}

		class SupportIncidentCancelEmailNotificationActionForTest : SupportIncidentCancelEmailNotificationAction
		{
			public SupportIncidentCancelEmailNotificationActionForTest(SupportIncident incident, string reason) : base(incident)
			{
				Comment = reason;
			}
		}

		class CancelEmailSendConfirmationFormForTest : CancelEmailSendConfirmationForm
		{
			public CancelEmailSendConfirmationFormForTest(SupportIncidentCancelEmailNotificationAction action) : base(action)
			{
				Shown += delegate
				{
					CloseButton.PerformClick();
				};
			}
		}
	}
}
