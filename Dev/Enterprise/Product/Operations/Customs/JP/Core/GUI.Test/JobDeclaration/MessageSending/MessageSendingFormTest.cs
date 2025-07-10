using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Business.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var messageSendingParent = new DeclarationMessageSendingObjectParent(jobDeclaration);
			return new MessageSendingForm(messageSendingParent);
		}

		public void TestFormHeading()
		{
			var declaration = Declaration;
			declaration.SetCurrentMessageSendingContext(new MessageSendingContext { ProcedureCode = JPProcedureCodeList.Codes.FHL });

			var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);

			using (var form = new MessageSendingForm(sendingObjectParent))
			{
				form.Show();
				AssertEquals("Export FHL Messages", form.FormHeading);
			}

			SetCompanyWithMailboxCredential(declaration);

			using (var form = new MessageSendingForm(sendingObjectParent))
			{
				form.Show();
				AssertEquals("Send or Export FHL Messages", form.FormHeading);
			}
		}

		public void TestExportMessageControls()
		{
			var declaration = Declaration;
			var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);

			using var form = new MessageSendingForm(sendingObjectParent);
			form.Show();
			var exportPathTextBox = form.FindSingle<ZTextBox>("ExportPathTextBox");

			AssertEquals("ExportPathTextBox Anchor", AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom, exportPathTextBox.Anchor);
			Assert("Should always be visible", exportPathTextBox.Visible);
			Assert("Should always be visible", form.FindSingleOrDefault<ZButton>("ExportPathButton").Visible);
			Assert("Should always be visible", form.FindSingleOrDefault<ZButton>("ExportButton").Visible);
			Assert("Should always be visible", form.FindSingleOrDefault<ZButton>("SendButton").Visible);
		}

		public void TestNewColumns()
		{
			var declaration = Declaration;
			var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);

			using (var form = new MessageSendingForm(sendingObjectParent))
			{
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");

				AssertNotNull(grid.GetColumnStyle("InputReference"));
				AssertNotNull(grid.GetColumnStyle("EntryNumber"));
				AssertNotNull(grid.GetColumnStyle("EntryStatus"));
				AssertNotNull(grid.GetColumnStyle("Status"));
				AssertNotNull(grid.GetColumnStyle("DeclarationCorrectionCopyRequest"));
				AssertNull(grid.GetColumnStyle("Action"));
			}

			declaration.SetCurrentMessageSendingContext(new MessageSendingContext { ProcedureCode = JPProcedureCodeList.Codes.ECR, Action = ActionList.Codes.One });
			sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);

			using (var form = new MessageSendingForm(sendingObjectParent))
			{
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");

				AssertNotNull(grid.GetColumnStyle("ProcedureCode"));
				AssertNotNull(grid.GetColumnStyle("InputReference"));
				AssertNotNull(grid.GetColumnStyle("EntryNumber"));
				AssertNotNull(grid.GetColumnStyle("EntryStatus"));
				AssertNotNull(grid.GetColumnStyle("Status"));
				AssertNotNull(grid.GetColumnStyle("DeclarationCorrectionCopyRequest"));
				Assert(!grid.GetColumnStyle("Action").IsReadOnly);
			}
		}

		public void TestSendButtonEnabled()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			declaration.CreateMessageErrorForTest = true;
			declaration.Validation.ValidateAll();

			SetCompanyWithMailboxCredential(declaration);

			var messageSendingParent = new DeclarationMessageSendingObjectParent(declaration);
			using (var form = new MessageSendingForm(messageSendingParent))
			{
				form.Show();

				var sendButton = form.FindSingle<ZButton>("SendButton");
				Assert(!sendButton.Enabled);

				var messageSendingObject = messageSendingParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;
				Assert(!sendButton.Enabled);

				messageSendingParent.AllowSendWithError = true;
				Assert(sendButton.Enabled);

				messageSendingObject.ShouldSend = false;
				Assert(!sendButton.Enabled);
			}
		}

		public void TestContinueToSendCheckBoxReadOnly()
		{
			var declaration = Declaration;
			declaration.CreateMessageErrorForTest = true;
			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);

			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				var continueToSendCheckBox = form.Controls.Find("ContinueToSendCheckBox", true)[0] as ZCheckBox;
				var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.FirstOrDefault() as MessageSendingObject;
				Assert(!messageSendingObject.ShouldSend);
				Assert(continueToSendCheckBox.ReadOnly);

				messageSendingObject.ShouldSend = true;
				Assert(!continueToSendCheckBox.ReadOnly);
			}
		}

		public void TestAdditionalWarningsTextBox()
		{
			var declaration = Declaration;
			declaration.CreateWarningForTest = true;
			declaration.Validation.ValidateAll();
			var jobDeclarationMessageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();

			using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
			{
				form.Show();
				var additionalWarningsTextBox = form.Controls.Find("AdditionalWarningsTextBox", true)[0] as ZTextBox;

				messageSendingObject.ShouldSend = false;
				AssertNullOrEmpty(additionalWarningsTextBox.Text);
				messageSendingObject.ShouldSend = true;
				AssertContains("Test warning on JE_TransportModeInfo.", additionalWarningsTextBox.Text);
			}
		}

		public void TestCheckIsOkToSend_Textbox()
		{
			var declaration = Declaration;
			declaration.CreateMessageErrorForTest = true;
			declaration.Validation.ValidateAll();

			var jobDeclarationMessageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();

			using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
			{
				form.Show();
				var validationErrorsTextBox = form.FindSingle<ZTextBox>("ValidationErrorsTextBox");

				messageSendingObject.ShouldSend = false;
				AssertNullOrEmpty(validationErrorsTextBox.Text);

				messageSendingObject.ShouldSend = true;
				AssertContains("Test message error on JE_TransportModeInfo.", validationErrorsTextBox.Text);
			}
		}

		public void TestCheckIsOkToSend_Popup()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			Env.Security.AllowMessageErrors.IsAllowed = false;
			GlbStaff.CurrentUser.GS_IsController = false;
			var staff = Factory.New<GlbStaff>();
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_IsController = true;
			Factory.Save();

			var declaration = Declaration;
			declaration.CreateErrorForTest = true;
			declaration.Validation.ValidateAll();

			SetCompanyWithMailboxCredential(declaration);

			var jobDeclarationMessageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);

			using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
			{
				form.Show();
				var sendButton = form.FindSingle<ZButton>("SendButton");
				jobDeclarationMessageSendingObjectParent.AllowSendWithError = true;
				var messageSendingObject = jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;

				sendButton.PerformClick();
				AssertContains("Should contain errors from declaration", "Test error on JE_TransportModeInfo.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.CreateErrorForTest = false;
				declaration.CreateMessageErrorForTest = true;
				declaration.Validation.ValidateAll();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendButton.PerformClick();
				AssertContains("Should contain message errors from declaration", "Test message error on JE_TransportModeInfo.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				sendButton.PerformClick();
				AssertContains("Should contain message from supervisor override", "The supervisor must have its Security Rights > Operate > Customs > Supervisor Overrides > Allow Message Errors set to Is Allowed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCheckIsOkToSend_ErrorOnFormShouldPreventSendAndPopNotification()
		{
			var declaration = Declaration;
			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.EDA, SendTarget = SendTarget.FlatFile };
			declaration.SetCurrentMessageSendingContext(messageSendingContext);
			SetCompanyWithMailboxCredential(declaration);

			var jobDeclarationMessageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			jobDeclarationMessageSendingObjectParent.ExportPath = "";

			using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
			{
				form.Show();
				var sendButton = form.FindSingle<ZButton>("SendButton");
				jobDeclarationMessageSendingObjectParent.AllowSendWithError = true;
				var messageSendingObject = jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendButton.PerformClick();
				AssertEquals("Please fix these errors before sending any messages:\r\n\r\nError - ExportPath: Please select a valid directory path to export the message flat file.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCheckIsOkToSendWhenECR_Popup()
		{
			var declaration = Declaration;
			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.ECR };
			declaration.SetCurrentMessageSendingContext(messageSendingContext);
			declaration.CreateMessageErrorForTest = true;
			declaration.Validation.ValidateAll();

			SetCompanyWithMailboxCredential(declaration);

			var jobDeclarationMessageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);

			using (var form = new MessageSendingForm(jobDeclarationMessageSendingObjectParent))
			{
				form.Show();
				var sendButton = form.FindSingle<ZButton>("SendButton");
				jobDeclarationMessageSendingObjectParent.AllowSendWithError = true;
				var messageSendingObject = jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendButton.PerformClick();
				AssertContains("Should contain message errors from declaration that are specific for ECR", "Test message error on JE_ReceiptModeInfo.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("Should not contain message errors from declaration that are not specific for ECR", "Test message error on JE_TransportModeInfo.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCancelButton_ClickCore()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			decl.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			decl.JE_MessageType = JPJobMessageTypeList.Codes.Export;

			var entryHeader = decl.CustomsEntryHeaders.AddNew();
			var instruction = decl.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			instruction.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "123", true);

			decl.SetCurrentMessageSendingContext(new MessageSendingContext
			{
				ProcedureCode = JPProcedureCodeList.Codes.ECR,
				Action = ActionList.Codes.One,
				EntryHeadersToBeSent = [entryHeader]
			});

			var messageSendingParent = new DeclarationMessageSendingObjectParent(decl);

			CombineAssertions("Cancel", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				using var form = new MessageSendingForm(messageSendingParent);
				form.Show();
				var cancelButton = form.FindSingle<ZButton>("CancelButton2");
				cancelButton.PerformClick();
				AssertEquals("123", instruction.ExportControlNumber);
				AssertEquals("Message", "You have canceled the sending of ECR message. Export Control Number will not be deleted from NACCS system. Do you wish to delete it in CargoWise, so that you may manually input another one?", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			CombineAssertions("OK", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				using var form = new MessageSendingForm(messageSendingParent);
				form.Show();
				var cancelButton = form.FindSingle<ZButton>("CancelButton2");
				cancelButton.PerformClick();
				AssertNullOrEmpty("Should be empty", instruction.ExportControlNumber);
				AssertEquals("Caption", "Delete Export Control Number", UnitTestUserNotification.Instance.LastMessage.Caption);
			});
		}

		DeclarationForTestSendingObject Declaration
		{
			get
			{
				var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_GS_NKCusAgent = "TT";
				declaration.JE_CustomsProfile = "123-3";
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;

				return declaration;
			}
		}

		void SetCompanyWithMailboxCredential(JobDeclaration declaration)
		{
			var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "TEST", DomainName = "TEST", Status = XtCredentialStatusList.Codes.Registered };
			JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(declaration.Company);
			var mailboxCredential = wrapper.MailboxCredential;
			mailboxCredential.GP_MailBoxID = "TEST";
			mailboxCredential.CurrentDecryptedPassword = "TEST";
		}
	}
}
