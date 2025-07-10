using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
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
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class GenericMessageDeliveryMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var headerContent = @"<SenderID>XXXYYYZZZ</SenderID><RecipientID>111222333</RecipientID><InterchangeType>XXX</InterchangeType><InterchangeNumber>123456</InterchangeNumber>";
			var bodyContent = @"<ns0:SomeRootElement xmlns:ns0=""http://SomeSchema/Test""><Description>This is a dummy message that is added to the body of the generic message interchange</Description></ns0:SomeRootElement>";
			var xmlContent = $@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><Header>{headerContent}</Header><Body>{bodyContent}</Body></ns0:GenericMessageInterchange>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("XXXYYYZZZ");
			message.Setup(m => m.RecipientID).Returns("111222333");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new GenericMessageDeliveryMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.GenericMessageDelivery, interchange.EI_ApplicationCode);
			AssertEquals("XXX", interchange.EI_InterchangeType);
			AssertEquals(bodyContent, interchange.EI_BodyText);
			AssertEquals(headerContent, interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("XXXYYYZZZ", interchange.EI_From);
			AssertEquals("111222333", interchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name'", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(0, interchange.ContainedMessages.Count);
			AssertEquals("Unknown Interchange Type 'XXX'; receive GMD Interchange must be in the supported list 'Enterprise.Messaging.Integration.GenericMessageDeliveryInterchangeTypeList'", ErrorReporter.LastMessageReported);
			TestHelpers.CleanErrorReporter();
		}

		public void TestCreateInterchangeForNonXmlBody()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var headerContent = @"<SenderID>XXXYYYZZZ</SenderID><RecipientID>111222333</RecipientID><InterchangeType>XXX</InterchangeType><InterchangeNumber>123456</InterchangeNumber>";
			var bodyContent = @"This text is an unformatted string not in XML format";
			var xmlContent = $@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><Header>{headerContent}</Header><Body>{bodyContent}</Body></ns0:GenericMessageInterchange>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("XXXYYYZZZ");
			message.Setup(m => m.RecipientID).Returns("111222333");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new GenericMessageDeliveryMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.GenericMessageDelivery, interchange.EI_ApplicationCode);
			AssertEquals("XXX", interchange.EI_InterchangeType);
			AssertEquals(bodyContent, interchange.EI_BodyText);
			AssertEquals(headerContent, interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("XXXYYYZZZ", interchange.EI_From);
			AssertEquals("111222333", interchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(0, interchange.ContainedMessages.Count);
			AssertEquals("Unknown Interchange Type 'XXX'; receive GMD Interchange must be in the supported list 'Enterprise.Messaging.Integration.GenericMessageDeliveryInterchangeTypeList'", ErrorReporter.LastMessageReported);
			TestHelpers.CleanErrorReporter();
		}

		public void TestCreateInterchangeWithProvidedInterchangeNumber()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var headerContent = @"<SenderID>XXXYYYZZZ</SenderID><RecipientID>111222333</RecipientID><InterchangeType>XXX</InterchangeType><InterchangeNumber>123456</InterchangeNumber>";
			var bodyContent = @"<ns0:SomeRootElement xmlns:ns0=""http://SomeSchema/Test""><Description>This is a dummy message that is added to the body of the generic message interchange</Description></ns0:SomeRootElement>";
			var xmlContent = $@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><Header>{headerContent}</Header><Body>{bodyContent}</Body></ns0:GenericMessageInterchange>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("XXXYYYZZZ");
			message.Setup(m => m.RecipientID).Returns("111222333");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new GenericMessageDeliveryMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals("123456", interchange.EI_InterchangeNum);
			AssertEquals("Unknown Interchange Type 'XXX'; receive GMD Interchange must be in the supported list 'Enterprise.Messaging.Integration.GenericMessageDeliveryInterchangeTypeList'", ErrorReporter.LastMessageReported);
			TestHelpers.CleanErrorReporter();
		}

		public void TestCreateInterchangeWithoutInterchangeNumber()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var headerContent = @"<SenderID>XXXYYYZZZ</SenderID><RecipientID>111222333</RecipientID><InterchangeType>XXX</InterchangeType><InterchangeNumber></InterchangeNumber>";
			var bodyContent = @"<ns0:SomeRootElement xmlns:ns0=""http://SomeSchema/Test""><Description>This is a dummy message that is added to the body of the generic message interchange</Description></ns0:SomeRootElement>";
			var xmlContent = $@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><Header>{headerContent}</Header><Body>{bodyContent}</Body></ns0:GenericMessageInterchange>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("XXXYYYZZZ");
			message.Setup(m => m.RecipientID).Returns("111222333");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new GenericMessageDeliveryMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.Load<EDIInterchange>(new ZQuery()).OrderByDescending(x => x.EI_InterchangeDateTime).First();
			AssertNotNullOrEmpty(interchange.EI_InterchangeNum);
			AssertEquals("Unknown Interchange Type 'XXX'; receive GMD Interchange must be in the supported list 'Enterprise.Messaging.Integration.GenericMessageDeliveryInterchangeTypeList'", ErrorReporter.LastMessageReported);
			TestHelpers.CleanErrorReporter();
		}

		public void TestCreateInterchangeWithExistingInterchangeNumber()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var headerContent = @"<SenderID>XXXYYYZZZ</SenderID><RecipientID>111222333</RecipientID><InterchangeType>XXX</InterchangeType><InterchangeNumber>123456</InterchangeNumber>";
			var bodyContent = @"<ns0:SomeRootElement xmlns:ns0=""http://SomeSchema/Test""><Description>This is a dummy message that is added to the body of the generic message interchange</Description></ns0:SomeRootElement>";
			var xmlContent = $@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><Header>{headerContent}</Header><Body>{bodyContent}</Body></ns0:GenericMessageInterchange>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("XXXYYYZZZ");
			message.Setup(m => m.RecipientID).Returns("111222333");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new GenericMessageDeliveryMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());

			trackingID = Guid.NewGuid();
			message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("XXXYYYZZZ");
			message.Setup(m => m.RecipientID).Returns("111222333");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			handler = new GenericMessageDeliveryMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());

			AssertEquals("Both the successful and failed interchanges should be saved, but with different statuses.", 2, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchanges = Factory.Load<EDIInterchange>(new ZQuery()).OrderByDescending(x => x.EI_Status);
			var successfulInterchange = interchanges.FirstOrDefault();
			var failedInterchange = interchanges.LastOrDefault();
			AssertEquals("123456", successfulInterchange.EI_InterchangeNum);
			AssertNotEquals("123456", failedInterchange.EI_InterchangeNum);
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, failedInterchange.EI_Status);
			var expectedNote = failedInterchange.Notes.FindByDescription("eHub: Duplication Detected");
			AssertEquals("Interchange should have a note of type 'eHub: Duplication Detected'", 1, expectedNote.Length);
			AssertEquals("The duplicate message should be stored in note", "The value of Interchange Number + From + To must be unique on EDIInterchange. The duplicate value(s) are: (123456, XXXYYYZZZ, 111222333).", expectedNote[0].ST_NoteDataAsText);
			AssertEquals("Unknown Interchange Type 'XXX'; receive GMD Interchange must be in the supported list 'Enterprise.Messaging.Integration.GenericMessageDeliveryInterchangeTypeList'", ErrorReporter.LastMessageReported);
			TestHelpers.CleanErrorReporter();
		}

		public void TestExtractBodyAndHeaderNodesWhenXmlNamespacePrefixWasUsed()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var headerContent = @"<ns0:SenderID>XXXYYYZZZ</ns0:SenderID><ns0:RecipientID>111222333</ns0:RecipientID><ns0:InterchangeType>ZZA</ns0:InterchangeType><ns0:InterchangeNumber>123456</ns0:InterchangeNumber>";
			var bodyContent = @"<SomeRootElement><Description>This is a dummy message that is added to the body of the generic message interchange</Description></SomeRootElement>";
			var xmlContent = $@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><ns0:Header>{headerContent}</ns0:Header><ns0:Body>{bodyContent}</ns0:Body></ns0:GenericMessageInterchange>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("XXXYYYZZZ");
			message.Setup(m => m.RecipientID).Returns("111222333");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new GenericMessageDeliveryMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.GenericMessageDelivery, interchange.EI_ApplicationCode);
			AssertEquals("ZZA", interchange.EI_InterchangeType);
			AssertEquals(bodyContent, interchange.EI_BodyText);
			var nsGMD = "http://cargowise.com/ehub/core/genericmessagedelivery";
			var expectedHeaderContent = $"<ns0:SenderID xmlns:ns0=\"{nsGMD}\">XXXYYYZZZ</ns0:SenderID><ns0:RecipientID xmlns:ns0=\"{nsGMD}\">111222333</ns0:RecipientID><ns0:InterchangeType xmlns:ns0=\"{nsGMD}\">ZZA</ns0:InterchangeType><ns0:InterchangeNumber xmlns:ns0=\"{nsGMD}\">123456</ns0:InterchangeNumber>";
			AssertEquals(expectedHeaderContent, interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("XXXYYYZZZ", interchange.EI_From);
			AssertEquals("111222333", interchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name'", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		[ExpectNoExceptions()]
		public void TestNoExceptionThrowWhenNodeIsNotFound()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var headerContent = @"<ns0:SenderID>XXXYYYZZZ</ns0:SenderID><ns0:RecipientID>111222333</ns0:RecipientID><ns0:InterchangeType>ZZA</ns0:InterchangeType><ns0:InterchangeNumber>123456</ns0:InterchangeNumber>";
			var xmlContentWithoutBodyElement = $@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><ns0:Header>{headerContent}</ns0:Header></ns0:GenericMessageInterchange>";
			writer.Write(xmlContentWithoutBodyElement);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("XXXYYYZZZ");
			message.Setup(m => m.RecipientID).Returns("111222333");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			var handler = new GenericMessageDeliveryMessageHandler();
			handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
		}
	}
}
