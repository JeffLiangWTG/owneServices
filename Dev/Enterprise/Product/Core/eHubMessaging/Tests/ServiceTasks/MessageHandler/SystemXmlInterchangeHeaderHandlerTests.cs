using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler
{
	class SystemXmlInterchangeHeaderHandlerTests : TestCaseWithFactory
	{
		public void TestCustomerServiceResponse()
		{
			string messageXml = @"<CustomerServiceResponse xmlns=""http://www.cargowise.com/Schemas/System""><Anything /></CustomerServiceResponse>";
			CreateAndValidateInterchange(messageXml, SystemMessageList.Codes.CustomerServiceResponse);
		}

		public void TestCurrentVersionReport()
		{
			string messageXml = @"<CurrentVersionReport xmlns=""http://www.cargowise.com/Schemas/System""><Anything /></CurrentVersionReport>";
			CreateAndValidateInterchange(messageXml, SystemMessageList.Codes.CurrentVersionReport);
		}

		public void TestDeliveredVersionReport()
		{
			string messageXml = @"<DeliveredVersionReport xmlns=""http://www.cargowise.com/Schemas/System""><Anything /></DeliveredVersionReport>";
			CreateAndValidateInterchange(messageXml, SystemMessageList.Codes.DeliveredVersionReport);
		}

		public void TestLogsReport()
		{
			string messageXml = @"<LogsReport xmlns=""http://www.cargowise.com/Schemas/System""><Anything /></LogsReport>";
			CreateAndValidateInterchange(messageXml, SystemMessageList.Codes.LogsReport);
		}

		public void TestLinkTrack()
		{
			string messageXml = @"<LinkTrack xmlns=""http://www.cargowise.com/Schemas/System""><Anything /></LinkTrack>";
			CreateAndValidateInterchange(messageXml, SystemMessageList.Codes.LinkTrack);
		}

		public void TestUnknownMessage()
		{
			string messageXml = @"<MessageFromTheFuture xmlns=""http://www.cargowise.com/Schemas/System""><Anything /></MessageFromTheFuture>";
			CreateAndValidateInterchange(messageXml, EDIMessageSubTypeList.Codes.Unknown);
		}

		public void CreateAndValidateInterchange(string messageXml, string expectedMessageSubType)
		{
			TestHelpers.DropSequenceIfExists("SystemEDIInterchangeNumber-00000000-0000-0000-0000-000000000000");
			TestHelpers.DropSequenceIfExists("SystemEDIMessageNumber-00000000-0000-0000-0000-000000000000");
			string interchangeXml = InterchangeXmlTemplate.Replace("***MESSAGE GOES HERE***", messageXml);
			var stream = new MemoryStream(Encoding.ASCII.GetBytes(interchangeXml));
			var message = new eHubMessage(Guid.NewGuid(), "Sender1", "Recipient1", MessageSchemaType.Xml, ApplicationCodeList.Codes.SYS, EDIInterchangeTypeList.Descriptions.SYS, stream);
			var notification = new NotificationBuffer();

			AssertEquals("Precondition", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var handler = HandlerFactory.GetHandler(message.SchemaName) as SystemXmlInterchangeHeaderHandler;
			AssertNotNull(handler);
			var company = TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current);
			handler.SaveMessage(message, company, notification);
			AssertEquals(notification.AsString, "");
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			EDIInterchange interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

			AssertEquals(EDIInterchange.Status.Received, interchange.EI_ReceiveTransmit);
			AssertEquals(message.SenderID, interchange.EI_From);
			AssertEquals(message.RecipientID, interchange.EI_To);
			AssertEquals(company.FirstActiveBranch.PK, interchange.EI_GB);
			AssertEquals(message.TrackingID, interchange.EI_SessionGUID);
			AssertEquals("SYS00000000000000001", interchange.EI_InterchangeNum);

			TextReader headerReader = interchange.GetEI_HeaderTextReader();
			AssertEquals("header", "", headerReader.ReadToEnd());
			headerReader.Close();

			TextReader bodyReader = interchange.GetEI_BodyTextReader();
			AssertEquals("body", "", bodyReader.ReadToEnd());
			bodyReader.Close();

			TextReader footerReader = interchange.GetEI_FooterTextReader();
			AssertEquals("footer", "", footerReader.ReadToEnd());
			footerReader.Close();

			foreach (EDIMessage msg in interchange.ContainedMessages)
			{
				if (msg.EM_MessageSubType == EDIMessageSubTypeList.Codes.Unknown)
				{
					AssertEquals(EDIMessage.Status.Failed, msg.EM_Status);
				}
				else
				{
					AssertEquals(EDIMessage.Status.Queued, msg.EM_Status);
				}
				AssertEquals(false, msg.EM_IsTestMessage);
				AssertEquals(EDIMessageTypeList.Codes.XMS, msg.EM_MessageType);
				AssertEquals(expectedMessageSubType, msg.EM_MessageSubType);
				AssertEquals(interchange.PK, msg.EM_EI);
				AssertEquals(interchange.EI_GB, msg.EM_GB);
				AssertEquals("SYS00000000000000001", msg.EM_MessageNum);
				AssertEquals(ApplicationCodeList.Codes.SYS, msg.EM_ApplicationCode);
				TextReader messageReader = msg.GetEM_MessageTextReader();
				AssertEquals("Message is different", true, CompareXmlString(messageReader.ReadToEnd(), messageXml));
				messageReader.Close();
			}
		}

		const string InterchangeXmlTemplate = @"<?xml version=""1.0""?>
<SystemInterchange xmlns=""http://www.cargowise.com/Schemas/System"">
  <Body>
	***MESSAGE GOES HERE***
  </Body>
</SystemInterchange>";

		public bool CompareXmlString(string text1, string text2)
		{
			return MessageHandlerTestHelper.CompareXmlString(text1, text2);
		}
	}
}
