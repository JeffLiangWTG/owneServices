using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using EDIMessageSubTypeList = Enterprise.Customs.IN.Business.EDIMessageSubTypeList;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

sealed class AirCgmCHCMI02MessageProcessorTest : BaseManifestMessageProcessorAbstractTest
{
	protected override IEmailMessageProcessor GetProcessor() => new AirCgmCHCMI02MessageProcessor();

	protected override string MessageID => IN.Business.Constants.MessageID.AirCgmAcknowledgement;

	[TestDate(2024, 9, 27)]
	public void TestGetLinkedObject()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI02.out");
		AssertGetLinkedObject("Failed to find matching Manifest", null, "Warning - Failed to find matching Manifest by Bill Number: 23257531084, Filght Number: MH194, IGM Number: 2414523, Sender ID: INBOM4.", attachment);

		CreateManifestHeader("23257531084", string.Empty, string.Empty, string.Empty);
		CreateManifestHeader("23257531084", "MH194", "2414523", "INBOM4").AMA_TransportMode = Core.Constants.TransportModes.Sea;
		CreateManifestHeader("23257531084", "MH194", "2414523", "INBOM4").AMA_RN_NKCountry = "TR";
		CreateManifestHeader("23257531084", "MH194", "2414523", "INBOM4").AMA_ManifestType = INManifestTypes.Codes.IGM;
		CreateManifestHeader("23257531084", "MH194", "2414523", "INBOM4").AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
		CreateManifestHeader("00000000000", "MH194", "2414523", "INBOM4");
		Factory.Save();

		AssertGetLinkedObject("Failed to find matching Manifest", null, "Warning - Failed to find matching Manifest by Bill Number: 23257531084, Filght Number: MH194, IGM Number: 2414523, Sender ID: INBOM4.", attachment);

		var header = CreateManifestHeader("23257531084", "MH194", "2414523", "INBOM4");
		CreateManifestHeader("23257531084", "XXXXX", "2414523", "INBOM4");
		CreateManifestHeader("23257531084", "MH194", "XXXXXXX", "INBOM4");
		CreateManifestHeader("23257531084", "MH194", "2414523", "INBOXX");
		Factory.Save();

