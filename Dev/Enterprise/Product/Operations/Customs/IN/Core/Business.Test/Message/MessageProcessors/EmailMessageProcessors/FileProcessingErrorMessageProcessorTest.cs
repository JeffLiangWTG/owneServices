using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(FileProcessingErrorMessageProcessor))]
sealed class FileProcessingErrorMessageProcessorTest : TestCaseWithFactory
{
	public void TestCanProcess()
	{
		var emailInfo = new EmailInfo
		{
			Subject = "Message ID CMCHI01",
			HasAttachments = false
		};
		var processor = new FileProcessingErrorMessageProcessor();
		Assert(processor.CanProcess(emailInfo));
		emailInfo.Subject = "Invalid subject";
		Assert(!processor.CanProcess(emailInfo));
		emailInfo.Subject = "Message ID CMCHI01";
		emailInfo.HasAttachments = true;
		Assert(!processor.CanProcess(emailInfo));
	}

	public void TestParseEmailSubjectInfo()
	{
		CombineAssertions(() =>
		{
			foreach (var keyValuePair in FileProcessingErrorMessageProcessor.MessageSubTypeDictionary)
			{
				var messageId = keyValuePair.Key;
				var messageSubType = keyValuePair.Value;
				var emailSubject = CreateEmailSubject("00000001", messageId, "20240819", "INBLR4");
				var emailSubjectInfo = CreateEmailSubjectInfo("00000001", messageSubType, new DateTime(2024, 8, 19), "INBLR4");
				AssertExpectedSubjectInfo(emailSubject, emailSubjectInfo);
			}

			var illegalEmailSubject = CreateEmailSubject("ASd000010", "Test", "20240819", "INBLR4");
			var errorEmailSubjectInfo = CreateEmailSubjectInfo(ZString.Empty, ZString.Empty, new DateTime(2024, 8, 19), "INBLR4");
			AssertExpectedSubjectInfo(illegalEmailSubject, errorEmailSubjectInfo, "Cannot get Message Sub Type by Message ID : Test.");

			var blankEmailSubjectInfo = CreateEmailSubjectInfo(ZString.Empty, ZString.Empty, null, ZString.Empty);
			AssertExpectedSubjectInfo(ZString.Empty, blankEmailSubjectInfo);
			AssertExpectedSubjectInfo("Filling status", blankEmailSubjectInfo);
		});

		void AssertExpectedSubjectInfo(string emailSubject, FileProcessingErrorMessageProcessor.EmailSubjectInfo expectedSubjectInfo, string expectedErrorReported = null)
		{
			var emailSubjectInfo = FileProcessingErrorMessageProcessor.ParseEmailSubjectInfo(emailSubject);
			AssertEquals("Message Number", expectedSubjectInfo.MessageNumber, emailSubjectInfo.MessageNumber);
			AssertEquals("Message Sub Type", expectedSubjectInfo.MessageSubType, emailSubjectInfo.MessageSubType);
			AssertEquals("Filling Date", expectedSubjectInfo.FilingDate, emailSubjectInfo.FilingDate);
			AssertEquals("Receiver ID", expectedSubjectInfo.ReceiverID, emailSubjectInfo.ReceiverID);
			AssertEquals(expectedSubjectInfo.IsEmpty, emailSubjectInfo.IsEmpty);

			if (expectedErrorReported != null)
			{
				AssertEquals("Error report", expectedErrorReported, ErrorReporter.LastMessageReported);
			}
			ErrorReporter.Clear();
		}

		string CreateEmailSubject(string controlNo, string messageID, string filingDate, string receiverID) => $"Filling status - control no. {controlNo}, filing date {filingDate}, Receiver ID {receiverID}, Message ID {messageID}";

		FileProcessingErrorMessageProcessor.EmailSubjectInfo CreateEmailSubjectInfo(string messageNum, string messageSubType, DateTime? fillingDate, string receiverID) =>
			new()
			{
				MessageNumber = messageNum,
				MessageSubType = messageSubType,
				FilingDate = fillingDate,
				ReceiverID = receiverID
			};
	}

