using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR20SupporterProviderTest : XMLMessageTestHelper<GOVCBRR20SupporterProviderTest>
	{
		public void TestGetSupporterFor()
		{
			//system is able to find GOVCBRR20ExportSupporter with 830, 5AS.
			//system finds null with everything else.
			var supporterProvider = new MultiPurposeResponseSupporterProvider();
			AssertNotNull(supporterProvider.GetSupporterForR20(ElectronicDocumentTypeList.Codes._830));
			AssertNotNull(supporterProvider.GetSupporterForR20(ElectronicDocumentTypeList.Codes._5AS));
			AssertNotNull(supporterProvider.GetSupporterForR20(ElectronicDocumentTypeList.Codes._DKJ));
			AssertNull(supporterProvider.GetSupporterForR20("AAA"));
		}

		public void TestD72()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);

			var outgoingMessage1 = CreateOutgoingMessage(entry, ElectronicDocumentTypeList.Codes._D72);
			outgoingMessage1.EM_ApplicationReference = "1";

			var outgoingMessage2 = CreateOutgoingMessage(entry, ElectronicDocumentTypeList.Codes._D72);
			outgoingMessage2.EM_ApplicationReference = "2";

			var incomingMessage2 = CreateMessageForTest("GOVCBRR20_D72_VersionID2.xml");
			var incomingMessage1 = CreateMessageForTest("GOVCBRR20_D72.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage1.Reload();
			incomingMessage2.Reload();

			AssertEquals("message ApplicationReference is updated to Outgoing Message No.", outgoingMessage1.EM_MessageNum, incomingMessage1.EM_ApplicationReference);
			AssertEquals("message ApplicationReference is updated to Outgoing Message No.", outgoingMessage2.EM_MessageNum, incomingMessage2.EM_ApplicationReference);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();
		}

		void CreateEntryForImport(bool setCusAgent, ZString entryType)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			importEntry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = importEntry.MergedLines.AddNew().PK;
			var entryNumber = importEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
		}
		JobDeclaration declaration;
		CusEntryHeader importEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForImport(ZString entryType, ZString em_MessageType)
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true, entryType);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_MessageType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR20MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}
		EDIMessage CreateOutgoingMessage(CusEntryHeader entry, string em_messgeType)
		{
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_messgeType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = entry.PK;
			outgoingMessage.EM_LinkedObject = entry;
			outgoingMessage.EM_MessageSubType = "";
			return outgoingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Common.Incoming";
	}
}
