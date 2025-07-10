using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ResponseMessageProcessorBondedTest : XmlIncomingMessageProcessorTest<ResponseMessageProcessor>
{
	public void TestProcessImportNegativeResponse_CreateCancelTheWarehouseJobEvent_WhenIsIntoWarehouseWarehousing()
	{
		const string resourceDetails = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_NegativeResponse.xml";
		var (inwardEntry, receivedMessage, invoiceLine) = SetupResponseMessageProcessor(resourceDetails, inward: true, declarationType: "IMP");

		AssertEquals("PRE-CONDITION", true, invoiceLine.IsIntoWarehouseWarehousing);
		inwardEntry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreationHeld;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfResponseMessages(inwardEntry, 1);
		AssertEquals(nameof(inwardEntry.CH_Status), "ERO", inwardEntry.CH_Status);

		var cancelTheWarehouseJobLog = inwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
		AssertNotNull(cancelTheWarehouseJobLog);
		AssertEquals(true, cancelTheWarehouseJobLog.IsInDatabase);
	}

	public void TestProcessImportNegativeResponse_CreateCancelTheWarehouseJobEvent_WhenIsOutOfWarehouseWarehousing()
	{
		const string resourceDetails = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_NegativeResponse.xml";
		var (outwardEntry, receivedMessage, invoiceLine) = SetupResponseMessageProcessor(resourceDetails, inward: false, declarationType: "IMP");

		AssertEquals("PRE-CONDITION, IsOutOfWarehouseWarehousing", true, invoiceLine.IsOutOfWarehouseWarehousing);
		outwardEntry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCreatedPending;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfResponseMessages(outwardEntry, 1);
		AssertEquals(nameof(outwardEntry.CH_Status), "ERO", outwardEntry.CH_Status);

		var cancelTheWarehouseJobLog = outwardEntry.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
		AssertNotNull(cancelTheWarehouseJobLog);
		AssertEquals(true, cancelTheWarehouseJobLog.IsInDatabase);
	}

	public void TestProcessImportPositiveWholeClearanceResponse_CreateWarehouseJobCanNowBeFinalisedEvent_WhenIsIntoWarehouseWarehousing()
	{
		const string resourceDetails = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponseWithClearance.xml";
		var (inwardEntry, receivedMessage, invoiceLine) = SetupResponseMessageProcessor(resourceDetails, inward: true, declarationType: "IMP");

		AssertEquals("PRE-CONDITION", true, invoiceLine.IsIntoWarehouseWarehousing);
		inwardEntry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreationHeld;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfResponseMessages(inwardEntry, 1);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(inwardEntry.CH_Status), "CLO", inwardEntry.CH_Status);
			AssertEquals(nameof(inwardEntry.CH_EntryStatus), "ICC", inwardEntry.CH_EntryStatus);

			var acceptLog = inwardEntry.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised);
			AssertNotNull(acceptLog);
			AssertEquals(true, acceptLog.IsInDatabase);
		});
	}

	public void TestProcessExportPositiveWholeClearanceResponse_CreateWarehouseJobCanNowBeFinalisedEvent_WhenIsOutOfWarehouseWarehousing()
	{
		const string resourceDetails = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveResponseWithClearance.xml";
		var (outwardEntry, receivedMessage, invoiceLine) = SetupResponseMessageProcessor(resourceDetails, inward: false, declarationType: "EXP");

		AssertEquals("PRE-CONDITION", true, invoiceLine.IsOutOfWarehouseWarehousing);
		outwardEntry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.OutwardHolding;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfResponseMessages(outwardEntry, 1);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(outwardEntry.CH_Status), "CLO", outwardEntry.CH_Status);
			AssertEquals(nameof(outwardEntry.CH_EntryStatus), "ECC", outwardEntry.CH_EntryStatus);
			var acceptLog = outwardEntry.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised);
			AssertNotNull(acceptLog);
			AssertEquals(true, acceptLog.IsInDatabase);
		});
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "RES" };

	protected override ResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
	{
		return new ResponseMessageProcessor(logger);
	}

	void AssertNumberOfResponseMessages(CusEntryHeader entryHeader, int expectedAcknowledgement)
	{
		AssertEquals("Number of Response Messages", expectedAcknowledgement, entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == "RES"));
	}

	(CusEntryHeader entry, EDIMessage receivedMessage, JobComInvoiceLine invoiceLine) SetupResponseMessageProcessor(string resourceDetails, bool inward, string declarationType)
	{
		var factory = Factory;
		var response = ManifestResourceHelper.ReadManifestResourceContent(resourceDetails);
		(var declaration, var entry, _, var receivedMessage) = PrepareTestData(declarationType: declarationType, messageText: response, messageType: "RES");
		declaration.JE_CustomsOffice = "IT279100";
		entry.CH_Status = Common.Shared.MessageStatusList.Codes.AcknowledgedOriginal;

		var whsDataTestHelper = new WhsDataTestHelper(factory);
		declaration.JE_OH_Importer = whsDataTestHelper.Importer.PK;
		declaration.WarehouseDocAddress.E2_OA_Address = whsDataTestHelper.Warehouse.MainAddress.PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();

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

		entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;
		var procedure = WarehouseTestHelper.CreateOutwardCusProcedure(factory, declarationType);
		if (inward)
		{
			entryInstruction.CEI_OA_Warehouse2 = whsDataTestHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			procedure = WarehouseTestHelper.CreateInwardCusProcedure(factory);
		}
		invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;

		return (entry, receivedMessage, invoiceLine);
	}
}
