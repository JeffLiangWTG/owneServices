using System;
using System.IO;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler
{
	class NativeOrganizationHandlingTests : TestCaseWithFactory
	{
		public void TestNativeOrganizationHandling()
		{
			using (Stream nativeOrganizationUDMEnvelop = ResourceManager.GetFileResource("TestFiles.NativeOrganizationUDMEnvelop.xml"))
			{
				var message = new Mock<IeHubMessage>();
				Guid trackingID = new Guid("728e874f-8f09-4860-a2a0-082eb62743a5");
				message.Setup(m => m.TrackingID).Returns(trackingID);
				message.Setup(m => m.SenderID).Returns("SenderID");
				message.Setup(m => m.RecipientID).Returns("RecipientID");
				message.Setup(m => m.SchemaName).Returns("Schema");
				message.Setup(m => m.MessageStream).Returns(nativeOrganizationUDMEnvelop);

				var handler = new Mock<UniversalInterchangeHeaderHandler>() { CallBase = true };

				var notification = new NotificationBuffer();
				handler.Object.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);
				AssertEDIInterchange(trackingID);
				AssertEquals(notification.AsString, "");

				handler.VerifyAll();
			}
		}

		void AssertEDIInterchange(Guid trackingID)
		{
			var ediInterchange = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingID));
			AssertNotNull("EDIInterchange matching EI_SessionGUID", ediInterchange);
			CombineAssertions(delegate
			{
				AssertEquals("ediInterchange.EI_ApplicationCode", ApplicationCodeList.Codes.NativeDataMessaging, ediInterchange.EI_ApplicationCode);
				AssertEquals("ediInterchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, ediInterchange.EI_InterchangeType);
				AssertEquals("ediInterchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, ediInterchange.EI_ReceiveTransmit);
				AssertEquals("ediInterchange.EI_From", "SenderID", ediInterchange.EI_From);
				AssertEquals("ediInterchange.EI_To", "RecipientID", ediInterchange.EI_To);
				AssertEquals("ediInterchange.EI_Status", EDIInterchange.Status.Received, ediInterchange.EI_Status);

				var expectedInterchangeHeader = "<Header xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\n    <SenderID>HYEDAUIKB</SenderID>\n    <RecipientID>test</RecipientID>\n  </Header>";

				AssertEquals("ediInterchange.EI_HeaderNText", expectedInterchangeHeader, ediInterchange.EI_HeaderNText);
				AssertEquals("ediInterchange.EI_BodyText", ResourceManager.GetFileResourceString("TestFiles.NativeOrganizationUDMEnvelopBody.xml").Trim(), ediInterchange.EI_BodyText);
				AssertEquals("ediInterchange.EI_FooterNText", "", ediInterchange.EI_FooterNText);
			});

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, ediInterchange.PK));
			AssertEquals("ediMessages.Length", 1, ediMessages.Length);
			var ediMessage = ediMessages[0];
			CombineAssertions(delegate
			{
				AssertEquals("ediMessage.EM_ApplicationCode", ApplicationCodeList.Codes.NativeDataMessaging, ediMessage.EM_ApplicationCode);
				AssertEquals("ediMessage.EM_MessageType", EDIMessageTypeList.Codes.XDC, ediMessage.EM_MessageType);
				AssertEquals("ediMessage.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlNativeOrganization, ediMessage.EM_MessageSubType);
				AssertEquals("ediMessage.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, ediMessage.EM_ReceiveTransmit);
				AssertEquals("ediMessage.EM_Status", EDIMessage.Status.Queued, ediMessage.EM_Status);
				AssertStartsWith("ediMessage.EM_MessageText", @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">", ediMessage.EM_MessageText);
				AssertEndsWith("ediMessage.EM_MessageText", @"</Native>", ediMessage.EM_MessageText);
			});
		}
	}
}
