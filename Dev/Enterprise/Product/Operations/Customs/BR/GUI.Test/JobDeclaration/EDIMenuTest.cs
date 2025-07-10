using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class EDIMenuTest : TestCaseWithFactory
	{
		public void TestEDIMenu()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var ediMenu = new EDIMenu { Declaration = declaration })
			{
				var sendCustomsMenuItem = ediMenu.MenuItems.FindByText("Send to Customs");
				AssertNotNull("There should be a Send to Customs menu", sendCustomsMenuItem);
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				ediMenu.RefreshMenu();
				AssertEquals("Send to Customs menu should be visible for air export declarations", true, sendCustomsMenuItem.Visible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				ediMenu.RefreshMenu();
				AssertEquals("Send to Customs menu should be visible for air export declarations.", true, sendCustomsMenuItem.Visible);
				var importNfeMenuItem = ediMenu.MenuItems.FindByText("Import NF-e");
				AssertNotNull("There should be a Import NF-e menu", importNfeMenuItem);
				ediMenu.RefreshMenu();
				AssertEquals("Import NF-e menu should be visible for export declarations.", true, importNfeMenuItem.Visible);
			}
		}

		public void TestImportNfeMenuItem_Click()
		{
			using (var testMenu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				testMenu.Declaration = declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				testMenu.RefreshMenu();
				Assert("Import NF-e menu should be invisible", !testMenu.importNfeMenuItem.Visible);
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				testMenu.RefreshMenu();
				Assert("Import NF-e menu should be visible", !testMenu.importNfeMenuItem.Visible);
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				testMenu.RefreshMenu();
				Assert("Import NF-e menu should be visible", testMenu.importNfeMenuItem.Visible);
				testMenu.importNfeMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType<NFEImportForm>("NFEImportForm popped up", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestExportSendToCustomsMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.Declaration = declaration;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				testMenu.RefreshMenu();
				AssertEquals(false, testMenu.sendToCustomsMenuItem.Visible);
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("Declaration B00001000 has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertType<JobDeclarationMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("1 message has been sent.", 1, declaration.CustomsEntryHeaders[0].Messages.Count);

				var declarationInAnotherFactory = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				AssertEquals("1 CDE message has been sent and save.", MessageTypeList.Codes.CDE, declarationInAnotherFactory.CustomsEntryHeaders[0].Messages[0].EM_MessageType);
			}
		}

		public void TestImportSendToCustomsMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.Declaration = declaration;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				testMenu.RefreshMenu();
				AssertEquals(false, testMenu.sendToCustomsMenuItem.Visible);
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("Declaration B00001000 has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertType<JobDeclarationMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("1 message has been sent.", 1, declaration.CustomsEntryHeaders[0].Messages.Count);

				var declarationInAnotherFactory = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				AssertEquals("1 CDI message has been sent and save.", MessageTypeList.Codes.CDI, declarationInAnotherFactory.CustomsEntryHeaders[0].Messages[0].EM_MessageType);
			}
		}

		public void TestImportLicenseSendToCustomsMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.Declaration = declaration;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				testMenu.RefreshMenu();
				AssertEquals(false, testMenu.sendToCustomsMenuItem.Visible);
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("Declaration B00001000 has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertType<JobDeclarationMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("1 message has been sent.", 1, declaration.CustomsEntryHeaders[0].Messages.Count);

				var declarationInAnotherFactory = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				AssertEquals("1 LIC message has been sent and save.", BRJobMessageTypeList.Codes.ImportLicense, declarationInAnotherFactory.CustomsEntryHeaders[0].Messages[0].EM_MessageType);
			}
		}

		public void TestLPCOSendToCustomsMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
				testMenu.Declaration = declaration;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				testMenu.RefreshMenu();
				AssertEquals(false, testMenu.sendToCustomsMenuItem.Visible);
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("Declaration B00001000 has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertType<JobDeclarationMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("1 message has been sent.", 1, declaration.CustomsEntryHeaders[0].Messages.Count);

				var declarationInAnotherFactory = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				AssertEquals("1 LPCO message has been sent and save.", BRJobMessageTypeList.Codes.LPCO, declarationInAnotherFactory.CustomsEntryHeaders[0].Messages[0].EM_MessageType);
			}
		}

		public void TestImportLicenseLoadResponseMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				testMenu.Declaration = declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals("Load Response from Customs", testMenu.importLicenseLoadResponseMenuItem.Text);

				testMenu.RefreshMenu();
				Assert("'Load Response from Customs' menu should be invisible", !testMenu.importLicenseLoadResponseMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("'Load Response from Customs' menu should be invisible", !testMenu.importLicenseLoadResponseMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				Assert("'Load Response from Customs' menu should be visible", testMenu.importLicenseLoadResponseMenuItem.Visible);
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.importLicenseLoadResponseMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.MovementReferenceNumberSetter("2000010001");
				declaration.Factory.Save();
				testMenu.importLicenseLoadResponseMenuItem.PerformClick();
				AssertEquals("The Declaration B00001000 has no Entries without Movement Reference Number", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entry.MovementReferenceNumberSetter("");
				Factory.Save();
				testMenu.importLicenseLoadResponseMenuItem.PerformClick();
				AssertType<ImportLicenseResponseMessageForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestExportDataFromEntriesMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();

				testMenu.Declaration = declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals("Message should be Export Data from Entries", "Export Data from Entries", testMenu.exportDataFromEntriesMenuItem.Text);

				testMenu.RefreshMenu();
				Assert("Export Data from Entries menu should be visible", testMenu.exportDataFromEntriesMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("Export Data from Entries menu should NOT be visible", !testMenu.exportDataFromEntriesMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.RefreshMenu();
				Assert("Export Data from Entries menu should NOT be visible", !testMenu.exportDataFromEntriesMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				Assert("Export Data from Entries menu should be visible", testMenu.exportDataFromEntriesMenuItem.Visible);
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.exportDataFromEntriesMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				testMenu.exportDataFromEntriesMenuItem.PerformClick();
				AssertEquals("Declaration B00001000 has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);

				var entryheader = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();
				testMenu.exportDataFromEntriesMenuItem.PerformClick();
				AssertType<NFeExportForm>("NFeExportForm popped up", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSendToCustomsMenuItem_ZSaveException()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			using (var testMenu = new EDIMenu())
			{
				testMenu.Declaration = declaration;
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var declarationInAnotherFactory = newFactory.Load<JobDeclaration>(declaration.PK);
				declarationInAnotherFactory.CustomsEntryHeaders[0].CH_Status = BRMessageStatusList.Codes.Rejected;
				newFactory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.sendToCustomsMenuItem.PerformClick();

				AssertContains("0 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("0 message has been sent.", 0, declaration.CustomsEntryHeaders[0].Messages.Count);
			}

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			using (var testMenu = new EDIMenu())
			{
				testMenu.Declaration = declaration;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.sendToCustomsMenuItem.PerformClick();

				AssertContains("0 message(s) have been sent because no changes have been detected at either the Header or Line level.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("0 message has been sent.", 0, declaration.CustomsEntryHeaders[0].Messages.Count);
			}
		}

		public void TestUpdateImportLicenseStatusMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var testMenu = new EDIMenu())
			{
				testMenu.Declaration = declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("Message should be Update Import License Status", "Update Import License Status", testMenu.updateImportLicenseStatusMenuItem.Text);

				testMenu.RefreshMenu();
				Assert("Update Import License Status menu should be visible", testMenu.updateImportLicenseStatusMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("Update Import License Status menu should NOT be visible", !testMenu.updateImportLicenseStatusMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				Assert("Update Import License Status menu should NOT be visible", !testMenu.updateImportLicenseStatusMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.RefreshMenu();
				Assert("Update Import License Status menu should NOT be visible", !testMenu.updateImportLicenseStatusMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

				var message = Factory.NewWithValidTestData<BREDIMessage>();
				message.EM_ApplicationCode = ApplicationCodes.BRCustoms;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				message.EM_ApplicationReference = "RLI00000000001000001";

				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

				Factory.Save();

				entry.ImportLicenseIdentifierNumber.CE_EntryNum = "BXI000010021";
				entry.ImportLicenseIdentifierNumber.CE_EntryType = "MRN";

				message.EM_LinkedObject = entry;

				entry.Messages.Add(message);

				testMenu.updateImportLicenseStatusMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType<ImportLicenseResponseMessageForm>("UpdateImportLicenseStatusForm popped up", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestGenerateAdditionalInformationMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.CustomsEntryInstructions.AddNew();
				var instruction = declaration.CustomsEntryInstructions[0];
				testMenu.Declaration = declaration;

				AssertNullOrEmpty(instruction.AdditionalInformation);
				AssertEquals("Message should be Generate Additional Information", "Generate Additional Information", testMenu.generateAdditionalInformationMenuItem.Text);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.RefreshMenu();
				Assert("Generate Additional Information menu should be visible", testMenu.generateAdditionalInformationMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				Assert("Generate Additional Information menu should be visible", testMenu.generateAdditionalInformationMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("Generate Additional Information menu should NOT be visible", !testMenu.generateAdditionalInformationMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.RefreshMenu();
				Assert("Generate Additional Information menu should NOT be visible", !testMenu.generateAdditionalInformationMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
				testMenu.RefreshMenu();
				Assert("Generate Additional Information menu should NOT be visible", !testMenu.generateAdditionalInformationMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertNullOrEmpty(instruction.AdditionalInformation);
				Assert("Pre-condition", declaration.HasChanges);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.generateAdditionalInformationMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty(instruction.AdditionalInformation);

				declaration.Factory.Save();
				AssertNullOrEmpty(instruction.AdditionalInformation);

				testMenu.generateAdditionalInformationMenuItem.PerformClick();
				AssertNullOrEmpty(instruction.AdditionalInformation);

				instruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.OnlyFreeText;
				testMenu.generateAdditionalInformationMenuItem.PerformClick();
				AssertNotNullOrEmpty(instruction.AdditionalInformation);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				instruction.AdditionalInformation = ZString.Empty;
				AssertNullOrEmpty(instruction.AdditionalInformation);
				Assert("Pre-condition", declaration.HasChanges);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.generateAdditionalInformationMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty(instruction.AdditionalInformation);

				declaration.Factory.Save();
				AssertNullOrEmpty(instruction.AdditionalInformation);

				testMenu.generateAdditionalInformationMenuItem.PerformClick();
				AssertNotNullOrEmpty(instruction.AdditionalInformation);
			}
		}

		public void TestUpdateImportLicenseStatus_BlockAccess()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var testMenu = new EDIMenu())
			{
				testMenu.Declaration = declaration;

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.updateImportLicenseStatusMenuItem.PerformClick();
				AssertEquals("The Declaration B00001000 has no Movement Reference Number - Please register an Entry before attempting to update its status.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGenerateEntriesMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				testMenu.Declaration = declaration;

				AssertEquals("GenerateEntriesMenuItem label should be", "Generate Entries (&Merge)", testMenu.GenerateEntriesMenuItem.Text);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.RefreshMenu();
				AssertEquals("GenerateEntriesMenuItem label should be", "Generate Entries (&Merge)", testMenu.GenerateEntriesMenuItem.Text);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				AssertEquals("GenerateEntriesMenuItem label should be", "Generate Entries (&Merge)", testMenu.GenerateEntriesMenuItem.Text);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				AssertEquals("GenerateEntriesMenuItem label should be", "Generate Entries (&Merge)", testMenu.GenerateEntriesMenuItem.Text);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.RefreshMenu();
				AssertEquals("GenerateEntriesMenuItem label should be", "Generate Licenses", testMenu.GenerateEntriesMenuItem.Text);
			}
		}

		public void TestImportLicenseFromXMLMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				testMenu.Declaration = declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				Assert("'Load Import License(s) from XML' menu should be invisible", !testMenu.importLicenseFromXMLMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("'Load Import License(s) from XML' menu should be invisible", !testMenu.importLicenseFromXMLMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.RefreshMenu();
				Assert("'Load Import License(s) from XML' menu should be invisible", !testMenu.importLicenseFromXMLMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.RefreshMenu();
				Assert("'Load Import License(s) from XML' menu should be visible", testMenu.importLicenseFromXMLMenuItem.Visible);
				testMenu.Declaration = declaration;
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.importLicenseFromXMLMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				testMenu.importLicenseFromXMLMenuItem.PerformClick();
				AssertType<ImportLicenseFromXMLForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestImportLicenseCopyCommercialInvoiceLineMenuItem()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice1 = declaration1.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			var invoice2 = declaration1.Invoices.AddNew();
			invoice2.InvoiceLines.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			Factory.Save();

			using (var form = new ZForm())
			using (var testMenu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(testMenu);
				testMenu.Declaration = declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				Assert("'&Copy/Import Invoice Line' menu should be invisible", !testMenu.copyCommercialInvoiceLineMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("'&Copy/Import Invoice Line' menu should be invisible", !testMenu.copyCommercialInvoiceLineMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.RefreshMenu();
				Assert("'&Copy/Import Invoice Line' menu should be invisible", !testMenu.copyCommercialInvoiceLineMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.RefreshMenu();
				Assert("'&Copy/Import Invoice Line' menu should be visible", testMenu.copyCommercialInvoiceLineMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				testMenu.Declaration = declaration;
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.copyCommercialInvoiceLineMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testMenu.copyCommercialInvoiceLineMenuItem.PerformClick();
				AssertType<EmbeddedModulePopup>(ZFormModaliser.LastFormShownForTest);
				var searchPopup = ZFormModaliser.ActiveForm as EmbeddedModulePopup;
				AssertNotNull("expected search popup to be opened", searchPopup);
				var activeFilters = searchPopup.Module_ForTest.FilterBusinessObject.ActiveModuleFilters;
				AssertEquals("Two filters are defaulted", 4, activeFilters.Count);
				var filterShipment = activeFilters[0];
				var filterImporterSupplier = activeFilters[1];
				var filterAttachedToDeclaration = activeFilters[2];
				AssertEquals("filter: Shipment Type", Constants.FilterConstants.CommercialInvoice.ShipmentType, filterShipment.Description);
				AssertEquals("filter: Importer Supplier", Constants.FilterConstants.CommercialInvoice.ImporterSupplier, filterImporterSupplier.Description);
				AssertEquals("filter: Attached To Declaration", Constants.FilterConstants.CommercialInvoice.AttachedToDeclaration, filterAttachedToDeclaration.Description);

				searchPopup.Module_ForTest.PerformSearch_ForTest();
				AssertEquals("should find 2 records", 2, searchPopup.Module_ForTest.GridCollection.Count);
				AssertNotNull(searchPopup.Module_ForTest.GridCollection.FindByPK(invoice1.PK));
				AssertNotNull(searchPopup.Module_ForTest.GridCollection.FindByPK(invoice2.PK));

				AssertEquals("Pre-condition: One invoice on the declaration", 1, declaration.Invoices.Count);
				AssertEquals("Pre-condition: No invoice lines on the declaration", 0, declaration.InvoiceLines.Count);
				searchPopup.Module_ForTest.DisplayGrid.Select(0);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				searchPopup.ExposedOKButtonForTesting.PerformClick();
				AssertType<JobComInvoiceHeaderCopyOptionsForm>(ZFormModaliser.LastFormShownDialogForTest);

				AssertEquals("One invoice is copied", 2, declaration.Invoices.Count);
				AssertEquals("Two invoice lines are copied", 2, declaration.InvoiceLines.Count);
			}
		}

		public void TestUpdateImportEntryNumberMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				testMenu.Declaration = declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				Assert("'Update Entry Number' menu should be invisible", !testMenu.updateImportEntryNumberMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("'Update Entry Number' menu should be invisible", !testMenu.updateImportEntryNumberMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.RefreshMenu();
				Assert("'Update Entry Number' menu should be invisible", !testMenu.updateImportEntryNumberMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.RefreshMenu();
				Assert("'Update Entry Number' menu should be visible", testMenu.updateImportEntryNumberMenuItem.Visible);

				testMenu.Declaration = declaration;
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.updateImportEntryNumberMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				testMenu.updateImportEntryNumberMenuItem.PerformClick();
				AssertEquals("Declaration B00001000 has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);

				var entryheader = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();
				testMenu.updateImportEntryNumberMenuItem.PerformClick();
				AssertType<UpdateImportEntryNumberForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestUpdateEntryStatusMenuItem()
		{
			using (var testMenu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				testMenu.Declaration = declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				Assert("'Update Entry Status' menu should be invisible", !testMenu.updateImportEntryStatusMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("'Update Entry Status' menu should be invisible", !testMenu.updateImportEntryStatusMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				testMenu.RefreshMenu();
				Assert("'Update Entry Status' menu should be invisible", !testMenu.updateImportEntryStatusMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testMenu.RefreshMenu();
				Assert("'Update Entry Status' menu should be visible", testMenu.updateImportEntryStatusMenuItem.Visible);

				testMenu.Declaration = declaration;
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.updateImportEntryStatusMenuItem.PerformClick();
				AssertEquals("Ask pre save", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				testMenu.updateImportEntryStatusMenuItem.PerformClick();
				AssertEquals("Declaration B00001000 has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);

				var entryheader = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();
				testMenu.updateImportEntryStatusMenuItem.PerformClick();
				AssertType<UpdateImportEntryStatusForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