	public void TestGetOutgoingMessageByEmail()
	{
		CreateMessageForTest();
		CreateMessageForTest().EM_MessageOwner = "INBOM4";
		CreateMessageForTest().EM_MessageSubType = EDIMessageSubTypeList.Codes.SeaCgm;
		CreateMessageForTest().EM_MessageNum = "00000008";
		CreateMessageForTest().EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		CreateMessageForTest().EM_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
		CreateMessageForTest().EM_SystemCreateTimeUtc = new DateTime(2024, 8, 2);

		var emailSubjectInfo = new FileProcessingErrorMessageProcessor.EmailSubjectInfo
		{
			MessageNumber = "00000018",
			MessageSubType = EDIMessageSubTypeList.Codes.AirCgm,
			ReceiverID = "INBLR4",
			FilingDate = new DateTime(2024, 8, 3)
		};

		var result = FileProcessingErrorMessageProcessor.GetOutgoingMessageByEmail(Factory, emailSubjectInfo);
		AssertNotNull("Message", result);
		CombineAssertions(() =>
		{
			AssertEquals("Test Message SubType", EDIMessageSubTypeList.Codes.AirCgm, result.EM_MessageSubType);
			AssertEquals("Test Message Number", "00000018", result.EM_MessageNum);
			AssertEquals("Test Receive Transmit", EDIMessage.Direction.Transmit, result.EM_ReceiveTransmit);
			AssertEquals("Test Application Code", ApplicationCodeList.Codes.INCustoms, result.EM_ApplicationCode);
			AssertEquals("Test Create Time", new DateTime(2024, 8, 3, 10, 5, 6), result.EM_SystemCreateTimeUtc);
		});
	}

