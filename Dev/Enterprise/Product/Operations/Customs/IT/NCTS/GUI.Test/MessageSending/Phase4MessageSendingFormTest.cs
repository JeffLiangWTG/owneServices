using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(Phase4MessageSendingForm))]
sealed class Phase4MessageSendingFormTest : MessageSendingObjectFormTest
{
	protected override Form GetFormToBashCore() => new Phase4MessageSendingForm(messageSendingObjectParent);

	[RequiresSTA]
	public void TestSendButtonNotVisible()
	{
		using (var form = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			form.Show();

			var sendButton = form.FindSingle<ZButton>("SendButton");
			AssertEquals(nameof(sendButton.Visible), false, sendButton.Visible);
		}
	}

	[RequiresSTA]
	public void TestSendSplitButtonVisible()
	{
		using (var form = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			form.Show();

			var sendSplitButton = form.SendSplitButton;
			AssertEquals(nameof(sendSplitButton.Visible), true, sendSplitButton.Visible);
		}
	}

	[RequiresSTA]
	public void TestErrorDialogFilenameGenerationOutOfRange_AutomaticSend()
	{
		AssertErrorDialogFilenameGenerationOutOfRange((sendSplitButton) => sendSplitButton.PerformClick(), expectErrorDialogWhenOutOfRange: true);
	}

	[RequiresSTA]
	public void TestErrorDialogFilenameGenerationOutOfRange_ManualSend()
	{
		AssertErrorDialogFilenameGenerationOutOfRange((sendSplitButton) => GetContextMenuStripItem(sendSplitButton, "&Manual Send").PerformClick(), expectErrorDialogWhenOutOfRange: true);
	}

	public void TestErrorDialogFilenameGenerationOutOfRange_FallbackProcedureSend()
	{
		AssertErrorDialogFilenameGenerationOutOfRange((sendSplitButton) => GetContextMenuStripItem(sendSplitButton, "&Fallback Procedure").PerformClick(), expectErrorDialogWhenOutOfRange: false);
	}

