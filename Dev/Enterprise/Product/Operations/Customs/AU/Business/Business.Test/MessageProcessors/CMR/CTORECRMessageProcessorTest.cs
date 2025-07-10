using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTORECRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestLoadMessage()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CTORECRLoadMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.MovementStatusResponseSubTypes.Load, incomingMessage.EM_MessageSubType);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.CTOREC;

		protected override ZString GetExpectedMessageName() => "CTO Receival Notice Response(CTORECR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override Type IncomingMessageType => typeof(CMRCTORECRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "S00002023";
			outgoingMessage = (CMRCTORECMessage)declaration.Messages.AddNew(typeof(CMRCTORECMessage));
			processor = new CTORECRMessageProcessorTestHelper(logger);
		}
		JobDeclaration declaration;
		CTORECRMessageProcessorTestHelper processor;

		sealed class CTORECRMessageProcessorTestHelper : CTORECRMessageProcessor
		{
			public CTORECRMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
