using System;
using System.IO;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class CanadianCustomsReplyHandlerTests : TestCaseWithFactory
	{
		public void TestCanadianCustomsReplyHandler()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write(@"<CanadianCustomsReply xmlns=""http://cargowise.com/ehub/products/canadiancustoms""><Reference>YUSAIRXPN - INETCECPT</Reference><Content>VU5CK1VOT0E6MytJTkVUQ0VDUFQrWVVTQUlSWFBOKzE0MDgyNjoyMzIwKzM0OTgrKysrKysxJ1VORytDVVNSRVMrQ0NSK1UxMDIwN1YxKzE0MDgyNjoyMzIwKzU3NStVTitEOjAwQSdVTkgrMStDVVNSRVM6RDowMEE6VU4nQkdNKzo6OjY4Nys4MDM2UzAwMDQzMTYzVkFOKzExJ0RUTSs5OjIwMTQwODI2MjMxNjoyMDMnR0lTKzE0J0VSUCsyOjExNjg6MjInRVJDK1MyMidVTlQrNysxJ1VORSsxKzU3NSdVTlorMSszNDk4Jw==</Content></CanadianCustomsReply>");
			writer.Flush();

			var trackingId = Guid.NewGuid();

			var mockRepository = new MockRepository(MockBehavior.Default);
			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.XMS);
			message.Setup(m => m.RecipientID).Returns(GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			message.Setup(m => m.SenderID).Returns("CACustoms");
			message.Setup(m => m.TrackingID).Returns(trackingId);
			message.Setup(m => m.Filename).Returns("FileName");
			message.Setup(m => m.EmailSubject).Returns("");

			AssertEquals("PRECONDITION: 0 interchange in database", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new CanadianCustomsReplyHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.CACustoms, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.CanadianCustoms, interchange.EI_InterchangeType);
			AssertEquals("UNB+UNOA:3+INETCECPT+YUSAIRXPN+140826:2320+3498++++++1'UNG+CUSRES+CCR+U10207V1+140826:2320+575+UN+D:00A'UNH+1+CUSRES:D:00A:UN'BGM+:::687+8036S00043163VAN+11'DTM+9:201408262316:203'GIS+14'ERP+2:1168:22'ERC+S22'UNT+7+1'UNE+1+575'UNZ+1+3498'", interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingId, interchange.EI_SessionGUID);
			AssertEquals("INETCECPT", interchange.EI_From);
			AssertEquals("YUSAIRXPN", interchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		public void TestCanadianCustomsReplyHandler_CAD()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write(@"<CanadianCustomsReply xmlns=""http://cargowise.com/ehub/products/canadiancustoms""><Reference>3333333333333100001001</Reference><Content>PERvY3VtZW50TWV0YURhdGEgeG1sbnM9InVybjp3Y286ZGF0YW1vZGVsOldDTzpEZWNsYXJhdGlvbjoxIj48Q29tbXVuaWNhdGlvbk1ldGFEYXRhPjxBcHBsaWNhdGlvblJlZmVyZW5jZUlEPjMzMzMzMzMzMzMzMzMxMDAwMDEwMDE8L0FwcGxpY2F0aW9uUmVmZXJlbmNlSUQ+PFJlY2lwaWVudD48SUQ+MTIzNDU2MTcwUk0wMDAxPC9JRD48L1JlY2lwaWVudD48L0NvbW11bmljYXRpb25NZXRhRGF0YT48UmVzcG9uc2U+PElzc3VlRGF0ZVRpbWU+PERhdGVUaW1lU3RyaW5nPjIwMjExMDA0MTAwODEyPC9EYXRlVGltZVN0cmluZz48L0lzc3VlRGF0ZVRpbWU+PFN0YXR1cz48TmFtZUNvZGU+MjAwPC9OYW1lQ29kZT48L1N0YXR1cz48L1Jlc3BvbnNlPjwvRG9jdW1lbnRNZXRhRGF0YT4=</Content></CanadianCustomsReply>");
			writer.Flush();

			var trackingId = Guid.NewGuid();

			var mockRepository = new MockRepository(MockBehavior.Default);
			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.XMS);
			message.Setup(m => m.RecipientID).Returns("HYETSTTST");
			message.Setup(m => m.SenderID).Returns("CACustoms");
			message.Setup(m => m.TrackingID).Returns(trackingId);
			message.Setup(m => m.Filename).Returns("FileName");
			message.Setup(m => m.EmailSubject).Returns("CAD");

			AssertEquals("PRECONDITION: 0 interchange in database", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new CanadianCustomsReplyHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.CACustoms, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.CanadianCustoms, interchange.EI_InterchangeType);
			AssertEquals("<DocumentMetaData xmlns=\"urn:wco:datamodel:WCO:Declaration:1\"><CommunicationMetaData><ApplicationReferenceID>3333333333333100001001</ApplicationReferenceID><Recipient><ID>123456170RM0001</ID></Recipient></CommunicationMetaData><Response><IssueDateTime><DateTimeString>20211004100812</DateTimeString></IssueDateTime><Status><NameCode>200</NameCode></Status></Response></DocumentMetaData>", interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingId, interchange.EI_SessionGUID);
			AssertEquals("CACustoms", interchange.EI_From);
			AssertEquals("HYETSTTST", interchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
