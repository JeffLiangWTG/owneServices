using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class ACIMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestOverrides()
		{
			var expMessageProcessor = new ACIMessageProcessor(logger);
			AssertEquals("ApplicationCode", EDIMessage.ApplicationCodes.CAACI, expMessageProcessor.ApplicationCode);
			AssertEquals("MessageFriendlyName", "CA Customs ACI", expMessageProcessor.MessageFriendlyName);
		}

		public void TestAMessage()
		{
			const string messageText =
@"UNH+MSGREFNO123+CUSRES:D:00A:UN'
BGM+:::687+8990CCN22222+11'
DTM+9:200406161523:203'
GIS+14'
ERP+2:987654321:28'
ERC+ZZZ'
FTX+AAO+++SEGMENT NAD BYTE OFFSET 383'
FTX+AAO+++SEGMENT NAD LINE 18 ELEM 3164 [6.0] ELEM TOO LONG'
UNT+9+MSGREFNO123'
";
			var aciMessage = GetEDIMessage<ACIEDIMessage>(messageText);
			aciMessageProcessor.ProcessMessage(aciMessage);
			AssertEquals(EDIMessage.Status.Failed, aciMessage.EM_Status);
		}

		public void TestAMessage_MessageSyntaxError_IncludeSentMessageText()
		{
			var helper = new CusSCATestHelper();
			var factory = helper.HelperFactory;
			var oceanBill = helper.OceanBill;
			var house = helper.House;
			house.SupplementaryReferenceNumber = "8005S1800576430";

			var requestEHubID = ZGuid.NewZGuid();
			var requestInterchange = factory.NewWithValidTestData<Enterprise.Messaging.Business.EDIInterchange>();
			requestInterchange.EI_HeaderText = "RequestHEADER";
			requestInterchange.EI_BodyText = "BODY";
			requestInterchange.EI_FooterText = "FOOTER";
			requestInterchange.EI_InterchangeType = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.XMS;
			requestInterchange.EI_Status = "FAL";
			requestInterchange.EI_SessionGUID = requestEHubID;

			var responseEHubID = ZGuid.NewZGuid();
			var responseInterchange = factory.NewWithValidTestData<Enterprise.Messaging.Business.EDIInterchange>();
			responseInterchange.EI_HeaderText = "ResponseHEADER";
			responseInterchange.EI_BodyText = "BODY";
			responseInterchange.EI_FooterText = "FOOTER";
			responseInterchange.EI_InterchangeType = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.XMS;
			responseInterchange.EI_Status = "FAL";
			responseInterchange.EI_SessionGUID = responseEHubID;

			var outMessage = SyntaxErrorMessageTest.GetSentEDIMessage<SUPRPTMessage>(factory, SyntaxErrorMessageTest.SupplementaryCargoReportMessageText, new ZDateTime(2018, 12, 31));
			outMessage.EM_EI = requestInterchange.PK;
			house.Messages.Add(outMessage);
			factory.Save();

			var shipment = factory.New<ForwardingShipment>();
			house.CA_JS = shipment.PK;
			factory.Save();

			string aciMessageText =
			$@"UNH+1+CUSRES:D:00A:UN
BGM+:::687+8005S1800576430+11
DTM+9:201901030537:203
GIS+14
ERP+2:{outMessage.EM_MessageNum}:28
ERC+ZZZ
FTX+AAO+++SEGMENTFTXLINE32ELEM4440 4.1 INVALID CHARS
ERP+2:{outMessage.EM_MessageNum}:28
ERC+ZZZ
FTX+AAO+++SEGMENTFTXLINE46ELEM4440 4.1 INVALID CHARS
UNT+11+1
";
			var aciMessage = SyntaxErrorMessageTest.GetRecivedEDIMessage<ACIEDIMessage>(factory, aciMessageText, new ZDateTime(2019, 1, 1));
			aciMessage.EM_EI = responseInterchange.PK;
			aciMessageProcessor.ProcessMessage(aciMessage);

			AssertEquals($@"Can response message's linked object be found? 'Y';
Can last request message be found? 'Y';
Did last request message sent with message errors? 'N';
Request message eHub Tracking ID: '{requestEHubID.ToString()}';
Response message eHub Tracking ID: '{responseEHubID.ToString()}';
<Request>{SyntaxErrorMessageTest.SupplementaryCargoReportMessageText.Replace("\r\n", "'")}</Request><Response>{aciMessageText.Replace("\r\n", "'")}</Response>;
<RequestInterChange>RequestHEADER</RequestInterChange><ResponseInterChange>ResponseHEADER</ResponseInterChange>", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		ACIMessageProcessor aciMessageProcessor;

		protected override void SetUp()
		{
			base.SetUp();
			aciMessageProcessor = new ACIMessageProcessor(logger);
		}
	}
}
