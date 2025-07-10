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

[TestedType(typeof(AirCgmCHCMI01MessageProcessor))]
sealed class AirCgmCHCMI01MessageProcessorTest : BaseManifestMessageProcessorAbstractTest
{
	protected override IEmailMessageProcessor GetProcessor() => new AirCgmCHCMI01MessageProcessor();

	protected override string MessageID => IN.Business.Constants.MessageID.AirCgm;

	public void TestGetLinkedObject()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI01.out");
		AssertGetLinkedObject("Failed to find matching Manifest", null, "Warning - Failed to find matching Manifest by Bill Number: 23257531084, Bill Issue Date: 10-Sep-24 00:00:00, Filght Number: MH194, Message Number: 0000001, Customs House Code: INBLR4.");

		CreateManifestHeader("23257531084", string.Empty, ZDate.Empty, string.Empty);
		CreateManifestHeader("23257531084", "MH194", new ZDate(2024, 9, 10), "INBLR4").AMA_TransportMode = Core.Constants.TransportModes.Sea;
		CreateManifestHeader("23257531084", "MH194", new ZDate(2024, 9, 10), "INBLR4").AMA_RN_NKCountry = "TR";
		CreateManifestHeader("23257531084", "MH194", new ZDate(2024, 9, 10), "INBLR4").AMA_ManifestType = INManifestTypes.Codes.IGM;
		CreateManifestHeader("23257531084", "MH194", new ZDate(2024, 9, 10), "INBLR4").AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
		CreateManifestHeader("00000000000", "MH194", new ZDate(2024, 9, 10), "INBLR4");
		Factory.Save();

		AssertGetLinkedObject("Failed to find matching Manifest", null, "Warning - Failed to find matching Manifest by Bill Number: 23257531084, Bill Issue Date: 10-Sep-24 00:00:00, Filght Number: MH194, Message Number: 0000001, Customs House Code: INBLR4.");

		var header = CreateManifestHeader("23257531084", "MH194", new ZDate(2024, 9, 10), "INBLR4");
		CreateManifestHeader("23257531084", "XXXXX", new ZDate(2024, 9, 10), "INBLR4");
		CreateManifestHeader("23257531084", "MH194", new ZDate(2024, 1, 7), "INBLR4");
		CreateManifestHeader("23257531084", "MH194", new ZDate(2024, 9, 10), "INBOXX");
		Factory.Save();

		AssertGetLinkedObject("Failed to find matching Manifest", null, "Warning - Failed to find matching Manifest by Bill Number: 23257531084, Bill Issue Date: 10-Sep-24 00:00:00, Filght Number: MH194, Message Number: 0000001, Customs House Code: INBLR4.");

		CreateOutGoingMessage("INBLR4", "0000001").EM_LinkedObject = header;
		Factory.Save();

		AssertGetLinkedObject("Found matching Manifest by Bill Number, Filght Number, Message Number, Customs House Code", header, "");

		attachment = attachment.Replace("10092024", "20241009");

		AssertGetLinkedObject("Failed to find matching Manifest", null, "Warning - Failed to find matching Manifest by Bill Number: 23257531084, Bill Issue Date: , Filght Number: MH194, Message Number: 0000001, Customs House Code: INBLR4.");

		AssertEquals("Invalid date format. Expected format should be ddMMyyyy for Bill Issue Date", ErrorReporter.LastMessageReported);
		ErrorReporter.Clear();

		void AssertGetLinkedObject(string message, BusinessObject expectedResult, string expectedLogs)
		{
			CombineAssertions(message, () =>
			{
				var incomingMessage = CreateIncomingMessage();
				var log = new BatchProcessor.LoggingInformation();
				AssertEquals("Result", expectedResult, new AirCgmCHCMI01MessageProcessor().GetLinkedObject(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log));
				AssertMultilineASCIIEquals("Logs", expectedLogs, string.Join("\r\n", log.Logs.Select(x => x.ToString())));
			});
		}
	}

	public void TestProcess()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI01.out");
		Factory.Save();

		AssertMessageProcessed("Positive - Found matching Manifest by Bill Number, Filght Number, Message Number, Customs House Code",
			true, "CGM", EDIMessageSubTypeList.Codes.AirCgmNegativeAcknowledgement, "Debug - Message processing by AirCgmCHCMI01MessageProcessor");

		attachment = string.Empty;
		AssertMessageProcessed("Failed to find matching Manifest", false, string.Empty, string.Empty, @"Debug - Message processing by AirCgmCHCMI01MessageProcessor
Error - Failed to deserialize the message text to a valid AirCgmCmchi01.");

		void AssertMessageProcessed(string message, bool expectedResult, string expectdMessageType, string expectedMessageSubType, string expectedLogs)
		{
			CombineAssertions(message, () =>
			{
				var incomingMessage = CreateIncomingMessage();
				incomingMessage.EM_LinkedObject = CreateManifestHeader("23257531084", string.Empty, ZDate.Empty, string.Empty);
				var log = new BatchProcessor.LoggingInformation();
				AssertEquals("Process", expectedResult, new AirCgmCHCMI01MessageProcessor().Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log));
				AssertEquals("EM_MessageType", expectdMessageType, incomingMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", expectedMessageSubType, incomingMessage.EM_MessageSubType);
				AssertMultilineASCIIEquals("Logs", expectedLogs, string.Join("\r\n", log.Logs.Select(x => x.ToString())));
			});
		}
	}

	public void TestSetMessageStatusAndCustomsStatus()
	{
		var attachment = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Manifest.Business.Testing.Message.MessageProcessors.TestFiles.CHCMI01.out");
		var header = CreateManifestHeader("23257531084", string.Empty, ZDate.Empty, string.Empty);
		Factory.Save();

		var log = new BatchProcessor.LoggingInformation();
		var processor = new AirCgmCHCMI01MessageProcessor();
		var incomingMessage = CreateIncomingMessage();
		incomingMessage.EM_LinkedObject = header;
		processor.Process(incomingMessage, new EmailInfo() { AttachmentText = "Test" }, log);

		CombineAssertions(() =>
		{
			AssertEquals("CustomsStatus", string.Empty, header.RegistrationStatus);
			AssertEquals("MessageStatus", string.Empty, header.MessageStatus);

			incomingMessage.EM_LinkedObject = header;
			processor.Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log);
			AssertEquals("MessageStatus", MessageStatusList.Codes.ErrorResponseReceived, header.MessageStatus);

			var linkObject = Factory.NewWithValidTestData<OrgHeader>();
			incomingMessage.EM_LinkedObject = linkObject;
			processor.Process(incomingMessage, new EmailInfo() { AttachmentText = attachment }, log);
			AssertEquals("LinkObject is not IMessageAttachee, type is Enterprise.MasterFiles.Business.OrgHeader", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		});
	}

	CGMAsycudaManifestHeader CreateManifestHeader(ZString billNumber, ZString voyage, ZDate billIssueDate, ZString customsOffice)
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.SuspendCheckBusinessObjectType();
		header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		header.MasterBill.ABL_BillNumber = billNumber;
		header.AMA_Voyage = voyage;
		header.MasterBill.ABL_BillIssueDate = billIssueDate;
		header.AMA_CustomsOffice = customsOffice;
		return header;
	}

	EDIMessage CreateOutGoingMessage(ZString messageOwner, ZString messageNumber)
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_MessageSubType = EDIMessageSubTypeList.Codes.AirCgm;
		message.EM_MessageType = "CGM";
		message.EM_MessageOwner = messageOwner;
		message.EM_MessageNum = messageNumber;
		return message;
	}

	EDIMessage CreateIncomingMessage()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		return message;
	}
}
