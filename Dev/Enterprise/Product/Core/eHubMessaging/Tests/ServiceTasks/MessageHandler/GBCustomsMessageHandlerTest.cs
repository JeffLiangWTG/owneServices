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
	class GBCustomsMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write("<Body>SomeMessageBody</Body>");
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			_ = message.Setup(m => m.MessageStream).Returns(messageStream);
			_ = message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			_ = message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			_ = message.Setup(m => m.SenderID).Returns("GBCustoms");
			_ = message.Setup(m => m.TrackingID).Returns(trackingID);
			_ = message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new GBCustomsMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.GBCustoms, interchange.EI_InterchangeType);
			AssertEquals("<Body>SomeMessageBody</Body>", interchange.EI_BodyText);

			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("GBCustoms", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		public void TestCreateInterchangeForGVMS()
		{
			RunCommonCdsProviderTest("GVMS", EDIInterchange.ApplicationCodes.GbCustomsGVMSManifest);
		}

		public void TestCreateInterchangeForCTC()
		{
			RunCommonCdsProviderTest("CTCGB", "GCT");
		}

		public void TestCreateInterchangeForCTC_InPhase5()
		{
			RunCommonCdsProviderTest("CTCGB", ApplicationCodeList.Codes.GbCustomsNCTS, @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""CTCGB"">
    <s0:ConversationID />
    <ServiceReference>0000002147483647</ServiceReference>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""XML"" Encoding=""none"">
    &lt;ncts:CC057C xmlns:ncts=""http://ncts.dgtaxud.ec"" PhaseID=""NCTS5.1""&gt;
        &lt;messageSender&gt;NTA.GB&lt;/messageSender&gt;
        &lt;messageRecipient&gt;GB945390992000&lt;/messageRecipient&gt;
        &lt;preparationDateAndTime&gt;2023-10-27T06:33:20&lt;/preparationDateAndTime&gt;
        &lt;messageIdentification&gt;ZMGY04JBGSVUXQ&lt;/messageIdentification&gt;
        &lt;messageType&gt;CC057C&lt;/messageType&gt;
        &lt;correlationIdentifier&gt;NCT00000959/51&lt;/correlationIdentifier&gt;
        &lt;TransitOperation&gt;
            &lt;MRN&gt;23GB0000601010C2F6&lt;/MRN&gt;
            &lt;businessRejectionType&gt;007&lt;/businessRejectionType&gt;
            &lt;rejectionDateAndTime&gt;2023-10-27T06:33:20&lt;/rejectionDateAndTime&gt;
            &lt;rejectionCode&gt;12&lt;/rejectionCode&gt;
        &lt;/TransitOperation&gt;
        &lt;CustomsOfficeOfDestinationActual&gt;
            &lt;referenceNumber&gt;GB000011&lt;/referenceNumber&gt;
        &lt;/CustomsOfficeOfDestinationActual&gt;
        &lt;TraderAtDestination&gt;
            &lt;identificationNumber&gt;GB954131533000&lt;/identificationNumber&gt;
        &lt;/TraderAtDestination&gt;
        &lt;FunctionalError&gt;
            &lt;errorPointer&gt;/CC007C/Authorisation[1]/referenceNumber&lt;/errorPointer&gt;
            &lt;errorCode&gt;14&lt;/errorCode&gt;
            &lt;errorReason&gt;G0033&lt;/errorReason&gt;
            &lt;originalAttributeValue&gt;HHHHHHHHHHHHHHHH&lt;/originalAttributeValue&gt;
        &lt;/FunctionalError&gt;
    &lt;/ncts:CC057C&gt;
</s0:ResponseBody>
</s0:GBCustomsBusinessResponse>
");
		}

		public void TestCreateInterchangeForCTC_WithServiceReferenceInPhase5()
		{
			RunCommonCdsProviderTest("CTCGB", ApplicationCodeList.Codes.GbCustomsNCTS, @"
				<GBCustomsBusinessResponse>
					<ResponseHeader Provider=""CTCGB"">
						<NotificaitonBoxId>456XYZ</NotificaitonBoxId>
						<ServiceReference>653b59afcc780431</ServiceReference>
					</ResponseHeader>
					<ResponseBody ContentType = ""XML"" Encoding=""none"">
						doesn't really matter because first condition is met: ServiceReference is not an integer
					</ResponseBody>
				</GBCustomsBusinessResponse>");
		}

		public void TestCreateInterchangeForICSNI()
		{
			RunCommonCdsProviderTest("ICSNI", "GIN");
		}

		public void TestCreateInterchangeForICSGB()
		{
			RunCommonCdsProviderTest("ICSGB", "GIG");
		}

		public void TestCreateInterchangeForEMCS()
		{
			var messageContent = @"<GBCustomsBusinessResponse>
<ResponseHeader>
<Provider>EMCS</Provider>
<NotificaitonBoxId>456XYZ</NotificaitonBoxId>
</ResponseHeader>
<ResponseBody ContentType = ""WhoCaresButProbablyJson"" EncoDIng = ""baSE64"">
ewogICJub3RpZmljYXRpb25JZCI6ICIxZWQ1ZjQwNy04MDk2LTQwZDEtODdlZi05YTJhMTAzZWViODUiLAogICJib3hJZCI6ICI1MGRjYTNmYy1jMzdjLTRmMDMtYjcxOS02MzU3MTMzMzYyNGMiLAogICJtZXNzYWdlQ29udGVudFR5cGUiOiAiYXBwbGljYXRpb24vanNvbiIsCiAgIm1lc3NhZ2UiOiAie1wia2V5XCI6XCJ2YWx1ZTxmb28+YmFyXCJ9IiwKICAic3RhdHVzIjogIlJFQ0VJVkVEIiwKICAiY3JlYXRlZERhdGVUaW1lIjogIjIwMjAtMDYtMDFUMTA6MjA6MjMuMTYwKzAwMDAiCn0=
</ResponseBody>
</GBCustomsBusinessResponse>
";
			RunCommonCdsProviderTest("EMCS", "GEX", messageContent);
		}

		void RunCommonCdsProviderTest(string providerCoder, string expectedInterchangeAppCode, string msgContent = null)
		{
			var messageContent = msgContent ?? @"<GBCustomsBusinessResponse>
<ResponseHeader Provider=""" + providerCoder + @""">
<NotificaitonBoxId>456XYZ</NotificaitonBoxId>
</ResponseHeader>
<ResponseBody ContentType = ""WhoCaresButProbablyJson"" EncoDIng = ""baSE64"">
ewogICJub3RpZmljYXRpb25JZCI6ICIxZWQ1ZjQwNy04MDk2LTQwZDEtODdlZi05YTJhMTAzZWViODUiLAogICJib3hJZCI6ICI1MGRjYTNmYy1jMzdjLTRmMDMtYjcxOS02MzU3MTMzMzYyNGMiLAogICJtZXNzYWdlQ29udGVudFR5cGUiOiAiYXBwbGljYXRpb24vanNvbiIsCiAgIm1lc3NhZ2UiOiAie1wia2V5XCI6XCJ2YWx1ZTxmb28+YmFyXCJ9IiwKICAic3RhdHVzIjogIlJFQ0VJVkVEIiwKICAiY3JlYXRlZERhdGVUaW1lIjogIjIwMjAtMDYtMDFUMTA6MjA6MjMuMTYwKzAwMDAiCn0=
</ResponseBody>
</GBCustomsBusinessResponse>
";

			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			writer.Write(messageContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			_ = message.Setup(m => m.MessageStream).Returns(messageStream);
			_ = message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			_ = message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			_ = message.Setup(m => m.SenderID).Returns("GBCustoms");
			_ = message.Setup(m => m.TrackingID).Returns(trackingID);
			_ = message.Setup(m => m.Filename).Returns("FileName");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var handler = new GBCustomsMessageHandler();
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
			}
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(expectedInterchangeAppCode, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.GBCustoms, interchange.EI_InterchangeType);
			AssertEquals(messageContent, interchange.EI_BodyText);
			AssertEquals("", interchange.EI_FooterText);
			AssertEquals("", interchange.EI_HeaderText);
			AssertEquals(trackingID, interchange.EI_SessionGUID);
			AssertEquals("GBCustoms", interchange.EI_From);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			var expectedNote = interchange.Notes.FindByDescription("File Name");
			AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
			AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}
	}
}
