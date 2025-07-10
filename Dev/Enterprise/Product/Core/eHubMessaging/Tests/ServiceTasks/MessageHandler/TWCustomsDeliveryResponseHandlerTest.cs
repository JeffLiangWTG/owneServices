using System;
using System.IO;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class TWCustomsDeliveryResponseHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write(@"<TWCustomsDeliveryNotification xmlns='http://cargowise.com/ehub/products/TWCustomsDeliveryResponse' xmlns:ns0='http://cargowise.com/ehub/core/2011/02' version='1.1'>
<Header>
	<SenderID>TWCustomsTest</SenderID>
	<RecipientID>WTLJTCJLU</RecipientID>
</Header>
<Body>
	<UniversalEvent xmlns='http://www.cargowise.com/Schemas/Universal/2012/11' version='1.1'>
		<Event>
			<EventTime>3/03/2020 2:20:24 AM</EventTime>
			<EventType>SNT</EventType>
			<ContextCollection>
				<Context>
					<Type>EntryNumber</Type>
					<Value>CA 0958000060</Value>
				</Context>
				<Context>
					<Type>EntryNumberType</Type>
					<Value>EXP</Value>
				</Context>
				<Context>
					<Type>EntryNumberCountryOfIssue</Type>
					<Value>TW</Value>
				</Context>
				<Context>
					<Type>MessageType</Type>
					<Value>ICD</Value>
				</Context>
				<Context>
					<Type>InterchangeNumber</Type>
					<Value>123456</Value>
				</Context>
			</ContextCollection>
		</Event>
	</UniversalEvent>
</Body>
</TWCustomsDeliveryNotification>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.TaiwanCustoms);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("TWCustoms");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new TWCustomsDeliveryResponseHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

			AssertEquals(EDIInterchange.ApplicationCodes.TaiwanCustoms, interchange.EI_ApplicationCode);
			AssertEquals("ICD", interchange.EI_InterchangeType);
			AssertEquals(XmlInterchangeHandlerTests.GetFileResourceString("TWCustomsDeliveryNotification.xml"), interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("TWCustoms", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
		}
	}
}
