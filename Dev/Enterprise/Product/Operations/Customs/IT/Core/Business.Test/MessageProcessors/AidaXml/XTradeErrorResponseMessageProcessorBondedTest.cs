using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XTradeErrorResponseMessageProcessorBondedTest : XmlIncomingMessageProcessorTest<XTradeErrorResponseMessageProcessor>
{
	public void TestProcessMessage_CreateCancelTheWarehouseJobEvent_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForImportAndIsIntoWarehouseWarehousing()
	{
		const string resourceDetails = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml";
		var (inwardEntry, receivedMessage, invoiceLine1) = SetUpXTradeErrorResponseMessageProcessor(resourceDetails, interchangeType: "IMP", isInward: true);

		AssertEquals("PRE-CONDITION", true, invoiceLine1.IsIntoWarehouseWarehousing);
		inwardEntry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreationHeld;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(inwardEntry, 1);
		AssertEquals(nameof(inwardEntry.CH_Status), "FFT", inwardEntry.CH_Status);
		var cancelTheWarehouseJobLog = inwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
		AssertNotNull(cancelTheWarehouseJobLog);
		AssertEquals(true, cancelTheWarehouseJobLog.IsInDatabase);
	}

	public void TestProcessMessage_CreateCancelTheWarehouseJobEvent_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForExportAndIsOutOfWarehouseWarehousing()
	{
		const string resourceDetails = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml";
		var (outwardEntry, receivedMessage, invoiceLine1) = SetUpXTradeErrorResponseMessageProcessor(resourceDetails, interchangeType: "EXP", isInward: false);

		AssertEquals("PRE-CONDITION, IsOutOfWarehouseWarehousing", true, invoiceLine1.IsOutOfWarehouseWarehousing);
		AssertEquals("PRE-CONDITION, IsOutwardBondedWarehousingEnabled", true, outwardEntry.IsOutwardBondedWarehousingEnabled);
		outwardEntry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.OutwardHolding;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(outwardEntry, 1);
		AssertEquals(nameof(outwardEntry.CH_Status), "FFT", outwardEntry.CH_Status);
		var cancelTheWarehouseJobLog = outwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
		AssertNotNull(cancelTheWarehouseJobLog);
		AssertEquals(true, cancelTheWarehouseJobLog.IsInDatabase);
	}

	public void TestProcessMessage_DoNotUpdateWarehouseWhenResponseHasBusinessError()
	{
		const string resourceDetails = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_MultipleEvents_NotTransmissionFailure.xml";
		var (inwardEntry, receivedMessage, invoiceLine1) = SetUpXTradeErrorResponseMessageProcessor(resourceDetails, interchangeType: "IMP", isInward: true);
		AssertEquals("PRE-CONDITION", true, invoiceLine1.IsIntoWarehouseWarehousing);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(inwardEntry, 1);
		AssertEquals(nameof(inwardEntry.CH_Status), "AWO", inwardEntry.CH_Status);
		var cancelTheWarehouseJobLog = inwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
		AssertNull(cancelTheWarehouseJobLog);
	}

	public void TestProcessMessage_DoNotUpdateWarehouseWhenWarehouseStatusIsNotPending()
	{
		const string resourceDetails = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml";
		var (outwardEntry, receivedMessage, invoiceLine1) = SetUpXTradeErrorResponseMessageProcessor(resourceDetails, interchangeType: "EXP", isInward: false);

		AssertEquals("PRE-CONDITION, IsOutOfWarehouseWarehousing", true, invoiceLine1.IsOutOfWarehouseWarehousing);
		AssertEquals("PRE-CONDITION, IsOutwardBondedWarehousingEnabled", true, outwardEntry.IsOutwardBondedWarehousingEnabled);
		outwardEntry.CH_WarehouseTransactionStatus = "";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(outwardEntry, 1);
		AssertEquals(nameof(outwardEntry.CH_Status), "FFT", outwardEntry.CH_Status);
		AssertNull("CancelTheWarehouseJob", outwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob));
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "ERR" };

	protected override XTradeErrorResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
		=> new(logger);

	void AssertNumberOfErrorMessages(CusEntryHeader entryHeader, int expectedNumberOfErrorMessages)
	{
		var numberOfErrorMessages = entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == "ERR");
		AssertEquals("Number of Error Messages", expectedNumberOfErrorMessages, numberOfErrorMessages);
	}

	(CusEntryHeader inwardEntry, EDIMessage receivedMessage, JobComInvoiceLine invoiceLine1) SetUpXTradeErrorResponseMessageProcessor(string resourceDetails, string interchangeType, bool isInward)
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(resourceDetails);
		(var declaration, var inwardEntry, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "ERR");
		sentMessage.Interchange.EI_InterchangeType = interchangeType;
		inwardEntry.CH_Status = "AWO";

		var factory = Factory;
		var whsDataTestHelper = new WhsDataTestHelper(factory);
		declaration.JE_OH_Importer = whsDataTestHelper.Importer.PK;
		declaration.WarehouseDocAddress.E2_OA_Address = whsDataTestHelper.Warehouse.MainAddress.PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_OA_Warehouse2 = whsDataTestHelper.WhsWarehouse.WW_OA_WarehouseAddress;
		inwardEntry.CH_CEI_Instruction = entryInstruction.PK;
		inwardEntry.EntryNumber = "ENT001";

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var mergedLine = inwardEntry.MergedLines.AddNew();
		invoiceLine1.JI_CL = mergedLine.PK;
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_PartNo = whsDataTestHelper.Part.OP_PartNum;
		invoiceLine1.JI_InvoiceQuantity = 1;
		invoiceLine1.JI_CustomsQuantity = 1;
		invoiceLine1.JI_ValuationCode = MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;

		var procedure = WarehouseTestHelper.CreateOutwardCusProcedure(factory);
		if (isInward)
		{
			procedure = WarehouseTestHelper.CreateInwardCusProcedure(factory);
		}
		invoiceLine1.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;

		return (inwardEntry, receivedMessage, invoiceLine1);
	}
}
