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
	class ConfigurationMessageHandlerTests : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write("<Configuration>SomeConfiguration</Configuration>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.eHub);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("eHub");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new ConfigurationMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.eHub, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.Configuration, interchange.EI_InterchangeType);
			AssertEquals("<Configuration>SomeConfiguration</Configuration>", interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("eHub", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);

			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		public void TestCreateInterchangeForOFXConfig()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write("<Configuration>SomeConfiguration</Configuration>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.eHub);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("XHUB_OFX_CONFIG");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new ConfigurationMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.OFX, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.Configuration, interchange.EI_InterchangeType);
			AssertEquals("<Configuration>SomeConfiguration</Configuration>", interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("XHUB_OFX_CONFIG", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);

			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
