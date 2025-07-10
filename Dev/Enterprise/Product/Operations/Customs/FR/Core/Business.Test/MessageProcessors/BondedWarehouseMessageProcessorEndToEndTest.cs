using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class BondedWarehouseMessageProcessorEndToEndTest : TestCaseWithFactory
	{
		public void TestInward()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var (declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper);
				var messageInitiator = (SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
				invoiceLine.JI_BondedWhsQuantity = 10;

				var inventoryAutomationAction = InventoryAutomationAction.Inward;
				// send first original message, the hold note should be created, whs status should be ICH.
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Original);
				AssertStmNoteHasQuantity(entry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertNoStmNote(entry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertSendMessageCalled();
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry.CH_WarehouseTransactionStatus);

				// update the whs quantity
				invoiceLine.JI_BondedWhsQuantity = 100;

				// send second original message, should be abandoned as the first original message hasn't got response.
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Original);
				AssertEquals(pendingError, messageInitiator.InvalidOperationText);
				AssertStmNoteHasQuantity(entry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertNoStmNote(entry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertSendMessageNotCalled();
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry.CH_WarehouseTransactionStatus);

				// send amendment message, the modification note should be created, whs status is still ICH.
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				AssertStmNoteHasQuantity(entry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertStmNoteHasQuantity(entry, WarehouseConstants.UniversalModificationShipmentNoteDescription, 100);
				AssertSendMessageCalled();
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry.CH_WarehouseTransactionStatus);

				// receive error, the modification should be discarded.
				SimulateProcessIncomingMessage(entry, "1000", EntryActionCodeList.Codes.MAP, "DeltaCImportErreurResponseMessage.xml");
				AssertStmNoteHasQuantity(entry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertNoStmNote(entry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry.CH_WarehouseTransactionStatus);

				// resend modification, and receive valid response. The modification note should be promoted to hold. (Whs quantity becomes 100)
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				SimulateProcessIncomingMessage(entry, "1001", EntryActionCodeList.Codes.MAP, "DeltaCImportANTResponseMessage.xml");
				AssertStmNoteHasQuantity(entry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 100);
				AssertNoStmNote(entry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertSendMessageCalled();
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry.CH_WarehouseTransactionStatus);

				// send validation, and receive BAE, now we should create inventory.
				SimulateProcessIncomingMessage(entry, "1002", EntryActionCodeList.Codes.VAA, "DeltaCImportBAEResponseMessage.xml");
				AssertNoStmNote(entry, WarehouseConstants.UniversalHoldShipmentNoteDescription);
				AssertNoStmNote(entry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreated, entry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 100);

				var query = new ZQuery(WhsDocketSchema.WD_TotalUnits, SQLComparisonOperator.Equal, 100);
				var dockets = Factory.Load<IWhsDocket>(query);
				AssertEquals(1, dockets.Length);
				var docketLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, dockets[0].PK);
				var docketLines = Factory.Load<IWhsDocketLine>(docketLineQuery);
				AssertEquals(1, docketLines.Length);
				AssertContains("After receiving the BAE message, entry status should be updated to 100.", "ENTRYSTATUS=100", docketLines[0].CustomsData.WB_AddInfo);

				// change whs quantity, and send REC, the inventory should be updated with new quantity.
				invoiceLine.JI_BondedWhsQuantity = 1000;
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardUpdatedPending, entry.CH_WarehouseTransactionStatus);
				var inventory = WhsDataTestHelper.GetWhsInventoryFromDatabase("1901204207-1").FirstOrDefault();
				AssertEquals(WhsDataTestHelper.InventoryHeldStatusCode, inventory.WI_InventoryStatus);

				// if REC is rejected with errors detected, we should rollback to the previous status.
				SimulateProcessIncomingMessage(entry, "1003", EntryActionCodeList.Codes.REC, "DeltaCImportErreurResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardUpdated, entry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 100);

				// re-send REC
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardUpdatedPending, entry.CH_WarehouseTransactionStatus);

				// if REC is rejected with status REF returned, we should rollback to the previous status.
				// note that when receiving REF, there will be a BAE (which indicates the previous status code) coming together with REF.
				SimulateProcessIncomingMessage(entry, "1004", EntryActionCodeList.Codes.REC, "DeltaCImportBAEResponseMessage.xml", "DeltaCImportREFResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardUpdated, entry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 100);

				// re-send REC
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardUpdatedPending, entry.CH_WarehouseTransactionStatus);

				// if REC is passed, we should update the quantity on inventory.
				SimulateProcessIncomingMessage(entry, "1005", EntryActionCodeList.Codes.REC, "DeltaCImportACCResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardUpdated, entry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 1000);

				// send INV for cancellation
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Withdrawal);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, entry.CH_WarehouseTransactionStatus);

				// if INV is rejected, nothing happened.
				SimulateProcessIncomingMessage(entry, "1006", EntryActionCodeList.Codes.INV, "DeltaCImportErreurResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardUpdated, entry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 1000);

				// re-send INV
				declaration.SendMessageWithBondedWarehouseAutomation(entry, inventoryAutomationAction, SendMessage, MessageAction.Withdrawal);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, entry.CH_WarehouseTransactionStatus);

				// if INV is accepted, the inventory should be withdrawn.
				SimulateProcessIncomingMessage(entry, "1007", EntryActionCodeList.Codes.INV, "DeltaCImportINVResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceled, entry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 0);
			}
		}

		public void TestOutward()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var (inwardDec, inwardEntry, inwardInvoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, "B00002377", "0000000000", true, 100000m);
				inwardInvoiceLine.JI_BondedWhsQuantity = 10000m;

				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

				var (outwardDec, outwardEntry, outwardInvoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, "B00002378", "0000000001", false, 1m);
				var outwardMessageInitiator = (SendsMessagesToCustomsShutterUpperer)outwardDec.MessageInitiator;
				outwardInvoiceLine.JI_BondedWhsQuantity = 10;

				var inventoryAutomationAction = InventoryAutomationAction.Outward;
				// send first original message, the hold note should be created, whs status should be OCP.
				outwardDec.SendMessageWithBondedWarehouseAutomation(outwardEntry, inventoryAutomationAction, SendMessage, MessageAction.Original);
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertSendMessageCalled();
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9990);

				// update the whs quantity
				outwardInvoiceLine.JI_BondedWhsQuantity = 100;

				// send second original message, should be abandoned as the first original message hasn't got response.
				outwardDec.SendMessageWithBondedWarehouseAutomation(outwardEntry, inventoryAutomationAction, SendMessage, MessageAction.Original);
				AssertEquals(pendingError, outwardMessageInitiator.InvalidOperationText);
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertSendMessageNotCalled();
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9990);

				// send amendment message, the modification note should be created, whs status is still OCP.
				outwardDec.SendMessageWithBondedWarehouseAutomation(outwardEntry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription, 100);
				AssertSendMessageCalled();
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9900);

				// receive error, the modification should be discarded.
				SimulateProcessIncomingMessage(outwardEntry, "1000", EntryActionCodeList.Codes.MAP, "DeltaCImportErreurResponseMessage.xml");
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9990);

				// resend modification, and receive valid response. The modification note should be promoted to hold. (Whs quantity becomes 100)
				outwardDec.SendMessageWithBondedWarehouseAutomation(outwardEntry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 10);
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription, 100);
				SimulateProcessIncomingMessage(outwardEntry, "1001", EntryActionCodeList.Codes.MAP, "DeltaCImportANTResponseMessage.xml");
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 100);
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertSendMessageCalled();
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9900);

				// send validation, and receive BAE, now we should create inventory.
				SimulateProcessIncomingMessage(outwardEntry, "1002", EntryActionCodeList.Codes.VAA, "DeltaCImportBAEResponseMessage.xml");
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription);
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreated, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9900);

				// change whs quantity, and send REC, the inventory should be updated with new quantity.
				outwardInvoiceLine.JI_BondedWhsQuantity = 1000;
				outwardDec.SendMessageWithBondedWarehouseAutomation(outwardEntry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				AssertStmNoteHasQuantity(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription, 1000);
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9000);

				// if REC is rejected, we should rollback to the previous status.
				SimulateProcessIncomingMessage(outwardEntry, "1003", EntryActionCodeList.Codes.REC, "DeltaCImportErreurResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardEntry.CH_WarehouseTransactionStatus);
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalHoldShipmentNoteDescription);
				AssertNoStmNote(outwardEntry, WarehouseConstants.UniversalModificationShipmentNoteDescription);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9900);

				// re-send REC
				outwardDec.SendMessageWithBondedWarehouseAutomation(outwardEntry, inventoryAutomationAction, SendMessage, MessageAction.Amendment);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardEntry.CH_WarehouseTransactionStatus);

				// if REC is passed, we should update the quantity on inventory.
				SimulateProcessIncomingMessage(outwardEntry, "1004", EntryActionCodeList.Codes.REC, "DeltaCImportACCResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9000);

				// send INV for cancellation
				outwardDec.SendMessageWithBondedWarehouseAutomation(outwardEntry, inventoryAutomationAction, SendMessage, MessageAction.Withdrawal);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardHolding, outwardEntry.CH_WarehouseTransactionStatus);

				// if INV is rejected, nothing happened.
				SimulateProcessIncomingMessage(outwardEntry, "1005", EntryActionCodeList.Codes.INV, "DeltaCImportErreurResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 9000);

				// re-send INV
				outwardDec.SendMessageWithBondedWarehouseAutomation(outwardEntry, inventoryAutomationAction, SendMessage, MessageAction.Withdrawal);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardHolding, outwardEntry.CH_WarehouseTransactionStatus);

				// if INV is accepted, the inventory should be withdrawn.
				SimulateProcessIncomingMessage(outwardEntry, "1006", EntryActionCodeList.Codes.INV, "DeltaCImportINVResponseMessage.xml");
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardEntry.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 10000);
			}
		}

		public void TestShouldNotUpdateBondedWarehouseIfNotPending()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var (declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper);
				invoiceLine.JI_BondedWhsQuantity = 10;

				SimulateProcessIncomingMessage(entry, "1001", EntryActionCodeList.Codes.EAV, "DeltaCImportBAEResponseMessage.xml");
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 0);
			}
		}

		public void TestShouldUpdateBondedWarehouseIfPending()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var (declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper);
				invoiceLine.JI_BondedWhsQuantity = 10;
				entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;

				SimulateProcessIncomingMessage(entry, "1001", EntryActionCodeList.Codes.EAV, "DeltaCImportBAEResponseMessage.xml");
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("1901204207-1", 10);
			}
		}

		public (JobDeclaration, CusEntryHeader, JobComInvoiceLine) CreateDeclarationWithBondedWarehouseSupported(WhsDataTestHelper helper, string declarationReference = "B00177613", string correlationID = "0000000001", bool isInward = true, decimal quantity = 1m)
		{
			var declaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, declarationReference, "1901204207", quantity);
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.SetSupportsBondedWarehousingForTesting(true);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			if (isInward)
			{
				entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;
			}
			else
			{
				entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;
			}

			var entry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Single();
			entry.CorrelationID = correlationID;
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var invoiceLine = entry.InvoiceLines.Cast<JobComInvoiceLine>().Single();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var procedure = isInward ? CreateInwardCusProcedure() : CreateOutwardCusProcedure();
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;

			return ((JobDeclaration)declaration, entry, invoiceLine);
		}

		void SimulateProcessIncomingMessage(CusEntryHeader entry, string interchangeNo, string outgoingMessageSubType, params string[] incomingMessageTestFiles)
		{
			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = interchangeNo;
			var outgoingMessage = Factory.New<FREDIMessage>();
			outgoingMessage.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			entry.Messages.Add(outgoingMessage);
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_MessageSubType = outgoingMessageSubType;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = interchangeNo + ".";
			Factory.Save();

			foreach (var incomingMessageTestFile in incomingMessageTestFiles)
			{
				var incomingMessage = Factory.NewWithValidTestData<DeltaCImportFREDIMessage>();
				incomingMessage.EM_EI = incomingInterchange.PK;
				incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				incomingMessage.EM_MessageText = ReadTestFiles(incomingMessageTestFile);
				new ImportDeltaCResponseMessageProcessor(new LoggingInformation()).ProcessMessage(incomingMessage);
				incomingMessage.Factory.Save();
			}
		}

		void AssertStmNoteHasQuantity(CusEntryHeader entry, string description, decimal whsQuantity)
		{
			var notes = entry.Notes;
			var note = notes.FindByDescription(description).Single();
			var xml = Compressor.UncompressAsString(note.ST_NoteData);
			AssertXMLContains($"<BondedWarehouseQuantity>{whsQuantity}</BondedWarehouseQuantity>", xml);
		}

		void AssertNoStmNote(CusEntryHeader entry, string description)
		{
			var notes = entry.Notes;
			if (notes == null)
			{
				Assert(true);
			}
			else
			{
				var notesFound = notes.FindByDescription(description);
				AssertEquals(true, notesFound == null || !notesFound.Any());
			}
		}

		void AssertSendMessageCalled()
		{
			AssertEquals(true, sendMessageCalled);
			sendMessageCalled = false;
		}

		void AssertSendMessageNotCalled()
		{
			AssertEquals(false, sendMessageCalled);
		}

		bool SendMessage()
		{
			sendMessageCalled = true;
			return true;
		}

		bool sendMessageCalled;
		readonly string pendingError = "There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs. Alternatively, disable the warehouse integration and re-enable it after receiving all response back from Customs.";

		RefCusProcedure CreateInwardCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "10", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();
			return procedure;
		}

		RefCusProcedure CreateOutwardCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "42", "71", "C33", "", "IMP", "42P");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			Factory.Save();
			return procedure;
		}

		string ReadTestFiles(string testFile)
		{
			return resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles." + testFile);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
