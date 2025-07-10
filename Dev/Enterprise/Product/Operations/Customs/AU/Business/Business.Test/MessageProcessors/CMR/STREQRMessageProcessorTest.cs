using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class STREQRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestDeclarationStatusMessage()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			var exportLine = header.Lines.AddNew();
			exportLine.EL_UserReferenceNum = "K00001824";
			outgoingMessage.EM_LinkedObject = exportLine;
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("STREQDeclarationStatusResponse.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals("ERR", exportLine.EL_DocumentStatus);
			AssertEquals("EMB", exportLine.EL_DocumentStatusConditions);
		}

		protected override ZString GetExpectedMessageCode() => "STR";

		protected override ZString GetExpectedMessageName() => "Status Request Response (STREQR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override Type IncomingMessageType => typeof(CMRSTREQRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			outgoingMessage = (EDIMessage)Factory.New(typeof(CMRSTREQMessage));
			LoggingInformation logger = new LoggingInformation();
			processor = new STREQRMessageProcessor(logger);
		}
		STREQRMessageProcessor processor;
	}
}
