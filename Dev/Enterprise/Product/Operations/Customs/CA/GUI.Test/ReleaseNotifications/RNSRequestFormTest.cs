using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using IUserNotification = Enterprise.Customs.Business.MessageManagers.IUserNotification;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(RNSRequestForm))]
	sealed class RNSRequestFormTest : ZFormBasherTest
	{
		public void TestSendButtonClick()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory);
			using (var form = new RNSRequestForm(rnsRequestBO))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				form.Show();
				form.SendButton.PerformClick();
				AssertEquals("System cannot send a RNS Status Query message as Cargo Control Number must be specified.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have sent a message", ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				AssertEquals("Is form disposed", false, form.IsDisposed);

				rnsRequestBO.CargoControlNumber = "1234567890";
				form.SendButton.PerformClick();
				AssertEquals("Request RNS Status Query message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have sent a message", ++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				AssertEquals("Is form disposed", true, form.IsDisposed);
			}

			rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory) { CargoControlNumber = "1234567890" };
			UnitTestUserNotification.Instance.ClearMessages();
			using (var form = new TestRNSRequestForm(rnsRequestBO))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				form.Show();
				form.SendButton.PerformClick();
				var notificationExposed = form.Notification_Exposed;
				AssertNotNull(notificationExposed);
				AssertEquals("notification.ContainsValidationErrors", true, notificationExposed.ContainsValidationErrors);
				AssertEquals("notification.ContainsAdditionalWarnings", true, notificationExposed.ContainsAdditionalWarnings);
				AssertEquals("notification.IsWaitingForResponse", true, notificationExposed.IsWaitingForResponse);
				AssertEquals("notification.HasShown", true, notificationExposed.HasShowMessageInstructionFormBeenCalled);

				AssertEquals("notification.ValidationErrors", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Office Code: You have not entered an Office Code.
Sub Location Code: You have not entered a value.

Do you want to send the message(s) despite these errors?", notificationExposed.ValidationErrorsMessage);

				AssertEquals("notification.AdditionalWarning", @"An RNS Request has already been sent and is awaiting a CBSA response (CCN: 1234567890).
Sending another one now may cause you, or the other party who sent the original request, to  not receive a response.
Please be aware that acknowledgement responses are sent to all relevant parties, so you should not need to resend this request.
If you do not receive a response within a reasonable period of time, then try resending the request at that time.


Are you sure that you want to resend to the CBSA?", notificationExposed.AdditionalWarningsMessage);

				AssertNull("Last Message", notificationExposed.LastMessage);
				AssertEquals("Should not have sent a message", ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				AssertEquals("Is form disposed", false, form.IsDisposed);
			}

			CombineAssertions(() =>
			{
				using (var form = new TestRNSRequestForm(rnsRequestBO))
				{
					var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
					form.Notification_Exposed_NextAnswer = true;
					form.Show();
					form.SendButton.PerformClick();
					var notificationExposed = form.Notification_Exposed;
					AssertNotNull(notificationExposed);
					AssertEquals("notification.ContainsValidationErrors", true, notificationExposed.ContainsValidationErrors);
					AssertEquals("notification.ContainsAdditionalWarnings", true, notificationExposed.ContainsAdditionalWarnings);
					AssertEquals("notification.IsWaitingForResponse", true, notificationExposed.IsWaitingForResponse);
					AssertEquals("notification.HasShown", true, notificationExposed.HasShowMessageInstructionFormBeenCalled);

					AssertEquals("notification.ValidationErrors", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Office Code: You have not entered an Office Code.
Sub Location Code: You have not entered a value.

Do you want to send the message(s) despite these errors?", notificationExposed.ValidationErrorsMessage);

					AssertEquals("notification.AdditionalWarning", @"An RNS Request has already been sent and is awaiting a CBSA response (CCN: 1234567890).
Sending another one now may cause you, or the other party who sent the original request, to  not receive a response.
Please be aware that acknowledgement responses are sent to all relevant parties, so you should not need to resend this request.
If you do not receive a response within a reasonable period of time, then try resending the request at that time.


Are you sure that you want to resend to the CBSA?", notificationExposed.AdditionalWarningsMessage);

					AssertEquals("LastMessage", "Request Warehouse Arrival Certification Message message queued for sending.", notificationExposed.LastMessage);
					AssertEquals("Should have sent a message", ++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
					AssertEquals("Is form disposed", true, form.IsDisposed);
				}
			});
		}

		public void TestAutoSendMessage()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory);
			using (var form = new RNSRequestForm(rnsRequestBO))
			{
				form.AutoSendMessage = false;
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				form.Show();
				rnsRequestBO.CargoControlNumber = "1234567890";
				form.SendButton.PerformClick();
				AssertNull("Should not have sent a message.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have sent a message", ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				AssertEquals("Is form disposed", true, form.IsDisposed);
			}
		}

		public void TestVisibilityOfConrols()
		{
			var rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
			using (var form = new RNSRequestForm(rnsRequestBO))
			{
				form.Show();
				Assert("DateOfArrival should be visible for Arrival message", form.DateOfArrivalZDateEdit.Visible);
				Assert("Office should be visible for Arrival message", form.OfficeCodeFindBox.Visible);
				Assert("Sub-Location should be visible for Arrival message", form.SubLocationCodeFindBox.Visible);
			}
			rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory);
			using (var form = new RNSRequestForm(rnsRequestBO))
			{
				form.Show();
				Assert("DateOfArrival should not be visible for Status Query", !form.DateOfArrivalZDateEdit.Visible);
				Assert("Office should not be visible for Status Query", !form.OfficeCodeFindBox.Visible);
				Assert("Sub-Location should not be visible for Status Query", !form.SubLocationCodeFindBox.Visible);
			}
		}

		public void TestFormText()
		{
			using (var form = new RNSRequestForm(new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory)))
			{
				form.Show();
				AssertEquals("RNS Request", form.Text);
			}
		}

		public void TestCargoControlNumberReadonly()
		{
			using (var form = new RNSRequestForm(new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory, false, true)))
			{
				form.Show();
				Assert("CargoControlNumberTextBox should be readonly", form.Controls.Find("CargoControlNumberTextBox", true)[0].GetReadOnly());
			}

			using (var form = new RNSRequestForm(new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory, true, true)))
			{
				form.Show();
				Assert("CargoControlNumberTextBox should be editable", !form.Controls.Find("CargoControlNumberTextBox", true)[0].GetReadOnly());
			}
		}

		public void TestTransactionNumberApplicablity()
		{
			using (var form = new RNSRequestForm(new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory, true, false)))
			{
				form.Show();
				Assert("TransactionNumberTextBox should be invisible", !form.Controls.Find("TransactionNumberTextBox", true)[0].Visible);
			}

			using (var form = new RNSRequestForm(new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory, true, true)))
			{
				form.Show();
				Assert("TransactionNumberTextBox should be visible", form.Controls.Find("TransactionNumberTextBox", true)[0].Visible);
			}
		}

		protected override Form GetFormToBashCore() => new RNSRequestForm(new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory));

		sealed class TestRNSRequestForm : RNSRequestForm
		{
			public TestRNSRequestForm(RNSRequestBO bizObj)
				: base(bizObj)
			{
				Notification_Exposed_NextAnswer = false;
			}

			internal TestMessageInstructionUserNotification Notification_Exposed { get; private set; }

			internal bool Notification_Exposed_NextAnswer;

			protected override IUserNotification GetNewUserNotification()
			{
				Notification_Exposed = new TestMessageInstructionUserNotification();
				Notification_Exposed.NextAnswer = Notification_Exposed_NextAnswer;
				return Notification_Exposed;
			}
		}
	}
}
