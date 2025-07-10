using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsXTradeErrorResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<CustomsXTradeErrorResponseMessageProcessor>
{
	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForImport()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		receivedMessage.Interchange.EI_TransportType = "XTT";
		sentMessage.Interchange.EI_InterchangeType = "IMP";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForExport()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		receivedMessage.Interchange.EI_TransportType = "XTT";
		sentMessage.Interchange.EI_InterchangeType = "EXP";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForTransit()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		receivedMessage.Interchange.EI_TransportType = "XTT";
		sentMessage.Interchange.EI_InterchangeType = "TRA";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForImportButNotXT()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		receivedMessage.Interchange.EI_TransportType = "HUB";
		sentMessage.Interchange.EI_InterchangeType = "IMP";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForExportButNotXT()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		receivedMessage.Interchange.EI_TransportType = "HUB";
		sentMessage.Interchange.EI_InterchangeType = "EXP";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForTransitButNotXT()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		receivedMessage.Interchange.EI_TransportType = "HUB";
		sentMessage.Interchange.EI_InterchangeType = "TRA";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForElectronicFolder()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		sentMessage.Interchange.EI_InterchangeType = "EFQ";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsNotAwaitingResponseAndTransmissionHasFailed()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		entryHeader.CH_Status = "ACO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasNotFailed()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_MultipleEvents_NotTransmissionFailure.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XER");
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_CorruptedUniversalInterchange()
	{
		var corruptedXml = "<UniversalEven????";
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(messageText: corruptedXml, messageType: "XER");
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_ExpiredCertificate_WhenErrorMessageIsNotMauCertificateRelated()
	{
		SetupMauCertificate();
		var (entryHeader, errorMessage) = PrepareExpiredCertificateTestData(EDIMessageTypeList.Codes.NewDeclaration, NotTransmissionFailureContent, true);
		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(errorMessage);
		Factory.Save();
		entryHeader.Messages.Reload(true);

		AssertEquals("Error message is linked to entry header", entryHeader.PK, errorMessage.EM_LinkedObject.PK);
		AssertEquals("Messages", 3, entryHeader.Messages.Count);
		AssertEquals("No IUT message", false, entryHeader.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == "IUT"));
	}

	public void TestProcessMessage_ExpiredCertificate_WhenOriginalMessageIsEur1Request()
	{
		SetupMauCertificate();
		Assert("Not Accepted", NotAcceptedContent);
		Assert("Bad Certificate", BadCertificateContent);

		void Assert(string message, string errorMessageText)
		{
			var (entryHeader, errorMessage) = PrepareExpiredCertificateTestData(EDIMessageTypeList.Codes.Eur1Request, errorMessageText, true);
			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			entryHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to entry header", entryHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 3, entryHeader.Messages.Count);
			AssertEquals($"{message}: no IUT message", false, entryHeader.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == "IUT"));
		}
	}

	public void TestProcessMessage_ExpiredCertificate_WhenOriginalMessageHasNoAcknowledgement()
	{
		SetupMauCertificate();
		Assert("Not Accepted", NotAcceptedContent);
		Assert("Bad Certificate", BadCertificateContent);

		void Assert(string message, string errorMessageText)
		{
			var (entryHeader, errorMessage) = PrepareExpiredCertificateTestData(EDIMessageTypeList.Codes.NewDeclaration, errorMessageText, false);
			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			entryHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to entry header", entryHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 2, entryHeader.Messages.Count);
			AssertEquals($"{message}: no IUT message", false, entryHeader.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == "IUT"));
		}
	}

	public void TestProcessMessage_ExpiredCertificate_WhenEntryHasNoCertificate()
	{
		Assert("Not Accepted", NotAcceptedContent);
		Assert("Bad Certificate", BadCertificateContent);

		void Assert(string message, string errorMessageText)
		{
			var (entryHeader, errorMessage) = PrepareExpiredCertificateTestData(EDIMessageTypeList.Codes.NewDeclaration, errorMessageText, true);
			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			entryHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to entry header", entryHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 3, entryHeader.Messages.Count);
			AssertEquals($"{message}: no IUT message", false, entryHeader.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == "IUT"));
		}
	}

	public void TestProcessMessage_ExpiredCertificate_WhenEntryHasExpiredCertificate()
	{
		SetupMauCertificate(ZDateTime.Today.AddDays(-1));
		Assert("Not Accepted", NotAcceptedContent);
		Assert("Bad Certificate", BadCertificateContent);

		void Assert(string message, string errorMessageText)
		{
			var (entryHeader, errorMessage) = PrepareExpiredCertificateTestData(EDIMessageTypeList.Codes.NewDeclaration, errorMessageText, true);
			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			entryHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to entry header", entryHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 3, entryHeader.Messages.Count);
			AssertEquals($"{message}: no IUT message", false, entryHeader.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == "IUT"));
		}
	}

	public void TestProcessMessage_ExpiredCertificate_IUTMessageCreated()
	{
		SetupMauCertificate();
		Assert("NEW Not Accepted", EDIMessageTypeList.Codes.NewDeclaration, NotAcceptedContent);
		Assert("AMD Not Accepted", EDIMessageTypeList.Codes.Amendment, NotAcceptedContent);
		Assert("CAN Not Accepted", EDIMessageTypeList.Codes.Cancellation, NotAcceptedContent);
		Assert("NEW Bad Certificate", EDIMessageTypeList.Codes.NewDeclaration, BadCertificateContent);
		Assert("AMD Bad Certificate", EDIMessageTypeList.Codes.Amendment, BadCertificateContent);
		Assert("CAN Bad Certificate", EDIMessageTypeList.Codes.Cancellation, BadCertificateContent);

		void Assert(string message, string messageType, string errorMessageText)
		{
			var (entryHeader, errorMessage) = PrepareExpiredCertificateTestData(messageType, errorMessageText, true);
			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			entryHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to entry header", entryHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 4, entryHeader.Messages.Count);

			var iutMessage = entryHeader.Messages.Cast<EDIMessage>().OrderBy(x => x.EM_SystemCreateTimeUtc).Last();

			CombineAssertions($"{message}: IUT message is created", () =>
			{
				AssertEquals("IUT", iutMessage.EM_MessageType);
				AssertContains("<iut>20230426D16001305248</iut>", iutMessage.EM_MessageText);
			});
		}
	}

	[TestDate]
	public void TestProcessMessage_ExpiredCertificate_WhenIVIAndEntryHasExpiredCertificate()
	{
		SetupMauCertificate(ZDateTime.Today.AddDays(-1));
		Assert("IVI Not Accepted", NotAcceptedContent);
		Assert("IVI Bad Certificate", BadCertificateContent);

		void Assert(string message, string errorMessageText)
		{
			var (sentInterchange, sentMessage, entryHeader, errorMessage) = PrepareIVIExpiredCertificateTestData(errorMessageText);
			CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(sentInterchange, "OPN", ZDateTime.Now.AddDays(1), "IVI");

			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			entryHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to entry header", entryHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 2, entryHeader.Messages.Count);
			AssertEquals($"{message}: no new IVI message created", 0, entryHeader.Messages.Cast<EDIMessage>().Count(x => x.PK != sentMessage.PK && x.EM_MessageType == "IVI"));
			CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(sentInterchange, "OPN", ZDateTime.Now.AddDays(1), "IVI");
		}
	}

	[TestDate]
	public void TestProcessMessage_ExpiredCertificate_IVIMessageCreated()
	{
		SetupMauCertificate();
		Assert("IVI Not Accepted", NotAcceptedContent);
		Assert("IVI Bad Certificate", BadCertificateContent);

		void Assert(string message, string errorMessageText)
		{
			var (sentInterchange, sentMessage, entryHeader, errorMessage) = PrepareIVIExpiredCertificateTestData(errorMessageText);
			CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(sentInterchange, "OPN", ZDateTime.Now.AddDays(1), "IVI");

			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(errorMessage);
			Factory.Save();
			entryHeader.Messages.Reload(true);

			AssertEquals($"{message}: error message is linked to entry header", entryHeader.PK, errorMessage.EM_LinkedObject.PK);
			AssertEquals($"{message}: messages", 3, entryHeader.Messages.Count);
			AssertEquals($"{message}: new IVI message created", 1, entryHeader.Messages.Cast<EDIMessage>().Count(x => x.PK != sentMessage.PK && x.EM_MessageType == "IVI"));
			CusPollingTransactionTestHelper.AssertNoPollingTransactionsForInterchange(sentInterchange);
		}
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "XER" };

	protected override CustomsXTradeErrorResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
		=> new CustomsXTradeErrorResponseMessageProcessor(logger);

	void AssertNumberOfErrorMessages(CusEntryHeader entryHeader, int expectedNumberOfErrorMessages)
	{
		var numberOfErrorMessages = entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == "XER");
		AssertEquals("Number of Error Messages", expectedNumberOfErrorMessages, numberOfErrorMessages);
	}

	void SetupMauCertificate(ZDateTime? expiryDate = null)
	{
		var password = Factory.New<GlbMauExternalPassword>();
		password.GP_GC = GlbCompany.CurrentCompany.PK;
		password.GP_UserID = "12345";
		password.GP_PasswordType = PasswordTypesList.Codes.ITM;
		password.GP_ExpiryDate = expiryDate ?? ZDateTime.MaxSmallDateTimeValue;
	}

	(CusEntryHeader entryHeader, EDIMessage errorMessage) PrepareExpiredCertificateTestData(string sentMessageType, string errorMessageText, bool setupAck)
	{
		var interchangeSessionID = ZGuid.NewZGuid();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = Customs.EU.Business.MessageTypeList.Codes.Export;
		declaration.JE_CustomsProfile = "12345";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sentInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		sentInterchange.EI_SessionGUID = interchangeSessionID;
		sentInterchange.EI_InterchangeType = sentMessageType;
		sentInterchange.IsTransmitInterchange = true;
		var sentMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		sentMessage.EM_MessageNum = "0001";
		sentMessage.EM_ApplicationReference = declaration.JE_MessageType;
		sentMessage.EM_MessageType = sentMessageType;
		sentInterchange.ContainedMessages.Add(sentMessage);
		entryHeader.Messages.Add(sentMessage);

		if (setupAck)
		{
			var ackInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
			ackInterchange.EI_SessionGUID = interchangeSessionID;
			ackInterchange.EI_InterchangeType = EDIMessageTypeList.Codes.Acknowledgment;
			var ackMessage = Factory.NewWithValidTestData<ITEDIMessage>();
			ackMessage.EM_MessageNum = "0002";
			ackMessage.EM_ApplicationReference = declaration.JE_MessageType;
			ackMessage.EM_MessageType = EDIMessageTypeList.Codes.Acknowledgment;
			ackMessage.EM_MessageText = AcknowledgmentContent;
			ackInterchange.ContainedMessages.Add(ackMessage);
			entryHeader.Messages.Add(ackMessage);
		}

		var receivedInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		receivedInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;
		var errorMessage = AddNewMessage(errorMessageText, EDIMessageTypeList.Codes.XtCustomsError, receivedInterchange);

		Factory.Save();
		return (entryHeader, errorMessage);
	}

	(EDIInterchange sentInterchange, EDIMessage sentMessage, CusEntryHeader entryHeader, EDIMessage errorMessage) PrepareIVIExpiredCertificateTestData(string errorMessageText)
	{
		var interchangeSessionID = ZGuid.NewZGuid();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = Customs.EU.Business.MessageTypeList.Codes.Export;
		declaration.JE_CustomsProfile = "12345";
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
		sentInterchange.EI_ApplicationCode = "ITH";
		sentInterchange.EI_SessionGUID = interchangeSessionID;
		sentInterchange.EI_InterchangeType = "IVI";
		sentInterchange.IsTransmitInterchange = true;
		var sentMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		sentMessage.EM_MessageNum = "0001";
		sentMessage.EM_ApplicationReference = declaration.JE_MessageType;
		sentMessage.EM_MessageType = "IVI";
		sentInterchange.ContainedMessages.Add(sentMessage);
		entryHeader.Messages.Add(sentMessage);

		var receivedInterchange = Factory.NewWithValidTestData<EDIInterchange>();
		receivedInterchange.EI_ApplicationCode = "ITH";
		receivedInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;
		var errorMessage = AddNewMessage(errorMessageText, EDIMessageTypeList.Codes.XtCustomsError, receivedInterchange);
		sentInterchange.CreateOrReOpenPollingTransaction();
		Factory.Save();
		return (sentInterchange, sentMessage, entryHeader, errorMessage);
	}

	static string AcknowledgmentContent => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveAcknowledgment.xml");

	static string NotAcceptedContent => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_NotAccepted.xml");

	static string BadCertificateContent => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_BadCertificate.xml");

	static string NotTransmissionFailureContent => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_MultipleEvents_NotTransmissionFailure.xml");
}
