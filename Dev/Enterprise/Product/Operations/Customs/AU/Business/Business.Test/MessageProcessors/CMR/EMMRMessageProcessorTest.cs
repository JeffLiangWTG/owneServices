using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EMMRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestProcessEMMRClearResponsee()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EMMRClearMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.EMM));
			AssertEquals("PermitNumber", "AAAACRCTP", wrapper.CAN);

			ZString expectedResult =
				@"Consol #: C00001268

Status: CLEAR
Status Description: THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.

Status of Lines:
Line: 0001
	CAN: AAAACRCPE
	Status: CLEAR
	Status Description: THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.

";

			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
		}

		public void TestProcessEMMRRejectionResponse()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EMMRRejectionMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Status", Constants.CMRConsolStatus.Rejected, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.EMM));
			AssertContains("Report", "Consol #: C00001268\r\n\r\nStatus: REJECTED\r\nStatus Description: THE TRANSACTION HAS BEEN REJECTED DUE TO ERRORS. PLEASE CORRECT AND RE-SEND THE MESSAGE.\r\n\r\n\r\nErrors:\r\n\tLINE 0000: THE COMBINATION OF DATE OF DEPARTURE, CRAFT DETAILS AND DEPARTURE CTO ESTABLISHMENT ID QU\r\n".Replace("\r\n", "<br>"), processor.SentReport.Body);
		}

		public void TestEdifactEscapeCharacterWorks()
		{
			FTXSegment fTX = new FTXSegment();
			fTX.Parse(new UNOCCMRCharacterSet(), "FTX+AAO+++LINE 0000?: THE COMBINATION OF DATE OF DEPARTURE, CRAFT DETAILS AND DEPARTURE CTO ESTABLISHMENT ID QU");
			AssertEquals("FreeText1", "LINE 0000: THE COMBINATION OF DATE OF DEPARTURE, CRAFT DETAILS AND DEPARTURE CTO ESTABLISHMENT ID QU", fTX.TextLiteral.FreeTextValue1);
			fTX.Parse(new UNOCCMRCharacterSet(), "FTX+AAO+++LINE 0000?: THE COMBINATION OF DATE OF DEPARTURE??");
			AssertEquals("FreeText1", "LINE 0000: THE COMBINATION OF DATE OF DEPARTURE?", fTX.TextLiteral.FreeTextValue1);
		}

		protected override ZString GetExpectedMessageCode() => "EMM";

		protected override ZString GetExpectedMessageName() => "Export Main Manifest Response(EMMR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001268";
			wrapper = new FreightConsolWrapper(consol);
			outgoingMessage = (CMREMMMessage)consol.Messages.AddNew(typeof(CMREMMMessage));
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			processor = new EMMRMessageProcessorTestHelper(logger);
		}

		protected override Type IncomingMessageType => typeof(CMREMMRMessage);

		ForwardingConsol consol;
		EMMRMessageProcessorTestHelper processor;
		FreightConsolWrapper wrapper;

		sealed class EMMRMessageProcessorTestHelper : EMMRMessageProcessor
		{
			public EMMRMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
