using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ShippingBillQueryPendingMessageProcessor))]
sealed class ShippingBillQueryPendingMessageProcessorTest : TestCaseWithFactory
{
	public void TestCanProcess()
	{
		var emailInfo = new EmailInfo
		{
			Subject = "Shipping Bill Pending for Query Reply",
			HasAttachments = false,
		};
		var processor = new ShippingBillQueryPendingMessageProcessor();
		Assert(processor.CanProcess(emailInfo));
		emailInfo.Subject = "Invalid Subject";
		Assert(!processor.CanProcess(emailInfo));
		emailInfo.Subject = "Shipping Bill Pending for Query Reply";
		emailInfo.HasAttachments = true;
		Assert(!processor.CanProcess(emailInfo));
	}

	public void TestGetLinkedObject()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		var emailContent = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment_SBQuery_WrongDate.eml");
		incomingMessage.EM_MessageText = emailContent;

		var log = new BatchProcessor.LoggingInformation();
		_ = new INCUniversalCustomsMessageProcessor().GetLinkedBusinessObjectMetaData(incomingMessage, log);
		AssertMultilineASCIIEquals("Logs", "Cannot extract SB Number and SB Date from email body.", log.Logs.First(x => x.Type == Integration.LogType.Error).Message);

		incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		emailContent = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment_SBQuery_WrongBody.eml");
		incomingMessage.EM_MessageText = emailContent;
		log = new BatchProcessor.LoggingInformation();
		_ = new INCUniversalCustomsMessageProcessor().GetLinkedBusinessObjectMetaData(incomingMessage, log);
		AssertMultilineASCIIEquals("Logs", "Email body does not match expected format for Shipping Bill query.", log.Logs.First(x => x.Type == Integration.LogType.Error).Message);

		incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		emailContent = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment_SBQuery.eml");
		incomingMessage.EM_MessageText = emailContent;
		log = new BatchProcessor.LoggingInformation();
		_ = new INCUniversalCustomsMessageProcessor().GetLinkedBusinessObjectMetaData(incomingMessage, log);
		AssertMultilineASCIIEquals("Logs", "Entry Header with Shipping Bill number 1234567 and date 2025-02-12 not found.", log.Logs.First(x => x.Type == Integration.LogType.Warning).Message);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.ShippingBillNumber = "1234567";
		entryInstruction.ShippingBillDate = new DateTime(2025, 2, 12);
		Factory.Save();

		incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		emailContent = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment_SBQuery.eml");
		incomingMessage.EM_MessageText = emailContent;
		var result  = new INCUniversalCustomsMessageProcessor().GetLinkedBusinessObjectMetaData(incomingMessage, log);
		AssertEquals("linkedTableName", entryHeader.TableName, result.ReturnValue.LinkTableName);
		AssertEquals("linkedObjectID", entryHeader.PK, result.ReturnValue.LinkUniqueID);
	}

	public void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.ShippingBillNumber = "1234567";
		entryInstruction.ShippingBillDate = new DateTime(2025, 2, 12);
		Factory.Save();

		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		incomingMessage.EM_LinkedObject = entryHeader;
		var emailContent = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment_SBQuery.eml");
		incomingMessage.EM_MessageText = emailContent;
		var log = new BatchProcessor.LoggingInformation();
		new INCUniversalCustomsMessageProcessor().ProcessMessage(incomingMessage, log, null);
		AssertEquals("MessageType", EDIMessageTypeList.Codes.ShippingBill, incomingMessage.EM_MessageType);
		AssertEquals("MessageSubType", EDIMessageSubTypeList.Codes.ShippingBillQuery, incomingMessage.EM_MessageSubType);
		AssertEquals("MessageStatus", EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
	}
}
