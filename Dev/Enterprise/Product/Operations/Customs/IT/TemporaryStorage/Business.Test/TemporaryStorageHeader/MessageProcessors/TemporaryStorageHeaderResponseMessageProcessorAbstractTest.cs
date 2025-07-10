using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

abstract class TemporaryStorageHeaderResponseMessageProcessorAbstractTest<T> : TestCaseWithFactory
	where T : XmlIncomingMessageProcessor
{
	protected (TemporaryStorageHeader header, TemporaryStorageBill bill, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestData(string messageText, string responseMessageType = "RES", string sentMessageType = "NEW")
	{
		var header = Factory.New<TemporaryStorageHeader>();

		var bill = header.Bills.FirstOrDefault() ?? header.Bills.AddNew();
		var item1 = bill.PackedItems.AddNew();
		item1.API_LineNo = 1;
		var item2 = bill.PackedItems.AddNew();
		item2.API_LineNo = 2;

		var lrnEntryNumber = GetNewCusEntryNumber(bill, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		lrnEntryNumber.CE_EntryNum = "2024CRI0000000000111";

		var (sentMessage, sessionGuid) = AddNewSentMessage(header, sentMessageType);

		var receivedMessage = GetReceivedMessage(messageText, responseMessageType, sessionGuid);

		return (header, bill, sentMessage, receivedMessage);
	}

	protected CusEntryNumber GetNewCusEntryNumber(TemporaryStorageBill parent, ZString entryType)
	{
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_ParentID = parent.PK;
		cusEntryNumber.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
		cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		cusEntryNumber.CE_EntryType = entryType;
		cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

		return cusEntryNumber;
	}

	protected virtual ITEDIMessage GetReceivedMessage(string messageText, string messageType, ZGuid sentSessionGuid)
	{
		var receivedInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		receivedInterchange.EI_SessionGUID = sentSessionGuid;

		var receivedMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		receivedInterchange.ContainedMessages.Add(receivedMessage);
		receivedMessage.EM_MessageText = messageText ?? ZString.Empty;
		receivedMessage.EM_MessageType = messageType ?? ZString.Empty;
		receivedMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
		receivedMessage.EM_SystemLastEditTimeUtc = ZDateTime.Now;
		receivedMessage.EM_EI = receivedInterchange.PK;
		receivedMessage.EM_Status = "RCV";
		receivedMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

		return receivedMessage;
	}

	protected (EDIMessage sentMessage, ZGuid sentSessionGuid) AddNewSentMessage(TemporaryStorageHeader header, string messageType)
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		var sentInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		sentInterchange.EI_SessionGUID = sentSessionGuid;
		sentInterchange.IsTransmitInterchange = true;

		var sentMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		sentMessage.EM_ApplicationCode = "ITH";
		sentMessage.EM_Status = "SNT";
		sentMessage.EM_MessageNum = "0001";
		sentMessage.EM_MessageType = messageType;
		sentMessage.EM_EI = sentInterchange.PK;

		header.Messages.Add(sentMessage);
		sentInterchange.ContainedMessages.Add(sentMessage);

		return (sentMessage, sentSessionGuid);
	}

	protected void AssertEntryNumber(CusEntryNumber entryNumber, ZString expectedEntryType, ZString expectedEntryNum, ZString expectedCategory, ZDateTime expectedIssueDate)
	{
		CombineAssertions($"Entry Number {expectedEntryType}", () =>
		{
			AssertEquals("CE_EntryType", expectedEntryType, entryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", expectedEntryNum, entryNumber.CE_EntryNum);
			AssertEquals("CE_Category", expectedCategory, entryNumber.CE_Category);
			AssertEquals("CE_IssueDate", expectedIssueDate, entryNumber.CE_IssueDate.Date);
		});
	}

	protected CusEntryNumber GetCusEntryNumber(BusinessObject parent, string entryType)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, parent.PK);
		query.AddToFilter(CusEntryNumSchema.CE_ParentTable, parent.TableName);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		query.AddToFilter(CusEntryNumSchema.CE_Category, "CUS");
		return Factory.LoadTop1<CusEntryNumber>(query);
	}

	protected void AssertNumberOfResponseMessages(TemporaryStorageHeader header, int expectedNumberOfMessages, string responseMessageType = "RES")
	{
		var numberOfMessages = header.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == responseMessageType);
		AssertEquals($"Number of {responseMessageType} messages", expectedNumberOfMessages, numberOfMessages);
	}

	protected T GetMessageProcessor()
	{
		return GetNewResponseMessageProcessor(logger);
	}

	protected LoggingInformationForTesting logger = new LoggingInformationForTesting();

	protected abstract T GetNewResponseMessageProcessor(LoggingInformation logger);

	protected const string PositiveResponseManifestResourceKey_AllOutcomesPositive = "Enterprise.Customs.IT.TemporaryStorage.Business.Testing.TemporaryStorageHeader.MessageProcessors.TestFiles.TemporaryStorage_G4_PositiveResponse_AllOutcomesPositive_ResultCode200.xml";
	protected const string PositiveResponseManifestResourceKey_SomeOutcomesPositive = "Enterprise.Customs.IT.TemporaryStorage.Business.Testing.TemporaryStorageHeader.MessageProcessors.TestFiles.TemporaryStorage_G4_PositiveResponse_SomeOutcomesPositive_ResultCode200.xml";
	protected const string PositiveResponseManifestResourceKey_AllOutcomesNegative = "Enterprise.Customs.IT.TemporaryStorage.Business.Testing.TemporaryStorageHeader.MessageProcessors.TestFiles.TemporaryStorage_G4_PositiveResponse_AllOutcomesNegative_ResultCode200.xml";
	protected const string NegativeResponseManifestResourceKey = "Enterprise.Customs.IT.TemporaryStorage.Business.Testing.TemporaryStorageHeader.MessageProcessors.TestFiles.TemporaryStorage_G4_NegativeResponseResultCode198.xml";
	protected const string PositiveAcknowledgementResourceKey = "Enterprise.Customs.IT.TemporaryStorage.Business.Testing.TemporaryStorageHeader.MessageProcessors.TestFiles.TemporaryStorage_G4_PositiveAcknowledgment_ResultCode20.xml";
	protected const string NegativeAcknowledgementResourceKey = "Enterprise.Customs.IT.TemporaryStorage.Business.Testing.TemporaryStorageHeader.MessageProcessors.TestFiles.TemporaryStorage_G4_NegativeAcknowledgment_ResultCode10.xml";
}
