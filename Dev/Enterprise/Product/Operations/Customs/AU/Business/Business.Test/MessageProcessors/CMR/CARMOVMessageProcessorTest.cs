using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CARMOVMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestProcessLoadMessageNotify()
		{
			Env.Registry.AUCustoms.AlertCarrierMovementLoad = true;
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CARMOVLoadMessage.txt")).Replace("\r\n", "");
			processor.PreProcessMessage(incomingMessage);
			AssertEquals("PreProcessed", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Processed", EDIMessage.Status.Received, incomingMessage.EM_Status);

			AssertEquals("Subject", "CARMOV - LOAD Advice Received", processor.SentReport.Subject);
			AssertEquals("Report", "Status: LOAD\r\nStatus Description: THE REPORTED EXPORT SHIPMENT MAY BE LOADED.\r\n\tVessel Lloyds Number: 9004499\r\n\tVoyage Number: 109\r\n\tCTO Establishment ID: FL53K\r\n\tCAN: AAAAGN64Y\r\n\tContainer Number: GLDU0981319\r\n", processor.SentReport.Body);
			AssertEquals("Attachments", 1, processor.SentReport.Attachments.Count);
		}

		public void TestProcessLoadMessageDontNotify()
		{
			Env.Registry.AUCustoms.AlertCarrierMovementLoad = false;
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CARMOVLoadMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			Assert("ReportNotSent", processor.SentReport == null);
		}

		public void TestProcessDoNotLoadMessage()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CARMOVDoNotLoadMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Subject", "CARMOV - DO NOT LOAD Advice Received", processor.SentReport.Subject);
			AssertEquals("Report", "Status: DO NOT LOA\r\nStatus Description: THE REPORTED EXPORT SHIPMENT MAY NOT BE LOADED. RECEIVAL NOTICE MAY CONTAIN INCORRECT INFORMATION.\r\n\tVessel Lloyds Number: 8518089\r\n\tVoyage Number: V369N\r\n\tCTO Establishment ID: FE99N\r\n\tCAN: AAAAETMHY\r\n\tContainer Number: CELU3023978\r\n", processor.SentReport.Body);
		}

		public void TestGetBranchGuidForCTOID()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			OrgHeader cTOOrganisation = Factory.New<OrgHeader>();

			AssertNull(processor.GetBranchGuidForCTOID(Factory, "12345"));
			cTOOrganisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "12345");
			AssertNull(processor.GetBranchGuidForCTOID(Factory, "12345"));
			cTOOrganisation.OH_RL_NKClosestPort = "AUNTL";
			AssertNull(processor.GetBranchGuidForCTOID(Factory, "12345"));
			branch.GB_RL_NKHomePort = "AUNTL";
			AssertNull(processor.GetBranchGuidForCTOID(Factory, "12345"));
			cTOOrganisation.OH_IsSeaCTO = true;
			AssertNull(processor.GetBranchGuidForCTOID(Factory, "12345"));
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals("Branch", branch, processor.GetBranchGuidForCTOID(Factory, "12345"));
		}

		public void TestGetCTOEstablishmentID()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("CARMOVDoNotLoadMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			string cTOID = processor.GetCTOEstablishmentID();
			AssertEquals("CTOID", "FE99N", cTOID);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.CARMOV;

		protected override ZString GetExpectedMessageName() => "Carrier Movement Advice (CARMOV)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override Type IncomingMessageType => typeof(CMRCARMOVMessage);

		protected override void SetUp()
		{
			base.SetUp();
			processor = new TestHelperCARMOVMessageProcessor(logger);
		}
		TestHelperCARMOVMessageProcessor processor;

		sealed class TestHelperCARMOVMessageProcessor : CARMOVMessageProcessor
		{
			public TestHelperCARMOVMessageProcessor(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
