using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AcknowledgementResponseMessageProcessorBondedTest : XmlIncomingMessageProcessorTest<AcknowledgementResponseMessageProcessor>
{
	public void TestProcessNegativeAcknowledgement_CreateCancelTheWarehouseJobEvent_WhenIsIntoWarehouseWarehousing()
	{
		var (inwardEntry, receivedMessage, invoiceLine) = SetupAcknowledgementResponseMessageProcessorBonded(isInward: true);
		AssertEquals("PRE-CONDITION, IsIntoWarehouseWarehousing", true, invoiceLine.IsIntoWarehouseWarehousing);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfAcknowledgement(inwardEntry, 1);
		AssertEquals(nameof(inwardEntry.CH_Status), "ERO", inwardEntry.CH_Status);
		var cancelTheWarehouseJobLog = inwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
		AssertNotNull(cancelTheWarehouseJobLog);
		AssertEquals(true, cancelTheWarehouseJobLog.IsInDatabase);
	}

	public void TestProcessNegativeAcknowledgement_CreateCancelTheWarehouseJobEvent_WhenIsOutOfWarehouseWarehousing()
	{
		var (outwardEntry, receivedMessage, invoiceLine) = SetupAcknowledgementResponseMessageProcessorBonded(isInward: false);
		AssertEquals("PRE-CONDITION, IsOutOfWarehouseWarehousing", true, invoiceLine.IsOutOfWarehouseWarehousing);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfAcknowledgement(outwardEntry, 1);
		AssertEquals(nameof(outwardEntry.CH_Status), "ERO", outwardEntry.CH_Status);
		var cancelTheWarehouseJobLog = outwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
		AssertNotNull(cancelTheWarehouseJobLog);
		AssertEquals(true, cancelTheWarehouseJobLog.IsInDatabase);
	}

	public void TestProcessAcknowledgement_WhenIsOutOfWarehouseButWarehouseButWarehouseIsNotInAValidPendingStatus()
	{
		var (outwardEntry, receivedMessage, invoiceLine) = SetupAcknowledgementResponseMessageProcessorBonded(isInward: false);
		outwardEntry.EntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		outwardEntry.CH_WarehouseTransactionStatus = ZString.Empty;
		AssertEquals("PRE-CONDITION, IsOutOfWarehouseWarehousing", true, invoiceLine.IsOutOfWarehouseWarehousing);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfAcknowledgement(outwardEntry, 1);
		AssertEquals(nameof(outwardEntry.CH_Status), "ERO", outwardEntry.CH_Status);
		AssertNull("No CancelTheWarehouseJob event should be logged", outwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob));
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "ACK" };

	protected override AcknowledgementResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
	{
		return new AcknowledgementResponseMessageProcessor(logger);
	}

	(CusEntryHeader entry, EDIMessage receivedMessage, JobComInvoiceLine invoiceLine)
		SetupAcknowledgementResponseMessageProcessorBonded(bool isInward)
	{
		var negativeAcknowledgement3 = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("3", "L'Autorità di certificazione non è ritenuta sicura");
		(var declaration, var entry, _, var receivedMessage) = PrepareTestData(messageText: negativeAcknowledgement3, messageType: "ACK");
		entry.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var factory = Factory;
		var whsDataTestHelper = new WhsDataTestHelper(factory);
		declaration.JE_OH_Importer = whsDataTestHelper.Importer.PK;
		declaration.WarehouseDocAddress.E2_OA_Address = whsDataTestHelper.Warehouse.MainAddress.PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_OA_Warehouse2 = whsDataTestHelper.WhsWarehouse.WW_OA_WarehouseAddress;
		entry.CH_CEI_Instruction = entryInstruction.PK;
		entry.EntryNumber = "ENT001";

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine = invoice1.InvoiceLines.AddNew();
		var mergedLine = entry.MergedLines.AddNew();
		invoiceLine.JI_CL = mergedLine.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_PartNo = whsDataTestHelper.Part.OP_PartNum;
		invoiceLine.JI_InvoiceQuantity = 1;
		invoiceLine.JI_CustomsQuantity = 1;
		invoiceLine.JI_ValuationCode = MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;

		var procedure = whsDataTestHelper.OutwardCusProcedure;
		entry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.OutwardHolding;
		if (isInward)
		{
			procedure = whsDataTestHelper.InwardCusProcedure;
			entry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreationHeld;
		}
		invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;

		return (entry, receivedMessage, invoiceLine);
	}

	void AssertNumberOfAcknowledgement(CusEntryHeader entryHeader, int expectedNumberOfMessages)
	{
		var numberOfMessages = entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == "ACK");

		AssertEquals("Number of ACK messages", expectedNumberOfMessages, numberOfMessages);
	}
}