		AssertGetLinkedObject("Found matching Manifest by Bill Number, Filght Number, IGM Number", header, "", attachment);
	}

	[TestDate(2024, 9, 27)]
	public void TestGetLinkedObjectWhenIgmDetailsEmpty()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI02EmptyIgmDetails.out");

		var headerWithIgm = CreateManifestHeader("23157531084", "MH194", "2414523", "INBOM4");
		Factory.Save();

		AssertGetLinkedObject("Failed to find matching Manifest with IGM number empty", null, "Warning - Failed to find matching Manifest by Bill Number: 23157531084, Filght Number: MH194, IGM Number: , Sender ID: INBOM4.", attachment);

		var headerWithoutIgm = CreateManifestHeader("23157531084", "MH194", string.Empty, "INBOM4");
		Factory.Save();

		AssertGetLinkedObject("Found matching Manifest with IGM number empty", headerWithoutIgm, "", attachment);
	}

	public void TestProcess()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI02.out");
		Factory.Save();

		AssertMessageProcessed("Positive - Found matching Manifest by Bill Number, Filght Number, IGM Number",
			true, "CGM", EDIMessageSubTypeList.Codes.AirCgmPositiveAcknowledgement, "Debug - Message processing by AirCgmCHCMI02MessageProcessor");

		attachment = attachment.Replace("\u001d000", "\u001d001");
		AssertMessageProcessed("Negative - Found matching Manifest by Bill Number, Filght Number, IGM Number",
			true, "CGM", EDIMessageSubTypeList.Codes.AirCgmNegativeAcknowledgement, "Debug - Message processing by AirCgmCHCMI02MessageProcessor");

		attachment = string.Empty;
		AssertMessageProcessed("Failed to find matching Manifest", false, string.Empty, string.Empty, @"Debug - Message processing by AirCgmCHCMI02MessageProcessor
Error - Failed to deserialize the message text to a valid AirCgmAckChcmi02.");

		void AssertMessageProcessed(string message, bool expectedResult, string expectdMessageType, string expectedMessageSubType, string expectedLogs)
		{
			CombineAssertions(message, () =>
			{
				var incomingMessage = CreateIncomingMessage();
				incomingMessage.EM_LinkedObject = CreateManifestHeader("23257531084", string.Empty, string.Empty, string.Empty);
				var log = new BatchProcessor.LoggingInformation();
				AssertEquals("Process", expectedResult, new AirCgmCHCMI02MessageProcessor().Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log));
				AssertEquals("EM_MessageType", expectdMessageType, incomingMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", expectedMessageSubType, incomingMessage.EM_MessageSubType);
				AssertMultilineASCIIEquals("Logs", expectedLogs, string.Join("\r\n", log.Logs.Select(x => x.ToString())));
			});
		}
	}

	public void TestSetMessageStatusAndCustomsStatus()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI02.out");
		var header = CreateManifestHeader("23257531084", string.Empty, string.Empty, string.Empty);
		var bill1 = header.Bills.AddNew();
		bill1.ABL_BillNumber = "HAWB1234";
		bill1.ABL_BillIssueDate = new ZDate(2024, 1, 7);

		var bill2 = header.Bills.AddNew();
		bill2.ABL_BillNumber = "HAWB5678";
		bill2.ABL_BillIssueDate = new ZDate(2024, 1, 7);

		var bill3 = header.Bills.AddNew();
		bill3.ABL_BillNumber = "HAWB1111";
		bill3.ABL_BillIssueDate = new ZDate(2024, 1, 7);
		Factory.Save();

		var log = new BatchProcessor.LoggingInformation();
		var processor = new AirCgmCHCMI02MessageProcessor();
		var incomingMessage = CreateIncomingMessage();
		incomingMessage.EM_LinkedObject = header;
		processor.Process(incomingMessage, new EmailInfo() { AttachmentText = "Test" }, log);

		CombineAssertions(() =>
		{
			AssertEquals("CustomsStatus when not link", string.Empty, header.RegistrationStatus);
			AssertEquals("MessageStatus when not link", string.Empty, header.MessageStatus);

			incomingMessage.EM_LinkedObject = header;
			processor.Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log);
			AssertEquals("CustomsStatus for linked header", RegistrationStatusList.Codes.ManifestRegistered, header.RegistrationStatus);
			AssertEquals("MessageStatus for linked header", MessageStatusList.Codes.MessageAccepted, header.MessageStatus);
			AssertEquals("MessageStatus for bill1", MessageStatusList.Codes.MessageAccepted, bill1.ABL_MessageStatus);
			AssertEquals("MessageStatus for bill2", MessageStatusList.Codes.ErrorResponseReceived, bill2.ABL_MessageStatus);
			AssertEquals("MessageStatus for bill3", MessageStatusList.Codes.ErrorResponseReceived, bill3.ABL_MessageStatus);

			attachment = attachment.Replace("\u001d000", "\u001d001");
			processor.Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log);
			AssertEquals("CustomsStatus when not positive", RegistrationStatusList.Codes.ManifestRegistered, header.RegistrationStatus);
			AssertEquals("MessageStatus when not positive", MessageStatusList.Codes.ErrorResponseReceived, header.MessageStatus);

			var linkObject = Factory.NewWithValidTestData<OrgHeader>();
			incomingMessage.EM_LinkedObject = linkObject;
			processor.Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log);
			AssertEquals("LinkObject is not IMessageAttachee, type is Enterprise.MasterFiles.Business.OrgHeader", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		});
	}

	void AssertGetLinkedObject(string message, BusinessObject expectedResult, string expectedLogs, string attachment)
	{
		CombineAssertions(message, () =>
		{
			var incomingMessage = CreateIncomingMessage();
			var log = new BatchProcessor.LoggingInformation();
			AssertSame("Result", expectedResult, new AirCgmCHCMI02MessageProcessor().GetLinkedObject(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log));
			AssertMultilineASCIIEquals("Logs", expectedLogs, string.Join("\r\n", log.Logs.Select(x => x.ToString())));
		});
	}

	CGMAsycudaManifestHeader CreateManifestHeader(ZString billNumber, ZString voyage, ZString igmNumber, ZString customsOffice)
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.SuspendCheckBusinessObjectType();
		header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		header.MasterBill.ABL_BillNumber = billNumber;
		header.AMA_Voyage = voyage;
		header.ImportGeneralManifestNumber = igmNumber;
		header.AMA_CustomsOffice = customsOffice;
		return header;
	}

	EDIMessage CreateIncomingMessage()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		return message;
	}
}
