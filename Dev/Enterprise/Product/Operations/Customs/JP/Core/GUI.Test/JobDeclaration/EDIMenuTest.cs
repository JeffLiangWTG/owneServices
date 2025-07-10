using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.JP.Business.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.JP.Business.CusEntryInstruction;
using TransportTypeList = Enterprise.Customs.JP.Business.TransportTypeList;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(EDIMenu))]
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestSubmit_Visibilty()
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			using var menu = new EDIMenu();
			menu.Declaration = declaration;

			var submitMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
			Assert("Should be visible.", submitMenu.Visible);
		}

		public void TestSubmit_Action()
		{
			AssertEquals(0, entryHeader.Messages.Count);

			using (var form = new JobDeclarationForm(declaration))
			{
				var menu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendOrExportMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var submitMenu = sendOrExportMenu.MenuItems.FindByText("EDA - Export Customs Declaration Registration");

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;
					var messageSendingParent = dialog.BusinessEntity;
					var action = messageSendingParent.SendingObjectsCollection[0];
					action.ShouldSend = true;

					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				submitMenu.PerformClick();

				AssertEquals(1, entryHeader.Messages.Count);
			}
		}

		public void TestGenerateMerge_Visibilty()
		{
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals("Import/ITF", false, menu.GenerateEntriesMenuItem.Visible);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.RefreshMenu();
					AssertEquals("Export/ITF", false, menu.GenerateEntriesMenuItem.Visible);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals("Import/BLT", true, menu.GenerateEntriesMenuItem.Visible);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					AssertEquals("Export/BLT", true, menu.GenerateEntriesMenuItem.Visible);
				});
			}
		}

		public void TestGenerateMerge_Action()
		{
			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			Factory.Save();

			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;

				CombineAssertions(() =>
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					menu.RefreshMenu();
					menu.GenerateEntriesMenuItem.PerformClick();
					AssertEquals("Import Entry Header Count", 1, declaration.CustomsEntryHeaders.Count);

					declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					menu.RefreshMenu();
					menu.GenerateEntriesMenuItem.PerformClick();
					AssertEquals("Export Entry Header Count", 1, declaration.CustomsEntryHeaders.Count);
				});
			}
		}

		public void TestImportNACCSMessageMenu_Visibilty()
		{
			using (var menu = new EDIMenu())
			{
				menu.RefreshMenu();
				menu.Declaration = declaration;

				var importMenu = menu.MenuItems.FindByText("&Import NACCS Message (.txt)");

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.RefreshMenu();

				Assert("Should always be true.", importMenu.Visible);
			}
		}

		public void TestMSXRegisterSupportingDocuments_Action()
		{
			AssertEquals(0, entryHeader.Messages.Count);
			entryHeader.CH_PhaseStatus = CustomsDeclarationPhases.Codes.IDC;

			using (var form = new JobDeclarationForm(declaration))
			{
				var menu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendOrExportNACCSMessageMenuItem = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var submitMenu = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("MSX - Register Supporting Documents");

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MSXMessageSendingForm)obj;
					var messageSendingParent = dialog.BusinessEntity;
					var action = messageSendingParent.SendingObjectsCollection[0];
					action.ShouldSend = true;
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.ClearMessages();

				submitMenu.PerformClick();

				AssertEquals("MSX Message Generated", 1, entryHeader.Messages.Count);
			}
		}

		public void TestSendOrExportNACCSMessageMenuItems_Visibilty()
		{
			SetCredentials();
			SetCompanyWithMailboxCredential();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			using (var menu = new EDIMenu())
			{
				menu.RefreshMenu();
				menu.Declaration = declaration;

				var sendOrExportNACCSMessageMenuItem = menu.MenuItems.FindByText("Send/Export NACCS Message");

				var menuItemVisibleTuple = new (string Name, Func<bool> VisibleFunc)[]
				{
					("IDA - Import Customs Declaration Registration", () => declaration.IsImport),
					("IDC - Import Customs Declaration Submission", () => declaration.IsImport),

					("EDA - Export Customs Declaration Registration", () => declaration.IsExport),
					("EAC – Export Post-Permit Amendment Submission", () => declaration.IsExport),
					("EDC - Export Customs Declaration Submission", () => declaration.IsExport),
					("CEW - Export Declaration Post-Move-In Processing", () => declaration.IsExport),
					("ECR - Export Cargo Registration", () => declaration.IsExport && declaration.IsSea),
					("MSX - Register Supporting Documents", () => true)
				};

				CombineAssertions(() =>
				{
					foreach (var kv in menuItemVisibleTuple)
					{
						var subMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText(kv.Name);
						AssertEquals("Import - " + kv.Name, subMenuItem.Visible, kv.VisibleFunc() == declaration.IsImport);
					}

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					menu.RefreshMenu();

					foreach (var kv in menuItemVisibleTuple)
					{
						var subMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText(kv.Name);
						AssertEquals("Export - " + kv.Name, subMenuItem.Visible, kv.VisibleFunc() == declaration.IsExport);
					}

					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					menu.RefreshMenu();
					foreach (var kv in menuItemVisibleTuple)
					{
						var subMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText(kv.Name);
						AssertEquals("Export - " + kv.Name, subMenuItem.Visible, kv.VisibleFunc() == declaration.IsExport && declaration.IsSea);
					}
				});
			}
		}

		public void TestSendOrExportNACCSMessageMenuItems_Action_ExportDeclaration()
		{
			SetCredentials();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			using var form = new JobDeclarationForm(declaration);
			var menu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
			var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");

			CombineAssertions(() =>
			{
				AssertSendOrExportNACCSMessageSubMenuItem_Click(sendOrExportNACCSMessageMenu, JPProcedureCodeList.Codes.EDA, "EDA - Export Customs Declaration Registration");
				AssertSendOrExportNACCSMessageSubMenuItem_Click(sendOrExportNACCSMessageMenu, JPProcedureCodeList.Codes.EAC, "EAC – Export Post-Permit Amendment Submission");
				AssertSendOrExportNACCSMessageSubMenuItem_Click(sendOrExportNACCSMessageMenu, JPProcedureCodeList.Codes.EDC, "EDC - Export Customs Declaration Submission");
				AssertSendOrExportNACCSMessageSubMenuItem_Click(sendOrExportNACCSMessageMenu, JPProcedureCodeList.Codes.CEW, "CEW - Export Declaration Post-Move-In Processing");
				AssertSendOrExportNACCSMessageSubMenuItem_Click(sendOrExportNACCSMessageMenu, JPProcedureCodeList.Codes.ECR, "ECR - Export Cargo Registration");
			});
		}

		public void TestSendOrExportNACCSMessageMenuItems_Action_ImportDeclaration()
		{
			SetCredentials();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			using var form = new JobDeclarationForm(declaration);
			var menu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
			var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");

			CombineAssertions(() =>
			{
				AssertSendOrExportNACCSMessageSubMenuItem_Click(sendOrExportNACCSMessageMenu, JPProcedureCodeList.Codes.IDA, "IDA - Import Customs Declaration Registration");
				AssertSendOrExportNACCSMessageSubMenuItem_Click(sendOrExportNACCSMessageMenu, JPProcedureCodeList.Codes.IDC, "IDC - Import Customs Declaration Submission");
			});
		}

		void AssertSendOrExportNACCSMessageSubMenuItem_Click(MenuItem parentMenuItem, string procedureCode, string menuItemText)
		{
			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();

			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_CEI_Instruction = declaration.CustomsEntryInstructions[0].PK;
			var entryLine = header.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			header.CH_DataModel = Core.Constants.CountryCodes.Japan;

			declaration.Factory.Save();

			var menuItem = parentMenuItem.MenuItems.FindByText(menuItemText);

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (MessageSendingForm)obj;

				var messageSendingParent = dialog.BusinessEntity;

				var sendingObject = messageSendingParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			});

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			menuItem.PerformClick();

			AssertEquals($"Expect create 1 message after the {menuItemText} is clicked.", 1, header.Messages.Count);
			AssertEquals($"Expect the message type is {procedureCode} after the {menuItemText} is clicked.", procedureCode.Substring(0, 3), header.Messages[0].EM_MessageType);
		}

		public void TestSendOrExportNACCSMessageMenuItems_Export_Action()
		{
			entryInstruction.JP_CustomsNotes = "テスト地方消費税";
			using (var form = new JobDeclarationForm(declaration))
			using (var dir = new TestTemporaryDirectory())
			{
				var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.Factory.Save();

				var menu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var menuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("EDA - Export Customs Declaration Registration");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;

					var messageSendingParent = dialog.BusinessEntity;
					messageSendingParent.ExportPath = dir.Directory.FullName;
					((MessageSendingContext)messageSendingParent.Context).SendTarget = SendTarget.FlatFile;

					var sendingObject = messageSendingParent.SendingObjectsCollection[0];
					sendingObject.ShouldSend = true;
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.ClearMessages();

				menuItem.PerformClick();

				AssertEquals("Message is created.", 1, entryHeader.Messages.Count);
				CombineAssertions(() =>
				{
					AssertEquals("EM_ApplicationReference", EDIMessage.FlatFile, entryHeader.Messages[0].EM_ApplicationReference);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Sent, entryHeader.Messages[0].EM_Status);
					AssertEquals("CH_Status", JPMessageStatusList.Codes.Exported, entryHeader.CH_Status);
					var files = dir.Directory.GetFiles();
					AssertEquals("Should create a flat file for saving the new message", 1, files.Length);
					Assert("Should export the message data to the file.", files[0].Length > 0);
					AssertEquals($"Export has been completed. All files can be found at {dir.Directory.FullName}.", UnitTestUserNotification.Instance.LastMessage.Text);
					using (var stream = new StreamReader(files[0].OpenRead(), encoding: JPMessageUtils.MessageDataEncodingShiftJIS))
					{
						var text = stream.ReadToEnd();
						Assert("Encoding", text.Contains("テスト地方消費税"));
					}
				});
			}
		}

		public void TestIsEDABusinessReady()
		{
			SetCredentials();
			using var form = new JobDeclarationForm(declaration);
			var menu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
			var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");

			var menuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("EDA - Export Customs Declaration Registration");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(31);

			declaration.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			menuItem.PerformClick();
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(
				"Date of Departure must be within 30 days from today.",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendingWithOverriddenValuesLog()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.Factory.Save();

				var menu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var menuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("EDA - Export Customs Declaration Registration");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;

					var messageSendingParent = dialog.BusinessEntity;
					((MessageSendingContext)messageSendingParent.Context).SendTarget = SendTarget.FlatFile;

					var sendingObject = messageSendingParent.SendingObjectsCollection[0];
					sendingObject.ShouldSend = true;

					messageSendingParent.UseVisualData = true;
					var contentProviders = messageSendingParent.GetContentProviders();
					var visualObjectParent = messageSendingParent.VisualObjectParent;
					visualObjectParent.InitializeVisualObjects(contentProviders);
					var visualObject = visualObjectParent.VisualObjects.Cast<MessageVisualObject>().FirstOrDefault();
					visualObject.Header.Cast<EditableFieldBizObject>().FirstOrDefault().OverrideValue = "X";
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				menuItem.PerformClick();
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.Authorised.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
				var sendingWithOverriddenValuesLogs = new BusinessObjectFactory().Load<StmALog>(query).Where(log => log.SL_Reference.StartsWith("Sending with overridden values|EDIMessage Number="));
				AssertEquals(1, sendingWithOverriddenValuesLogs.Count());
			}
		}

		public void TestECRSendingWithCustomsEntryInstructionHasNoEntryHeader()
		{
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.Factory.Save();

				var menu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var menuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("ECR - Export Cargo Registration");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				menuItem.PerformClick();

				AssertEquals("Detected that the Instruction has no corresponding entry header. Choose 'Yes' to generate entries and proceed to the message sending screen. Alternatively, select 'No' to cancel.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Instruction lacks entry header.", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			}
		}

		void SetCredentials()
		{
			var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "dummy", DomainName = "dummy", Status = XtCredentialStatusList.Codes.Unregistered };
			JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}

		void SetCompanyWithMailboxCredential()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(company);

			var mailboxCredential = wrapper.MailboxCredential;
			mailboxCredential.GP_MailBoxID = "TEST";
			mailboxCredential.CurrentDecryptedPassword = "TEST";

			declaration.JE_GC = company.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		CusEntryInstruction entryInstruction;
	}
}
