using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class Import5BAHeaderTest : XMLMessageTestHelper<Import5BAHeaderTest>
	{
		[TestDate(2021, 03, 30)]
		public void TestFullData()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";
			Factory.Save();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0000000001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "품명");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0000000002", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "품명2");

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "신청인상호";
			broker.OH_IsBroker = true;

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "성명";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = ContactAllocationType.CEOForKRCustoms;

			var brokerAddress = broker.MainAddress;
			brokerAddress.OA_CompanyNameOverride = "신청인상호";
			brokerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			brokerAddress.OA_Address1 = "기본주소";
			brokerAddress.OA_Address2 = "상세주소";
			brokerAddress.OA_PostCode = "46512";
			brokerAddress.CustomsCodes.AddNew(IdentificationType.RoadNameCode, "101010");
			brokerAddress.CustomsCodes.AddNew(IdentificationType.BuildingNumber, "10201");
			brokerAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			broker.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0000000000", Core.Constants.CountryCodes.KoreaSouth);
			branch.GB_OH_OrgProxy = broker.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_ParentID = entry.PK;
			entryNum.CE_EntryNum = "02455874051";
			entryNum.CE_IssueDate = new ZDate(2021, 03, 31);
			entryNum.CE_EntryType = SharedJobMessageTypeList.Codes.Import;

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_AgreedDutyRatePreferenceCode = "CUD";
			instruction.CEI_AgreedDutyRate = 4.2m;
			instruction.CEI_JE = declaration.PK;
			entry.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "0000000001";

			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Tariff = "0000000001";
			invoiceLine1.JI_Description = "Timber";
			invoiceLine1.JI_SequenceNumber = 1;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "0000000002";

			JobComInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Tariff = "0000000002";
			invoiceLine2.JI_Description = "Plastic";
			invoiceLine2.JI_SequenceNumber = 1;

			var import5BA = new Import5BAHeaderCreator().Create(entry);
			var result = new GOVCBR5BAMessageBuilder(import5BA).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5BAHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5BA_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		[TestDate(2007, 06, 08)]
		public void TestRealData()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";
			Factory.Save();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "6912001090", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "CERAMIC CUPS");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9401809000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "SEATS");

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "성신관세사무소";
			broker.OH_IsBroker = true;

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "이상규";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = ContactAllocationType.CEOForKRCustoms;

			var brokerAddress = broker.MainAddress;
			brokerAddress.OA_CompanyNameOverride = "성신관세사무소";
			brokerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			brokerAddress.OA_Address1 = "서울시 강남구 논현동";
			brokerAddress.OA_Address2 = "212-2";
			brokerAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			broker.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "2110768443", Core.Constants.CountryCodes.KoreaSouth);

			branch.GB_OH_OrgProxy = broker.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_ParentID = entry.PK;
			entryNum.CE_EntryNum = "1292607100065U";
			entryNum.CE_IssueDate = new ZDate(2007, 06, 08);
			entryNum.CE_EntryType = SharedJobMessageTypeList.Codes.Import;

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_AgreedDutyRatePreferenceCode = "A";
			instruction.CEI_AgreedDutyRate = 8m;
			instruction.CEI_JE = declaration.PK;
			entry.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "6912001090";

			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Tariff = "6912001090";
			invoiceLine1.JI_Description = "TUMBLER";
			invoiceLine1.JI_SequenceNumber = 1;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "9401809000";

			JobComInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Tariff = "9401809000";
			invoiceLine2.JI_Description = "STOOL";
			invoiceLine2.JI_SequenceNumber = 1;

			var import5BA = new Import5BAHeaderCreator().Create(entry);
			var result = new GOVCBR5BAMessageBuilder(import5BA).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5BAHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5BA_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public void TestSerialisationOfDataProvider()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";
			Factory.Save();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "6912001090", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "CERAMIC CUPS");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9401809000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "SEATS");

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "성신관세사무소";
			broker.OH_IsBroker = true;
			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "이상규";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = ContactAllocationType.CEOForKRCustoms;

			var declarantAddress = broker.MainAddress;
			declarantAddress.OA_CompanyNameOverride = "성신관세사무소";
			declarantAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			declarantAddress.OA_Address1 = "서울시 강남구 논현동";
			declarantAddress.OA_Address2 = "212-2";
			broker.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "2110768443", Core.Constants.CountryCodes.KoreaSouth);

			branch.GB_OH_OrgProxy = broker.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			declaration.JE_GB = branch.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_ParentID = entry.PK;
			entryNum.CE_EntryNum = "1292607100065U";
			entryNum.CE_IssueDate = new ZDate(2007, 06, 08);
			entryNum.CE_EntryType = SharedJobMessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_AgreedDutyRatePreferenceCode = "A";
			instruction.CEI_AgreedDutyRate = 8m;
			entry.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "6912001090";

			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Tariff = "6912001090";
			invoiceLine1.JI_Description = "TUMBLER";
			invoiceLine1.JI_SequenceNumber = 1;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "9401809000";

			JobComInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Tariff = "9401809000";
			invoiceLine2.JI_Description = "STOOL";
			invoiceLine2.JI_SequenceNumber = 1;

			var import5BA = new Import5BAHeaderCreator().Create(entry);
			var serialisedXml = string.Empty;

			using (var stream = KRXmlObjectSerializer.Serialize(import5BA))
			{
				serialisedXml = stream.WriteToString();
			}

			AssertContains("<TariffRateClassification>A</TariffRateClassification>", serialisedXml);
			AssertContains("<TariffRate>8</TariffRate>", serialisedXml);
			AssertNotContains("Declarant is not amendable. XmlIgnored", nameof(IImport5BAHeader.Declarant), serialisedXml);
			AssertNotContains("ImportDeclarationNumber is not amendable. XmlIgnored", nameof(IImport5BAHeader.ImportDeclarationNumber), serialisedXml);
			AssertNotContains("ImportDeclarationDate is not amendable. XmlIgnored", nameof(IImport5BAHeader.ImportDeclarationDate), serialisedXml);
			AssertContains("<EntryLineNo>1</EntryLineNo>", serialisedXml);
			AssertContains("<HSCode>6912001090</HSCode>", nameof(IImport5BALine.HSCode), serialisedXml);
			AssertContains("<HSDescription>CERAMIC CUPS</HSDescription>", nameof(IImport5BALine.HSDescription), serialisedXml);
			AssertContains("<InvoiceDescription>TUMBLER</InvoiceDescription>", nameof(IImport5BALine.InvoiceDescription), serialisedXml);
			AssertContains("<EntryLineNo>2</EntryLineNo>", serialisedXml);
			AssertContains("<HSCode>9401809000</HSCode>", nameof(IImport5BALine.HSCode), serialisedXml);
			AssertContains("<HSDescription>SEATS</HSDescription>", nameof(IImport5BALine.HSDescription), serialisedXml);
			AssertContains("<InvoiceDescription>STOOL</InvoiceDescription>", nameof(IImport5BALine.InvoiceDescription), serialisedXml);
		}

		public void TestHasNotEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(0, entry.MergedLines.Count);

			var import5BA = new Import5BAHeaderCreator().Create(entry);
			AssertNotNull("EntryLines can be not null.", import5BA.EntryLines);

			var result = new GOVCBR5BAMessageBuilder(import5BA).GenerateMessage();
			AssertNotNull(result.Consignment);
		}
	}
}
