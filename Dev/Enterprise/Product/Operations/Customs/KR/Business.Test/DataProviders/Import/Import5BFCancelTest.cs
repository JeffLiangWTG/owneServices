using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class Import5BFCancelTest : XMLMessageTestHelper<Import5BFCancelTest>
	{
		[TestDate(2020, 02, 24)]
		public void TestRealData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4163720002736M";

			var messageSendingObject = new CancellationMessageSendingObject(entry);
			messageSendingObject.CancellationReason = "위험물(UN NO.1017)로써위험물창고로이고키위함";

			var import5BF = new Import5BFCancelCreator().Create(entry, messageSendingObject);

			var result = new GOVCBR5BFMessageBuilder(import5BF).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5BFCancelTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5BF_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}
		public void Test5BFMessageSendingObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4163720002736M";

			var messageSendingObject = new CancellationMessageSendingObject(entry);
			messageSendingObject.CancellationReason = "Test";

			var import5BF = new Import5BFCancelCreator().Create(entry, messageSendingObject);
			AssertEquals("Test", import5BF.ApplicationReason);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
