using System;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class TRCustomsMessageHandlerForETradeRegistrationTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var stream = XmlInterchangeHandlerTests.GetFileResource("TRCustomsResponse.xml");

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(stream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.TRCustoms);
			message.Setup(m => m.SchemaName).Returns(EDIInterchangeTypeList.Descriptions.TRCustomsETradeRegistration);
			message.Setup(m => m.SenderID).Returns("TROCustomsTest");
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new TRCustomsMessageHandlerForETradeRegistration();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());

			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("EI_FooterText", "", interchange.EI_FooterText);
				AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.TRCustoms, interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeType", EDIInterchangeTypeList.Codes.TRCustomsETradeRegistration, interchange.EI_InterchangeType);
				AssertEquals("EI_SessionGUID", trackingID, interchange.EI_SessionGUID);
				AssertEquals("interchange", EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("ContainedMessages.Count", 0, interchange.ContainedMessages.Count);
				AssertEquals("EI_HeaderText", "", interchange.EI_HeaderText);
			});

			var expectedBodyText = XmlInterchangeHandlerTests.GetFileResourceString("TRCustomsResponseBody.xml");

			MessageHandlerTestHelper.CompareXmlString(expectedBodyText, interchange.EI_BodyText);
		}
	}
}
