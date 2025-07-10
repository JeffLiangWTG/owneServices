using System;
using System.IO;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class USCustomsImportMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchangeCore()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write("<InboundMessage><Header>Inbound Message Header</Header><Body>Inbound Message Body</Body><Footer>Inbound Message Footer</Footer></InboundMessage>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.USCustomsImport);
			message.Setup(m => m.RecipientID).Returns(GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			message.Setup(m => m.SenderID).Returns("USC");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new USCustomsImportMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.USCustomsImport, interchange.EI_ApplicationCode);
			AssertEquals(string.Empty, interchange.EI_InterchangeType);
			AssertEquals("Inbound Message Body", interchange.EI_BodyText);
			AssertEquals("Inbound Message Footer", interchange.EI_FooterText);
			AssertEquals("Inbound Message Header", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("USC", interchange.EI_From);
			AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
		}
	}
}
