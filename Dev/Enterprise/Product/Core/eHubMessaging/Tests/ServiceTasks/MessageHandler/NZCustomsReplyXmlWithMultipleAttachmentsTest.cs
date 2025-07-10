using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class NZCustomsReplyXmlWithMultipleAttachmentsTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			using (var messageStream = ResourceManager.GetFileResource("TestFiles.NZCustomsReplyXmlWithAttachments.xml"))
			{
				messageStream.Position = 0;
				var trackingId = Guid.NewGuid();
				var message = new Mock<IeHubMessage>();
				message.Setup(m => m.MessageStream).Returns(messageStream);
				message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.NewZealandCustoms);
				message.Setup(m => m.RecipientID).Returns(GlbCompany.CurrentCompany.LicenceKeyIdentifier);
				message.Setup(m => m.SenderID).Returns("NZCustoms");
				message.Setup(m => m.TrackingID).Returns(trackingId);
				message.Setup(m => m.Filename).Returns("FileName");

				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				var handler = new NZCustomsReplyHandler();
				using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					Db.Connection.RunTransactioned(() => handler.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer()));
				}
				AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertEquals(EDIInterchange.ApplicationCodes.NewZealandCustoms, interchange.EI_ApplicationCode);
				AssertEquals(EDIInterchangeTypeList.Codes.NZCustoms, interchange.EI_InterchangeType);
				AssertEquals("<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>\n  <WCODataModelVersion>3.2</WCODataModelVersion>\n  <WCODocumentName>RES</WCODocumentName>\n  <CountryCode>NZ</CountryCode>\n  <AgencyAssignedCustomizedDocumentName>RESOCR</AgencyAssignedCustomizedDocumentName>\n  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>\n  <Response>\n    <IssueDateTime formatCode=\"204\" >20130314140641</IssueDateTime>\n    <FunctionalReferenceID>6</FunctionalReferenceID>\n    <FunctionCode>24</FunctionCode>\n    <OverallDeclaration>\n      <Declaration>\n        <ID>94950455</ID>\n        <AcceptanceDateTime formatCode=\"204\" >20130314140641</AcceptanceDateTime>\n        <FunctionalReferenceID>C00001032</FunctionalReferenceID>\n        <VersionID/>\n        <Submitter>\n          <ID>00009908C</ID>\n        </Submitter>\n        <ResponsibleGovernmentAgency>\n          <ID>NZCS</ID>\n        </ResponsibleGovernmentAgency>\n      </Declaration>\n    </OverallDeclaration>\n    <Status>\n      <EffectiveDateTime formatCode=\"204\" >20130314140641</EffectiveDateTime>\n      <NameCode>847</NameCode>\n      <ReleaseDateTime formatCode=\"204\" >20130314140641</ReleaseDateTime>\n      <Pointer>\n        <SequenceNumeric>1</SequenceNumeric>\n        <DocumentSectionCode>07B</DocumentSectionCode>\n      </Pointer>\n      <Pointer>\n        <SequenceNumeric>1</SequenceNumeric>\n        <DocumentSectionCode>42A</DocumentSectionCode>\n      </Pointer>\n      <Pointer>\n        <SequenceNumeric>1</SequenceNumeric>\n        <DocumentSectionCode>08B</DocumentSectionCode>\n        <TagID>G007</TagID>\n      </Pointer>\n    </Status>\n  </Response>\n</DocumentMetadata>".Trim(), interchange.EI_BodyText.ToString().Trim());
				AssertEquals("", interchange.EI_FooterText);
				AssertEquals("", interchange.EI_HeaderText);
				AssertEquals(trackingId, interchange.EI_SessionGUID);
				AssertEquals("CUSMOD", interchange.EI_From);
				AssertEquals("00009908C", interchange.EI_To);
				AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
				var expectedNote = interchange.Notes.FindByDescription("File Name");
				AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
				AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
				AssertEquals(0, interchange.ContainedMessages.Count);

				AssertEquals("Interchange should have 2 attachments", 2, interchange.DocManagerInfo.AllEDocs.Count);

				AssertEquals("attachment1.txt", interchange.DocManagerInfo.AllEDocs[0].FileName);
				AssertEquals("txt", interchange.DocManagerInfo.AllEDocs[0].DocType);
				AssertEquals("Attachemnt 1", interchange.DocManagerInfo.AllEDocs[0].GetImageDataReader().ConvertToUTF8StringAndCloseStream());

				AssertEquals("attachment2", interchange.DocManagerInfo.AllEDocs[1].FileNameOnly);
				AssertEquals("jpg", interchange.DocManagerInfo.AllEDocs[1].DocType);
				AssertEquals(2937, interchange.DocManagerInfo.AllEDocs[1].ImageData.Length);
			}
		}

		public void TestCreateInterchange_FailDueInvalidAttachmentFormat()
		{
			string messageText = ResourceManager.GetFileResourceString("TestFiles.NZCustomsReplyXmlWithAttachmentsInvalid.xml");
			using (var messageStream = ResourceManager.GetFileResource("TestFiles.NZCustomsReplyXmlWithAttachmentsInvalid.xml"))
			{
				messageStream.Position = 0;
				var trackingId = Guid.NewGuid();
				var message = new Mock<IeHubMessage>();
				message.Setup(m => m.MessageStream).Returns(messageStream);
				message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.NewZealandCustoms);
				message.Setup(m => m.RecipientID).Returns(GlbCompany.CurrentCompany.LicenceKeyIdentifier);
				message.Setup(m => m.SenderID).Returns("NZCustoms");
				message.Setup(m => m.TrackingID).Returns(trackingId);
				message.Setup(m => m.Filename).Returns("FileName");

				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				var handler = new NZCustomsReplyHandler();
				using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					handler.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current), new NotificationBuffer());
				}
				Factory.Save();
				AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertEquals(EDIInterchange.ApplicationCodes.NewZealandCustoms, interchange.EI_ApplicationCode);
				AssertEquals(EDIInterchangeTypeList.Codes.NZCustoms, interchange.EI_InterchangeType);
				AssertEquals(messageText, interchange.EI_BodyText.ToString());
				AssertEquals("", interchange.EI_FooterText);
				AssertEquals("", interchange.EI_HeaderText);
				AssertEquals(trackingId, interchange.EI_SessionGUID);
				AssertEquals("CUSMOD", interchange.EI_From);
				AssertEquals("00009908C", interchange.EI_To);
				AssertEquals(EDIInterchange.Status.Failed, interchange.EI_Status);
				var expectedNote = interchange.Notes.FindByDescription("File Name");
				AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
				AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
				expectedNote = interchange.Notes.FindByDescription("Parse message error");
				AssertContains("interchange note should include exception message", "The maximum length of this property is 4 characters, but 5 were entered", expectedNote[0].ST_NoteDataAsText);
				AssertEquals(0, interchange.ContainedMessages.Count);
				AssertEquals("Storage docs should be created", 0, interchange.DocManagerInfo.AllEDocs.Count);
				AssertContains("The maximum length of this property is 4 characters, but 5 were entered", ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();
			}
		}
	}
}
