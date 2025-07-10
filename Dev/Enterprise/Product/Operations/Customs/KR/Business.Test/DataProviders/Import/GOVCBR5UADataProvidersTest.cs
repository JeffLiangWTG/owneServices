using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5UADataProvidersTest : XMLMessageTestHelper<GOVCBR5UADataProvidersTest>
	{
		[TestDate(2021, 04, 01)]
		public void TestSerialisationRealData()
		{
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "레디코리아";
			broker.OH_IsBroker = true;
			var payerAddress = broker.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			payerAddress.OA_CompanyNameOverride = "레디코리아";
			payerAddress.OA_Address1 = "서울시 마로구 상암동 상암빌딩 123";
			payerAddress.OA_RN_NKCountryCode = "KR";

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "김택윤";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = "KRC";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "02";
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.Branch.GB_OH_OrgProxy = broker.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1235620215452";
			entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UA).CE_EntryLineReference = "1";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_DataModel = Core.Constants.CountryCodes.KoreaSouth;
			entry.CH_CEI_Instruction = instruction.PK;
			var sessionalData = instruction.AmendmentSessionalDataCollection.AddNew();

			var sessionDetails = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData, 1);
			sessionDetails.PenaltyExemptionReasonCode = "B5";
			sessionDetails.PenaltyExemptionReason = "기타사유기타사유정정사유정정으로 인한 사유";
			sessionDetails.PenaltyExemptionAmount = 100m;

			var import5UAHeader = new Import5UAHeaderCreator().Create(entry, sessionDetails, new System.DateTime(2021, 04, 01), 1, "A");
			Factory.Save();

			var result = new GOVCBR5UAMessageBuilder(import5UAHeader).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5UADataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5UA_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}

			AssertEquals("1235620215452", import5UAHeader.ImportDeclarationNumber);
			AssertEquals("010", import5UAHeader.DeclarationCustomsOffice);
			AssertEquals("02", import5UAHeader.DeclarationCustomsDivision);
			AssertEquals("20210401", import5UAHeader.AmendmentDeclarationDate.ToString("yyyyMMdd"));
			AssertEquals(1, import5UAHeader.AmendmentVersionNo);
			AssertEquals("B5", import5UAHeader.PenaltyExemptionReasonsCode);
			AssertEquals("기타사유기타사유정정사유정정으로 인한 사유", import5UAHeader.PenaltyExemptionReason);
			AssertEquals("레디코리아", import5UAHeader.Declarant.CompanyName);
			AssertEquals("김택윤", import5UAHeader.Declarant.RepresentativeName);
			AssertEquals(1, import5UAHeader.SequenceNo);
			AssertEquals(100m, import5UAHeader.PenaltyExemptionAmount);
			AssertEquals("A", import5UAHeader.PenaltyType);
			AssertEquals("B", import5UAHeader.ExemptionProcessCode);
		}

		[TestDate(2014, 01, 01)]
		public void TestSerialisationTestData()
		{
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "상호";
			broker.OH_IsBroker = true;
			var payerAddress = broker.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			payerAddress.OA_CompanyNameOverride = "상호";
			payerAddress.OA_Address1 = "서울시 마로구 상암동 상암빌딩 123";
			payerAddress.OA_RN_NKCountryCode = "KR";

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "신청인명";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = "KRC";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.Branch.GB_OH_OrgProxy = broker.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4062001070010U";

			var sessionDetails = new Import5UASessionDetails();
			sessionDetails.PenaltyExemptionReasonCode = "B5";
			sessionDetails.PenaltyExemptionReason = "사유내용";
			sessionDetails.DutyPenaltyExemption5UASequenceNumber = 2;
			sessionDetails.PenaltyExemptionAmount = 200m;

			var import5UAHeader = new Import5UAHeaderCreator().Create(entry, sessionDetails, new System.DateTime(2014, 01, 01), 1, "A");
			Factory.Save();

			var result = new GOVCBR5UAMessageBuilder(import5UAHeader).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5UADataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5UA_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}

			AssertEquals("4062001070010U", import5UAHeader.ImportDeclarationNumber);
			AssertEquals("010", import5UAHeader.DeclarationCustomsOffice);
			AssertEquals("10", import5UAHeader.DeclarationCustomsDivision);
			AssertEquals("20140101", import5UAHeader.AmendmentDeclarationDate.ToString("yyyyMMdd"));
			AssertEquals(1, import5UAHeader.AmendmentVersionNo);
			AssertEquals("B5", import5UAHeader.PenaltyExemptionReasonsCode);
			AssertEquals("사유내용", import5UAHeader.PenaltyExemptionReason);
			AssertEquals("상호", import5UAHeader.Declarant.CompanyName);
			AssertEquals("신청인명", import5UAHeader.Declarant.RepresentativeName);
			AssertEquals(2, import5UAHeader.SequenceNo);
			AssertEquals(200m, import5UAHeader.PenaltyExemptionAmount);
			AssertEquals("A", import5UAHeader.PenaltyType);
			AssertEquals("B", import5UAHeader.ExemptionProcessCode);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
