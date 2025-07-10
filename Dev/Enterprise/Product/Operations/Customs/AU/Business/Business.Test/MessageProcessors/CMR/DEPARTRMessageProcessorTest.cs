using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DEPARTRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestProcessStandaloneResponse()
		{
			SetupExportCustomsManifestHeader();
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Status", CMR3CharDocumentStatus.Clear.Code, header.ED_DepartureReportStatus);
		}

		protected override ZString GetExpectedMessageCode() => "DEP";

		protected override ZString GetExpectedMessageName() => "Export Departure Report (DEPARTR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override Type IncomingMessageType => typeof(CMRDEPARTRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			LoggingInformation logger = new LoggingInformation();
			processor = new DEPARTRMessageProcessor(logger);

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("DEPARTClearMessage.txt"));
		}
		DEPARTRMessageProcessor processor;
		ExportCustomsManifestHeader header;

		void SetupExportCustomsManifestHeader()
		{
			header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_BGMReference = "K00001455";
		}
	}
}
