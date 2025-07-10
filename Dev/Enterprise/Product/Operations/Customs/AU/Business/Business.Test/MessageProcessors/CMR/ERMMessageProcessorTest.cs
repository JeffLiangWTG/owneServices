using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ERMMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestERMResponseMessage()
		{
			var mAWB = Factory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MessageReference = "A00008674";
			var outMessage = (CMRAIRCRMessage)hAWB.Messages.AddNew(typeof(CMRAIRCRMessage));
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outMessage.EM_Status = EDIMessage.Status.Sent;
			outMessage.EM_MessageNum = "1";
			outMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			outMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-30);
			var incomingMessage = Factory.New<CMRCUSRESMessage>();
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ERMCCFErrorMessage.txt")).Replace("\r\n", "");
			incomingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.AIRCR;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var incomingMessage2 = factory2.Load<EDIMessage>(incomingMessage.PK);
			var logger = new LoggingInformation();
			var allMessageProcessor = new CMRAllMessageProcessor(logger);
			allMessageProcessor.ProcessMessage(incomingMessage2);
			factory2.Save();
			AssertEquals("Message type still OK", CMRMessage.CMRMessageTypes.AIRCR, incomingMessage2.EM_MessageType);
			var hAWB2 = factory2.Load<CusHAWB>(hAWB.PK);
			AssertEquals(EDIMessage.Status.Received, incomingMessage2.EM_Status);
			AssertEquals("Message added to linked object", 2, hAWB2.Messages.Count);
			AssertEquals("Linked object message status set to rejected", CMRBaseStatuses.Codes.OriginalRejected, hAWB2.CMRMessageStatus.Code);
		}

		protected override ZString GetExpectedMessageCode() => "ACR";

		protected override ZString GetExpectedMessageName() => "CCF Error/Reject Message Response (ERM)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override Type IncomingMessageType => typeof(CMRCUSRESMessage);

		protected override void SetUp()
		{
			base.SetUp();
			var logger = new LoggingInformation();
			processor = new ERMMessageProcessor(logger);
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ERMCCFErrorMessage.txt")).Replace("\r\n", "");
		}
		ERMMessageProcessor processor;
	}
}
