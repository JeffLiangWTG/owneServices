using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5BDDataProvidersTest : XMLMessageTestHelper<GOVCBR5BDDataProvidersTest>
	{
		[TestDate(2021, 04, 06)]
		public void TestSerialisationAndDeserialisation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "6N00221000010M";
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime("2021-04-06");

			var messageSendingObject = new EarlyReleaseMiscMessageSendingObject(entry);
			messageSendingObject.AmendmentReason = "수리전반출신청";
			messageSendingObject.SecurityType = "99";
			messageSendingObject.SecurityStartDate = new ZDateTime("2021-04-06");
			messageSendingObject.SecurityEndDate = new ZDateTime("2021-05-05");
			messageSendingObject.SecurityAmount = 100000m;
			messageSendingObject.OtherSecurityType = "현금";
			messageSendingObject.ReasonForEarlyRemoval = "01";

			var import5BD = new Import5BDCreator().Create(entry, messageSendingObject);
			var result = new GOVCBR5BDMessageBuilder(import5BD).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5BDDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5BD_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
				AssertXMLEquals(testFile, serialisedXml);
			}
			AssertEquals("6N00221000010M", import5BD.ImportDeclarationNumber);
			AssertEquals("수리전반출신청", import5BD.RequestReason);
			AssertEquals("99", import5BD.SecurityType);
			AssertEquals("20210406", import5BD.SecurityStartDate.ToString("yyyyMMdd"));
			AssertEquals("20210505", import5BD.SecurityEndDate.ToString("yyyyMMdd"));
			AssertEquals(100000m, import5BD.SecurityAmount);
			AssertEquals("현금", import5BD.OtherSecurityType);
			AssertEquals("01", import5BD.ReasonForEarlyRemoval);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
