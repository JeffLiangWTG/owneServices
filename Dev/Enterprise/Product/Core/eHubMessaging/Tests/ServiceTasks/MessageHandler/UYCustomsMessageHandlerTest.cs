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
	class UYCustomsMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write("<DAERespuesta><Respuestas></Respuestas><Signature></Signature></DAERespuesta>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.UYCustoms);
			message.Setup(m => m.SchemaName).Returns(EDIInterchangeTypeList.Descriptions.UYCustoms);
			message.Setup(m => m.SenderID).Returns("UYCustomsTest");
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new UYCustomsMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals("<DAERespuesta><Respuestas></Respuestas><Signature></Signature></DAERespuesta>", interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(EDIInterchange.ApplicationCodes.UYCustoms, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.UYCustoms, interchange.EI_InterchangeType);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
