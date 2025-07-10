using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Tests;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.Business.DownloadHandler.Tests
{
	class TelematicsInterchangeHandlerTest : TestCaseWithFactory
	{
		const string testMessageBodyPbd_Success = @"
<ns0:TelematicsInterchange xmlns:ns0=""http://cargowise.com/ehub/products/telematics/2013/05"">
  <Header>
    <SenderID>TELEMATIC</SenderID>
    <RecipientID>TELEMATIC</RecipientID>
  </Header>
  <Body>
    <ProtobufData>CAEQARpDH4sIAAAAAAAEANNiYGDgSBFS4xI4tXLy3hNnEl2an8d/CZmbclGIMzMgIz8vVcE/WIm9wsIs3syEAQAKS62WLgAAAA==</ProtobufData>
  </Body>
</ns0:TelematicsInterchange>
		";

		const string testMessageBodyTxd_Success = @"
<ns0:TelematicsInterchange xmlns:ns0=""http://cargowise.com/ehub/products/telematics/2013/05"">
  <Header>
    <SenderID>DummyFrom</SenderID>
    <RecipientID>DummyTo</RecipientID>
  </Header>
  <Body>
    <TelematicsXmlData>
      <DataFlowReadyMessage CargoWiseOneLicense=""MyEdiLicence"" />
    </TelematicsXmlData>
    <TelematicsXmlData>
      <ServerRegistrationRequestMessage EHubId=""TELMIDSERV"" />
    </TelematicsXmlData>
  </Body>
</ns0:TelematicsInterchange>
";

		const string testMessageBody_Unknown = @"
<ns0:TelematicsInterchange xmlns:ns0=""http://cargowise.com/ehub/products/telematics/2013/05"">
  <Header>
    <SenderID>TELEMATIC</SenderID>
    <RecipientID>TELEMATIC</RecipientID>
  </Header>
  <Body>
    <DONTASKMEIDONTKNOW>CAEQARpDH4sIAAAAAAAEANNiYGDgSBFS4xI4tXLy3hNnEl2an8d/CZmbclGIMzMgIz8vVcE/WIm9wsIs3syEAQAKS62WLgAAAA==</DONTASKMEIDONTKNOW>
  </Body>
</ns0:TelematicsInterchange>
		";

		const string testMessageBody_Failed = @"
<ns0:TelematicsInterchange xmlns:ns0=""http://cargowise.com/ehub/products/telematics/2013/05"">
  <Header>
    <SenderID>TELEMATIC</SenderID>
    <RecipientID>TELEMATIC</RecipientID>
  </Header>
  <Body>
    CAEQARpDH4sIAAAAAAAEANNiYGDgSBFS4xI4tXLy3hNnEl2an8d/CZmbclGIMzMgIz8vVcE/WIm9wsIs3syEAQAKS62WLgAAAA==
  </Body>
</ns0:TelematicsInterchange>
		";

		TelematicsInterchangeHandler handler;

		protected override void SetUp()
		{
			base.SetUp();
			handler = new TelematicsInterchangeHandler();
		}

		public void TestTelematicsInterchangeHandler_Successful()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PRE: Test should start with empty EDIMessage Table", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
				AssertEquals("PRE: Test should start with empty EDIInterchange Table", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			});
			var messageBytes = Encoding.Default.GetBytes(testMessageBodyPbd_Success);
			var messageStream = new MemoryStream(messageBytes);
			var testeHubMessage = new eHubMessage(Guid.NewGuid(), "TestSender", "TestRecipient", CargoWise.eHub.Common.MessageSchemaType.Xml, "TEL", "TestSchemaName", messageStream, "TestEmailSubjest", "TestFilename");

			handler.SaveMessage(testeHubMessage, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());

			CombineAssertions(() =>
			{
				var reloadedEDIMessages = Factory.Load<EDIMessage>(new ZQuery());
				var reloadedEDIInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("Incorrect # of EDIInterchange after processing", 1, reloadedEDIInterchanges.Length);
				AssertEquals("Incorrect # of EDIMessage after processing", 1, reloadedEDIMessages.Length);

				AssertEquals("Incorrect Interchange Status", "RCV", reloadedEDIInterchanges[0].EI_Status);
				AssertEquals("Incorrect Message Status", "QUE", reloadedEDIMessages[0].EM_Status);
				AssertEquals("Incorrect Message SubType", "PBD", reloadedEDIMessages[0].EM_MessageSubType);
				AssertEquals("Incorrect Message Type", "XDC", reloadedEDIMessages[0].EM_MessageType);
				AssertEquals("Incorrect Message Text", "<ProtobufData>CAEQARpDH4sIAAAAAAAEANNiYGDgSBFS4xI4tXLy3hNnEl2an8d/CZmbclGIMzMgIz8vVcE/WIm9wsIs3syEAQAKS62WLgAAAA==</ProtobufData>", reloadedEDIMessages[0].EM_MessageText);
			});
		}

		public void TestTelematicsInterchangeHandlerTelematicsXmlData_Successful()
		{
			// Arrange
			CombineAssertions(() =>
			{
				AssertEquals("PRE: Test should start with empty EDIMessage Table", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
				AssertEquals("PRE: Test should start with empty EDIInterchange Table", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			});
			var messageBytes = Encoding.Default.GetBytes(testMessageBodyTxd_Success);
			var messageStream = new MemoryStream(messageBytes);
			var testeHubMessage = new eHubMessage(Guid.NewGuid(), "TestSender", "TestRecipient", CargoWise.eHub.Common.MessageSchemaType.Xml, "TEL", "TestSchemaName", messageStream, "TestEmailSubjest", "TestFilename");

			// Act
			handler.SaveMessage(testeHubMessage, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());

			// Assert
			CombineAssertions(() =>
			{
				var reloadedEDIMessages = Factory.Load<EDIMessage>(new ZQuery());
				var reloadedEDIInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("Incorrect # of EDIInterchange after processing", 1, reloadedEDIInterchanges.Length);
				AssertEquals("Incorrect # of EDIMessage after processing", 2, reloadedEDIMessages.Length);

				var ediInterchange = reloadedEDIInterchanges[0];
				AssertEquals(ReceiveTransmitList.Codes.Receive, ediInterchange.EI_Status);
				AssertEquals(ApplicationCodeList.Codes.Telematics, ediInterchange.EI_ApplicationCode);
				AssertEquals(EDIInterchangeTypeList.Codes.Telematics, ediInterchange.EI_InterchangeType);
				AssertEquals(EDIInterchangeStatusList.Codes.Received, ediInterchange.EI_Status);

				foreach (var ediMessage in reloadedEDIMessages)
				{
					AssertEquals(ApplicationCodeList.Codes.Telematics, ediMessage.EM_ApplicationCode);
					AssertEquals(EDIMessageTypeList.Codes.XDC, ediMessage.EM_MessageType);
					AssertEquals(TelematicsMessageList.Codes.TelematicsXmlData, ediMessage.EM_MessageSubType);
					AssertEquals(ReceiveTransmitList.Codes.Receive, ediMessage.EM_ReceiveTransmit);
					AssertEquals(EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				}

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"<TelematicsXmlData>\r\n      <DataFlowReadyMessage CargoWiseOneLicense=\"MyEdiLicence\" />\r\n    </TelematicsXmlData>",
						"<TelematicsXmlData>\r\n      <ServerRegistrationRequestMessage EHubId=\"TELMIDSERV\" />\r\n    </TelematicsXmlData>",
					},
					reloadedEDIMessages.Select(message => message.EM_MessageText));
			});
		}

		public void TestTelematicsInterchangeHandler_Unknown()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PRE: Test should start with empty EDIMessage Table", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
				AssertEquals("PRE: Test should start with empty EDIInterchange Table", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			});
			var messageBytes = Encoding.Default.GetBytes(testMessageBody_Unknown);
			var messageStream = new MemoryStream(messageBytes);
			var testeHubMessage = new eHubMessage(Guid.NewGuid(), "TestSender", "TestRecipient", CargoWise.eHub.Common.MessageSchemaType.Xml, "TEL", "TestSchemaName", messageStream, "TestEmailSubjest", "TestFilename");

			handler.SaveMessage(testeHubMessage, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());

			CombineAssertions(() =>
			{
				var reloadedEDIMessages = Factory.Load<EDIMessage>(new ZQuery());
				var reloadedEDIInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("Incorrect # of EDIInterchange after processing", 1, reloadedEDIInterchanges.Length);
				AssertEquals("Incorrect # of EDIMessage after processing", 0, reloadedEDIMessages.Length);

				AssertEquals("Incorrect Interchange Status", "FAL", reloadedEDIInterchanges[0].EI_Status);
			});
		}

		public void TestTelematicsInterchangeHandler_Failed()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PRE: Test should start with empty EDIMessage Table", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
				AssertEquals("PRE: Test should start with empty EDIInterchange Table", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			});
			var messageBytes = Encoding.Default.GetBytes(testMessageBody_Failed);
			var messageStream = new MemoryStream(messageBytes);
			var testeHubMessage = new eHubMessage(Guid.NewGuid(), "TestSender", "TestRecipient", CargoWise.eHub.Common.MessageSchemaType.Xml, "TEL", "TestSchemaName", messageStream, "TestEmailSubjest", "TestFilename");

			handler.SaveMessage(testeHubMessage, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());

			CombineAssertions(() =>
			{
				var reloadedEDIMessages = Factory.Load<EDIMessage>(new ZQuery());
				var reloadedEDIInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("Incorrect # of EDIInterchange after processing", 1, reloadedEDIInterchanges.Length);
				AssertEquals("Incorrect # of EDIMessage after processing", 0, reloadedEDIMessages.Length);
			});
		}
	}
}
