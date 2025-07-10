using System.Linq;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class Import5BBHeaderCreatorTest : XMLMessageTestHelper<Import5BBHeaderCreatorTest>
	{
		public void TestDeclarationCustomsOffice()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, latest5BAHeader, new AmendedItemCollection(System.Array.Empty<AmendedItem>(), DataItemIDList));
			AssertEquals("010", import5BBHeader.DeclarationCustomsOffice);
		}

		public void TestDeclarationCustomsDivision()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, latest5BAHeader, new AmendedItemCollection(System.Array.Empty<AmendedItem>(), DataItemIDList));
			AssertEquals("21", import5BBHeader.DeclarationCustomsDivision);
		}

		public void TestImportDeclarationNumber()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, latest5BAHeader, new AmendedItemCollection(System.Array.Empty<AmendedItem>(), DataItemIDList));
			AssertEquals("4062001070010U", import5BBHeader.ImportDeclarationNumber);
		}

		public void TestDeclarantCompanyName()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, latest5BAHeader, new AmendedItemCollection(System.Array.Empty<AmendedItem>(), DataItemIDList));
			AssertEquals("(주)유한상사", import5BBHeader.Declarant.CompanyName);
		}

		public void TestTariffRateClassification()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var entryInstruction = entry.Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().SingleOrDefault();
			entryInstruction.CEI_AgreedDutyRatePreferenceCode = "FAU1";
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));

			AssertEquals(2, import5BBHeader.EntryLines.Length);
			AssertEquals(1, import5BBHeader.EntryLines[0].EntryLineNo);
			AssertEquals("05", import5BBHeader.EntryLines[0].AmendDataItemID);
			AssertEquals("C1", import5BBHeader.EntryLines[0].BeforeDescription);
			AssertEquals("FAU1", import5BBHeader.EntryLines[0].AfterDescription);

			AssertEquals(2, import5BBHeader.EntryLines[1].EntryLineNo);
			AssertEquals("05", import5BBHeader.EntryLines[1].AmendDataItemID);
			AssertEquals("C1", import5BBHeader.EntryLines[1].BeforeDescription);
			AssertEquals("FAU1", import5BBHeader.EntryLines[1].AfterDescription);
		}

		public void TestTariffRate()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var entryInstruction = entry.Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().SingleOrDefault();
			entryInstruction.CEI_AgreedDutyRate = 4.52m;
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));

			AssertEquals(2, import5BBHeader.EntryLines.Length);
			AssertEquals(1, import5BBHeader.EntryLines[0].EntryLineNo);
			AssertEquals("04", import5BBHeader.EntryLines[0].AmendDataItemID);
			AssertEquals("8.25", import5BBHeader.EntryLines[0].BeforeDescription);
			AssertEquals("4.52", import5BBHeader.EntryLines[0].AfterDescription);

			AssertEquals(2, import5BBHeader.EntryLines[1].EntryLineNo);
			AssertEquals("04", import5BBHeader.EntryLines[1].AmendDataItemID);
			AssertEquals("8.25", import5BBHeader.EntryLines[1].BeforeDescription);
			AssertEquals("4.52", import5BBHeader.EntryLines[1].AfterDescription);
		}

		public void TestTariffRateAndClassification()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var entryInstruction = entry.Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().SingleOrDefault();
			entryInstruction.CEI_AgreedDutyRate = 4.52m;
			entryInstruction.CEI_AgreedDutyRatePreferenceCode = "FAU1";
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));

			AssertEquals(4, import5BBHeader.EntryLines.Length);
			AssertEquals(1, import5BBHeader.EntryLines[0].EntryLineNo);
			AssertEquals("04", import5BBHeader.EntryLines[0].AmendDataItemID);
			AssertEquals("8.25", import5BBHeader.EntryLines[0].BeforeDescription);
			AssertEquals("4.52", import5BBHeader.EntryLines[0].AfterDescription);

			AssertEquals(1, import5BBHeader.EntryLines[1].EntryLineNo);
			AssertEquals("05", import5BBHeader.EntryLines[1].AmendDataItemID);
			AssertEquals("C1", import5BBHeader.EntryLines[1].BeforeDescription);
			AssertEquals("FAU1", import5BBHeader.EntryLines[1].AfterDescription);

			AssertEquals(2, import5BBHeader.EntryLines[2].EntryLineNo);
			AssertEquals("04", import5BBHeader.EntryLines[2].AmendDataItemID);
			AssertEquals("8.25", import5BBHeader.EntryLines[2].BeforeDescription);
			AssertEquals("4.52", import5BBHeader.EntryLines[2].AfterDescription);

			AssertEquals(2, import5BBHeader.EntryLines[3].EntryLineNo);
			AssertEquals("05", import5BBHeader.EntryLines[3].AmendDataItemID);
			AssertEquals("C1", import5BBHeader.EntryLines[3].BeforeDescription);
			AssertEquals("FAU1", import5BBHeader.EntryLines[3].AfterDescription);
		}

		public void TestEntryLineIsAddedInAmendment()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 3;
			entryLine1.CL_AdValoremTariff = "8523292991";
			var invoiceLine1 = entry.Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Those recorded video";
			invoiceLine1.JI_Tariff = "8523292991";
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 4;
			entryLine2.CL_AdValoremTariff = "0712200000";
			var invoiceLine2 = entry.Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Onion";
			invoiceLine2.JI_Tariff = "0712200000";
			invoiceLine2.JI_CL = entryLine2.PK;
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));

			AssertEquals(2, import5BBHeader.EntryLines.Length);
			AssertEquals(3, import5BBHeader.EntryLines[0].EntryLineNo);
			AssertEquals("8523292991", import5BBHeader.EntryLines[0].HSCode);
			AssertEquals("비디오 녹화된 것", import5BBHeader.EntryLines[0].HSDescription);
			AssertEquals("Those recorded video", import5BBHeader.EntryLines[0].InvoiceDescription);

			AssertEquals(4, import5BBHeader.EntryLines[1].EntryLineNo);
			AssertEquals("0712200000", import5BBHeader.EntryLines[1].HSCode);
			AssertEquals("양파", import5BBHeader.EntryLines[1].HSDescription);
			AssertEquals("Onion", import5BBHeader.EntryLines[1].InvoiceDescription);
		}

		public void TestEntryLineIsDeletedInAmendment()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			entry.MergedLines[0].Delete();
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));

			AssertEquals(1, import5BBHeader.EntryLines.Length);
			AssertEquals(1, import5BBHeader.EntryLines[0].EntryLineNo);
		}

		public void TestWhenOriginalHadThreeLinesAndTwoLinesAreDeletedInTwo5BB()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 3;
			entryLine1.CL_AdValoremTariff = "8523292991";
			var invoiceLine1 = entry.Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Those recorded video";
			invoiceLine1.JI_Tariff = "8523292991";
			invoiceLine1.JI_CL = entryLine1.PK;
			var import5BA = new Import5BAHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(import5BA))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5BA, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5BA);
			entry.Factory.Save();

			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1).Delete();
			import5BA = new Import5BAHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(import5BA))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5BA, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5BA);
			Factory.Save();

			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 2).Delete();
			Import5BAHeader latest5BAHeaerLodged = null;
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5BA, EntrySnapshotStatus.Lodged);
			if (snapshot != null)
			{
				using (var textReader = snapshot.GetCES_SnapshotXmlReader())
				{
					latest5BAHeaerLodged = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Import5BAHeader>(textReader);
				}
			}
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeaerLodged, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));

			AssertEquals(1, import5BBHeader.EntryLines.Length);
			AssertEquals("EntryLine 2 is deleted in this amendment", 2, import5BBHeader.EntryLines[0].EntryLineNo);
		}

		public void TestEntryLineIsChangedInAmendment()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var entryLine1 = entry.MergedLines[0];
			entryLine1.CL_AdValoremTariff = "8523292991";
			entryLine1.InvoiceLines[0].JI_Tariff = "8523292991";
			var entryLine2 = entry.MergedLines[1];
			entryLine2.InvoiceLines[0].JI_Description = "Those recorded video";
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));

			AssertEquals(3, import5BBHeader.EntryLines.Length);
			AssertEquals(1, import5BBHeader.EntryLines[0].EntryLineNo);
			AssertEquals("01", import5BBHeader.EntryLines[0].AmendDataItemID);
			AssertEquals("양송이 버섯", import5BBHeader.EntryLines[0].BeforeDescription);
			AssertEquals("비디오 녹화된 것", import5BBHeader.EntryLines[0].AfterDescription);

			AssertEquals(1, import5BBHeader.EntryLines[1].EntryLineNo);
			AssertEquals("03", import5BBHeader.EntryLines[1].AmendDataItemID);
			AssertEquals("0712311000", import5BBHeader.EntryLines[1].BeforeDescription);
			AssertEquals("8523292991", import5BBHeader.EntryLines[1].AfterDescription);

			AssertEquals(2, import5BBHeader.EntryLines[2].EntryLineNo);
			AssertEquals("02", import5BBHeader.EntryLines[2].AmendDataItemID);
			AssertEquals("Red ginseng tea", import5BBHeader.EntryLines[2].BeforeDescription);
			AssertEquals("Those recorded video", import5BBHeader.EntryLines[2].AfterDescription);
		}

		[TestDate(2021, 8, 3)]
		public void Test5BBHeaderCreatorEndToEnd_Delete()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			entry.MergedLines[0].Delete();
			var amendmentMock = new Mock<IAmendmentDetails>();
			amendmentMock.Setup(m => m.AmendmentType).Returns("D");
			amendmentMock.Setup(m => m.AmendmentVersionNo).Returns(1);
			amendmentMock.Setup(m => m.AmendReasonDescription).Returns("란 삭제로 인한 정정");
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));
			var result = new GOVCBR5BBMessageBuilder(import5BBHeader, amendmentMock.Object).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5BBHeaderCreatorTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5BB_Delete.xml");
			using (var actualStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(actualStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
				AssertXMLEquals(testFile, serialisedXml);
			}
			amendmentMock.VerifyAll();
		}

		[TestDate(2021, 8, 3)]
		public void Test5BBHeaderCreatorEndToEnd_Add()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 3;
			entryLine1.CL_AdValoremTariff = "8523292991";
			var invoiceLine1 = entry.Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Those recorded video";
			invoiceLine1.JI_Tariff = "8523292991";
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 4;
			entryLine2.CL_AdValoremTariff = "0712200000";
			var invoiceLine2 = entry.Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Onion";
			invoiceLine2.JI_Tariff = "0712200000";
			invoiceLine2.JI_CL = entryLine2.PK;

			var amendmentMock = new Mock<IAmendmentDetails>();
			amendmentMock.Setup(m => m.AmendmentType).Returns("A");
			amendmentMock.Setup(m => m.AmendmentVersionNo).Returns(2);
			amendmentMock.Setup(m => m.AmendReasonDescription).Returns("란 추가로 인한 정정");
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));
			var result = new GOVCBR5BBMessageBuilder(import5BBHeader, amendmentMock.Object).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5BBHeaderCreatorTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5BB_Add.xml");
			using (var actualStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(actualStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
				AssertXMLEquals(testFile, serialisedXml);
			}
			amendmentMock.VerifyAll();
		}

		[TestDate(2021, 8, 3)]
		public void Test5BBHeaderCreatorEndToEnd_Update()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var latest5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var entryLine1 = entry.MergedLines[0];
			entryLine1.CL_AdValoremTariff = "8523292991";
			entryLine1.InvoiceLines[0].JI_Tariff = "8523292991";
			var entryLine2 = entry.MergedLines[1];
			entryLine2.InvoiceLines[0].JI_Description = "Those recorded video";
			var entryInstruction = entry.Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().SingleOrDefault();
			entryInstruction.CEI_AgreedDutyRate = 4.52m;
			entryInstruction.CEI_AgreedDutyRatePreferenceCode = "FAU1";

			var amendmentMock = new Mock<IAmendmentDetails>();
			amendmentMock.Setup(m => m.AmendmentType).Returns("U");
			amendmentMock.Setup(m => m.AmendmentVersionNo).Returns(3);
			amendmentMock.Setup(m => m.AmendReasonDescription).Returns("기재오류로 인한 정정");
			var current5BAHeader = new Import5BAHeaderCreator().Create(entry);
			var amendedItems = new DataProviderComparer<IImport5BAHeader>().Compare(ElectronicDocumentTypeList.Codes._5BA, latest5BAHeader, current5BAHeader, Enumerable.Empty<string>());
			var import5BBHeader = new Import5BBHeaderCreator().Create(entry, current5BAHeader, new AmendedItemCollection(amendedItems, DataItemIDList));
			var result = new GOVCBR5BBMessageBuilder(import5BBHeader, amendmentMock.Object).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5BBHeaderCreatorTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5BB_Update.xml");
			using (var actualStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(actualStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
				AssertXMLEquals(testFile, serialisedXml);
			}
			amendmentMock.VerifyAll();
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		CodeDescriptionPairList DataItemIDList => Factory.GetCachedValue<GOVCBR5BADataItemIDList>();
	}
}