	public void TestSetMessageStatus()
	{
		var manifest = Factory.New<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>() as BusinessObject;

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		outgoingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.AirCgm;
		outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.ConsolGeneralManifest;
		outgoingMessage.EM_MessageOwner = "INBLR4";
		outgoingMessage.EM_SystemCreateTimeUtc = new DateTime(2024, 8, 19);
		outgoingMessage.EM_LinkedObject = manifest;
		outgoingMessage.EM_MessageNum = "0000001";

		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

		var log = new BatchProcessor.LoggingInformation();
		var processor = new INCUniversalCustomsMessageProcessor();
		processor.ProcessMessage(incomingMessage, log, null);

		CombineAssertions(() =>
		{
			AssertEquals("MessageStatus when matching fail", ZString.Empty, ((IMessageAttachee)manifest).MessageStatus);

			var emailContent = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");
			incomingMessage.EM_MessageText = emailContent;
			processor.ProcessMessage(incomingMessage, log, null);
			AssertEquals("MessageStatus when matching success", MessageStatusList.Codes.ErrorResponseReceived, ((IMessageAttachee)manifest).MessageStatus);

			var linkObject = Factory.NewWithValidTestData<OrgHeader>();
			outgoingMessage.EM_LinkedObject = linkObject;
			processor.ProcessMessage(incomingMessage, log, null);
			AssertEquals("LinkObject is not IMessageAttachee, type is Enterprise.MasterFiles.Business.OrgHeader", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		});
	}

	public void TestProcessMessage()
	{
		var manifest = Factory.New<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>() as BusinessObject;

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		outgoingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.AirCgm;
		outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.ConsolGeneralManifest;
		outgoingMessage.EM_MessageOwner = "INBLR4";
		outgoingMessage.EM_SystemCreateTimeUtc = new DateTime(2024, 8, 19);
		outgoingMessage.EM_LinkedObject = manifest;
		outgoingMessage.EM_MessageNum = "0000001";

		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

		AssertProcessEmailMessage("Empty Message text", string.Empty, string.Empty,
			expectedLogs: "\tCould not find message processor for the email with Subject: ''.", processorName: "EmailMessageProcessor");

		var emailContent = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");
		incomingMessage.EM_MessageText = emailContent;
		TestProcessMessageByMessageTextAndMessageData("Valid Email Subject", EDIMessageTypeList.Codes.ConsolGeneralManifest, EDIMessageSubTypeList.Codes.AirCgmNegativeAcknowledgement);

		var originalEmailSubject = "Filling status - control no. 0000001, filing date 20240819, Receiver   ID INBLR4, Message ID CMCHI01";

		var subjectWithInvalidMessageId = originalEmailSubject.Replace("CMCHI01", "CMCHI55");
		incomingMessage.EM_MessageText = emailContent.Replace(originalEmailSubject, subjectWithInvalidMessageId);
		TestProcessMessageByMessageTextAndMessageData("Invalid Message ID", string.Empty, string.Empty,
			expectedLogs: $"\tCannot extract Message Number, Message Sub Type, Filling Date & Receiver ID from email subject: '{subjectWithInvalidMessageId}'.",
			expectedMessageReported: "Cannot get Message Sub Type by Message ID : CMCHI55.");

		var errorEmailSubjects = new Dictionary<string, string>
		{
			{ "Invalid control no", originalEmailSubject.Replace("0000001", "0000002") },
			{ "Invalid filing date", originalEmailSubject.Replace("20240819", "20240820") },
			{ "Invalid Receiver ID", originalEmailSubject.Replace("INBLR4", "INBLR5") },
		};
		foreach (var errorEmailSubject in errorEmailSubjects)
		{
			var message = errorEmailSubject.Key;
			var subject = errorEmailSubject.Value;
			incomingMessage.EM_MessageText = emailContent.Replace(originalEmailSubject, subject);
			TestProcessMessageByMessageTextAndMessageData(message, string.Empty, string.Empty,
				$"\tFailed to load the outgoing message by email subject info: {FileProcessingErrorMessageProcessor.ParseEmailSubjectInfo(subject)}.");
		}

		void TestProcessMessageByMessageTextAndMessageData(string message, string expecteMessageType, string expectedMessageSubType, string expectedLogs = "", string processorName = "FileProcessingErrorMessageProcessor", string expectedMessageReported = null)
		{
			AssertProcessEmailMessage(message, expecteMessageType, expectedMessageSubType, expectedLogs: expectedLogs, processorName, expectedMessageReported: expectedMessageReported);

			incomingMessage.EM_MessageData = ZBlob.FromUTF8(incomingMessage.EM_MessageText);
			AssertProcessEmailMessage(message, expecteMessageType, expectedMessageSubType, expectedLogs: expectedLogs, processorName, expectedMessageReported: expectedMessageReported);
			incomingMessage.EM_MessageData = null;
		}

		void AssertProcessEmailMessage(string message, string expecteMessageType, string expectedMessageSubType, string expectedLogs, string processorName = "FileProcessingErrorMessageProcessor", string expectedMessageReported = null)
		{
			CombineAssertions(message, () =>
			{
				var log = new BatchProcessor.LoggingInformation();
				new INCUniversalCustomsMessageProcessor().ProcessMessage(incomingMessage, log, null);

				AssertEquals("MessageType", expecteMessageType, incomingMessage.EM_MessageType);
				AssertEquals("MessageSubType", expectedMessageSubType, incomingMessage.EM_MessageSubType);

				AssertMultilineASCIIEquals("Logs", expectedLogs, string.Join("\r\n", log.UserLogStrings.Cast<string>()));
				AssertEquals("DebugLogStrings", $"Debug - Message processing by {processorName}", log.Logs.First(x => x.Type == Integration.LogType.Debug).ToString());

				if (expectedMessageReported != null)
				{
					AssertEquals("Error Reported", expectedMessageReported, ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}

				incomingMessage.EM_MessageType = string.Empty;
				incomingMessage.EM_MessageSubType = string.Empty;
				ErrorReporter.Clear();
			});
		}
	}

	public void TestGetLinkedObject()
	{
		var manifest = Factory.New<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>() as BusinessObject;

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		outgoingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.AirCgm;
		outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.ConsolGeneralManifest;
		outgoingMessage.EM_MessageOwner = "INBLR4";
		outgoingMessage.EM_SystemCreateTimeUtc = new DateTime(2024, 8, 19);
		outgoingMessage.EM_LinkedObject = manifest;
		outgoingMessage.EM_MessageNum = "0000001";

		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

		TestGetLinkedObject("Empty Message text", string.Empty, ZGuid.Empty, expectedLogs: "\tCould not find message processor for the email with Subject: ''.", processorName: "EmailMessageProcessor");

		var emailContent = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");
		incomingMessage.EM_MessageText = emailContent;
		TestGetLinkedObjectReadMessageTextAndMessageData("Valid Email Subject", manifest.TableName, manifest.PK);

		incomingMessage.EM_MessageText = emailContent.Replace("0000001", "");
		TestGetLinkedObjectReadMessageTextAndMessageData("Empty control no.", string.Empty, ZGuid.Empty, expectedLogs: "\tCannot extract Message Number, Message Sub Type, Filling Date & Receiver ID from email subject: 'Filling status - control no. , filing date 20240819, Receiver   ID INBLR4, Message ID CMCHI01'.");

		incomingMessage.EM_MessageText = emailContent.Replace("CMCHI01", "");
		TestGetLinkedObjectReadMessageTextAndMessageData("Empty Message ID", string.Empty, ZGuid.Empty, expectedLogs: "\tCould not find message processor for the email with Subject: 'Filling status - control no. 0000001, filing date 20240819, Receiver   ID INBLR4, Message ID'.", processorName: "EmailMessageProcessor");

		incomingMessage.EM_MessageText = emailContent.Replace("CMCHI01", "CMCHI55");
		TestGetLinkedObjectReadMessageTextAndMessageData("Invalid Message ID", string.Empty, ZGuid.Empty, expectedLogs: "\tCannot extract Message Number, Message Sub Type, Filling Date & Receiver ID from email subject: 'Filling status - control no. 0000001, filing date 20240819, Receiver   ID INBLR4, Message ID CMCHI55'.",
			expectedMessageReported: "Cannot get Message Sub Type by Message ID : CMCHI55.");

		incomingMessage.EM_MessageText = ZString.Empty;
		incomingMessage.EM_MessageData = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");
		TestGetLinkedObject("Valid Email Subject", manifest.TableName, manifest.PK, "");

		void TestGetLinkedObjectReadMessageTextAndMessageData(string message, string linkedTableName, ZGuid linkedObjectID, string expectedLogs = "", string processorName = "FileProcessingErrorMessageProcessor", string expectedMessageReported = null)
		{
			TestGetLinkedObject(message, linkedTableName, linkedObjectID, expectedLogs: expectedLogs, processorName, expectedMessageReported: expectedMessageReported);

			incomingMessage.EM_MessageData = ZBlob.FromUTF8(incomingMessage.EM_MessageText);
			TestGetLinkedObject(message, linkedTableName, linkedObjectID, expectedLogs: expectedLogs, processorName, expectedMessageReported: expectedMessageReported);
			incomingMessage.EM_MessageData = null;
		}

		void TestGetLinkedObject(string message, string linkedTableName, ZGuid linkedObjectID, string expectedLogs, string processorName = "FileProcessingErrorMessageProcessor", string expectedMessageReported = null)
		{
			CombineAssertions(message, () =>
			{
				var log = new BatchProcessor.LoggingInformation();
				var result = new INCUniversalCustomsMessageProcessor().GetLinkedBusinessObjectMetaData(incomingMessage, log);
				AssertEquals("linkedTableName", linkedTableName, result.ReturnValue.LinkTableName);
				AssertEquals("linkedObjectID", linkedObjectID, result.ReturnValue.LinkUniqueID);
				AssertMultilineASCIIEquals("Logs", expectedLogs, string.Join("\r\n", log.UserLogStrings.Cast<string>()));
				AssertEquals("DebugLogStrings", $"Debug - Get LinkedObject by {processorName}", log.Logs.First(x => x.Type == Integration.LogType.Debug).ToString());

				if (expectedMessageReported != null)
				{
					AssertEquals("Error Reported", expectedMessageReported, ErrorReporter.LastMessageReported);
				}
				ErrorReporter.Clear();
			});
		}
	}

	EDIMessage CreateMessageForTest()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageSubType = EDIMessageSubTypeList.Codes.AirCgm;
		message.EM_MessageNum = "00000018";
		message.EM_MessageOwner = "INBLR4";
		message.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_SystemCreateTimeUtc = new DateTime(2024, 8, 3, 10, 5, 6);

		return message;
	}
}
