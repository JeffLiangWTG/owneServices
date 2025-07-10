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
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class NZCustomsReplyXmlMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write(@"<ns0:NZCustomsReply xmlns:ns0=""http://cargowise.com/ehub/products/""><ns0:Reference>00009908C</ns0:Reference><ns0:Content>PERvY3VtZW50TWV0YWRhdGEgeG1sbnM9J3Vybjp3Y286ZGF0YW1vZGVsOldDTzpETToxJyB4bWxuczp4c2k9J2h0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlJz4KICA8V0NPRGF0YU1vZGVsVmVyc2lvbj4zLjI8L1dDT0RhdGFNb2RlbFZlcnNpb24+CiAgPFdDT0RvY3VtZW50TmFtZT5SRVM8L1dDT0RvY3VtZW50TmFtZT4KICA8Q291bnRyeUNvZGU+Tlo8L0NvdW50cnlDb2RlPgogIDxBZ2VuY3lBc3NpZ25lZEN1c3RvbWl6ZWREb2N1bWVudE5hbWU+UkVTT0NSPC9BZ2VuY3lBc3NpZ25lZEN1c3RvbWl6ZWREb2N1bWVudE5hbWU+CiAgPEFnZW5jeUFzc2lnbmVkQ3VzdG9taXplZERvY3VtZW50VmVyc2lvbj5WMS4wPC9BZ2VuY3lBc3NpZ25lZEN1c3RvbWl6ZWREb2N1bWVudFZlcnNpb24+CiAgPFJlc3BvbnNlPgogICAgPElzc3VlRGF0ZVRpbWUgZm9ybWF0Q29kZT0iMjA0IiA+MjAxMzAzMTQxNDA2NDE8L0lzc3VlRGF0ZVRpbWU+CiAgICA8RnVuY3Rpb25hbFJlZmVyZW5jZUlEPjY8L0Z1bmN0aW9uYWxSZWZlcmVuY2VJRD4KICAgIDxGdW5jdGlvbkNvZGU+MjQ8L0Z1bmN0aW9uQ29kZT4KICAgIDxPdmVyYWxsRGVjbGFyYXRpb24+CiAgICAgIDxEZWNsYXJhdGlvbj4KICAgICAgICA8SUQ+OTQ5NTA0NTU8L0lEPgogICAgICAgIDxBY2NlcHRhbmNlRGF0ZVRpbWUgZm9ybWF0Q29kZT0iMjA0IiA+MjAxMzAzMTQxNDA2NDE8L0FjY2VwdGFuY2VEYXRlVGltZT4KICAgICAgICA8RnVuY3Rpb25hbFJlZmVyZW5jZUlEPkMwMDAwMTAzMjwvRnVuY3Rpb25hbFJlZmVyZW5jZUlEPgogICAgICAgIDxWZXJzaW9uSUQvPgogICAgICAgIDxTdWJtaXR0ZXI+CiAgICAgICAgICA8SUQ+MDAwMDk5MDhDPC9JRD4KICAgICAgICA8L1N1Ym1pdHRlcj4KICAgICAgICA8UmVzcG9uc2libGVHb3Zlcm5tZW50QWdlbmN5PgogICAgICAgICAgPElEPk5aQ1M8L0lEPgogICAgICAgIDwvUmVzcG9uc2libGVHb3Zlcm5tZW50QWdlbmN5PgogICAgICA8L0RlY2xhcmF0aW9uPgogICAgPC9PdmVyYWxsRGVjbGFyYXRpb24+CiAgICA8U3RhdHVzPgogICAgICA8RWZmZWN0aXZlRGF0ZVRpbWUgZm9ybWF0Q29kZT0iMjA0IiA+MjAxMzAzMTQxNDA2NDE8L0VmZmVjdGl2ZURhdGVUaW1lPgogICAgICA8TmFtZUNvZGU+ODQ3PC9OYW1lQ29kZT4KICAgICAgPFJlbGVhc2VEYXRlVGltZSBmb3JtYXRDb2RlPSIyMDQiID4yMDEzMDMxNDE0MDY0MTwvUmVsZWFzZURhdGVUaW1lPgogICAgICA8UG9pbnRlcj4KICAgICAgICA8U2VxdWVuY2VOdW1lcmljPjE8L1NlcXVlbmNlTnVtZXJpYz4KICAgICAgICA8RG9jdW1lbnRTZWN0aW9uQ29kZT4wN0I8L0RvY3VtZW50U2VjdGlvbkNvZGU+CiAgICAgIDwvUG9pbnRlcj4KICAgICAgPFBvaW50ZXI+CiAgICAgICAgPFNlcXVlbmNlTnVtZXJpYz4xPC9TZXF1ZW5jZU51bWVyaWM+CiAgICAgICAgPERvY3VtZW50U2VjdGlvbkNvZGU+NDJBPC9Eb2N1bWVudFNlY3Rpb25Db2RlPgogICAgICA8L1BvaW50ZXI+CiAgICAgIDxQb2ludGVyPgogICAgICAgIDxTZXF1ZW5jZU51bWVyaWM+MTwvU2VxdWVuY2VOdW1lcmljPgogICAgICAgIDxEb2N1bWVudFNlY3Rpb25Db2RlPjA4QjwvRG9jdW1lbnRTZWN0aW9uQ29kZT4KICAgICAgICA8VGFnSUQ+RzAwNzwvVGFnSUQ+CiAgICAgIDwvUG9pbnRlcj4KICAgIDwvU3RhdHVzPgogIDwvUmVzcG9uc2U+CjwvRG9jdW1lbnRNZXRhZGF0YT4K</ns0:Content></ns0:NZCustomsReply>");
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
				handler.SaveMessageFromAdapter(message.Object);
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.NewZealandCustoms, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.NZCustoms, interchange.EI_InterchangeType);
			AssertEquals("<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>\n  <WCODataModelVersion>3.2</WCODataModelVersion>\n  <WCODocumentName>RES</WCODocumentName>\n  <CountryCode>NZ</CountryCode>\n  <AgencyAssignedCustomizedDocumentName>RESOCR</AgencyAssignedCustomizedDocumentName>\n  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>\n  <Response>\n    <IssueDateTime formatCode=\"204\" >20130314140641</IssueDateTime>\n    <FunctionalReferenceID>6</FunctionalReferenceID>\n    <FunctionCode>24</FunctionCode>\n    <OverallDeclaration>\n      <Declaration>\n        <ID>94950455</ID>\n        <AcceptanceDateTime formatCode=\"204\" >20130314140641</AcceptanceDateTime>\n        <FunctionalReferenceID>C00001032</FunctionalReferenceID>\n        <VersionID/>\n        <Submitter>\n          <ID>00009908C</ID>\n        </Submitter>\n        <ResponsibleGovernmentAgency>\n          <ID>NZCS</ID>\n        </ResponsibleGovernmentAgency>\n      </Declaration>\n    </OverallDeclaration>\n    <Status>\n      <EffectiveDateTime formatCode=\"204\" >20130314140641</EffectiveDateTime>\n      <NameCode>847</NameCode>\n      <ReleaseDateTime formatCode=\"204\" >20130314140641</ReleaseDateTime>\n      <Pointer>\n        <SequenceNumeric>1</SequenceNumeric>\n        <DocumentSectionCode>07B</DocumentSectionCode>\n      </Pointer>\n      <Pointer>\n        <SequenceNumeric>1</SequenceNumeric>\n        <DocumentSectionCode>42A</DocumentSectionCode>\n      </Pointer>\n      <Pointer>\n        <SequenceNumeric>1</SequenceNumeric>\n        <DocumentSectionCode>08B</DocumentSectionCode>\n        <TagID>G007</TagID>\n      </Pointer>\n    </Status>\n  </Response>\n</DocumentMetadata>".Trim(), interchange.EI_BodyText.ToString().Trim());
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
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
