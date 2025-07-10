using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExportAmendDF3Test : XMLMessageTestHelper<LocalExportAmendDF3Test>
	{
		[TestDate(2021, 04, 06)]
		public void TestFullData()
		{
			var entry = new TestDataSetupHelper(Factory).GetDF3Entry();
			var localExportDF3 = new LocalExportAmendDF3Creator().Create(entry);
			var result = new GOVCBRDF3MessageBuilder(localExportDF3).GenerateMessage();
			var fileReader = new TestFileReader(typeof(LocalExportAmendDF3Test));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBRDF3_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Outgoing";
	}
}