	void AssertErrorDialogFilenameGenerationOutOfRange(Action<SendingModeSplitButton> clickAction, bool expectErrorDialogWhenOutOfRange)
	{
		var sendingObject = (NctsHeaderDepartureMessageSendingObject)messageSendingObjectParent.SendingObjectsCollection.SingleOrDefault();
		sendingObject.ShouldSend = true;

		using (var form = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			form.Show();
			var sendSplitButton = form.SendSplitButton;
			sendSplitButton.Enabled = true;
			const string accNotFoundErrMsg = "No Account could be found for the selected Node.Please check that the selected Node in Misc.Tab is valid.";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			clickAction(sendSplitButton);

			if (expectErrorDialogWhenOutOfRange)
			{
				AssertEquals("Error dialog expected as the account is null", accNotFoundErrMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form has not been closed", DialogResult.None, form.DialogResult);
			}
			else
			{
				AssertNotEquals("Error dialog not expected", accNotFoundErrMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form has been closed as error dialog was not expected", DialogResult.OK, form.DialogResult);
			}
		}

		const string expectedError =
			"The filename range for the account 11111111111-001:1111 has run out for today, it is not possible to generate a new filename.\r\n" +
			"Please check the Range Start and Range End in Registry>Customs>Italy>Account Management>Company for the account or consider using another account if available.";

		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", "DEC1")
			.Build();

		var account1111 = accountCollection[0];

		nctsHeader.BH_CustomsProfile = "1111-DEC1";
		using (var form = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			form.Show();
			var sendSplitButton = form.SendSplitButton;
			sendSplitButton.Enabled = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			clickAction(sendSplitButton);
			AssertNotEquals("Error dialog not expected as there are slots available", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form has been closed", DialogResult.OK, form.DialogResult);
		}

		//generate '00' and '01'sequences
		var filenameGenerator = new DailySequenceNumberGenerator(account1111, Factory);
		filenameGenerator.Generate();
		filenameGenerator.Generate();

		using (var form = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			form.Show();
			var sendSplitButton = form.SendSplitButton;
			sendSplitButton.Enabled = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			clickAction(sendSplitButton);
			if (expectErrorDialogWhenOutOfRange)
			{
				AssertEquals("Error dialog expected as there are no slots available", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form has not been closed", DialogResult.None, form.DialogResult);
			}
			else
			{
				AssertNotEquals("Error dialog not expected", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form has been closed as error dialog was not expected", DialogResult.OK, form.DialogResult);
			}
		}
	}

	ZToolStripMenuItem GetContextMenuStripItem(SendingModeSplitButton sendSplitButton, ZString menuItemText) => sendSplitButton.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(x => x.Text == menuItemText);

	[RequiresSTA]
	public void TestConfirmationFormPopoutWhenHeaderIsNotAllowedToBeSend()
	{
		nctsHeader.BH_JobReference = "0001";
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("12345", "DEC1")
			.Build();

		nctsHeader.BH_CustomsProfile = "12345";

		var expectedWarningMessage = @"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.

Entries:
0001: MDS";

		var sendingObject = (NctsHeaderDepartureMessageSendingObject)messageSendingObjectParent.SendingObjectsCollection.SingleOrDefault();
		sendingObject.ShouldSend = true;

		using (var form = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			form.Show();

			var sendButton = form.FindSingle<ZButton>("SendSplitButton");
			sendButton.Enabled = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			nctsHeader.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent;
			sendButton.PerformClick();
			AssertEquals("User confirmation message box text", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			nctsHeader.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			sendButton.PerformClick();
			AssertNull("User confirmation message box text", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	[RequiresSTA]
	public void TestErrorDialogForSubscriberNotFound_AutomaticSend()
	{
		AssertErrorDialogSubscriberNotFound((sendSplitButton) => sendSplitButton.PerformClick(), true);
	}

	public void TestNoErrorDialogForSubscriberNotFound_ManualSend()
	{
		AssertErrorDialogSubscriberNotFound((sendSplitButton) => GetContextMenuStripItem(sendSplitButton, "&Manual Send").PerformClick(), false);
	}

	[RequiresSTA]
	public void TestNoErrorDialogForSubscriberNotFound_FallbackProcedureSend()
	{
		AssertErrorDialogSubscriberNotFound((sendSplitButton) => GetContextMenuStripItem(sendSplitButton, "&Fallback Procedure").PerformClick(), false);
	}

	void AssertErrorDialogSubscriberNotFound(Action<SendingModeSplitButton> clickAction, bool expectErrorDialogWhenSubscriberNotFound)
	{
		const string expectedError = "Please select a Subscriber in Misc. tab";

		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", "DEC1")
			.Build();

		var sendingObject = (NctsHeaderDepartureMessageSendingObject)messageSendingObjectParent.SendingObjectsCollection.SingleOrDefault();
		sendingObject.ShouldSend = true;

		nctsHeader.BH_CustomsProfile = "1111-DEC1";
		nctsHeader.Subscriber = "CRR";

		using (var form = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			form.Show();
			var sendSplitButton = form.FindSingle<SendingModeSplitButton>("SendSplitButton");
			sendSplitButton.Enabled = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			clickAction(sendSplitButton);
			AssertNotEquals("Error dialog not expected as subscriber is specified", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form has been closed", DialogResult.OK, form.DialogResult);
		}

		nctsHeader.Subscriber = "";

		using (var form = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			form.Show();
			var sendSplitButton = form.FindSingle<SendingModeSplitButton>("SendSplitButton");
			sendSplitButton.Enabled = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			clickAction(sendSplitButton);

			if (expectErrorDialogWhenSubscriberNotFound)
			{
				AssertEquals("Error dialog expected as subscriber not found", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form has not been closed", DialogResult.None, form.DialogResult);
			}
			else
			{
				AssertNotEquals("Error dialog not expected as there is no signature of the Subscriber to be done", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form has been closed as error dialog was not expected", DialogResult.OK, form.DialogResult);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		messageSendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);
		nctsHeader.Subscriber = "CRR";
	}

	NctsHeader nctsHeader;
	NctsHeaderDepartureMessageSendingObjectParent messageSendingObjectParent;
}
