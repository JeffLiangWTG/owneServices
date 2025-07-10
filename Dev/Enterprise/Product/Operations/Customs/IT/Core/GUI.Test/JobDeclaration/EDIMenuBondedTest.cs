using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction;
using GlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;
using ITGlbCompanyWrapper = Enterprise.Customs.IT.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EDIMenuBondedTest : TestCaseForAttachGUI
{
	public void TestSendCustomsMessages_WarehouseTransactionStatusIsInwardCreationHeld_WhenIsIntoWarehouseWarehousing()
	{
		var factory = Factory;
		var whsDataTestHelper = new WhsDataTestHelper(factory);
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var menu = new EDIMenu())
		{
			menu.Declaration = declaration;

			var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");
			declaration.JE_MessageType = "IMP";

			declaration.JE_OH_Importer = whsDataTestHelper.Importer.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = whsDataTestHelper.Warehouse.MainAddress.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H2";
			entryInstruction.CEI_OA_Warehouse2 = whsDataTestHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.EntryNumber = "ENT001";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = mergedLine.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = whsDataTestHelper.Part.OP_PartNum;
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_CustomsQuantity = 1;
			invoiceLine1.JI_ValuationCode = MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			var procedure = WarehouseTestHelper.CreateInwardCusProcedure(factory);
			invoiceLine1.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
			AssertEquals("PRE-CONDITION, IsIntoWarehouseWarehousing", true, invoiceLine1.IsIntoWarehouseWarehousing);
			AssertEquals("PRE-CONDITION, IsInwardBondedWarehousingEnabled", true, entryHeader.IsInwardBondedWarehousingEnabled);

			declaration.JE_CustomsProfile = "1111-DEC1";
			factory.Save();
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(form
				=>
			{
				var messageSendingForm = (MessageSendingForm)form;
				var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
				messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				messageSendingObjectParent.SendingObjectsCollection[0].MessageType = EDIMessageTypeList.Codes.NewDeclaration;
			});
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendCustomsMessagesMenuItem.PerformClick();
			CombineAssertions(() =>
			{
				AssertEquals("Last message after sending", "Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Messages count after sending", 1, entryHeader.Messages.Count);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entryHeader.CH_WarehouseTransactionStatus);
				AssertStmNoteHasQuantity(entryHeader, WarehouseConstants.UniversalHoldShipmentNoteDescription, 1);
			});
		}
	}

	public void TestSendCustomsMessages_WarehouseTransactionStatusIsOutwardCreatedPending_WhenIsOutOfWarehouseWarehousing()
	{
		var factory = Factory;
		var helper = new WhsDataTestHelper(factory);
		using (helper.WhsHelper.UsePutawayEngineManagerMock())
		using (helper.WhsHelper.UseAllocationEngineMock())
		{
			var (inwardDec, inwardEntry, inwardInvoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, "B00002377", true, 100000m);
			inwardInvoiceLine.JI_BondedWhsQuantity = 10000m;

			inwardEntry.PublishShipmentForWHSInward(false);
			inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

			using (var menu = new EDIMenu())
			{
				var (outwardDec, outwardEntry, outwardInvoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, "B00002378", false, 1m);
				outwardInvoiceLine.JI_BondedWhsQuantity = 10;
				outwardInvoiceLine.EntryInstruction.CEI_Style = "H1";
				using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(outwardDec, true))
				{
					menu.Declaration = outwardDec;
					var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");

					AssertEquals("PRE-CONDITION, IsOutOfWarehouseWarehousing", true, outwardInvoiceLine.IsOutOfWarehouseWarehousing);
					AssertEquals("PRE-CONDITION, IsOutwardBondedWarehousingEnabled", true, outwardEntry.IsOutwardBondedWarehousingEnabled);
					outwardDec.JE_CustomsProfile = "1111-DEC1";
					factory.Save();

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(form
						=>
					{
						var messageSendingForm = (MessageSendingForm)form;
						var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
						messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
						messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
						messageSendingObjectParent.SendingObjectsCollection[0].MessageType = EDIMessageTypeList.Codes.NewDeclaration;
					});
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					sendCustomsMessagesMenuItem.PerformClick();
					CombineAssertions(() =>
					{
						AssertEquals("Last message after sending", "Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Messages count after sending", 1, outwardEntry.Messages.Count);
						AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9990);
					});
				}
			}
		}
	}

	public void TestSendCustomsMessages_WhenWarehouseIsRequestedButIsMissing()
	{
		using var formForTest = new JobDeclarationFormForTest(declaration);
		formForTest.Show();

		using var menu = formForTest.EDIMenu;
		menu.Declaration = declaration;

		var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");

		declaration.JE_MessageType = "IMP";
		declaration.JE_ScreeningStatus = "CLR";
		declaration.JE_CustomsProfile = "TEST-DEC1";
		declaration.JE_OH_Importer = new WhsDataTestHelper(Factory).Importer.PK;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;

		entryInstruction.CEI_Style = "H2";
		entryInstruction.CEI_Procedure = "71";

		var procedure = WarehouseTestHelper.CreateInwardCusProcedure(Factory);
		invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
		AssertEquals("PRE-CONDITION, IsIntoWarehouseWarehousing", true, invoiceLine.IsIntoWarehouseWarehousing);
		AssertEquals("PRE-CONDITION, IsInwardBondedWarehousingEnabled", true, entryHeader.IsInwardBondedWarehousingEnabled);

		Factory.Save();

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormShown(form
			=>
		{
			var messageSendingForm = (MessageSendingForm)form;
			var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
			messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		});

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		sendCustomsMessagesMenuItem.PerformClick();

		var text = UnitTestUserNotification.Instance.LastMessage.Text;

		AssertContains("Last message after sending", "No Warehouse was provided.", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("Messages count after sending", 0, entryHeader.Messages.Count);
	}

	public void TestSendCancellationMessage_WarehouseTransactionStatusInwardCanceledPendingWithdrawal_WhenIsIntoWarehouseWarehousing()
	{
		var factory = Factory;
		var whsDataTestHelper = new WhsDataTestHelper(factory);
		using var menu = new EDIMenu();
		var (declaration, entryHeader, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(whsDataTestHelper, "B00002378", true);
		menu.Declaration = declaration;

		declaration.SendMessageWithBondedWarehouseAutomation(entryHeader, InventoryAutomationAction.Inward, () => true, MessageAction.Original);
		entryHeader.PublishShipmentForWHSInwardFromLastHoldOrLatestData();
		entryHeader.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

		AssertEquals("PRE-CONDITION, IsIntoWarehouseWarehousing", true, entryHeader.IsIntoWarehouseWarehousing);
		AssertEquals("PRE-CONDITION, IsInwardBondedWarehousingEnabled", true, entryHeader.IsInwardBondedWarehousingEnabled);
		AssertEquals("PRE-CONDITION, CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, entryHeader.CH_WarehouseTransactionStatus);

		factory.Save();

		var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormShown(form =>
		{
			var messageSendingForm = (MessageSendingForm)form;
			var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
			messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			messageSendingObjectParent.SendingObjectsCollection[0].MessageType = EDIMessageTypeList.Codes.Cancellation;
		});
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		sendCustomsMessagesMenuItem.PerformClick();
		CombineAssertions(() =>
		{
			AssertEquals("Last message after sending", "Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Messages count after sending", 1, entryHeader.Messages.Count);
			AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, entryHeader.CH_WarehouseTransactionStatus);
			var cancelTheWarehouseJobLog = entryHeader.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
			AssertNotNull(cancelTheWarehouseJobLog);
		});
	}

	public void TestSendCancellationMessage_WarehouseTransactionStatusOutwardHolding_WhenIsOutOfWarehouseWarehousing()
	{
		var factory = Factory;
		var whsDataTestHelper = new WhsDataTestHelper(factory);

		var (inwardDec, inwardEntry, inwardInvoiceLine) = CreateDeclarationWithBondedWarehouseSupported(whsDataTestHelper, "B00002377", true, 100000);
		inwardInvoiceLine.JI_BondedWhsQuantity = 10000;

		inwardEntry.PublishShipmentForWHSInward(false);
		inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

		using var menu = new EDIMenu();
		var (declaration, entryHeader, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(whsDataTestHelper, "B00002378", false, 1);
		menu.Declaration = declaration;

		declaration.SendMessageWithBondedWarehouseAutomation(entryHeader, InventoryAutomationAction.Outward, () => true, MessageAction.Original);
		entryHeader.PublishShipmentForWHSOutwardFromLastHoldOrLatestData();
		entryHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);

		AssertEquals("PRE-CONDITION, IsOutOfWarehouseWarehousing", true, invoiceLine.IsOutOfWarehouseWarehousing);
		AssertEquals("PRE-CONDITION, IsOutwardBondedWarehousingEnabled", true, entryHeader.IsOutwardBondedWarehousingEnabled);
		AssertEquals("PRE-CONDITION, CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, entryHeader.CH_WarehouseTransactionStatus);

		factory.Save();

		var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormShown(form =>
		{
			var messageSendingForm = (MessageSendingForm)form;
			var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
			messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			messageSendingObjectParent.SendingObjectsCollection[0].MessageType = EDIMessageTypeList.Codes.Cancellation;
		});
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		sendCustomsMessagesMenuItem.PerformClick();
		CombineAssertions(() =>
		{
			AssertEquals("Last message after sending", "Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Messages count after sending", 1, entryHeader.Messages.Count);
			AssertEquals(WarehouseTransactionStatusList.Codes.OutwardHolding, entryHeader.CH_WarehouseTransactionStatus);
			var holdTheWarehouseOrderLog = entryHeader.Logs.MostRecentLogByEventTime(Events.HoldTheWarehouseOrder);
			AssertNotNull(holdTheWarehouseOrderLog);
		});
	}

	public void TestSendAmendmentMessage_WarehouseTransactionStatusInwardCreationHeld_WhenIsIntoWarehouseWarehousing()
	{
		var factory = Factory;
		var whsDataTestHelper = new WhsDataTestHelper(factory);
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var menu = new EDIMenu())
		{
			menu.Declaration = declaration;

			var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");
			declaration.JE_MessageType = "IMP";

			declaration.JE_OH_Importer = whsDataTestHelper.Importer.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = whsDataTestHelper.Warehouse.MainAddress.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H2";
			entryInstruction.CEI_OA_Warehouse2 = whsDataTestHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.EntryNumber = "ENT001";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = mergedLine.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = whsDataTestHelper.Part.OP_PartNum;
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_CustomsQuantity = 1;
			invoiceLine1.JI_ValuationCode = MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			var procedure = WarehouseTestHelper.CreateInwardCusProcedure(factory);
			invoiceLine1.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;

			AssertEquals("PRE-CONDITION, IsIntoWarehouseWarehousing", expected: true, invoiceLine1.IsIntoWarehouseWarehousing);
			AssertEquals("PRE-CONDITION, IsInwardBondedWarehousingEnabled", expected: true, entryHeader.IsInwardBondedWarehousingEnabled);

			declaration.JE_CustomsProfile = "1111-DEC1";
			entryHeader.MovementReferenceNumberSetter("24ITQYG08AAB1956J4");
			factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(form
				=>
			{
				var messageSendingForm = (MessageSendingForm)form;
				var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
				messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				messageSendingObjectParent.SendingObjectsCollection[0].MessageType = EDIMessageTypeList.Codes.Amendment;
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendCustomsMessagesMenuItem.PerformClick();
			CombineAssertions(() =>
			{
				AssertEquals("Last message after sending", "Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Messages count after sending", 1, entryHeader.Messages.Count);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entryHeader.CH_WarehouseTransactionStatus);
				AssertStmNoteHasQuantity(entryHeader, WarehouseConstants.UniversalHoldShipmentNoteDescription, 1);
			});
		}
	}

	public void TestSendAmendmentMessage_WarehouseTransactionStatusOutwardCreatedPending_WhenIsOutOfWarehouseWarehousing()
	{
		var factory = Factory;
		var helper = new WhsDataTestHelper(factory);
		using (helper.WhsHelper.UsePutawayEngineManagerMock())
		using (helper.WhsHelper.UseAllocationEngineMock())
		{
			var (inwardDec, inwardEntry, inwardInvoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, "B00002377", true, 100000m);
			inwardInvoiceLine.JI_BondedWhsQuantity = 10000m;

			inwardEntry.PublishShipmentForWHSInward(false);
			inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

			using (var menu = new EDIMenu())
			{
				var (outwardDec, outwardEntry, outwardInvoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, "B00002378", false, 1m);
				outwardInvoiceLine.JI_BondedWhsQuantity = 10;
				outwardInvoiceLine.EntryInstruction.CEI_Style = "H1";
				using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(outwardDec, true))
				{
					menu.Declaration = outwardDec;
					var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");

					AssertEquals("PRE-CONDITION, IsOutOfWarehouseWarehousing", expected: true, outwardInvoiceLine.IsOutOfWarehouseWarehousing);
					AssertEquals("PRE-CONDITION, IsOutwardBondedWarehousingEnabled", expected: true, outwardEntry.IsOutwardBondedWarehousingEnabled);

					outwardDec.JE_CustomsProfile = "1111-DEC1";
					outwardEntry.MovementReferenceNumberSetter("24ITQYG08AAB1956J4");
					factory.Save();

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(form
						=>
					{
						var messageSendingForm = (MessageSendingForm)form;
						var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
						messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
						messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
						messageSendingObjectParent.SendingObjectsCollection[0].MessageType = EDIMessageTypeList.Codes.Amendment;
					});
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					sendCustomsMessagesMenuItem.PerformClick();
					CombineAssertions(() =>
					{
						AssertEquals("Last message after sending", "Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Messages count after sending", 1, outwardEntry.Messages.Count);
						AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9990);
					});
				}
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		var factory = Factory;
		factory.New<OrgHeader>().OH_Code = "DEC1";
		declaration = factory.NewWithValidTestData<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";

		MockAndSubstituteCryptoApi();
		SetUpDeclarantAndCompany();
		SetUpStaff();
	}

	(JobDeclaration, CusEntryHeader, JobComInvoiceLine) CreateDeclarationWithBondedWarehouseSupported(WhsDataTestHelper helper, string declarationReference = "B00177613", bool isInward = true, decimal quantity = 1m)
	{
		var declaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, declarationReference, "1901204207", quantity);
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.SetSupportsBondedWarehousingForTesting(true);

		var entryInstruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
		if (isInward)
		{
			entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;
		}
		else
		{
			entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;
		}

		var entry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Single();
		entry.CH_CEI_Instruction = entryInstruction.PK;

		var invoiceLine = entry.InvoiceLines.Cast<JobComInvoiceLine>().Single();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var procedure = isInward ? WarehouseTestHelper.CreateInwardCusProcedure(Factory) : WarehouseTestHelper.CreateOutwardCusProcedure(Factory);
		invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
		return ((JobDeclaration)declaration, entry, invoiceLine);
	}

	void AssertStmNoteHasQuantity(CusEntryHeader entry, string description, decimal whsQuantity)
	{
		var notes = entry.Notes;
		var note = notes.FindByDescription(description).Single();
		AssertNotNull(note);
		var xml = CargoWise.IO.Compressor.UncompressAsString(note.ST_NoteData);
		AssertXMLContains($"<BondedWarehouseQuantity>{whsQuantity:F5}</BondedWarehouseQuantity>", xml);
	}

	void SetUpStaff()
	{
		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
		cryptokiCertificate.TokenPinStore.SetPin("123ABC");
	}

	void SetUpDeclarantAndCompany()
	{
		var factory = Factory;
		var declarant = factory.NewWithValidTestData<OrgHeader>();
		factory.Save();
		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", declarant.OH_Code)
			.Build();

		var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var companyWrapper = ITGlbCompanyWrapper.Get(company);
		var mauPassword = companyWrapper.PasswordCollection.AddNew();
		mauPassword.GP_UserID = "1111-DEC1";

		factory.Save();
	}

	void MockAndSubstituteCryptoApi()
	{
		var cryptoApiMock = new Mock<ICryptoApi>();
		cryptoApiMock
			.Setup(x =>
				x.SignXadesWithToken(
					It.IsAny<byte[]>(),
					It.IsAny<Chipset>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<DateTime>())
				)
			.Returns(Array.Empty<byte>());
		ObjectFactory.Substitute(cryptoApiMock.Object);
	}

	JobDeclaration declaration;
}
