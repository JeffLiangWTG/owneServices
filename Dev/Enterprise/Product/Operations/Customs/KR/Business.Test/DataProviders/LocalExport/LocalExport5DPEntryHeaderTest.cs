using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExport5DPEntryHeaderTest : XMLMessageTestHelper<LocalExport5DPEntryHeaderTest>
	{
		public void TestFullData()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			var result = new LocalExport5DPEntryHeaderCreator().Create(entry);
			var builder = new GOVCBR5DPMessageBuilder(result).GenerateMessage();
			var fileReader = new TestFileReader(typeof(LocalExport5DPEntryHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5DP.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(builder))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}

			AssertEquals("무한상사", result.Importer.CompanyName);
			AssertEquals("김대표", result.Importer.RepresentativeName);
			AssertEquals("기본주소", result.Importer.AddressLine1);
			AssertEquals("상세주소", result.Importer.AddressLine2);
			AssertEquals("32012", result.Importer.Postcode);
			AssertEquals("110001", result.Importer.RoadNameCode);
			AssertEquals("121200", result.Importer.BuildingNumber);
			AssertEquals("1200020212", result.Importer.BusinessRegNo);
		}

		public void TestRealData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01022010", "동화면세점", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			#region Supplier
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "스타럭스1011014", Core.Constants.CountryCodes.KoreaSouth);
			supplier.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1208624029", Core.Constants.CountryCodes.KoreaSouth);
			#endregion

			#region Manufacturer
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			#endregion

			#region Importer
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "(주)동화면세점";
			importer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1028106181", Core.Constants.CountryCodes.KoreaSouth);

			var importerAddress = importer.MainAddress;
			importerAddress.OA_OH = importer.PK;
			importerAddress.OA_CompanyNameOverride = "(주)동화면세점";
			importerAddress.OA_Address1 = "서울 종로구 세종로";
			importerAddress.OA_Address2 = "211";

			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "신정희";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._03;
			declaration.JE_EntryDate = new ZDate(2014, 01, 15);
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_ExportGoodsType = "1";
			declaration.JE_LocationOfGoods = "";
			declaration.JE_LocationOtherInformation = "01022010";
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_NoOfPacks = 1;
			invoice1.JZ_OH_Manufacturer = manufacturer.PK;
			invoice1.JZ_OH_Buyer = importer.PK;
			invoice1.JZ_Weight = 0.1;
			invoice1.JZ_WeightUQ = Core.Constants.Weight.Tonnes;
			invoice1.JZ_DRWApplicantType = "1";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_NoOfPacks = 10;
			invoice2.JZ_OH_Manufacturer = manufacturer.PK;
			invoice2.JZ_OH_Buyer = importer.PK;
			invoice2.JZ_Weight = 1000;
			invoice2.JZ_WeightUQ = Core.Constants.Weight.Grams;
			invoice2.JZ_DRWApplicantType = "1";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum.CE_EntryNum = "229261400005U";
			entryNum.CE_IssueDate = ZDateTime.Today;

			#region ILocalExportEntryLine
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "9102119010";
			entryLine1.CL_Description = "BATTERY OR ACCUMULATOR OPERATED WATCHW0001L2 Guess Watch 3PC USD90 USD270W0040G3 Guess Watch 10PC USD82.5 USD825W0040G5 Guess Watch 5PC USD82.5 USD412.5";
			entryLine1.CL_CustomsValue = 3892303m;

			JobComInvoiceLine invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_InvoiceQuantity = 40m;
			invoiceLine1.JI_InvoiceUQ = "U";
			invoiceLine1.JI_NoOfPacks = 1;
			invoiceLine1.JI_PackType = "CT";
			invoiceLine1.JI_NetWeight = 0m;
			invoiceLine1.JI_NetWeightUQ = DefaultWeightUnit;
			invoiceLine1.JI_LinePrice = 3892303m;
			invoiceLine1.JI_SequenceNumber = 1;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "9102119010";
			entryLine2.CL_Description = "BATTERY OR ACCUMULATOR OPERATED WATCHW0092L1 Guess Watch 5PC USD80 USD400W0111L4 Guess Watch 2PC USD90 USD180W0125L5 Guess Watch 8PC USD55 USD440";
			entryLine2.CL_CustomsValue = 3420849m;

			JobComInvoiceLine invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_InvoiceQuantity = 42m;
			invoiceLine2.JI_InvoiceUQ = "U";
			invoiceLine2.JI_NoOfPacks = 1;
			invoiceLine2.JI_PackType = "CT";
			invoiceLine2.JI_NetWeight = 0m;
			invoiceLine2.JI_NetWeightUQ = DefaultWeightUnit;
			invoiceLine2.JI_LinePrice = 3420849m;
			invoiceLine2.JI_SequenceNumber = 1;

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			entryLine3.CL_AdValoremTariff = "9102119010";
			entryLine3.CL_Description = "BATTERY OR ACCUMULATOR OPERATED WATCHW0243G3 Guess Watch 5PC USD122.5 USD612.5W0246G1 Guess Watch 3PC USD82.5 USD247.5W0247G3 Guess Watch 3PC USD82.5 USD247.5";
			entryLine3.CL_CustomsValue = 2719280m;

			JobComInvoiceLine invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_InvoiceQuantity = 27m;
			invoiceLine3.JI_InvoiceUQ = "U";
			invoiceLine3.JI_NoOfPacks = 1;
			invoiceLine3.JI_PackType = "CT";
			invoiceLine3.JI_NetWeight = 0m;
			invoiceLine3.JI_NetWeightUQ = DefaultWeightUnit;
			invoiceLine3.JI_LinePrice = 2719280m;
			invoiceLine3.JI_SequenceNumber = 1;

			var entryLine4 = entry.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			entryLine4.CL_AdValoremTariff = "9102119010";
			entryLine4.CL_Description = "BATTERY OR ACCUMULATOR OPERATED WATCHW10614L2 Guess Watch 3PC USD70 USD210W11010G1 Guess Watch 7PC USD82.5 USD577.5W13103L1 Guess Watch 10PC USD90 USD900";
			entryLine4.CL_CustomsValue = 3656576m;

			JobComInvoiceLine invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_InvoiceQuantity = 48m;
			invoiceLine4.JI_InvoiceUQ = "U";
			invoiceLine4.JI_NoOfPacks = 1;
			invoiceLine4.JI_PackType = "CT";
			invoiceLine4.JI_NetWeight = 0m;
			invoiceLine4.JI_NetWeightUQ = DefaultWeightUnit;
			invoiceLine4.JI_LinePrice = 3656576m;
			invoiceLine4.JI_SequenceNumber = 1;
			#endregion

			var localExport5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);

			var result = new GOVCBR5DPMessageBuilder(localExport5DP).GenerateMessage();
			var fileReader = new TestFileReader(typeof(LocalExport5DPEntryHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5DP_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public void TestEmptyXml()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithPartialData();
			var localExport5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);
			var result = new GOVCBR5DPMessageBuilder(localExport5DP).GenerateMessage();
			var fileReader = new TestFileReader(typeof(LocalExport5DPEntryHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5DP_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public void TestTariffHasRequiringInvQuantityInCustomsUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208100000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Has Attribute InvoiceQuantity in CU1");
			helper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff1);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208122222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Has Not Attribute");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice = declaration.Invoices.AddNew();
			var invoiceLines1 = invoice.InvoiceLines.AddNew();
			invoiceLines1.JI_Tariff = tariff1.ZZ1_TariffCode;
			invoiceLines1.JI_CustomsQuantity = 1;
			invoiceLines1.JI_CustomsUnitQty = "U";
			invoiceLines1.JI_InvoiceQuantity = 2;
			invoiceLines1.JI_InvoiceUQ = "BAG";

			var invoiceLines2 = invoice.InvoiceLines.AddNew();
			invoiceLines2.JI_Tariff = tariff2.ZZ1_TariffCode;
			invoiceLines2.JI_CustomsQuantity = 1;
			invoiceLines2.JI_CustomsUnitQty = "U";
			invoiceLines2.JI_InvoiceQuantity = 2;
			invoiceLines2.JI_InvoiceUQ = "BAG";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var result = new LocalExport5DPEntryHeaderCreator().Create(declaration.CustomsEntryHeaders[0]);
			AssertEquals("Has Attribute", "0208100000", result.EntryLines[0].HSCode);
			AssertEquals(1m, result.EntryLines[0].Quantity);
			AssertEquals("U", result.EntryLines[0].QuantityUnit);

			AssertEquals("Has Not Attribute", "0208122222", result.EntryLines[1].HSCode);
			AssertEquals(2m, result.EntryLines[1].Quantity);
			AssertEquals("BAG", result.EntryLines[1].QuantityUnit);
		}

		public void TestEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryNum1 = entry.EntryNumbers.AddNew();
			entryNum1.CE_EntryNum = "1083699012345";
			entryNum1.CE_EntryType = "LEX";
			entryNum1.CE_EntryLineReference = "2";
			Factory.Save();

			var localExport5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);
			AssertEquals("1083699012345", localExport5DP.DeclarationNumber);

			var entryNum2 = entry.EntryNumbers.AddNew();
			entryNum2.CE_EntryNum = "1083699098765";
			entryNum2.CE_EntryType = "LEX";
			entryNum2.CE_EntryLineReference = "1";
			localExport5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);
			AssertEquals("1083699098765", localExport5DP.DeclarationNumber);
		}

		public void TestSupplierAddress()
		{
			var supplierWithMainAddress = Factory.NewWithValidTestData<OrgHeader>();
			supplierWithMainAddress.OH_FullName = "5DP Company";
			supplierWithMainAddress.MainAddress.Address1 = "Main Address1";
			supplierWithMainAddress.MainAddress.Address2 = "Main Address2";
			var supplierContact = supplierWithMainAddress.Contacts.AddNew();
			supplierContact.OC_ContactName = "5DP CompanyRepresentative";
			supplierContact.Allocations.AddNew().PC_Type = ContactAllocationType.CEOForKRCustoms;
			supplierWithMainAddress.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "111111111111111", Core.Constants.CountryCodes.KoreaSouth);
			supplierWithMainAddress.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0000000000", Core.Constants.CountryCodes.KoreaSouth);
			
			var supplierWithCustomsAddress = Factory.NewWithValidTestData<OrgHeader>();
			supplierWithCustomsAddress.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			supplierWithCustomsAddress.OH_FullName = "5DP Company";
			supplierWithCustomsAddress.CustomsAddress.Address1 = "Customs Address1";
			supplierWithCustomsAddress.CustomsAddress.Address2 = "Customs Address2";
			supplierContact = supplierWithCustomsAddress.Contacts.AddNew();
			supplierContact.OC_ContactName = "5DP CompanyRepresentative";
			supplierContact.Allocations.AddNew().PC_Type = ContactAllocationType.CEOForKRCustoms;
			supplierWithCustomsAddress.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "111111111111111", Core.Constants.CountryCodes.KoreaSouth);
			supplierWithCustomsAddress.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0000000000", Core.Constants.CountryCodes.KoreaSouth);

			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			entry.Declaration.JE_OH_Supplier = supplierWithMainAddress.PK;
			var localExport5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);
			AssertEquals(localExport5DP.Supplier.AddressLine1, "Main Address1");
			AssertEquals(localExport5DP.Supplier.AddressLine2, "Main Address2");

			entry.Declaration.JE_OH_Supplier = supplierWithCustomsAddress.PK;
			localExport5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);
			AssertEquals(localExport5DP.Supplier.AddressLine1, "Customs Address1");
			AssertEquals(localExport5DP.Supplier.AddressLine2, "Customs Address2");
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Outgoing";
	}
}
