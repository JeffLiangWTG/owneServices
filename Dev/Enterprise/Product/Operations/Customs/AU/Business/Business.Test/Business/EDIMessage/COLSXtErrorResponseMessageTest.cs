using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(COLSXtErrorResponseMessage))]
	sealed class COLSXtErrorResponseMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFormattedMessageText()
		{
			var message = Factory.New<COLSXtErrorResponseMessage>();
			message.EM_MessageText = XtErrorXML;

			var expectedText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>xT</SenderID>
    <RecipientID>HYEDAUCMT</RecipientID>
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
      <Event>
        <EventTime>2024-04-26 03:36:47.331</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
          <Reason>Error MessageContract</Reason>
          <MessageType>XER</MessageType>
        </EventParameters>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

			AssertEquals("EM_FormattedMessageText - Should use the formatted message text.", expectedText, message.EM_FormattedMessageText);
			AssertEquals("EM_MessageInterpretation - Should use the formatted message text.", expectedText, message.EM_MessageInterpretation);
		}

		public void TestFormattedMessageText_WithEmbeddedFile()
		{
			var message = Factory.New<COLSXtErrorResponseMessage>();
			message.EM_MessageText = XtErrorXMLWithEmbeddedFile;

			var expectedText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>xT</SenderID>
    <RecipientID>HYEDAUCMT</RecipientID>
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
      <Event>
        <EventTime>2024-04-26 03:36:47.331</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
          <Reason>Error MessageContract</Reason>
          <MessageType>XER</MessageType>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>OriginalMessage</Type>
            <Value><![CDATA[PK!]]></Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

			AssertEquals("EM_FormattedMessageText - Should use the formatted message text.", expectedText, message.EM_FormattedMessageText);
			AssertEquals("EM_MessageInterpretation - Should use the formatted message text.", expectedText, message.EM_MessageInterpretation);
		}

		public void TestFormattedMessageText_InvalidUniversalInterchangeWithEmbeddedFileData()
		{
			var message = Factory.New<COLSXtErrorResponseMessage>();
			message.EM_MessageText = XtErrorXMLWithEmbeddedFile.Substring(0, XtErrorXMLWithEmbeddedFile.Length - 5);

			var expectedText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11""><Header><SenderID>xT</SenderID><RecipientID>HYEDAUCMT</RecipientID></Header><Body><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1""><Event><EventTime>2024-04-26 03:36:47.331</EventTime><EventType>IRJ</EventType><EventParameters><Reason>Error MessageContract</Reason><MessageType>XER</MessageType></EventParameters><ContextCollection><Context><Type>OriginalMessage</Type><Value><![CDATA[PK!]]></Value></Context></ContextCollection></Event></UniversalEvent></Body></UniversalInterch";

			AssertEquals("EM_FormattedMessageText - Should use the formatted message text.", expectedText, message.EM_FormattedMessageText);
			AssertEquals("EM_MessageInterpretation - Should use the formatted message text.", expectedText, message.EM_MessageInterpretation);
		}

		const string XtErrorXML = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID>xT</SenderID><RecipientID>HYEDAUCMT</RecipientID></Header><Body><UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\" version=\"1.1\"><Event><EventTime>2024-04-26 03:36:47.331</EventTime><EventType>IRJ</EventType><EventParameters><Reason>Error MessageContract</Reason><MessageType>XER</MessageType></EventParameters></Event></UniversalEvent></Body></UniversalInterchange>";
		const string XtErrorXMLWithEmbeddedFile = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID>xT</SenderID><RecipientID>HYEDAUCMT</RecipientID></Header><Body><UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\" version=\"1.1\"><Event><EventTime>2024-04-26 03:36:47.331</EventTime><EventType>IRJ</EventType><EventParameters><Reason>Error MessageContract</Reason><MessageType>XER</MessageType></EventParameters><ContextCollection><Context><Type>OriginalMessage</Type><Value><![CDATA[PK\u0003\u0004\u0014\u0000\u0006\u0000\b\u0000\u0000\u0000!]]></Value></Context></ContextCollection></Event></UniversalEvent></Body></UniversalInterchange>";
	}
}
