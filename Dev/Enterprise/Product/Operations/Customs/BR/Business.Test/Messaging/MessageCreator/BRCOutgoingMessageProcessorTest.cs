using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessage()
		{
			var message1 = CreateMessage(Factory, EDIMessage.ApplicationCodes.BRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Original);
			var message2 = CreateMessage(Factory, EDIMessage.ApplicationCodes.BRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Rectification);
			var message3 = CreateMessage(Factory, EDIMessage.ApplicationCodes.BRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDI);
			var message4 = CreateMessage(Factory, EDIMessage.ApplicationCodes.BRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.LIC);
			var message5 = CreateMessage(Factory, EDIMessage.ApplicationCodes.BRCustoms, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.LIC);
			var message6 = CreateMessage(Factory, EDIMessage.ApplicationCodes.BRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, MessageTypeList.Codes.LIC);
			var message7 = CreateMessage(Factory, EDIMessage.ApplicationCodes.USCustomsImport, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.LIC);
			var message8 = CreateMessage(Factory, EDIMessage.ApplicationCodes.BRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Original);
			var message9 = CreateMessage(Factory, EDIMessage.ApplicationCodes.BRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment);

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new BRCOutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertMessageProcessed("BRC-TRX-QUE-CDE-ORI", true, message1);
				AssertMessageProcessed("BRC-TRX-QUE-CDE-RET", true, message2);
				AssertMessageProcessed("BRC-TRX-QUE-CDI", true, message3);
				AssertMessageProcessed("BRC-TRX-QUE-LIC", true, message4);
				AssertMessageProcessed("BRC-RCV-QUE-LIC", false, message5);
				AssertMessageProcessed("BRC-TRX-SNT-LIC", false, message6);
				AssertMessageProcessed("USI-TRX-QUE-LIC", false, message7);
				AssertMessageProcessed("BRC-TRX-QUE-CAT-ORI", true, message8);
				AssertMessageProcessed("BRC-TRX-QUE-RTT-MTT", true, message9);
			});
		}

		void AssertMessageProcessed(string message, bool processed, EDIMessage ediMessage)
		{
			string oldStatus = ediMessage.EM_Status;
			ediMessage.Reload();
			AssertEquals(message, processed, ediMessage.EM_EI.IsValid);
			AssertEquals("EM_Status", processed ? EDIMessageStatusList.Codes.Sent : oldStatus, ediMessage.EM_Status);
		}

		BREDIMessage CreateMessage(BusinessObjectFactory factory, ZString applicationCode, ZString direction, ZString status, ZString type, string subType = null)
		{
			var message = factory.New<BREDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			return message;
		}
	}
}
