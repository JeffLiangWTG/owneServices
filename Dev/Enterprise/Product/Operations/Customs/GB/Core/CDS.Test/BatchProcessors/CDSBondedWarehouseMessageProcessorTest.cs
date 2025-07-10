using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CDSResponse.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSBondedWarehouseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestHasBeenWithdrawn()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177613");
			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;

			var outgoingMessage = SimulateProcessIncomingMessage(entry, "1001", "CAN", processedCancelMessage);
			var bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is cleared and message sub type is 'CAN' therefore should be true", true, bondedWarehouseProcessor.HasBeenWithdrawnExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177614");
			entry.CH_EntryStatus = EntryStatusList.Codes.AwaitingResponse;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1002", "CAN", processedCancelMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is NOT cleared therefore should be false", false, bondedWarehouseProcessor.HasBeenWithdrawnExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177615");
			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1003", "CAN", processedCancelMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is cleared and message sub type is 'CAN' therefore should be true", true, bondedWarehouseProcessor.HasBeenWithdrawnExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177616");
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1004", "NEW", processedCancelMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is cleared but message sub type is 'NEW' therefore should be false", false, bondedWarehouseProcessor.HasBeenWithdrawnExposed);
		}

		public void TestIsAmendmentError()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177613");
			entry.CH_Status = EDIMessageStatusList.Codes.Error;

			var outgoingMessage = SimulateProcessIncomingMessage(entry, "1001", "AMD", rejectedMessage);
			var bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error and message sub type is 'AMD' therefore should be true", true, bondedWarehouseProcessor.IsAmendmentErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177614");
			entry.CH_Status = EDIMessageStatusList.Codes.Withdrawn;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1002", "AMD", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is NOT error therefore should be false", false, bondedWarehouseProcessor.IsAmendmentErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177615");
			entry.CH_Status = EDIMessageStatusList.Codes.Error;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1003", "AMD", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error and message sub type is 'AMD' therefore should be true", true, bondedWarehouseProcessor.IsAmendmentErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177616");
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1004", "NEW", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error but message sub type is 'NEW' therefore should be false", false, bondedWarehouseProcessor.IsAmendmentErrorExposed);
		}

		public void TestIsAmendmentClear()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177613");
			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;

			var outgoingMessage = SimulateProcessIncomingMessage(entry, "1001", "AMD", processedOKMessage);
			var bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is cleared and message sub type is 'AMD' therefore should be true", true, bondedWarehouseProcessor.IsAmendmentClearExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177614");
			entry.CH_EntryStatus = EntryStatusList.Codes.AwaitingResponse;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1002", "AMD", processedOKMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is NOT cleared therefore should be false", false, bondedWarehouseProcessor.IsAmendmentClearExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177615");
			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1003", "AMD", processedOKMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is cleared and message sub type is 'AMD' therefore should be true", true, bondedWarehouseProcessor.IsAmendmentClearExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177616");
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1004", "NEW", processedOKMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is cleared but message sub type is 'NEW' therefore should be false", false, bondedWarehouseProcessor.IsAmendmentClearExposed);
		}

		public void TestIsOriginalError()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177613");
			entry.CH_Status = EDIMessageStatusList.Codes.Error;

			var outgoingMessage = SimulateProcessIncomingMessage(entry, "1001", "NEW", rejectedMessage);
			var bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error and message sub type is 'NEW' therefore should be true", true, bondedWarehouseProcessor.IsOriginalErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177614");
			entry.CH_Status = EDIMessageStatusList.Codes.Withdrawn;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1002", "NEW", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is NOT error therefore should be false", false, bondedWarehouseProcessor.IsOriginalErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177615");
			entry.CH_Status = EDIMessageStatusList.Codes.Error;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1003", "NEW", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error and message sub type is 'NEW' therefore should be true", true, bondedWarehouseProcessor.IsOriginalErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177616");
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1004", "AMD", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error but message sub type is 'AMD' therefore should be false", false, bondedWarehouseProcessor.IsOriginalErrorExposed);
		}

		public void TestIsWithdrawalError()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177613");
			entry.CH_Status = EDIMessageStatusList.Codes.Error;

			var outgoingMessage = SimulateProcessIncomingMessage(entry, "1001", "CAN", rejectedMessage);
			var bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error and message sub type is 'CAN' therefore should be true", true, bondedWarehouseProcessor.IsWithdrawalErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177614");
			entry.CH_Status = EDIMessageStatusList.Codes.Withdrawn;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1002", "CAN", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is NOT error therefore should be false", false, bondedWarehouseProcessor.IsWithdrawalErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177615");
			entry.CH_Status = EDIMessageStatusList.Codes.Error;
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1003", "CAN", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error and message sub type is 'CAN' therefore should be true", true, bondedWarehouseProcessor.IsWithdrawalErrorExposed);

			(declaration, entry, invoiceLine) = CreateDeclarationWithBondedWarehouseSupported(helper, declarationReference: "B00177616");
			outgoingMessage = SimulateProcessIncomingMessage(entry, "1004", "AMD", rejectedMessage);
			bondedWarehouseProcessor = new CDSBondedWarehouseMessageProcessorForTest(outgoingMessage.PK, null, (EmailDef emailDef, EDIMessage message) => { });
			AssertEquals("Status is error but message sub type is 'AMD' therefore should be false", false, bondedWarehouseProcessor.IsWithdrawalErrorExposed);
		}

		CDSEDIMessage SimulateProcessIncomingMessage(Business.Declaration.CusEntryHeader entry, string interchangeNo, string outgoingMessageSubType, string incomingMessageText)
		{
			entry.Messages.RemoveAndDeleteAll();
			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = interchangeNo;
			var outgoingMessage = Factory.New<CDSEDIMessage>();
			outgoingMessage.MessageNumberStrategy = new Business.GbMessageNumberStrategy(Factory, Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
			entry.Messages.Add(outgoingMessage);
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_MessageSubType = outgoingMessageSubType;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "CONVERSATIONID";

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = interchangeNo + ".";
			Factory.Save();

			var incomingMessage = Factory.NewWithValidTestData<CDSResponseEDIMessage>();
			incomingMessage.EM_EI = incomingInterchange.PK;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationReference = "CONVERSATIONID";
			incomingMessage.EM_MessageText = incomingMessageText;
			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(incomingMessage);
			incomingMessage.Factory.Save();

			return outgoingMessage;
		}

		protected override void SetUp()
		{
			base.SetUp();
			ResponseFunctionTests.SetUpZZRefData(Factory);
		}

		(JobDeclaration, Business.Declaration.CusEntryHeader, JobComInvoiceLine) CreateDeclarationWithBondedWarehouseSupported(WhsDataTestHelper helper, string declarationReference, string correlationID = "0000000001", bool isInward = true, decimal quantity = 1m)
		{
			var declaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, declarationReference, "1901204207", quantity);
			declaration.JE_ApplicationCode = "CDS";
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

			var entry = declaration.CustomsEntryHeaders.Cast<Business.Declaration.CusEntryHeader>().Single();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var invoiceLine = entry.InvoiceLines.Cast<JobComInvoiceLine>().Single();
			invoiceLine.JI_CEI = entryInstruction.PK;

			return ((JobDeclaration)declaration, entry, invoiceLine);
		}

		readonly ZString processedOKMessage = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>09</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";

		readonly ZString processedCancelMessage = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>10</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";

		readonly ZString rejectedMessage = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
	}

	class CDSBondedWarehouseMessageProcessorForTest : CDSBondedWarehouseMessageProcessor
	{
		internal CDSBondedWarehouseMessageProcessorForTest(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail)
			: base(messagePK, emailReportThatHasBeenDelayed, sendMail)
		{
		}

		public bool HasBeenWithdrawnExposed => HasBeenWithdrawn;

		public bool IsAmendmentErrorExposed => IsAmendmentError;

		public bool IsAmendmentClearExposed => IsAmendmentClear;

		public bool IsOriginalErrorExposed => IsOriginalError;

		public bool IsWithdrawalErrorExposed => IsWithdrawalError;
	}
}
