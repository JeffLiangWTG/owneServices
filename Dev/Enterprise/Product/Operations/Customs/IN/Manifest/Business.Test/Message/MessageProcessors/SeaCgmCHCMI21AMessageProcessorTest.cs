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

sealed class SeaCgmCHCMI21AMessageProcessorTest : BaseManifestMessageProcessorAbstractTest
{
	protected override IEmailMessageProcessor GetProcessor() => new SeaCgmCHCMI21AMessageProcessor();

	protected override string MessageID => IN.Business.Constants.MessageID.SeaCgmAcknowledgement;

	[TestDate(2024, 9, 27)]
	public void TestGetLinkedObject()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI21A.out");
		AssertGetLinkedObject("Failed to find matching Manifest", null, "Warning - Failed to find matching Manifest by Bill Number: 23257531084, Vessel Code: 2325753108, Voyage Number: 07012024, Line Number: 123.");

		CreateManifestHeader("23257531084", string.Empty, string.Empty, string.Empty);
		CreateManifestHeader("23257531084", "2325753108", "07012024", "123").AMA_TransportMode = Core.Constants.TransportModes.Air;
		CreateManifestHeader("23257531084", "2325753108", "07012024", "123").AMA_RN_NKCountry = "TR";
		CreateManifestHeader("23257531084", "2325753108", "07012024", "123").AMA_ManifestType = INManifestTypes.Codes.IGM;
		CreateManifestHeader("23257531084", "2325753108", "07012024", "123").AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
		CreateManifestHeader("00000000000", "2325753108", "07012024", "123");
		Factory.Save();

		AssertGetLinkedObject("Failed to find matching Manifest", null, "Warning - Failed to find matching Manifest by Bill Number: 23257531084, Vessel Code: 2325753108, Voyage Number: 07012024, Line Number: 123.");

		var header = CreateManifestHeader("23257531084", "2325753108", "07012024", "123");
		CreateManifestHeader("23257531084", "0000000000", "07012024", "123");
		CreateManifestHeader("23257531084", "2325753108", "00000000", "123");
		CreateManifestHeader("23257531084", "2325753108", "07012024", "000");
		Factory.Save();

		AssertGetLinkedObject("Positive - Found matching Manifest by Bill Number, Vessel Code, Voyage Number, Line Number", header, "");

		void AssertGetLinkedObject(string message, BusinessObject expectedResult, string expectedLogs)
		{
			CombineAssertions(message, () =>
			{
				var incomingMessage = CreateIncomingMessage();
				var log = new BatchProcessor.LoggingInformation();
				AssertEquals("Result", expectedResult, new SeaCgmCHCMI21AMessageProcessor().GetLinkedObject(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log));
				AssertMultilineASCIIEquals("Logs", expectedLogs, string.Join("\r\n", log.Logs.Select(x => x.ToString())));
			});
		}
	}

	public void TestProcess()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI21A.out");
		Factory.Save();

		AssertMessageProcessed("Positive - Found matching Manifest by Bill Number, Filght Number, IGM Number",
			true, "CGM", EDIMessageSubTypeList.Codes.SeaCgmPositiveAcknowledgement, "Debug - Message processing by SeaCgmCHCMI21AMessageProcessor");

		attachment = attachment.Replace("000", "001");
		AssertMessageProcessed("Negative - Found matching Manifest by Bill Number, Filght Number, IGM Number",
			true, "CGM", EDIMessageSubTypeList.Codes.SeaCgmNegativeAcknowledgement, "Debug - Message processing by SeaCgmCHCMI21AMessageProcessor");

		attachment = string.Empty;
		AssertMessageProcessed("Failed to find matching Manifest", false, string.Empty, string.Empty, @"Debug - Message processing by SeaCgmCHCMI21AMessageProcessor
Error - Failed to deserialize the message text to a valid SeaCgmAckChcmi21A.");

		void AssertMessageProcessed(string message, bool expectedResult, string expectdMessageType, string expectedMessageSubType, string expectedLogs)
		{
			CombineAssertions(message, () =>
			{
				var incomingMessage = CreateIncomingMessage();
				incomingMessage.EM_LinkedObject = CreateManifestHeader("23257531084", string.Empty, string.Empty, string.Empty);
				var log = new BatchProcessor.LoggingInformation();
				AssertEquals("Process", expectedResult, new SeaCgmCHCMI21AMessageProcessor().Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log));
				AssertEquals("EM_MessageType", expectdMessageType, incomingMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", expectedMessageSubType, incomingMessage.EM_MessageSubType);
				AssertMultilineASCIIEquals("Logs", expectedLogs, string.Join("\r\n", log.Logs.Select(x => x.ToString())));
			});
		}
	}

	public void TestSetMessageStatusAndCustomsStatus()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI21A.out");
		var header = CreateManifestHeader("23257531084", string.Empty, string.Empty, string.Empty);
		Factory.Save();

		var log = new BatchProcessor.LoggingInformation();
		var processor = new SeaCgmCHCMI21AMessageProcessor();
		var incomingMessage = CreateIncomingMessage();
		incomingMessage.EM_LinkedObject = header;
		processor.Process(incomingMessage, new EmailInfo() { AttachmentText = "Test" }, log);

		CombineAssertions(() =>
		{
			AssertEquals("CustomsStatus", string.Empty, header.RegistrationStatus);
			AssertEquals("MessageStatus", string.Empty, header.MessageStatus);

			processor.Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log);
			AssertEquals("CustomsStatus", RegistrationStatusList.Codes.ManifestRegistered, header.RegistrationStatus);
			AssertEquals("MessageStatus", MessageStatusList.Codes.MessageAccepted, header.MessageStatus);

			attachment = attachment.Replace("000", "001");
			processor.Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log);
			AssertEquals("CustomsStatus", RegistrationStatusList.Codes.ManifestRegistered, header.RegistrationStatus);
			AssertEquals("MessageStatus", MessageStatusList.Codes.ErrorResponseReceived, header.MessageStatus);

			var linkObject = Factory.NewWithValidTestData<OrgHeader>();
			incomingMessage.EM_LinkedObject = linkObject;
			processor.Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log);
			AssertEquals("LinkObject is not IMessageAttachee, type is Enterprise.MasterFiles.Business.OrgHeader", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		});
	}

	CGMAsycudaManifestHeader CreateManifestHeader(ZString billNumber, ZString vesselCode, ZString voyage, ZString lineNumber)
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.SuspendCheckBusinessObjectType();
		header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
		header.MasterBill.ABL_BillNumber = billNumber;
		header.MasterBill.ABL_CarrierReference = lineNumber;
		header.AMA_RadioCallSign = vesselCode;
		header.AMA_Voyage = voyage;
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
