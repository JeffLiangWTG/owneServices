using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCInterchangeProcessorTest : TestCaseWithFactory
	{
		[TestDate]
		public void TestProcessInterchangeWithOutMQContent()
		{
			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			interchangeToProcess.EI_InterchangeType = EDIMessage.ApplicationCodes.CMR;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "00000000000103";
			interchangeToProcess.EI_From = "AAA336C";
			interchangeToProcess.EI_To = "AAL364P";
			interchangeToProcess.EI_BodyText = interchangeBodyText;
			interchangeToProcess.EI_HeaderText = "UNA:+.? 'UNB+UNOC:3+AAA336C::AAA336C+AAL364P+130313:1458+00000000000103++++1++1'";
			interchangeToProcess.EI_FooterText = "UNZ+1+00000000000103'";
			Factory.Save();
			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "00000000000103000001"));
			AssertNotNull("Messages spawned", message);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", "ACR", message.EM_MessageType);
		}

		readonly string interchangeBodyText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+3GBJ 8GJH 2GF3:001+11'NAD+MR+AAL364P::95'RFF+ACW:AIRCR'RFF+AFM:9'RFF+ABO:A60001013/CM229::029'DTM+310:20130313035717:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5201:6:95'FTX+AAO+++THIS TRANSACTION WAS REJECTED'ERP+1'ERC+ERROR:80:95'ERC+CG5443:6:95'FTX+AAO+++HOUSE WAYBILL ARRIVAL DATE EXCEEDS THE TOLERABLE PERIOD (=4 DAYS) FROM THE REPORTING DATE'ERP+1'ERC+ERROR:80:95'ERC+CG0001:6:95'FTX+AAO+++IDENTIFIER FOR ORIGINAL TRANSACTION ALREADY EXISTS'CNT+55:002'UNT+21+000001'";

		[TestDate(2012, 7, 22)]
		public void TestProcessInterchangeWithMQContent()
		{
			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			interchangeToProcess.EI_InterchangeType = EDIMessage.ApplicationCodes.CMR;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "1234";
			interchangeToProcess.EI_From = "AUCustoms";
			interchangeToProcess.EI_To = "AAL364P";
			interchangeToProcess.EI_BodyText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingMQResponse.txt"));
			Factory.Save();

			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			AssertEquals("From is set from content", "AAA336C", interchangeToProcess.EI_From);
			AssertEquals("Interchange Num is set from content", "00000000000103", interchangeToProcess.EI_InterchangeNum);
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);
			AssertEquals("Header is set from content", "UNA:+.? 'UNB+UNOC:3+AAA336C::AAA336C+AAL364P+130313:1458+00000000000103++++1++1'", interchangeToProcess.EI_HeaderText);
			AssertEquals("Footer is set from content", "UNZ+1+00000000000103'", interchangeToProcess.EI_FooterText);
			AssertEquals("Body is set from content", interchangeBodyText, interchangeToProcess.EI_BodyText);
			var debugLogStrings = string.Join("\r\n", interchangeProcessor.Logger.DebugLogStrings.Cast<string>());
			AssertContains("Signature verified for interchange #1234", debugLogStrings);
			AssertContains("*** \tAcknowledgement message generated for inbound interchange #00000000000103", debugLogStrings);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "00000000000103000001"));
			AssertNotNull("Messages spawned", message);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", "ACR", message.EM_MessageType);

			message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, interchangeToProcess.PK));
			AssertNotNull("CTL Messages created", message);
			AssertEquals("Message to be sent", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message is a CTL", "CTL", message.EM_MessageType);
			AssertContains("Message is a CTL for processed interchange", "UCI+00000000000103+AAA336C", message.EM_MessageText);
		}

		[TestDate]
		public void TestProcessDuplicateInterchangeWithMQContent()
		{
			var duplicatedInterchange = Factory.New<EDIInterchange>();
			duplicatedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			duplicatedInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			duplicatedInterchange.EI_InterchangeType = EDIMessage.ApplicationCodes.CMR;
			duplicatedInterchange.EI_Status = EDIInterchange.Status.Received;
			duplicatedInterchange.EI_InterchangeNum = "00000000000103";
			duplicatedInterchange.EI_From = "AAA336C";
			duplicatedInterchange.EI_To = "AAL364P";

			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			interchangeToProcess.EI_InterchangeType = EDIMessage.ApplicationCodes.CMR;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "1234";
			interchangeToProcess.EI_From = "AUCustoms";
			interchangeToProcess.EI_To = "AAL364P";
			interchangeToProcess.EI_BodyText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IncomingMQResponse.txt"));
			Factory.Save();

			interchangeProcessor.ExecuteBatch();
			interchangeToProcess.Reload();
			var userLogStrings = string.Join("\r\n", interchangeProcessor.Logger.UserLogStrings.Cast<string>());
			AssertContains("CMR inbound interchange #00000000000103 acknowledged, but ignored, as it is a duplicate", userLogStrings);
			AssertEquals(EDIInterchange.Status.Error, interchangeToProcess.EI_Status);
			AssertEquals("AUCustoms", interchangeToProcess.EI_From);
			AssertEquals("1234", interchangeToProcess.EI_InterchangeNum);
			Assert("New interchange not deleted", !interchangeToProcess.IsDeleted);
			AssertEquals(1, interchangeToProcess.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "DUPLICATE")).Length);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "00000000000103000001"));
			AssertNull("Messages not spawned", message);
		}

		[TestDate]
		public void TestApplicationCodes()
		{
			var interchangeProcessor = new AUCInboundInterchangeProcessorForTesting();
			var applicationCodes = interchangeProcessor.ApplicationCodesExposed;

			Assert("CMR", applicationCodes.Any(x => x == EDIInterchange.ApplicationCodes.CMR));
			Assert("EXDOC", applicationCodes.Any(x => x == EDIInterchange.ApplicationCodes.EXDOC));
			Assert("OneStop", applicationCodes.Any(x => x == EDIInterchange.ApplicationCodes.OneStop));
			AssertEquals("Application Code Count", 3, applicationCodes.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var certificatesHelper = ObjectFactory.New<Integration.Customs.AU.ICertificateManagerHelper>(Factory);
			certificatesHelper.SetupValidCompanyCertificatesForTest();
			certificatesHelper.CreateCustomsCertificates2021();

			interchangeProcessor = new AUCInboundInterchangeProcessor(new LoggingInformation());
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			interchangeProcessor?.Dispose();
			base.TearDown();
		}
		AUCInboundInterchangeProcessor interchangeProcessor;

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.BatchProcessor.TestFiles." + fileName;

		sealed class AUCInboundInterchangeProcessorForTesting : AUCInboundInterchangeProcessor
		{
			public AUCInboundInterchangeProcessorForTesting()
				: base(new LoggingInformation())
			{
			}

			public string[] ApplicationCodesExposed => ApplicationCodes;
		}
	}
}
