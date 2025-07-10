using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EbdDocumentImportResponseMessageProcessorTest : TestCaseWithFactory
{
	public static ZString AcceptanceResponse => @"<ns2:ebdDocumentImportResponse schemaVersion=""0.2"" xsi:schemaLocation=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportResponse/v1 https://www.ezv.admin.ch/dam/ezv/de/dokumente/e-dec/E-Begleitdokument/Webservice/eBD_DocumentImportResponse_xsd.xsd.download.xsd/ebdDocumentImportResponse_v_0_2.xsd"" xmlns:ns2=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportResponse/v1"" xmlns:ns3=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportRequest/v1"" xmlns:xmime=""http://www.w3.org/2005/05/xmlmime"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
         <ns2:customsDeclarationNumber>22CHEI000043145845</ns2:customsDeclarationNumber>
         <ns2:dateAndTime>2022-07-18T16:38:20.309+02:00</ns2:dateAndTime>
         <ns2:accompanyingDocuments>
            <ns2:accompanyingDocument>
               <ns2:filename>TEST.pdf</ns2:filename>
               <ns2:status>200</ns2:status>
            </ns2:accompanyingDocument>
         </ns2:accompanyingDocuments>
      </ns2:ebdDocumentImportResponse>";

	public static ZString RejectionResponse => @"<ns2:ebdDocumentImportResponse schemaVersion=""0.2"" xsi:schemaLocation=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportResponse/v1 https://www.ezv.admin.ch/dam/ezv/de/dokumente/e-dec/E-Begleitdokument/Webservice/eBD_DocumentImportResponse_xsd.xsd.download.xsd/ebdDocumentImportResponse_v_0_2.xsd"" xmlns:ns2=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportResponse/v1"" xmlns:ns3=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportRequest/v1"" xmlns:xmime=""http://www.w3.org/2005/05/xmlmime"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
         <ns2:customsDeclarationNumber>22CHEI000043145845</ns2:customsDeclarationNumber>
         <ns2:dateAndTime>2022-07-18T16:38:20.309+02:00</ns2:dateAndTime>
         <ns2:accompanyingDocuments>
            <ns2:accompanyingDocument>
               <ns2:filename>TEST.xxx</ns2:filename>
               <ns2:status>502</ns2:status>
               <ns2:information>
                  <ns2:message language=""de"">
                     <ns2:text>Nur Dateien im PDF- oder Excel-Format können hochgeladen werden.</ns2:text>
                  </ns2:message>
                  <ns2:message language=""fr"">
                     <ns2:text>Seuls les fichiers au format PDF ou Excel peuvent être téléchargés.</ns2:text>
                  </ns2:message>
                  <ns2:message language=""it"">
                     <ns2:text>È consentito caricare soltanto file in formato PDF o Excel.</ns2:text>
                  </ns2:message>
                  <ns2:message language=""en"">
                     <ns2:text>Only files in PDF or Excel format can be uploaded.</ns2:text>
                  </ns2:message>
               </ns2:information>
            </ns2:accompanyingDocument>
         </ns2:accompanyingDocuments>
      </ns2:ebdDocumentImportResponse>";

	ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EbdDocumentImportResponseMessageProcessor(logger);

	LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	public void TestMessageLinked()
	{
		CombineAssertions(() =>
		{
			var factory = new BusinessObjectFactory();

			(CusEntryHeader entryHeader1, EDIMessage ediMessage1) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EBD, string.Empty, MessageSubTypeCodeList.Codes.Accepted, AcceptanceResponse);
			(CusEntryHeader entryHeader2, EDIMessage ediMessage2) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EBD, string.Empty, MessageSubTypeCodeList.Codes.CustomsRejected, RejectionResponse);

			var processor = GetMessageProcessor(Logger);
			MessageProcessorTestHelper.ProcessMessage(processor, ediMessage1);
			MessageProcessorTestHelper.ProcessMessage(processor, ediMessage2);

			AssertEquals("Status 1", EDIMessage.Status.ProcessedOK, ediMessage1.EM_Status);
			AssertEquals("Linked Table 1", CusEntryHeader.Schema.TableName, ediMessage1.EM_LinkTable);
			AssertEquals("Linked Entry Header 1", entryHeader1.PK, ediMessage1.EM_LinkUniqueID);

			AssertEquals("Status 2", EDIMessage.Status.ProcessedOK, ediMessage2.EM_Status);
			AssertEquals("Linked Table 2", CusEntryHeader.Schema.TableName, ediMessage2.EM_LinkTable);
			AssertEquals("Linked Entry Header 2", entryHeader2.PK, ediMessage2.EM_LinkUniqueID);
		});
	}

	public void TestEvent()
	{
		CombineAssertions(() =>
		{
			var factory = new BusinessObjectFactory();

			(CusEntryHeader entryHeader1, EDIMessage ediMessage1) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EBD, string.Empty, MessageSubTypeCodeList.Codes.Accepted, AcceptanceResponse);
			(CusEntryHeader entryHeader2, EDIMessage ediMessage2) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EBD, string.Empty, MessageSubTypeCodeList.Codes.CustomsRejected, RejectionResponse);

			var processor = GetMessageProcessor(Logger);
			MessageProcessorTestHelper.ProcessMessage(processor, ediMessage1);
			MessageProcessorTestHelper.ProcessMessage(processor, ediMessage2);

			var logEvent1 = entryHeader1.Logs.MostRecentLogByEventTime(Events.DocumentDelivered);
			var logEvent2 = entryHeader2.Logs.MostRecentLogByEventTime(Events.DocumentNotDelivered);

			AssertNotNull("Event 1 written", logEvent1);
			AssertEquals("Event 1 Reference", $"|FIL=TEST.pdf", logEvent1?.SL_Reference);
			AssertNotNull("Event 2 written", logEvent2);
			AssertEquals("Event 2 Reference", $"|FIL=TEST.xxx", logEvent2?.SL_Reference);
		});
	}
}
