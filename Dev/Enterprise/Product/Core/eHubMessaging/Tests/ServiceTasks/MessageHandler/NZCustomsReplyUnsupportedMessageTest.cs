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
	class NZCustomsReplyUnsupportedMessageTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write(@"<ns0:NZCustomsReply xmlns:ns0=""http://cargowise.com/ehub/products/""><ns0:Reference>00009908C</ns0:Reference><ns0:Content>JVBERi0xLjQKJeLjz9MKNCAwIG9iago8PC9Db2xvclNwYWNlL0RldmljZVJHQi9MZW5ndGggNjI1NTAvQml0c1BlckNvbXBvbmVudCA4L0hlaWdodCAzMDIvRmlsdGVyL0RDVERl</ns0:Content></ns0:NZCustomsReply>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.NewZealandCustoms);
			message.Setup(m => m.RecipientID).Returns(GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			message.Setup(m => m.SenderID).Returns("NZCustomsTest");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new NZCustomsReplyHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.NewZealandCustoms, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.NZCustoms, interchange.EI_InterchangeType);
			AssertEquals("%PDF-1.4\n%????\n4 0 obj\n<</ColorSpace/DeviceRGB/Length 62550/BitsPerComponent 8/Height 302/Filter/DCTDe", interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("CUSSWT", interchange.EI_From);
			AssertEquals("00009908C", interchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("Parse message error");
			AssertEquals("interchange should have a note of type 'Parse message error", 1, expectedNote.Length);
			AssertEquals("Message format was not recognized.", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
