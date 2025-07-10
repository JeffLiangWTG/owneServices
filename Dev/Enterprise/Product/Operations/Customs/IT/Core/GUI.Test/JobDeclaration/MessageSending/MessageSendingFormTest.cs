using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
{
	protected override Form GetFormToBashCore()
	{
		return new MessageSendingForm(declarationWrapper);
	}

	public void TestSendMessagesToCustomsMenuItem()
	{
		using (TemporarilySetDeclarationIsUCC6(declaration, false))
		using (var form = new JobDeclarationFormForTest(declaration))
		{
			var testMenu = form.EDIMenu;
			UnitTestUserNotification.Instance.ClearMessages();
			testMenu.RefreshMenu();
			var sendCustomsMessagesMenuItem = testMenu.MenuItems.FindByText("Send Customs Messages");
			AssertNotNull("Send Customs Messages menu item should not be null", sendCustomsMessagesMenuItem);
			Assert("Send Customs Messages menu item should be visible", sendCustomsMessagesMenuItem.Visible);
			sendCustomsMessagesMenuItem.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}

	public void TestConfirmationFormPopoutWhenDeclarationIsNotReadyToBeSent()
	{
		declaration.JE_CustomsProfile = "1111-DEC1";
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
		.AppendAccount("11111111111-001", "1111", "00", "01")
		.AppendAccountDetail("1111-DEC1", "DEC1")
		.Build();

		var jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		entryHeader.CH_Status = "AWO";
		entryHeader.CH_BGMReference = "0001";

		var expectedWarningMessage = @"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.

Entries:
0001: AWO";

		var messageSendingObject = jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().FirstOrDefault();
		messageSendingObject.ShouldSend = true;

		using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
		{
			form.Show();
			var sendButton = form.SendSplitButton;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			sendButton.Enabled = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			sendButton.PerformClick();
			AssertEquals("Warning allow sending", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		entryHeader.CH_Status = "ERO";
		using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var sendButton = form.SendSplitButton;
			sendButton.Enabled = true;
			sendButton.PerformClick();
			AssertNotEquals("Warning allow sending", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestSendButtonVisibility()
	{
		using (var form = new MessageSendingForm(declarationWrapper))
		{
			form.Show();
			var sendButton = form.FindSingle<ZButton>("SendButton");
			Assert("SendButton is hidden", !sendButton.Visible);
		}
	}

	public void TestSendSplitButtonStyle()
	{
		var jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject = jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().Single();

		void AssertButtonStyle(SendingModeSplitButton.ButtonStyle expectedButtonStyle)
		{
			using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
			{
				form.Show();
				var sendSplitButton = form.SendSplitButton;
				AssertEquals(sendSplitButton.SendButtonStyle, SendingModeSplitButton.ButtonStyle.MultipleActions);

				sendingObject.ShouldSend = ZBool.True;
				AssertEquals(sendSplitButton.SendButtonStyle, expectedButtonStyle);
			}
		}

		CombineAssertions("CEI_Style = ''", () =>
		{
			entryInstruction.CEI_Style = ZString.Empty;
			AssertButtonStyle(SendingModeSplitButton.ButtonStyle.MultipleActions);
		});

		sendingObject.ShouldSend = ZBool.False;
		CombineAssertions("CEI_Style = 'DSE'", () =>
		{
			entryInstruction.CEI_Style = SADDeclarationTypeList.Codes.DichiarazioneSemplificata;
			AssertButtonStyle(SendingModeSplitButton.ButtonStyle.FallbackAction);
		});

		sendingObject.ShouldSend = ZBool.False;
		CombineAssertions("CEI_Style = 'COD'", () =>
		{
			entryInstruction.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana;
			AssertButtonStyle(SendingModeSplitButton.ButtonStyle.MultipleActions);
		});

		sendingObject.ShouldSend = ZBool.False;
		CombineAssertions("CEI_SubStyle = 'D'", () =>
		{
			entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD;
			AssertButtonStyle(SendingModeSplitButton.ButtonStyle.FallbackAction);
		});
	}

	[UseSnapshotProtection]
	public void TestErrorDialogFilenameGenerationOutOfRange_AutomaticSend()
	{
		AssertErrorDialogFilenameGenerationOutOfRange((sendSplitButton) => sendSplitButton.PerformClick(), expectErrorDialogWhenOutOfRange: true);
	}

	[UseSnapshotProtection]
	public void TestErrorDialogFilenameGenerationOutOfRange_ManualSend()
	{
		AssertErrorDialogFilenameGenerationOutOfRange((sendSplitButton) => GetContextMenuStripItem(sendSplitButton, "&Manual Send").PerformClick(), expectErrorDialogWhenOutOfRange: true);
	}

	[UseSnapshotProtection]
	public void TestErrorDialogFilenameGenerationOutOfRange_FallbackProcedureSend()
	{
		AssertErrorDialogFilenameGenerationOutOfRange((sendSplitButton) => GetContextMenuStripItem(sendSplitButton, "&Fallback Procedure").PerformClick(), expectErrorDialogWhenOutOfRange: false);
	}

	void AssertErrorDialogFilenameGenerationOutOfRange(Action<SendingModeSplitButton> clickAction, bool expectErrorDialogWhenOutOfRange)
	{
		var sendingObject = (JobDeclarationMessageSendingObject)declarationWrapper.SendingObjectsCollection.SingleOrDefault();
		sendingObject.ShouldSend = true;

		using (var form = new MessageSendingForm(declarationWrapper))
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

		declaration.JE_CustomsProfile = "1111-DEC1";
		using (var form = new MessageSendingForm(declarationWrapper))
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

		using (var form = new MessageSendingForm(declarationWrapper))
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

	public void TestConfirmationFormPopoutWithMultipleEntries()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		declaration.JE_CustomsProfile = "1111-DEC1";
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		var entryLine2 = entryHeader2.MergedLines.AddNew();
		var invoiceLine2 = entryLine2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_JZ = declaration.Invoices.First().PK;
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
		.AppendAccount("11111111111-001", "1111", "00", "01")
		.AppendAccountDetail("1111-DEC1", "DEC1")
		.Build();

		var jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		entryHeader.CH_Status = "AWO";
		entryHeader.CH_BGMReference = "0001";
		entryHeader2.CH_Status = "AWO";
		entryHeader2.CH_BGMReference = "0002";

		var expectedWarningMessage = $@"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.

Entries:
";
		var expectedEntry001 = "0001: AWO";
		var expectedEntry002 = "0002: AWO";

		var messageSendingObject = jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>();
		var messageSendingObject1 = messageSendingObject.FirstOrDefault(x => x.CH_BGMReference == "0001");
		var messageSendingObject2 = messageSendingObject.FirstOrDefault(x => x.CH_BGMReference == "0002");

		using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
		{
			form.Show();
			var sendButton = form.SendSplitButton;

			sendButton.Enabled = true;
			sendButton.PerformClick();
			AssertNotContains("Doesn't contains a warning message for entry 001", expectedWarningMessage + expectedEntry001, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotContains("Doesn't contains a warning message for entry 002", expectedWarningMessage + expectedEntry002, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		messageSendingObject1.ShouldSend = true;
		using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
		{
			form.Show();
			var sendButton = form.SendSplitButton;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			sendButton.Enabled = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			sendButton.PerformClick();
			AssertEquals("When ShouldSend=True for item 001, Warning allow sending should be shown", expectedWarningMessage + expectedEntry001, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotContains("When ShouldSend=False for item 002, Warning allow sending shouldn't be shown", expectedWarningMessage + expectedEntry002, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		messageSendingObject1.ShouldSend = false;
		messageSendingObject2.ShouldSend = true;
		using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
		{
			form.Show();
			var sendButton = form.SendSplitButton;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			sendButton.Enabled = true;
			sendButton.PerformClick();
			AssertEquals("When ShouldSend=True for item 002, Warning allow sending should be shown", expectedWarningMessage + expectedEntry002, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotContains("When ShouldSend=False for item 001, Warning allow sending shouldn't be shown", expectedWarningMessage + expectedEntry001, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestErrorDialogForSubscriberNotFound_AutomaticSend()
	{
		AssertErrorDialogSubscriberNotFound((sendSplitButton) => sendSplitButton.PerformClick());
	}

	public void TestNoErrorDialogForSubscriberNotFound_ManualSend()
	{
		AssertNoErrorDialogSubscriberNotFound((sendSplitButton) => GetContextMenuStripItem(sendSplitButton, "&Manual Send").PerformClick());
	}

	public void TestNoErrorDialogForSubscriberNotFound_FallbackProcedureSend()
	{
		AssertNoErrorDialogSubscriberNotFound((sendSplitButton) => GetContextMenuStripItem(sendSplitButton, "&Fallback Procedure").PerformClick());
	}

	void AssertErrorDialogSubscriberNotFound(Action<SendingModeSplitButton> clickAction)
	{
		AssertErrorDialogForSubscriber(clickAction, (expectedError, form) =>
		{
			AssertEquals("Error dialog expected as declaration isUCC6 is false and subscriber not found", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form has not been closed", DialogResult.None, form.DialogResult);
		});
	}

	void AssertNoErrorDialogSubscriberNotFound(Action<SendingModeSplitButton> clickAction)
	{
		AssertErrorDialogForSubscriber(clickAction, (expectedError, form) =>
		{
			AssertNotEquals("Error dialog not expected as there is no signature of the Subscriber to be done", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form has been closed as error dialog was not expected", DialogResult.OK, form.DialogResult);
		});
	}

	void AssertErrorDialogForSubscriber(Action<SendingModeSplitButton> clickAction, Action<string, Form> assertWhenSubscriberMissing)
	{
		const string expectedError = "Please select a Subscriber in Misc. tab";

		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", "DEC1")
			.Build();

		var sendingObject = (JobDeclarationMessageSendingObject)declarationWrapper.SendingObjectsCollection.SingleOrDefault();
		sendingObject.ShouldSend = true;
		declaration.JE_CustomsProfile = "1111-DEC1";

		using (TemporarilySetDeclarationIsUCC6(declaration, false))
		using (var form = new MessageSendingForm(declarationWrapper))
		{
			form.Show();
			var sendSplitButton = form.FindSingle<SendingModeSplitButton>("SendSplitButton");
			sendSplitButton.Enabled = true;

			CombineAssertions("When Subscriber missing", () =>
			{
				declaration.JE_GS_NKCusAgent = "";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				clickAction(sendSplitButton);

				assertWhenSubscriberMissing(expectedError, form);
			});

			CombineAssertions("When Subscriber present", () =>
			{
				declaration.JE_GS_NKCusAgent = "CRR";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				clickAction(sendSplitButton);
				AssertNotEquals("Error dialog not expected as subscriber is specified", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form has been closed", DialogResult.OK, form.DialogResult);
			});
		}
	}

	ZToolStripMenuItem GetContextMenuStripItem(SendingModeSplitButton sendSplitButton, ZString menuItemText) => sendSplitButton.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(x => x.Text == menuItemText);
	IDisposable TemporarilySetDeclarationIsUCC6(JobDeclaration declaration, bool configurationValue) => ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_GS_NKCusAgent = "CRR";
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = entryLine.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_JZ = invoiceHeader.PK;
		declarationWrapper = new JobDeclarationMessageSendingObjectParent(declaration);

		var staff = GlbStaff.CurrentUser;
		var codCertificate = staff.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "00891230153";

		Factory.Save();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	JobDeclarationMessageSendingObjectParent declarationWrapper;
}
