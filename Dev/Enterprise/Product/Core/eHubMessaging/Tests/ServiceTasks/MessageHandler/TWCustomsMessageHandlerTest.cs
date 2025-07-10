using System;
using System.IO;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class TWCustomsMessageHandlerTest : TestCaseWithFactory
	{
		[TestDate(2020, 10, 21, 17, 00, 00)]
		public void TestCreateInterchange_TWCustoms()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write("<TWCustomsResponse xmlns='http://cargowise.com/ehub/products/TWCustoms'>" +
"	<Header>														" +
"		<Sender>sender</Sender>										" +
"		<Recipient>recipient</Recipient>							" +
"	</Header>														" +
"	<Body>															" +
"		<InterchangeNum>responseFile.Name</InterchangeNum>			" +
"		<Response xmlns:tsw='urn:SingleWindow:TW' xmlns:ds='urn:wco:datamodel:WCO:DS:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:twds='urn:wco:datamodel:TW:DS:1' xmlns:ccts='urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2' xmlns='urn:wco:datamodel:TW:NX903:R-00-03' xsi:schemaLocation='urn:wco:datamodel:TW:NX903:R-00-03 NX903.xsd'>												" +
"				<Status><NameCode>N</NameCode></Status>				" +
"				<Declaration><ID>BF 0977700037</ID><AdditionalInformation><tw_IssueDateTime>2020-02-24T11:43:24</tw_IssueDateTime></AdditionalInformation><Agent><ID>777</ID><RoleCode>CB</RoleCode><tw_SubBoxID>A</tw_SubBoxID></Agent><BorderTransportMeans><TypeCode>4</TypeCode></BorderTransportMeans><GoodsShipment><Consignment><TransportContractDocument><ID>297-74222805</ID><TypeCode>741</TypeCode></TransportContractDocument></Consignment><GovernmentAgencyGoodsItem><SequenceNumeric>0</SequenceNumeric><Status><NameCode>C06</NameCode></Status><Status><NameCode>D9L</NameCode></Status></GovernmentAgencyGoodsItem></GoodsShipment><GovernmentProcedure><tw_TransportTypeCode>2</tw_TransportTypeCode></GovernmentProcedure></Declaration>			" +
"		</Response>													" +
"	</Body>															" +
"</TWCustomsResponse>");
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

			var handler = new TWCustomsMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

			AssertEquals(EDIInterchange.ApplicationCodes.TaiwanCustoms, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.TWCustoms, interchange.EI_InterchangeType);
			AssertEquals(XmlInterchangeHandlerTests.GetFileResourceString("TWCustomsRespose.xml"), interchange.EI_BodyText);
			AssertContains("responseFile.Name20201022010000", interchange.EI_InterchangeNum);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("TWCustoms", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);

			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		[TestDate(2020, 10, 21, 17, 00, 00)]
		public void TestCreateInterchange_TWCustomsForwarderManifest()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write("<TWCustomsResponse xmlns='http://cargowise.com/ehub/products/TWCustomsForwarderManifest'>" +
"	<Header>														" +
"		<Sender>sender</Sender>										" +
"		<Recipient>recipient</Recipient>							" +
"	</Header>														" +
"	<Body>															" +
"		<InterchangeNum>responseFile.Name</InterchangeNum>			" +
"		<Response xmlns:tsw='urn:SingleWindow:TW' xmlns:ds='urn:wco:datamodel:WCO:DS:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:twds='urn:wco:datamodel:TW:DS:1' xmlns:ccts='urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2' xmlns='urn:wco:datamodel:TW:NX903:R-00-03' xsi:schemaLocation='urn:wco:datamodel:TW:NX903:R-00-03 NX903.xsd'>												" +
"				<Status><NameCode>N</NameCode></Status>				" +
"				<Declaration><ID>BF 0977700037</ID><AdditionalInformation><tw_IssueDateTime>2020-02-24T11:43:24</tw_IssueDateTime></AdditionalInformation><Agent><ID>777</ID><RoleCode>CB</RoleCode><tw_SubBoxID>A</tw_SubBoxID></Agent><BorderTransportMeans><TypeCode>4</TypeCode></BorderTransportMeans><GoodsShipment><Consignment><TransportContractDocument><ID>297-74222805</ID><TypeCode>741</TypeCode></TransportContractDocument></Consignment><GovernmentAgencyGoodsItem><SequenceNumeric>0</SequenceNumeric><Status><NameCode>C06</NameCode></Status><Status><NameCode>D9L</NameCode></Status></GovernmentAgencyGoodsItem></GoodsShipment><GovernmentProcedure><tw_TransportTypeCode>2</tw_TransportTypeCode></GovernmentProcedure></Declaration>			" +
"		</Response>													" +
"	</Body>															" +
"</TWCustomsResponse>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.TaiwanCustoms);
			message.Setup(m => m.SchemaName).Returns(EDIInterchangeTypeList.Descriptions.TWCustomsForwarderManifest);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("TWCustoms");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new TWCustomsMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

			AssertEquals(EDIInterchange.ApplicationCodes.TaiwanCustoms, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.TWCustomsForwarderManifest, interchange.EI_InterchangeType);
			AssertEquals(XmlInterchangeHandlerTests.GetFileResourceString("TWCustomsRespose.xml"), interchange.EI_BodyText);
			AssertContains("responseFile.Name20201022010000", interchange.EI_InterchangeNum);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("TWCustoms", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);

			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		[TestDate(2020, 10, 21, 17, 00, 00)]
		public void TestCreateInterchange_TWCustomslicensing()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write("<TWCustomsResponse xmlns='http://cargowise.com/ehub/products/TWCustomsLicensing'>" +
"	<Header>														" +
"		<Sender>sender</Sender>										" +
"		<Recipient>recipient</Recipient>							" +
"	</Header>														" +
"	<Body>															" +
"		<InterchangeNum>responseFile.Name</InterchangeNum>			" +
"		<Response xmlns:tsw='urn:SingleWindow:TW' xmlns:ds='urn:wco:datamodel:WCO:DS:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:twds='urn:wco:datamodel:TW:DS:1' xmlns:ccts='urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2' xmlns='urn:wco:datamodel:TW:NX903:R-00-03' xsi:schemaLocation='urn:wco:datamodel:TW:NX903:R-00-03 NX903.xsd'>												" +
"				<Status><NameCode>N</NameCode></Status>				" +
"				<Declaration><ID>BF 0977700037</ID><AdditionalInformation><tw_IssueDateTime>2020-02-24T11:43:24</tw_IssueDateTime></AdditionalInformation><Agent><ID>777</ID><RoleCode>CB</RoleCode><tw_SubBoxID>A</tw_SubBoxID></Agent><BorderTransportMeans><TypeCode>4</TypeCode></BorderTransportMeans><GoodsShipment><Consignment><TransportContractDocument><ID>297-74222805</ID><TypeCode>741</TypeCode></TransportContractDocument></Consignment><GovernmentAgencyGoodsItem><SequenceNumeric>0</SequenceNumeric><Status><NameCode>C06</NameCode></Status><Status><NameCode>D9L</NameCode></Status></GovernmentAgencyGoodsItem></GoodsShipment><GovernmentProcedure><tw_TransportTypeCode>2</tw_TransportTypeCode></GovernmentProcedure></Declaration>			" +
"		</Response>													" +
"	</Body>															" +
"</TWCustomsResponse>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.TaiwanCustoms);
			message.Setup(m => m.SchemaName).Returns(EDIInterchangeTypeList.Descriptions.TWCustomsLicensing);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("TWCustoms");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new TWCustomsMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

			AssertEquals(EDIInterchange.ApplicationCodes.TaiwanCustoms, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.TWCustomsLicensing, interchange.EI_InterchangeType);
			AssertEquals(XmlInterchangeHandlerTests.GetFileResourceString("TWCustomsRespose.xml"), interchange.EI_BodyText);
			AssertContains("responseFile.Name20201022010000", interchange.EI_InterchangeNum);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("TWCustoms", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);

			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
