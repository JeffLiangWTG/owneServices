using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IDLMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestProcessResponse()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IDLMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertContains("Report", "Declaration Reference: B00122382\r\n\r\nStatus: IDLE\r\n\r\n\r\nErrors:\r\n\tTHE CAN MENTIONED IN THE LINE TO WHICH THIS ADVICE RELATES HAS BEEN DEEMED AS IDLE BY CUSTOMS. IF THE CAN HAS BEEN EXPORTED, PROVIDE CUSTOMS WITH A PROOF OF EXPORT. IF THE EXPORTATION HAS BEEN DELAYED, AMEND THE DATE OF EXPORT ON THE CAN TO THE NEW DATE. IF THE GOODS ARE NOT BE EXPORTED, WITHDRAW THE CAN. IF NO ACTION IS TAKEN BY 03-JUL-2004 THEN THE CAN AND THE AUTHORITY TO DEAL FOR THE ASSOCIATED GOODS WILL BE REVOKED BY CUSTOMS.\r\n".Replace("\r\n", "<br>"), processor.SentReport.Body);
		}

		public void TestProcessResponseForStandAloneManifest()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_BGMReference = "K00001001";
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00122382";
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			outgoingMessage = declaration.Messages.AddNew(typeof(CMREXDMessage));
			outgoingMessage.EM_MessageSubType = "ORG";

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IDLMessage.txt")).Replace("\r\n", "").Replace("B00122382/1", "K00001001/1");
			processor.ProcessMessage(incomingMessage);
			AssertContains("Report", "Reference: K00001001\r\n\r\nStatus: IDLE\r\n\r\n\r\nErrors:\r\n\tTHE CAN MENTIONED IN THE LINE TO WHICH THIS ADVICE RELATES HAS BEEN DEEMED AS IDLE BY CUSTOMS. IF THE CAN HAS BEEN EXPORTED, PROVIDE CUSTOMS WITH A PROOF OF EXPORT. IF THE EXPORTATION HAS BEEN DELAYED, AMEND THE DATE OF EXPORT ON THE CAN TO THE NEW DATE. IF THE GOODS ARE NOT BE EXPORTED, WITHDRAW THE CAN. IF NO ACTION IS TAKEN BY 03-JUL-2004 THEN THE CAN AND THE AUTHORITY TO DEAL FOR THE ASSOCIATED GOODS WILL BE REVOKED BY CUSTOMS.\r\n".Replace("\r\n", "<br>"), processor.SentReport.Body);
		}

		protected override ZString GetExpectedMessageCode() => "IDL";

		protected override ZString GetExpectedMessageName() => "Idle EDN/CRN Advice (IDL)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00122382";
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			outgoingMessage = declaration.Messages.AddNew(typeof(CMREXDMessage));
			outgoingMessage.EM_MessageSubType = "ORG";
			processor = new TestHelperIDLMessageProcessor(logger);
		}

		protected override Type IncomingMessageType => typeof(CMRIDLMessage);

		TestHelperIDLMessageProcessor processor;
		JobDeclaration declaration;

		sealed class TestHelperIDLMessageProcessor : IDLMessageProcessor
		{
			public TestHelperIDLMessageProcessor(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
