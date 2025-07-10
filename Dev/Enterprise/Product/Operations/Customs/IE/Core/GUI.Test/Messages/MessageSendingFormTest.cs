using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm<AESMessageSendingAction>))]
	class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
	{
		public void TestWarningSplitContainer_Panel2Collapsed()
		{
			using (var form = (MessageSendingForm<AESMessageSendingAction>)GetFormToBash())
			{
				form.Show();
				var warningSplitContainer = form.FindSingle<CargoWise.Windows.UI.KSplitContainer>("WarningSplitContainer");
				AssertEquals("Panel2Collapsed", true, warningSplitContainer.Panel2Collapsed);
			}
		}

		public void TestConfirmReason()
		{
			using (var form = (MessageSendingForm<AESMessageSendingAction>)GetFormToBash())
			{
				form.Show();

				AssertEquals("ConfirmReason", string.Empty, form.ConfirmReason);

				form.ConfirmReason = "Test Reason";

				AssertEquals("ConfirmReason", "Test Reason", form.ConfirmReason);
			}
		}

		public void TestUserConfirmation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = "SNT";
			var messageSendingObjectParent = new AESMessageSendingActionParent(declaration);

			using (var form = new MessageSendingFormForExportTest(messageSendingObjectParent))
			{
				form.Show();

				var sendingAction = (AESMessageSendingAction)messageSendingObjectParent.SendingObjectsCollection.First();
				sendingAction.ShouldSend = true;
				form.SendButton.Enabled = true;
				form.SendButton.PerformClick();

				AssertType<ConfirmSendForm>("Last dialog form type = ConfirmSendForm", ZFormModaliser.LastFormShownDialogForTest);

				var confirmSendForm = ZFormModaliser.LastFormShownDialogForTest as ConfirmSendForm;
			}
		}

		public void TestAnnotationColumn()
		{
			using (var form = (ZForm)GetFormToBash())
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");

				var columnstyle = grid.GetColumnStyle(nameof(AESMessageSendingAction.Annotation));
				CombineAssertions(() =>
				{
					AssertEquals("Annotation Column Visible", true, columnstyle.IsVisible);
					AssertEquals("Annotation Column Name", "Annotation", columnstyle.ColumnName);
					AssertEquals("Annotation Column Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140), columnstyle.Width);
				});
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			_ = declaration.CustomsEntryHeaders.AddNew();
			_ = declaration.CustomsEntryHeaders.AddNew();
			var messageSendingObjectParent = new AESMessageSendingActionParent(declaration);
			return new MessageSendingForm<AESMessageSendingAction>(messageSendingObjectParent);
		}
		protected override MessageSendingFormWithValidationDetails GetFormToTestPreviewCheckbox(BaseMessageSendingObjectParent messageSendingObjectParent) =>
			new MessageSendingForm<AESMessageSendingAction>((AESMessageSendingActionParent)messageSendingObjectParent);

		protected override BaseMessageSendingObjectParent GetMessageSendingObjectParent()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			_ = declaration.CustomsEntryHeaders.AddNew();
			_ = declaration.CustomsEntryHeaders.AddNew();
			return new AESMessageSendingActionParent(declaration);
		}

		class MessageSendingFormForExportTest : MessageSendingForm<AESMessageSendingAction>
		{
			public MessageSendingFormForExportTest(AESMessageSendingActionParent parent) : base(parent)
			{
			}

			public new ZButton SendButton => base.SendButton;
		}

		class MessageSendingFormForImportTest : MessageSendingForm<AISMessageSendingAction>
		{
			public MessageSendingFormForImportTest(AISMessageSendingActionParent parent) : base(parent)
			{
			}

			public new ZButton SendButton => base.SendButton;
		}

		public void TestValidationOnSending()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = cei1.PK;
			entryHeader1.CH_Status = LogicalStatusList.Codes.Sent;
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = cei2.PK;

			var messageSendingObjectParent = new AESMessageSendingActionParent(declaration);
			using (var form = new MessageSendingObjectForm(messageSendingObjectParent))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				var testingObject1 = new AESMessageSendingAction(entryHeader1);
				testingObject1.MessageType = AESOutgoingMessageTypeList.Codes.ExportCancellation;
				var testingObject2 = new AESMessageSendingAction(entryHeader2);
				testingObject2.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
				messageSendingObjectParent.SendingObjectsCollection.Add(testingObject1);
				messageSendingObjectParent.SendingObjectsCollection.Add(testingObject2);
				testingObject1.ShouldSend = true;
				testingObject2.ShouldSend = true;
				sendButton.PerformClick();
				AssertContains("You have not entered an Annotation.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				testingObject1.ShouldSend = false;
				testingObject2.ShouldSend = true;
				sendButton.PerformClick();
				AssertNotContains("You have not entered an Annotation.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldTriggerWarningForResubmittingSameMessageEntryImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			TestForWarning(declaration, entryHeader, "", "", false, false);
			TestForWarning(declaration, entryHeader, "SNT", "", true, false);
			TestForWarning(declaration, entryHeader, "ACC", "ACC", true, true);
			TestForWarning(declaration, entryHeader, "REJ", "REJ", false, true);
			TestForWarning(declaration, entryHeader, "NOT", "NOT", false, true);
		}

		public void TestShouldTriggerWarningForResubmittingSameMessageEntryExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			TestForWarning(declaration, entryHeader, "", "", false, false);
			TestForWarning(declaration, entryHeader, "SNT", "", true, false);
			TestForWarning(declaration, entryHeader, "ACC", "ACC", true, true);
			TestForWarning(declaration, entryHeader, "REJ", "REJ", false, true);
			TestForWarning(declaration, entryHeader, "DRJ", "DRJ", false, true);
		}

		public void TestForWarning(JobDeclaration declaration, CusEntryHeader entryHeader, ZString status, ZString entryStatus, bool expectWarning, bool isMNRAvailable)
		{
			entryHeader.CH_Status = status;
			entryHeader.CH_EntryStatus = entryStatus;
			var messageSendingObjectParentExport = new AESMessageSendingActionParent(declaration);
			var messageSendingObjectParentImport = new AISMessageSendingActionParent(declaration);

			string warningMessage = @"Please note that you have not received a response for the message that you have submitted previously.
The consequences of sending duplicate messages means that you will have possibly 2 declarations for the same job.
This will require that at least one of these needs to be manually canceled.
Do you still want to submit another message?

If you are sure you want to submit another message, please enter reasons: ";

			string warningMessageforResubmit =
				@"The entry already has a MRN number, submitting the original message might overwrite the MRN number by creating a duplicate entry in Irish customs system.
The consequences of sending duplicate messages means that you will have possibly 2 declarations for the same job.
This will require that at least one of these needs to be manually canceled.
Do you still want to submit another message?

If you are sure you want to submit another message, please enter reasons: ";

			if (declaration.JE_MessageType == MessageTypeList.Codes.Import)
			{
				using (var form = new MessageSendingFormForImportTest(messageSendingObjectParentImport))
				{
					form.Show();

					var sendingAction = (AISMessageSendingAction)messageSendingObjectParentImport.SendingObjectsCollection.First();
					sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
					sendingAction.ShouldSend = true;
					form.SendButton.Enabled = true;
					form.SendButton.PerformClick();

					var confirmSendForm = ZFormModaliser.LastFormShownDialogForTest as ConfirmSendForm;

					if (expectWarning)
					{
						AssertType<ConfirmSendForm>("Warning should be displayed", confirmSendForm);
						if (isMNRAvailable)
						{
							AssertEquals("Warning Message", warningMessageforResubmit, confirmSendForm.WarningMessageLabel.Text);
						}
						else
						{
							AssertEquals("Warning Message", warningMessage, confirmSendForm.WarningMessageLabel.Text);
						}
					}
					else
					{
						AssertEquals("Warning should not be displayed", null, confirmSendForm);
					}
				}
			}
			else
			{
				using (var form = new MessageSendingFormForExportTest(messageSendingObjectParentExport))
				{
					form.Show();

					var sendingAction = (AESMessageSendingAction)messageSendingObjectParentExport.SendingObjectsCollection.First();
					sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
					sendingAction.ShouldSend = true;
					form.SendButton.Enabled = true;
					form.SendButton.PerformClick();

					var confirmSendForm = ZFormModaliser.LastFormShownDialogForTest as ConfirmSendForm;

					if (expectWarning)
					{
						AssertType<ConfirmSendForm>("Warning should be displayed", confirmSendForm);
						if (isMNRAvailable)
						{
							AssertEquals("Warning Message", warningMessageforResubmit, confirmSendForm.WarningMessageLabel.Text);
						}
						else
						{
							AssertEquals("Warning Message", warningMessage, confirmSendForm.WarningMessageLabel.Text);
						}
					}
					else
					{
						AssertEquals("Warning should not be displayed", null, confirmSendForm);
					}
				}
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
		}
	}
}
