using System;
using System.IO;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class ITCustomsRequestResponseMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var xmlContent =
				$@"<ITCustoms xmlns=""http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse""><Files><File><Name>845A0514.R01</Name></File></Files></ITCustoms>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(ApplicationCodeList.Codes.eHub);
			message.Setup(m => m.RecipientID).Returns("eHUB");
			message.Setup(m => m.SenderID).Returns("ITCustoms");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new ITCustomsRequestResponseMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(ApplicationCodeList.Codes.eHub, interchange.EI_ApplicationCode);
			AssertEquals("ITR", interchange.EI_InterchangeType);
			AssertEquals(xmlContent, interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("ITCustoms", interchange.EI_From);
			AssertEquals("eHUB", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals(xmlContent, interchange.EI_HeaderText);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
