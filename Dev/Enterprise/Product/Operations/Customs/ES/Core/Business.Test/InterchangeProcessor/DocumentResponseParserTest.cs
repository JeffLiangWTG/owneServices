using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DocumentResponseParserTest : TestCaseWithFactory
	{
		public void TestParseResponse()
		{
			var content = "JVBERi0x";
			var headerText = "{" +
				"\"custom.OriginalMsgId\":\"2357535\"," +
				"\"custom.ES.DocumentFileDescription\":\"Clearance Document\"," +
				"\"custom.ES.DocumentFilename\":\"23ES009999101500B4_E_AEAT_CLR.pdf\"," +
				"\"custom.ES.DocumentType\":\"CLR\"," +
				"}";

			var logger = new LoggingInformation();
			var parser = new DocumentResponseParser(content, headerText, logger);
			var result = parser.Parse();

			var expectedResult = @$"<AttachedDocument {XMLTestFileConstants.XmlnsLinkAttributes}>
  <FileName>23ES009999101500B4_E_AEAT_CLR.pdf</FileName>
  <Type>
    <Code>CLR</Code>
    <Description>Clearance Document</Description>
  </Type>
  <ImageData>JVBERi0x</ImageData>
  <IsPublished>true</IsPublished>
</AttachedDocument>";

			CombineAssertions(() =>
			{
				AssertEquals("String returned is a correct AttachedDocument xml", expectedResult, result);
				AssertEquals("logger has no data", 0, logger.UserLogStrings.Count);
			});
		}

		public void TestParseResponse_WrongHeaderText()
		{
			var content = "JVBERi0x";
			var headerText = "AAAA";

			var logger = new LoggingInformation();
			var parser = new DocumentResponseParser(content, headerText, logger);
			var result = parser.Parse();

			var expectedResult = $@"<AttachedDocument {XMLTestFileConstants.XmlnsLinkAttributes}>
  <Type />
  <ImageData>JVBERi0x</ImageData>
  <IsPublished>true</IsPublished>
</AttachedDocument>";

			CombineAssertions(() =>
			{
				AssertEquals("String returned is a correct AttachedDocument xml", expectedResult, result);
				AssertContains("logger", "Error processing Header Text, should be a correct json", logger.UserLogStrings[0]);
			});
		}
	}
}
