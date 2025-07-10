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
	class NZCustomsReplyEDIFACTMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write(@"<ns0:NZCustomsReply xmlns:ns0=""http://cargowise.com/ehub/products/""><ns0:Reference>B86020429</ns0:Reference><ns0:Content>VU5BOisuPyAnVU5CK1VOT0E6MitFWFQuVFNXLkdPVlQuTlo6WlpaKzAwMDA5OTA4QzpaWlorMTMwMTExOjE0MDErMjAnVU5IKzErQ1VTUkVTOkQ6OTZCOlVOK0I4NjAyMDQyOSdCR00rOTYzKzAwMDAwMDAwJ0dJUys4NTg6MTIwOjE0MydFUlArMDAxOjoyMCdFUkMrMTM4OjoxNDMnRVJQKzAwMTo6MzknRVJDKzEzOTo6MTQzJ1VOVCs4KzEnVU5aKzErMjAn</ns0:Content></ns0:NZCustomsReply>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.NewZealandCustoms);
			message.Setup(m => m.RecipientID).Returns(GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			message.Setup(m => m.SenderID).Returns("NZCustoms");
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
			AssertEquals(@"UNH+1+CUSRES:D:96B:UN+B86020429'BGM+963+00000000'GIS+858:120:143'ERP+001::20'ERC+138::143'ERP+001::39'ERC+139::143'UNT+8+1'", interchange.EI_BodyText);
			AssertEquals("UNZ+1+20'", interchange.EI_FooterText);
			AssertEquals("UNA:+.? 'UNB+UNOA:2+EXT.TSW.GOVT.NZ:ZZZ+00009908C:ZZZ+130111:1401+20'", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("CUSMOD", interchange.EI_From);
			AssertEquals("00009908C", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);

			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
