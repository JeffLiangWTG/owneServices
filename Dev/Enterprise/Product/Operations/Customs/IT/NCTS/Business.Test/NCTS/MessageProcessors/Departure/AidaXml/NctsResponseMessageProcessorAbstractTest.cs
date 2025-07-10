using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsResponseMessageProcessorAbstractTest<T> : TestCaseWithFactory
	where T : XmlIncomingMessageProcessor
{
	protected (NctsHeader nctsHeader, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestData(string messageText, string responseMessageType = "RES", string sentMessageType = "NEW")
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var customsOfficeOfPresentation = nctsHeader.MovementHeader.CustomsOffices.AddNew();
		customsOfficeOfPresentation.CY_Code = "DEP";
		customsOfficeOfPresentation.CY_Data = "IT279100";

		(var sentMessage, var sessionGuid) = AddNewSentMessage(nctsHeader.MovementHeader, sentMessageType);

		var receivedMessage = GetReceivedMessage(messageText, responseMessageType, sessionGuid);

		return (nctsHeader, sentMessage, receivedMessage);
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

		return receivedMessage;
	}

	protected (EDIMessage sentMessage, ZGuid sentSessionGuid) AddNewSentMessage(NctsDepartureMovementHeader movementHeader, string messageType)
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		var sentInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		sentInterchange.EI_SessionGUID = sentSessionGuid;
		sentInterchange.IsTransmitInterchange = true;
		sentInterchange.EI_InterchangeType = "TRA";

		var sentMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		sentMessage.EM_ApplicationCode = "ITH";
		sentMessage.EM_Status = "SNT";
		sentMessage.EM_MessageNum = "0001";
		sentMessage.EM_ApplicationReference = "TRA";
		sentMessage.EM_MessageType = messageType;
		sentMessage.EM_EI = sentInterchange.PK;

		movementHeader.Messages.Add(sentMessage);
		sentInterchange.ContainedMessages.Add(sentMessage);

		return (sentMessage, sentSessionGuid);
	}

	protected void AssertEntryNumber(CusEntryNumber entryNumber, ZString expectedEntryType, ZString expectedEntryNum, ZString expectedCategory, ZString expectedEntryLineReference, ZDateTime expectedIssueDate, string expectedEntryStatus = "")
	{
		CombineAssertions($"Entry Number {expectedEntryType}", () =>
		{
			AssertEquals("CE_EntryType", expectedEntryType, entryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", expectedEntryNum, entryNumber.CE_EntryNum);
			AssertEquals("CE_Category", expectedCategory, entryNumber.CE_Category);
			AssertEquals("CE_EntryLineReference", expectedEntryLineReference, entryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", expectedIssueDate, entryNumber.CE_IssueDate.Date);
			AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, entryNumber.CE_EntryIsSystemGenerated);
			AssertEquals("CE_EntryStatus", expectedEntryStatus, entryNumber.CE_EntryStatus);
		});
	}

	protected CusEntryNumber GetCusEntryNumber(NctsHeader nctsHeader, string entryType)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		query.AddToFilter(CusEntryNumSchema.CE_Category, "CUS");
		return Factory.LoadTop1<CusEntryNumber>(query);
	}

	protected void AssertNumberOfResponseMessages(NctsDepartureMovementHeader movementHeader, int expectedNumberOfMessages, string responseMessageType = "RES")
	{
		var numberOfMessages = movementHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == responseMessageType);
		AssertEquals($"Number of {responseMessageType} messages", expectedNumberOfMessages, numberOfMessages);
	}

	protected void AssertMessageProcessingFailure(EDIMessage receivedEdiMessage, EDIInterchange receivedEdiInterchange, ZString logText)
	{
		CombineAssertions(() =>
		{
			AssertEquals("Message status", "FAL", receivedEdiMessage.EM_Status);
			AssertEquals("Interchange status", "FAL", receivedEdiInterchange.EI_Status);

			var notes = receivedEdiInterchange.Notes.GetAllNotes().Cast<StmNote>().ToArray();
			AssertEquals("Notes Length", 1, notes.Length);

			var singleStmNote = notes.ElementAt(0);
			AssertEquals("Note ST_Description", "CargoWiseOne error", singleStmNote.ST_Description);
			AssertEquals("Note ST_NoteDataAsText", logText, singleStmNote.ST_NoteDataAsText);
		});
	}

	protected void AssertLoggerContainsLogText(ZString logText)
	{
		var logs = logger.AccumulatedLogMessages.ToString().Trim();
		AssertEquals("Logged Info", true, logs.Contains(logText));
	}

	protected T GetMessageProcessor()
	{
		return GetNewResponseMessageProcessor(logger);
	}

	protected LoggingInformationForTesting logger = new LoggingInformationForTesting();

	protected abstract T GetNewResponseMessageProcessor(LoggingInformation logger);

	protected const string SentMessageApplicationReference = "TRA";

	protected const string PositiveAckManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsAcknowledgementMessageAcquired.xml";
	protected const string PositiveResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponsePositiveWithMRN.xml";
	protected const string ClearanceResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponsePositiveWithMRNandClearance.xml";
}
