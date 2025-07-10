using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR105SDataProvidersTest : TestCaseWithFactory
	{
		public void Test5SCFTAHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			entry.CH_EntryReleaseDate = new ZDateTime(2023, 08, 18);
			var current5scHeader = new ImportFTAAmendmentHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>());

			AssertEquals(new ZDateTime(2023, 08, 18), current5scHeader.EntryReleaseDate);
			AssertNotNull(current5scHeader.Declarant);
			AssertEquals("상호", current5scHeader.Declarant.CompanyName);
			AssertEquals("김환태", current5scHeader.Declarant.RepresentativeName);
			AssertEquals("130", current5scHeader.DeclarationCustomsOffice);
			AssertEquals("10", current5scHeader.DeclarationCustomsDivision);

			entry.Declaration.JE_ExportDate = new ZDateTime(2023, 06, 18);

			var amendManager = new GOVCBR105AmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._105);
			var amendedItems = amendManager.AmendedItems;
			var importFTAAmmendHeader = new ImportFTAAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertEquals(1, importFTAAmmendHeader.Items.Length);
			var amendItem = importFTAAmmendHeader.Items.Cast<ImportFTAAmendmentItem>().FirstOrDefault(x => x.EntryLineNo == 0 && x.DataItemID == "14");
			AssertNotNull(amendItem);
			AssertEquals(ZString.Empty, amendItem.AmendType);
			AssertEquals("20230718", amendItem.BeforeDescription);
			AssertEquals("20230618", amendItem.AfterDescription);

			entry.MergedLines[0].Delete();
			amendManager = new GOVCBR105AmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._105);
			amendedItems = amendManager.AmendedItems;
			importFTAAmmendHeader = new ImportFTAAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertEquals(2, importFTAAmmendHeader.Items.Length);
			AssertEquals(ZString.Empty, importFTAAmmendHeader.Items[0].AmendType);
			AssertEquals(ImportFTAItemAmendTypeCodeList.Codes._99D, importFTAAmmendHeader.Items[1].AmendType);
		}

		public void TestEmpty()
		{
			var details105 = new Mock<IAmendmentDetails>();
			var detailsDHS = new Mock<IDHSAmendmentDetails>();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var header = new ImportFTAAmendmentHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>());

			var result1 = new GOVCBR105MessageBuilder(header, details105.Object).GenerateMessage();
			AssertNoExceptionThrown(() => new ImportFTAAmendmentHeaderCreator().Create(result1));

			var result2 = new GOVCBRDHSMessageBuilder(header, detailsDHS.Object).GenerateMessage();
			AssertNoExceptionThrown(() => new ImportFTAAmendmentHeaderCreator().Create(result2));
		}

		public void TestFullData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("PORT", "port");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "PORT", "CNYAT", "YANTAI", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var details105 = new Mock<IAmendmentDetails>();
			var detailsDHS = new Mock<IDHSAmendmentDetails>();

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_FullName = "상호";
			TestOrgDataSetUpHelper.AddOrgContact(broker, "대표자명", true);
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "030";
			declaration.JE_CustomsDivision = "20";
			declaration.Branch.GB_OH_OrgProxy = broker.PK;
			declaration.JE_ExportDate = new ZDateTime(2021, 1, 12);
			declaration.JE_RL_NKPortOfLoading = "CNYAT";
			declaration.JE_TransshipmentDate = new ZDateTime(2021, 1, 20);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_FTARelationArticleCode = "1";
			instruction.CEI_StatementNumber5WN = "192113334901920";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryReleaseDate = new ZDateTime("2024-01-01");
			entry.EntryNumber = "123452400001U";
			entry.CH_CEI_Instruction = instruction.PK;

			var amendedItems = new List<AmendedItem>();
			var amendedItem = new AmendedItem();
			amendedItem.EntityType = nameof(IImportFTALine);
			amendedItem.AmendType = EntityAmendType.Add;
			amendedItem.BeforeValue = "Before Value";
			amendedItem.AfterValue = "After Value";
			amendedItem.DataItemID = FTAAmendmentDataItemIDList.Codes._19;
			amendedItems.Add(amendedItem);

			var header = new ImportFTAAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertEquals(new ZDateTime("2024-01-01"), header.EntryReleaseDate);
			AssertEquals("030", header.DeclarationCustomsOffice);
			AssertEquals("20", header.DeclarationCustomsDivision);
			AssertEquals("상호", header.Declarant.CompanyName);
			AssertEquals("대표자명", header.Declarant.RepresentativeName);
			AssertEquals(0, header.Items[0].EntryLineNo);
			AssertEquals("99I", header.Items[0].AmendType);
			AssertEquals("Before Value", header.Items[0].BeforeDescription);
			AssertEquals("After Value", header.Items[0].AfterDescription);
			AssertEquals("19", header.Items[0].DataItemID);

			AssertEquals("123452400001U", header.ImportDeclarationNumber);
			AssertEquals("192113334901920", header.StatementNumber5WN);
			AssertEquals("1", header.LawCode);
			AssertEquals("CN", header.DepartureCountryCode);
			AssertEquals("Yantai", header.DeparturePort);
			AssertEquals(new ZDateTime(2021, 1, 12), header.DepartureDate);
			AssertEquals(new ZDateTime(2021, 1, 20), header.TransshipmentDate);
			AssertEquals(ZString.Empty, header.TransshipmentPort);
			AssertEquals(Constants.YesNo.No, header.TransshipmentYN);
			AssertEquals(ZString.Empty, header.TransshipmentCountryCode);

			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "KRABC";
			port.RL_PortName = "Incheon";
			port.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			Factory.Save();

			declaration.JE_TransshipmentPort = "KRABC";
			header = new ImportFTAAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertEquals("Incheon", header.TransshipmentPort);
			AssertEquals(Constants.YesNo.Yes, header.TransshipmentYN);
			AssertEquals("KR", header.TransshipmentCountryCode);

			var result1 = new GOVCBR105MessageBuilder(header, details105.Object).GenerateMessage();
			var newHeader1 = new ImportFTAAmendmentHeaderCreator().Create(result1);
			AssertEquals(newHeader1.EntryReleaseDate, header.EntryReleaseDate);
			AssertEquals(newHeader1.DeclarationCustomsOffice, header.DeclarationCustomsOffice);
			AssertEquals(newHeader1.DeclarationCustomsDivision, header.DeclarationCustomsDivision);
			AssertEquals(newHeader1.Declarant.CompanyName, header.Declarant.CompanyName);
			AssertEquals(newHeader1.Declarant.RepresentativeName, header.Declarant.RepresentativeName);
			AssertEquals(newHeader1.Items[0].EntryLineNo, header.Items[0].EntryLineNo);
			AssertEquals(newHeader1.Items[0].AmendType, header.Items[0].AmendType);
			AssertEquals(newHeader1.Items[0].BeforeDescription, header.Items[0].BeforeDescription);
			AssertEquals(newHeader1.Items[0].AfterDescription, header.Items[0].AfterDescription);
			AssertEquals(newHeader1.Items[0].DataItemID, header.Items[0].DataItemID);

			var result2 = new GOVCBRDHSMessageBuilder(header, detailsDHS.Object).GenerateMessage();
			var newHeader2 = new ImportFTAAmendmentHeaderCreator().Create(result2);
			AssertEquals(newHeader2.EntryReleaseDate, header.EntryReleaseDate);
			AssertEquals(newHeader2.DeclarationCustomsOffice, header.DeclarationCustomsOffice);
			AssertEquals(newHeader2.DeclarationCustomsDivision, header.DeclarationCustomsDivision);
			AssertEquals(newHeader2.Declarant.CompanyName, header.Declarant.CompanyName);
			AssertEquals(newHeader2.Declarant.RepresentativeName, header.Declarant.RepresentativeName);
			AssertEquals(newHeader2.Items[0].EntryLineNo, header.Items[0].EntryLineNo);
			AssertEquals(newHeader2.Items[0].AmendType, header.Items[0].AmendType);
			AssertEquals(newHeader2.Items[0].BeforeDescription, header.Items[0].BeforeDescription);
			AssertEquals(newHeader2.Items[0].AfterDescription, header.Items[0].AfterDescription);
			AssertEquals(newHeader2.Items[0].DataItemID, header.Items[0].DataItemID);
		}

		public void TestAmendmentItemForJI_Tariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_FTASequenceNumber = 1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CL = entryLine.PK;

			ImportFTAHeader header = new ImportFTACreator().Create(entry);
			var stream = KRXmlObjectSerializer.Serialize(header);
			AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5SC, stream);
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5SC);

			invoiceLine.JI_Tariff = "1234560000";
			var amendedItems = new GOVCBR105AmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._105).AmendedItems;
			AssertEquals(0, amendedItems.Count());

			invoiceLine.JI_Tariff = "9234567890";
			amendedItems = new GOVCBR105AmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._105).AmendedItems;
			AssertEquals(1, amendedItems.Count());
			AssertEquals(FTAAmendmentDataItemIDList.Codes._19, amendedItems.FirstOrDefault().DataItemID);
			AssertEquals("123456", amendedItems.FirstOrDefault().BeforeValue);
			AssertEquals("923456", amendedItems.FirstOrDefault().AfterValue);

			var importFTAAmmendHeader = new ImportFTAAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			AssertEquals(1, importFTAAmmendHeader.Items.Length);
			AssertEquals(FTAAmendmentDataItemIDList.Codes._19, importFTAAmmendHeader.Items[0].DataItemID);
			AssertEquals("123456", importFTAAmmendHeader.Items[0].BeforeDescription);
			AssertEquals("923456", importFTAAmmendHeader.Items[0].AfterDescription);
		}
	}
}
