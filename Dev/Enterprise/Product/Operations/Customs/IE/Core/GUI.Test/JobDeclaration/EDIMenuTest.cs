using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using GlbCompanyWrapper = Enterprise.Customs.IE.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestGenerateEntriesMenuItem_Visible()
		{
			using (var menu = GetMenu(DeclarationApplicationCodeList.Codes.Builtin))
			{
				menu.RefreshMenu();
				AssertEquals(true, menu.GenerateEntriesMenuItem.Visible);
			}
		}

		public void TestGenerateEntriesMenuItem_Invisible()
		{
			using (var menu = GetMenu(DeclarationApplicationCodeList.Codes.Interfaced))
			{
				menu.RefreshMenu();
				AssertEquals(false, menu.GenerateEntriesMenuItem.Visible);
			}
		}

		public void TestRefreshMenu_NoException_AsDeclarationIsNotCreated()
		{
			using (var menu = new EDIMenu())
			{
				AssertNoExceptionThrown(() =>
				{
					menu.RefreshMenu();
				});
			}
		}

		public void TestSendToCustoms_ImportUCC5()
		{
			TestSendToCustoms_Import<AISUCC5MessageSendingAction>(ImportDeclarationApplicationCodeList.Codes.V1);
		}

		public void TestSendToCustoms_ImportUCC6()
		{
			TestSendToCustoms_Import<AISMessageSendingAction>(ImportDeclarationApplicationCodeList.Codes.V2);
		}

		void TestSendToCustoms_Import<TSendingAction>(string applicationCode) where TSendingAction : CusEntryHeaderMessageSendingAction
		{
			CombineAssertions(() =>
			{
				using (var form = GetForm(setupCredential: true))
				{
					var declaration = form.Declaration;
					declaration.JE_ApplicationCode = applicationCode;
					declaration.CustomsEntryInstructions[0].CEI_Style = ImportDeclarationTypeList.Codes.H1;

					var menu = form.EDIMenu;
					var sendToCustomsMenuItem = menu.MenuItems.FindByText("Send to Customs");

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Declaration Has Changes", true, declaration.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					sendToCustomsMenuItem.PerformClick();
					AssertType<MessageSendingForm<TSendingAction>>("Last dialog form type = MessageSendingForm<AISMessageSendingAction>", ZFormModaliser.LastFormShownDialogForTest);
				}
			});
		}

		public void TestSendToCustoms_CredentialCheck_NoValidCredential()
		{
			using (var form = GetForm(false, setupCredential: false))
			{
				var declaration = (JobDeclaration)form.Declaration;
				var company = declaration.Company;
				var companyCredential = GlbCompanyWrapper.Get(company).GlbExternalPassword;
				companyCredential.CurrentDecryptedCertificatePassphrase = "HELLO";
				Factory.Save();
				var menu = form.EDIMenu;
				var sendToCustomsMenuItem = menu.MenuItems.FindByText("Send to Customs");

				CombineAssertions("No valid credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendToCustomsMenuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
				});
			}
		}

		public void TestSendToCustoms_CredentialCheck_ExpiredCredential()
		{
			using (var form = GetForm(false, setupCredential: false))
			{
				var declaration = (JobDeclaration)form.Declaration;
				var company = declaration.Company;
				var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
				companyCredential.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(-1);
				Factory.Save();
				AssertEquals("Pre-condition", PasswordStatusList.Codes.Valid, companyCredential.GP_PasswordStatus);
				var menu = form.EDIMenu;
				var sendToCustomsMenuItem = menu.MenuItems.FindByText("Send to Customs");

				CombineAssertions("No valid credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendToCustomsMenuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
					companyCredential.Reload();
					AssertEquals("Invalidated", PasswordStatusList.Codes.Invalid, companyCredential.GP_PasswordStatus);
				});
			}
		}

		public void TestSendToCustoms_CredentialCheck_NoCredential()
		{
			using (var form = GetForm(false, setupCredential: false))
			{
				var declaration = (JobDeclaration)form.Declaration;
				var company = declaration.Company;
				var menu = form.EDIMenu;
				var sendToCustomsMenuItem = menu.MenuItems.FindByText("Send to Customs");

				CombineAssertions("No credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendToCustomsMenuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
				});
			}
		}

		public void TestSendToCustoms_Export()
		{
			using (var form = GetForm(false, true))
			{
				var declaration = form.Declaration;
				var menu = form.EDIMenu;
				var sendToCustomsMenuItem = menu.MenuItems.FindByText("Send to Customs");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration Has Changes", true, declaration.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendToCustomsMenuItem.PerformClick();
				AssertType<MessageSendingForm<AESMessageSendingAction>>("Last dialog form type = MessageSendingForm<AESMessageSendingAction>", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSendToCustoms_ShouldCheckCredit()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (MessageSendingFormWithValidationDetails)obj;
				var action = (CusEntryHeaderMessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
				action.ShouldSend = true;
				if (action is AISMessageSendingAction aisMessageSendingAction)
				{
					aisMessageSendingAction.MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
				}
			});
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			const string notSentMessage = "Submit message with credit restriction canceled.";
			const string sentMessage = "1 message(s) queued for sending.";

			var testCases = new[]
			{
				(isImport: false, isCheckPass: false, expectedMessageCount: 0, expectedMessageText: notSentMessage),
				(isImport: false, isCheckPass: true, expectedMessageCount: 1, expectedMessageText: sentMessage),
				(isImport: true, isCheckPass: false, expectedMessageCount: 0, expectedMessageText: notSentMessage),
				(isImport: true, isCheckPass: true, expectedMessageCount: 1, expectedMessageText: sentMessage)
			};

			foreach (var (isImport, isCheckPass, expectedMessageCount, expectedMessageText) in testCases)
			{
				using (var form = GetForm(isImport, true))
				{
					var declaration = (JobDeclaration)form.Declaration;
					var menu = form.EDIMenu;

					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					declaration.DoMerge(new SendsMessagesToCustomsGUI());
					var entryHeader = declaration.CustomsEntryHeaders[0];

					var sendToCustomsMenuItem = menu.MenuItems.FindByText("Send to Customs");

					using (CheckCreditTestHelper.WithCreditCheck(Factory, declaration, isCheckPass))
					{
						Factory.Save();

						AssertEquals($"MessageType: {declaration.JE_MessageType}, Credit check passed: {isCheckPass}, Precondition", 0, entryHeader.Messages.Count);

						sendToCustomsMenuItem.PerformClick();

						CombineAssertions($"MessageType: {declaration.JE_MessageType}, Credit check passed: {isCheckPass}", () =>
						{
							AssertEquals("Last message after sending", expectedMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals("Messages count after sending", expectedMessageCount, entryHeader.Messages.Count);
						});
					}
				}
			}
		}

		public void TestSendToCustomsMenuItemVisibility()
		{
			CombineAssertions(() =>
			{
				using (var form = GetForm())
				{
					var declaration = form.Declaration;
					var menu = form.EDIMenu;
					var sendToCustomsMenuItem = menu.MenuItems.FindByText("Send to Customs");

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					var isDirectSendToCustomsForImportEnabled = false;
					var isExpectedVisible = false;
					AssertSendToCustomsMenuVisibility(declaration, menu, sendToCustomsMenuItem, isDirectSendToCustomsForImportEnabled, isExpectedVisible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					isDirectSendToCustomsForImportEnabled = true;
					isExpectedVisible = false;
					AssertSendToCustomsMenuVisibility(declaration, menu, sendToCustomsMenuItem, isDirectSendToCustomsForImportEnabled, isExpectedVisible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					isDirectSendToCustomsForImportEnabled = false;
					isExpectedVisible = false;
					AssertSendToCustomsMenuVisibility(declaration, menu, sendToCustomsMenuItem, isDirectSendToCustomsForImportEnabled, isExpectedVisible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					isDirectSendToCustomsForImportEnabled = false;
					isExpectedVisible = true;
					AssertSendToCustomsMenuVisibility(declaration, menu, sendToCustomsMenuItem, isDirectSendToCustomsForImportEnabled, isExpectedVisible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = "V2";
					isDirectSendToCustomsForImportEnabled = false;
					isExpectedVisible = false;
					AssertSendToCustomsMenuVisibility(declaration, menu, sendToCustomsMenuItem, isDirectSendToCustomsForImportEnabled, isExpectedVisible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = "V2";
					isDirectSendToCustomsForImportEnabled = true;
					isExpectedVisible = true;
					AssertSendToCustomsMenuVisibility(declaration, menu, sendToCustomsMenuItem, isDirectSendToCustomsForImportEnabled, isExpectedVisible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = "V1";
					isDirectSendToCustomsForImportEnabled = false;
					isExpectedVisible = false;
					AssertSendToCustomsMenuVisibility(declaration, menu, sendToCustomsMenuItem, isDirectSendToCustomsForImportEnabled, isExpectedVisible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = "V1";
					isDirectSendToCustomsForImportEnabled = true;
					isExpectedVisible = true;
					AssertSendToCustomsMenuVisibility(declaration, menu, sendToCustomsMenuItem, isDirectSendToCustomsForImportEnabled, isExpectedVisible);
				}
			});
		}

		void AssertSendToCustomsMenuVisibility(BaseJobDeclaration declaration, Customs.GUI.EDIMenu menu, MenuItem sendToCustomsMenuItem, bool isDirectSendToCustomsForImportEnabled, bool isExpectedVisible)
		{
			using (IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isDirectSendToCustomsForImportEnabled))
			{
				menu.RefreshMenu();
				AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}, {isDirectSendToCustomsForImportEnabled}", isExpectedVisible, sendToCustomsMenuItem.Visible);
			}
		}

		public void TestEvents()
		{
			TestATHEvent(
				messageStatus: LogicalStatusList.Codes.Sent,
				entryStatus: AISEntryStatusList.Codes.Accepted,
				expectedReference: Business.Constants.SendingMessageEventList.Reference
			);
			TestATHEvent(
				messageStatus: LogicalStatusList.Codes.Acknowledged,
				entryStatus: AISEntryStatusList.Codes.Accepted,
				expectedReference: Business.Constants.SendingMessageEventList.Resubmit
			);
		}
		void TestATHEvent(ZString messageStatus, ZString entryStatus, string expectedReference)
		{
			using var form = GetForm(isImport: true, setupCredential: true);
			var declaration = (JobDeclaration)form.Declaration;
			var menu = form.EDIMenu;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.DoMerge(new SendsMessagesToCustomsGUI());
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_Status = messageStatus;
			entryHeader.CH_EntryStatus = entryStatus;
			declaration.Logs.RemoveAndDeleteAll();
			var sendToCustomsMenuItem = menu.MenuItems.FindByText("Send to Customs");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var messageSendingForm = (MessageSendingForm<AISMessageSendingAction>)form;
				var messageSendingObjectsGrid = messageSendingForm.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				var formBinding = (KBindingSource)messageSendingForm.GetType().GetField("BindingSource", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(messageSendingForm);
				var sendingParent = (AISMessageSendingActionParent)formBinding.DataSource;
				foreach (AISMessageSendingAction sendingAction in sendingParent.SendingObjectsCollection)
				{
					sendingAction.ShouldSend = true;
					sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
				}
			});
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendToCustomsMenuItem.PerformClick();
			var logs = declaration.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals(
				"Should have ATH Event with Reference: " + expectedReference,
				true, logs.Any(log => log.Event.SE_Code == "ATH" && log.SL_Reference.Contains(expectedReference))
			);
		}

		public void TestUploadSupportingDocumentsMenuItemVisibility()
		{
			CombineAssertions(() =>
			{
				using (var form = GetForm())
				{
					var declaration = form.Declaration;
					IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
					var menu = form.EDIMenu;
					var sendToCustomsMenuItem = menu.MenuItems.FindByText("Upload Supporting Documents");

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, sendToCustomsMenuItem.Visible);
				}
			});
		}

		public void TestSendDocuments_SingleEntry()
		{
			using (var form = GetForm(isImport: false, setupCredential: true))
			{
				var declaration = (JobDeclaration)form.Declaration;
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();
				var menu = form.EDIMenu;
				menu.RefreshMenu();

				var sendDocumentsMenuItem = menu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(m => m.Text == "Upload Supporting Documents" && m.Visible);
				AssertEquals("No sub menu item for single entry", 0, sendDocumentsMenuItem.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendDocumentsMenuItem.PerformClick();
				AssertType<DocumentsSendingForm>("Last dialog form type = DocumentsSendingForm", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSendDocuments_MultipleEntries()
		{
			using (var form = GetForm(isImport: false, setupCredential: true))
			{
				var declaration = (JobDeclaration)form.Declaration;
				var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
				entryHeader1.MovementReferenceNumberSetter("MRN001");
				var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
				entryHeader2.MovementReferenceNumberSetter("MRN002");
				Factory.Save();

				var menu = form.EDIMenu;
				menu.RefreshMenu();
				var sendDocumentsMenuItem = menu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(m => m.Text == "Upload Supporting Documents" && m.Visible);
				var subMenus = sendDocumentsMenuItem.MenuItems.Cast<ZMenuItem>().ToArray();
				AssertEquals("Sub menu items for multiple entries", 2, subMenus.Length);
				AssertEquals("MRN001 as MenuItem text.", true, subMenus.Any(m => m.Text == "MRN001"));
				AssertEquals("MRN002 as MenuItem text.", true, subMenus.Any(m => m.Text == "MRN002"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				subMenus[0].PerformClick();
				AssertType<DocumentsSendingForm>("Last dialog form type = DocumentsSendingForm", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestUploadDocuments_UCC5() => TestUploadDocuments(ImportDeclarationApplicationCodeList.Codes.V1);

		public void TestUploadDocuments_UCC6() => TestUploadDocuments(ImportDeclarationApplicationCodeList.Codes.V2);

		void TestUploadDocuments(string applicationCode)
		{
			using (var form = GetForm(setupCredential: true))
			{
				var declaration = (JobDeclaration)form.Declaration;
				IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
				declaration.JE_ApplicationCode = applicationCode;
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();
				var menu = form.EDIMenu;
				menu.RefreshMenu();

				var sendDocumentsMenuItem = menu.MenuItems.Cast<MenuItem>().FirstOrDefault(m => m.Text == "Upload Documents" && m.Visible);
				AssertEquals("No sub menu item for single entry", 0, sendDocumentsMenuItem.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendDocumentsMenuItem.PerformClick();
				AssertType<AISDocumentsUploadForm>("Last dialog form type = AISDocumentsUploadForm", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestUploadDocumentsMenuItemVisibility()
		{
			CombineAssertions(() =>
			{
				using (var form = GetForm())
				{
					var declaration = form.Declaration;
					IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
					var menu = form.EDIMenu;
					var sendToCustomsMenuItem = menu.MenuItems.FindByText("Upload Documents");

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, sendToCustomsMenuItem.Visible);

					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);

					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, sendToCustomsMenuItem.Visible);
				}
			});
		}

		public void TestUploadDocuments_CheckBeforeSending_HasChangesOrNoCredential()
		{
			using (var form = GetForm())
			{
				var menu = form.EDIMenu;
				var sendToCustomsMenuItem = menu.MenuItems.FindByText("Upload Documents");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendToCustomsMenuItem.PerformClick();
				var declaration = form.Declaration;
				AssertEquals("Has Changes", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("No Credential", $"Cannot send message as Company ({declaration.Company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUploadDocuments_CheckBeforeSending_HasEntries()
		{
			using (var form = GetForm(setupCredential: true))
			{
				Factory.Save();
				var menu = form.EDIMenu;
				var sendToCustomsMenuItem = menu.MenuItems.FindByText("Upload Documents");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("No entry", "No entries exist – Please generate entries before attempting to send a message to customs.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.Declaration.ActiveEntryHeaders.AddNew();
				sendToCustomsMenuItem.PerformClick();
				AssertNull("Has entry", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendRefundApplication_MenuItemVisibility()
		{
			CombineAssertions(() =>
			{
				using (var form = GetForm())
				{
					var declaration = form.Declaration;
					var menu = form.EDIMenu;
					var refundApplicationMenuItem = menu.MenuItems.FindByText("Send Refund Application");

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, refundApplicationMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, refundApplicationMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, refundApplicationMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, refundApplicationMenuItem.Visible);

					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, refundApplicationMenuItem.Visible);
				}
			});
		}

		public void TestSendRefundApplication_UCC5() => TestSendRefundApplication(ImportDeclarationApplicationCodeList.Codes.V1);

		public void TestSendRefundApplication_UCC6() => TestSendRefundApplication(ImportDeclarationApplicationCodeList.Codes.V2);

		void TestSendRefundApplication(string applicationCode)
		{
			using (var form = GetForm(setupCredential: true))
			{
				var declaration = form.Declaration;
				declaration.JE_ApplicationCode = applicationCode;
				var menu = form.EDIMenu;
				var refundApplicationMenuItem = menu.MenuItems.FindByText("Send Refund Application");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				refundApplicationMenuItem.PerformClick();
				AssertType<RefundApplicationSendingForm>("Last dialog form type = MessageSendingForm<RefundApplicationForm>", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSendRefundApplication_CheckBeforeSending_HasChangesOrNoCredential()
		{
			using (var form = GetForm())
			{
				var menu = form.EDIMenu;
				var refundApplicationMenuItem = menu.MenuItems.FindByText("Send Refund Application");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				refundApplicationMenuItem.PerformClick();
				var declaration = form.Declaration;
				AssertEquals("Has Changes", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				refundApplicationMenuItem.PerformClick();
				AssertEquals("No Credential", $"Cannot send message as Company ({declaration.Company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendDepositRefundApplication_MenuItemVisibility()
		{
			CombineAssertions(() =>
			{
				using (var form = GetForm())
				{
					var declaration = form.Declaration;
					var menu = form.EDIMenu;
					var depositRefundApplicationMenuItem = menu.MenuItems.FindByText("Send Deposit Refund Application");

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, depositRefundApplicationMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, depositRefundApplicationMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", false, depositRefundApplicationMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, depositRefundApplicationMenuItem.Visible);

					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
					menu.RefreshMenu();
					AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", true, depositRefundApplicationMenuItem.Visible);
				}
			});
		}

		public void TestSendDepositRefundApplication_UCC5() => TestSendDepositRefundApplication(ImportDeclarationApplicationCodeList.Codes.V1);

		public void TestSendDepositRefundApplication_UCC6() => TestSendDepositRefundApplication(ImportDeclarationApplicationCodeList.Codes.V2);

		void TestSendDepositRefundApplication(string applicationCode)
		{
			using (var form = GetForm(setupCredential: true))
			{
				var declaration = form.Declaration;
				declaration.JE_ApplicationCode = applicationCode;
				var menu = form.EDIMenu;
				var depositRefundApplicationMenuItem = menu.MenuItems.FindByText("Send Deposit Refund Application");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				depositRefundApplicationMenuItem.PerformClick();
				AssertType<DepositRefundApplicationSendingForm>("Last dialog form type = MessageSendingForm<DepositRefundApplicationForm>", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSendDepositRefundApplication_CheckBeforeSending_HasChangesOrNoCredential()
		{
			using (var form = GetForm())
			{
				var menu = form.EDIMenu;
				var depositRefundApplicationMenuItem = menu.MenuItems.FindByText("Send Deposit Refund Application");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				depositRefundApplicationMenuItem.PerformClick();
				var declaration = form.Declaration;
				AssertEquals("Has Changes", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				depositRefundApplicationMenuItem.PerformClick();
				AssertEquals("No Credential", $"Cannot send message as Company ({declaration.Company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSupplementaryMenuItemsVisibility()
		{
			using var menu = GetMenu();
			var declaration = menu.Declaration;
			var supplementaryEntryMenuItem = menu.MenuItems.FindByText(EU.GUI.EDIMenuCaptions.SupplementaryEntry);
			AssertNotNull("Pre-requisite: SupplementaryEntryMenuItem exists", supplementaryEntryMenuItem);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			menu.RefreshMenu();
			AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", expected: false, supplementaryEntryMenuItem.Visible);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			menu.RefreshMenu();
			AssertEquals($"{declaration.JE_ApplicationCode}, {declaration.JE_MessageType}", expected: true, supplementaryEntryMenuItem.Visible);
		}

		public void TestRemovedEUMenuItems()
		{
			var sADHDataEntryFormMenuItemText = EU.GUI.EDIMenuCaptions.SADHDataEntryForm;
			var singleLineEntryMenuItemText = EU.GUI.EDIMenuCaptions.SingleLineEntry;
			var autoPopulateAuthorizationsMenuItemText = EU.GUI.EDIMenuCaptions.AutoPopulateAuthorizations;

			using var menu = GetMenu();
			using var euMenu = new EU.GUI.EDIMenu();

			var sADHDataEntryFormMenuItem = euMenu.MenuItems.FindByText(sADHDataEntryFormMenuItemText, findSubitems: true);
			var singleLineEntryMenuItem = euMenu.MenuItems.FindByText(singleLineEntryMenuItemText, findSubitems: true);
			var autoPopulateAuthorizationsMenuItem = euMenu.MenuItems.FindByText(autoPopulateAuthorizationsMenuItemText, findSubitems: true);

			CombineAssertions("Pre-requisite EU has these", () =>
			{
				AssertNotNull("SADHDataEntryFormMenuItem", sADHDataEntryFormMenuItem);
				AssertNotNull("SingleLineEntryMenuItem", singleLineEntryMenuItem);
				AssertNotNull("AutoPopulateAuthorizationsMenuItem", autoPopulateAuthorizationsMenuItem);
			});

			sADHDataEntryFormMenuItem = menu.MenuItems.FindByText(sADHDataEntryFormMenuItemText, findSubitems: true);
			singleLineEntryMenuItem = menu.MenuItems.FindByText(singleLineEntryMenuItemText, findSubitems: true);
			autoPopulateAuthorizationsMenuItem = menu.MenuItems.FindByText(autoPopulateAuthorizationsMenuItemText, findSubitems: true);

			CombineAssertions("IE does not have these", () =>
			{
				AssertNull("SADHDataEntryFormMenuItem", sADHDataEntryFormMenuItem);
				AssertNull("SingleLineEntryMenuItem", singleLineEntryMenuItem);
				AssertNull("AutoPopulateAuthorizationsMenuItem", autoPopulateAuthorizationsMenuItem);
			});
		}

		JobDeclarationFormTestClass GetForm(bool isImport = true, bool setupCredential = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = isImport ? ImportDeclarationApplicationCodeList.Codes.V1 : DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = isImport ? MessageTypeList.Codes.Import : MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions[0];
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			if (setupCredential)
			{
				InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			}
			var form = new JobDeclarationFormTestClass(declaration);
			var menu = form.EDIMenu;
			menu.Declaration = declaration;
			return form;
		}

		EDIMenu GetMenu(string applicationCode = "")
		{
			var menu = new EDIMenu();
			var declaration = Factory.New<JobDeclaration>();
			if (!string.IsNullOrEmpty(applicationCode))
			{
				declaration.JE_ApplicationCode = applicationCode;
			}
			menu.Declaration = declaration;
			return menu;
		}

		class JobDeclarationFormTestClass : BaseJobDeclarationFormTestClass
		{
			public JobDeclarationFormTestClass(JobDeclaration dec) : base(dec) { }

			protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

			public new KBindingSource BindingSource => BindingSource;
		}
	}
}
