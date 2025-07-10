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
	class ITCustomsMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange_Flatfile()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var xmlContent = $@"<ITMessage xmlns:ns=""http://cargowise.com/ehub/products/ITCustoms"">
									<MessageType>U</MessageType>
									<FileName>200119.ULR</FileName>
									<eHubTrackingIDFromSentInterchange>abcd</eHubTrackingIDFromSentInterchange>
									<Header> first line from the response message </Header>
									<Message>Body</Message>
								</ITMessage>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(ApplicationCodeList.Codes.ITCustoms);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("ITCustoms");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new ITCustomsMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(ApplicationCodeList.Codes.ITCustoms, interchange.EI_ApplicationCode);
			AssertEquals("U", interchange.EI_InterchangeType);
			AssertEquals("Body", interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("ITCustoms", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals($@"<ITMessage xmlns:ns=""http://cargowise.com/ehub/products/ITCustoms""><MessageType>U</MessageType><FileName>200119.ULR</FileName><eHubTrackingIDFromSentInterchange>abcd</eHubTrackingIDFromSentInterchange><Header> first line from the response message </Header></ITMessage>", interchange.EI_HeaderText);

			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		public void TestCreateInterchange_Xml()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var xmlContent = $@"<ITMessage> 
                                            <MessageType>L</MessageType> 
                                            <FileName>845A0715.Q06</FileName> 
                                            <eHubTrackingIDFromSentInterchange>4dc035b0-f4b7-42c4-ac45-aa83b14a1bec</eHubTrackingIDFromSentInterchange> 
                                            <Message><esito_bolletta /></Message>
                                        </ITMessage>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(ApplicationCodeList.Codes.ITCustoms);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("ITCustoms");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new ITCustomsMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(ApplicationCodeList.Codes.ITCustoms, interchange.EI_ApplicationCode);
			AssertEquals("L", interchange.EI_InterchangeType);
			AssertEquals("<esito_bolletta />", interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("ITCustoms", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals($@"<ITMessage><MessageType>L</MessageType><FileName>845A0715.Q06</FileName><eHubTrackingIDFromSentInterchange>4dc035b0-f4b7-42c4-ac45-aa83b14a1bec</eHubTrackingIDFromSentInterchange></ITMessage>", interchange.EI_HeaderText);

			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
