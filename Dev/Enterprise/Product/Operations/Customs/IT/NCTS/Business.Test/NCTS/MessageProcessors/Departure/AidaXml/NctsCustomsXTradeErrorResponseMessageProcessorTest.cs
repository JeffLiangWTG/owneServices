using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsCustomsXTradeErrorResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<CustomsXTradeErrorResponseMessageProcessor>
{
	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailed()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(CustomsXTradeErrorFileKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, responseMessageType: "XER");
		receivedMessage.Interchange.EI_TransportType = "XTT";
		sentMessage.Interchange.EI_InterchangeType = "NEW";
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_CustomsStatus = "AWO";

		var processor = GetNewResponseMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			var numberOfErrorMessages = movementHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == "XER");
			AssertEquals("Number of Error Messages", 1, numberOfErrorMessages);
			AssertEquals("BM_CustomsStatus", "AWO", movementHeader.BM_CustomsStatus);
		});
	}

	[TestDate]
	public void TestProcessMessage_ExpiredCertificate_WhenIRIAndEntryHasExpiredCertificate()
	{
		SetupMauCertificate(ZDateTime.Today.AddDays(-1));
		Assert("IRI Not Accepted", NotAcceptedContent);
		Assert("IRI Bad Certificate", BadCertificateContent);

		void Assert(string message, string errorMessageText)
		{
			var (sentInterchange, sentMessage, movementHeader, errorMessage) = PrepareIRIExpiredCertificateTestData(errorMessageText);
			CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(sentInterchange, "OPN", ZDateTime.Now.AddDays(1), "IRI");

			var processor = GetNewResponseMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			movementHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to movement header", movementHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 2, movementHeader.Messages.Count);
			AssertEquals($"{message}: no new IRI message created", 0, movementHeader.Messages.Cast<EDIMessage>().Count(x => x.PK != sentMessage.PK && x.EM_MessageType == "IRI"));
			CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(sentInterchange, "OPN", ZDateTime.Now.AddDays(1), "IRI");
		}
	}

	[TestDate]
	public void TestProcessMessage_ExpiredCertificate_IRIMessageCreated()
	{
		SetupMauCertificate();
		Assert("IRI Not Accepted", ManifestResourceHelper.ReadManifestResourceContent(NotAcceptedContent));
		Assert("IRI Bad Certificate", ManifestResourceHelper.ReadManifestResourceContent(BadCertificateContent));

		void Assert(string message, string errorMessageText)
		{
			var (sentInterchange, sentMessage, movementHeader, errorMessage) = PrepareIRIExpiredCertificateTestData(errorMessageText);
			CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(sentInterchange, "OPN", ZDateTime.Now.AddDays(1), "IRI");

			var processor = GetNewResponseMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			movementHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to movement header", movementHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 3, movementHeader.Messages.Count);
			AssertEquals($"{message}: new IRI message created", 1, movementHeader.Messages.Cast<EDIMessage>().Count(x => x.PK != sentMessage.PK && x.EM_MessageType == "IRI"));
			CusPollingTransactionTestHelper.AssertNoPollingTransactionsForInterchange(sentInterchange);
		}
	}

	void SetupMauCertificate(ZDateTime? expiryDate = null)
	{
		var password = Factory.New<GlbMauExternalPassword>();
		password.GP_GC = GlbCompany.CurrentCompany.PK;
		password.GP_UserID = "12345";
		password.GP_PasswordType = PasswordTypesList.Codes.ITM;
		password.GP_ExpiryDate = expiryDate ?? ZDateTime.MaxSmallDateTimeValue;
	}

	(EDIInterchange sentInterchange, EDIMessage sentMessage, NctsDepartureMovementHeader movementHeader, EDIMessage errorMessage) PrepareIRIExpiredCertificateTestData(string errorMessageText)
	{
		var interchangeSessionID = ZGuid.NewZGuid();
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsHeader.BH_CustomsProfile = "12345";
		var movementHeader = nctsHeader.MovementHeader;
		var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
		sentInterchange.EI_ApplicationCode = "ITH";
		sentInterchange.EI_SessionGUID = interchangeSessionID;
		sentInterchange.EI_InterchangeType = "IRI";
		sentInterchange.IsTransmitInterchange = true;
		var sentMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		sentMessage.EM_MessageNum = "0001";
		sentMessage.EM_MessageType = "IRI";
		sentInterchange.ContainedMessages.Add(sentMessage);
		movementHeader.Messages.Add(sentMessage);

		var receivedInterchange = Factory.NewWithValidTestData<EDIInterchange>();
		receivedInterchange.EI_ApplicationCode = "ITH";
		receivedInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;

		var errorMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		receivedInterchange.ContainedMessages.Add(errorMessage);
		errorMessage.EM_MessageText = errorMessageText;
		errorMessage.EM_MessageType = EDIMessageTypeList.Codes.XtCustomsError;
		sentInterchange.CreateOrReOpenPollingTransaction();
		Factory.Save();
		return (sentInterchange, sentMessage, movementHeader, errorMessage);
	}

	protected override CustomsXTradeErrorResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new CustomsXTradeErrorResponseMessageProcessor(logger);

	const string CustomsXTradeErrorFileKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsCustomsXTradeErrorFile.xml";

	const string NotAcceptedContent = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_NotAccepted.xml";

	const string BadCertificateContent = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_BadCertificate.xml";
}
