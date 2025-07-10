using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRD72DataProvidersTest : XMLMessageTestHelper<GOVCBRD72DataProvidersTest>
	{
		[TestDate(2020, 03, 20)]
		public void TestRealDataXml()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";
			Factory.Save();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "01234", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "WOMENS SWIMWEAR");

			#region IOrganization
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "크리스챤디올꾸뛰르코리아(주)";
			broker.OH_IsBroker = true;

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "레이몬드데이";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var brokerAddress = broker.MainAddress;
			brokerAddress.OA_CompanyNameOverride = broker.OH_FullName;
			brokerAddress.PrimaryOrgAddressAdditionalInfoDetail = ",";
			brokerAddress.Address1 = "서울시 강남구 도산대로 458 (청담동,";
			brokerAddress.Address2 = "리츠타워 701호,801호)";
			brokerAddress.OA_PostCode = "06062";
			brokerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			brokerAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			var brokerCusCode = broker.MainAddress.CustomsCodes.AddNew();
			brokerCusCode.OK_CodeType = Constants.IdentificationType.BusinessRegNo;
			brokerCusCode.OK_CustomsRegNo = "1208174197";
			brokerCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			branch.GB_OH_OrgProxy = broker.PK;
			#endregion

			#region D72Header
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "040";
			declaration.JE_CustomsDivision = "94";
			declaration.JE_MessageType = "IMP";
			declaration.JE_GB = branch.PK;
			var countryCode = declaration.CountryCode;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "2292620000314M";

			var entryNumber1 = entry.EntryNumbers.AddNew();
			entryNumber1.CE_ParentID = entry.PK;
			entryNumber1.CE_EntryType = ElectronicDocumentTypeList.Codes._D72;
			entryNumber1.CE_EntryLineReference = "1";
			entryNumber1.CE_RN_NKCountryCode = countryCode;

			var entryNum5FN = entry.EntryNumbers.AddNew();
			entryNum5FN.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum5FN.CE_EntryLineReference = "2";
			#endregion

			#region D72Line
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "EUR";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Model = "02JAC106I603C087";
			invoiceLine2.JI_InvoiceUQ = "PC";
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_LinePrice = 68m;
			invoiceLine2.JI_CountryOfOrigin = countryCode;
			invoiceLine2.JI_Tariff = "01234";
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_ScheduledReExportDate = new ZDateTime(2021, 03, 31);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Model = "9E1J29A3802 X0849";
			invoiceLine1.JI_InvoiceUQ = "PC";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_LinePrice = 610m;
			invoiceLine1.JI_CountryOfOrigin = countryCode;
			invoiceLine1.JI_Tariff = "01234";
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_ScheduledReExportDate = new ZDateTime(2021, 03, 31);

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CH = entry.PK;
			entryLine.CL_LineNumber = 002;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.CL_AdValoremTariff = "01234";
			#endregion

			#region D72 Non-BO
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, new ZDateTime(2021, 03, 31));
				messageSendingObject.ReasonDescription = "주문 수집 일정 변동에 따른 재수출기한 연장 신청";
				messageSendingObject.NewReExportDate = new ZDateTime(2021, 04, 01);

				AssertEquals(1, messageSendingObject.D72EntryLines.Count);
				#endregion

				var importD72 = new ImportD72Creator().Create(entry, messageSendingObject, 2);
				var result = new GOVCBRD72MessageBuilder(importD72).GenerateMessage();
				var fileReader = new TestFileReader(typeof(GOVCBRD72DataProvidersTest));
				var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBRD72_Result_D1.xml");
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(testFile, serialisedXml);
				}
				AssertEquals("2292620000314M", importD72.ImportDeclarationNumber);
				AssertEquals(2, importD72.SequenceNo);
				AssertEquals("040", importD72.DeclarationCustomsOffice);
				AssertEquals("94", importD72.DeclarationCustomsDivision);
				AssertEquals("2", importD72.DeclarantType);
				AssertEquals(new ZDateTime(2021, 03, 31), importD72.BeforeReExportScheduledDate);
				AssertEquals(new ZDateTime(2021, 04, 01), importD72.AfterReExportScheduledDate);
				AssertEquals("주문 수집 일정 변동에 따른 재수출기한 연장 신청", importD72.ReasonDescription);

				AssertEquals(2, importD72.Lines.Length);
				AssertEquals(2, importD72.Lines[0].EntryLineNo);
				AssertEquals(1, importD72.Lines[0].DetailLineNo);
				AssertEquals("First JI_SequenceNumber value of JobComInvoiceLine sorted by JI_SequenceNumber.", 1, importD72.Lines[0].DetailLineNo);
				AssertEquals("WOMENS SWIMWEAR", importD72.Lines[0].HSDescription);
				AssertEquals("9E1J29A3802 X0849", importD72.Lines[0].ItemDescription);
				AssertEquals("PC", importD72.Lines[0].QuantityUnit);
				AssertEquals(1m, importD72.Lines[0].Quantity);
				AssertEquals("EUR", importD72.Lines[0].AmountCurrency);
				AssertEquals(610m, importD72.Lines[0].Amount);
				AssertEquals("", importD72.Lines[0].Remark);

				AssertEquals(2, importD72.Lines[1].EntryLineNo);
				AssertEquals(2, importD72.Lines[1].DetailLineNo);
				AssertEquals("Second JI_SequenceNumber value of JobComInvoiceLine sorted by JI_SequenceNumber.", 2, importD72.Lines[1].DetailLineNo);
				AssertEquals("WOMENS SWIMWEAR", importD72.Lines[1].HSDescription);
				AssertEquals("02JAC106I603C087", importD72.Lines[1].ItemDescription);
				AssertEquals("PC", importD72.Lines[1].QuantityUnit);
				AssertEquals(1m, importD72.Lines[1].Quantity);
				AssertEquals("EUR", importD72.Lines[1].AmountCurrency);
				AssertEquals(68m, importD72.Lines[1].Amount);
				AssertEquals("", importD72.Lines[1].Remark);

				AssertNotNull(importD72.Declarant);
				var declarantChk = importD72.Declarant;
				AssertEquals("1208174197", declarantChk.BusinessRegNo);
				AssertEquals("크리스챤디올꾸뛰르코리아(주)", declarantChk.CompanyName);
				AssertEquals("06062", declarantChk.Postcode);
				AssertEquals("서울시 강남구 도산대로 458 (청담동,", declarantChk.AddressLine1);
				AssertEquals("리츠타워 701호,801호)", declarantChk.AddressLine2);
				AssertEquals("레이몬드데이", declarantChk.RepresentativeName);
			}
		}

		[TestDate(2014, 01, 01)]
		public void TestSampleFullDataXml()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "00123", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "신고품명");

			#region IOrganization
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			importer.OH_FullName = "상호";

			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "성명";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var importerAddress = importer.MainAddress;
			importerAddress.OA_CompanyNameOverride = importer.OH_FullName;
			importerAddress.PrimaryOrgAddressAdditionalInfoDetail = "건물관리번호9999,99999999";
			importerAddress.Address1 = "기본주소";
			importerAddress.Address2 = "상세주소";
			importerAddress.OA_PostCode = "99099";
			importerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var roadNameCode = importer.MainAddress.CustomsCodes.AddNew();
			roadNameCode.OK_CodeType = Constants.IdentificationType.RoadNameCode;
			roadNameCode.OK_CustomsRegNo = "99999999";
			roadNameCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var buildingNumber = importer.MainAddress.CustomsCodes.AddNew();
			buildingNumber.OK_CodeType = Constants.IdentificationType.BuildingNumber;
			buildingNumber.OK_CustomsRegNo = "건물관리번호9999";
			buildingNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var importerCusCode1 = importer.MainAddress.CustomsCodes.AddNew();
			importerCusCode1.OK_CodeType = Constants.IdentificationType.KoreanRegNoForResident;
			importerCusCode1.OK_CustomsRegNo = "0000000000";
			importerCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var importerCusCode2 = importer.MainAddress.CustomsCodes.AddNew();
			importerCusCode2.OK_CodeType = Constants.IdentificationType.PassportNo;
			importerCusCode2.OK_CustomsRegNo = "PassportNo";
			importerCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var importerCusCode3 = importer.MainAddress.CustomsCodes.AddNew();
			importerCusCode3.OK_CodeType = Constants.IdentificationType.KoreanRegNoForForeigner;
			importerCusCode3.OK_CustomsRegNo = "KoreanRegNoForForeigner";
			importerCusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			#endregion

			#region D72Header
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_MessageType = "IMP";
			declaration.JE_OA_ImporterAddress = importerAddress.PK;
			var countryCode = declaration.CountryCode;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "999999999999";

			var entryNumber1 = entry.EntryNumbers.AddNew();
			entryNumber1.CE_ParentID = entry.PK;
			entryNumber1.CE_EntryType = ElectronicDocumentTypeList.Codes._D72;
			entryNumber1.CE_EntryLineReference = "1";
			entryNumber1.CE_RN_NKCountryCode = countryCode;

			var entryNum5FN = entry.EntryNumbers.AddNew();
			entryNum5FN.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum5FN.CE_EntryLineReference = "1";
			#endregion

			#region D72Line
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Model = "Spec Description";
			invoiceLine1.JI_InvoiceUQ = "PC";
			invoiceLine1.JI_InvoiceQuantity = 500m;
			invoiceLine1.JI_LinePrice = 99m;
			invoiceLine1.JI_CountryOfOrigin = countryCode;
			invoiceLine1.JI_Tariff = "00123";
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_ScheduledReExportDate = new ZDateTime(2014, 01, 01);

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CH = entry.PK;
			entryLine1.CL_LineNumber = 001;
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.CL_AdValoremTariff = "00123";
			#endregion

			#region D72 Non-BO
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "62345"))
			{
				var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, new ZDateTime(2014, 01, 01));
				messageSendingObject.ReasonDescription = "신청사유";
				messageSendingObject.NewReExportDate = new ZDateTime(2014, 01, 02);

				var messageSendingLineObject = messageSendingObject.D72EntryLines;
				AssertEquals(1, messageSendingLineObject.Count);

				var messageSendingInvoiceLineObject = messageSendingObject.MessageSendingInvoiceLines;
				AssertEquals(1, messageSendingInvoiceLineObject.Count);
				messageSendingInvoiceLineObject[0].Remark = "비고내용";
				#endregion

				#region Test Serialisation With Xml 
				var importD72 = new ImportD72Creator().Create(entry, messageSendingObject, 2);
				var result = new GOVCBRD72MessageBuilder(importD72).GenerateMessage();
				var fileReader = new TestFileReader(typeof(GOVCBRD72DataProvidersTest));
				var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBRD72_Result_D2.xml");
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(testFile, serialisedXml);
				}
				#endregion

				AssertEquals("010", importD72.DeclarationCustomsOffice);
				AssertEquals("10", importD72.DeclarationCustomsDivision);
				AssertEquals("999999999999", importD72.ImportDeclarationNumber);
				AssertEquals(2, importD72.SequenceNo);
				AssertEquals("1", importD72.DeclarantType);
				AssertEquals("신청사유", importD72.ReasonDescription);
				AssertEquals(new ZDateTime(2014, 01, 02), importD72.AfterReExportScheduledDate);
				AssertEquals(new ZDateTime(2014, 01, 01), importD72.BeforeReExportScheduledDate);

				AssertEquals(1, importD72.Lines.Length);
				AssertEquals(1, importD72.Lines[0].EntryLineNo);
				AssertEquals(1, importD72.Lines[0].DetailLineNo);
				AssertEquals("신고품명", importD72.Lines[0].HSDescription);
				AssertEquals("Spec Description", importD72.Lines[0].ItemDescription);
				AssertEquals("PC", importD72.Lines[0].QuantityUnit);
				AssertEquals(500m, importD72.Lines[0].Quantity);
				AssertEquals("USD", importD72.Lines[0].AmountCurrency);
				AssertEquals(99m, importD72.Lines[0].Amount);
				AssertEquals("비고내용", importD72.Lines[0].Remark);

				AssertNotNull(importD72.Declarant);
				var declarantChk = importD72.Declarant;
				AssertEquals("0000000000", declarantChk.KoreanRegNoForResident);
				AssertEquals("상호", declarantChk.CompanyName);
				AssertEquals("99099", declarantChk.Postcode);
				AssertEquals("기본주소", declarantChk.AddressLine1);
				AssertEquals("상세주소", declarantChk.AddressLine2);
				AssertEquals("성명", declarantChk.RepresentativeName);
				AssertEquals("99999999", declarantChk.RoadNameCode);
				AssertEquals("건물관리번호9999", declarantChk.BuildingNumber);
			}
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		public void TestEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "999999999999";
			var entryNum5FN = entry.EntryNumbers.AddNew();
			entryNum5FN.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum5FN.CE_EntryLineReference = "2";

			var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, new ZDateTime("2024-01-01"));
			AssertEquals(0, messageSendingObject.D72EntryLines.Count);
			AssertEquals(0, messageSendingObject.MessageSendingInvoiceLines.Count);

			var header = new ImportD72Creator().Create(entry, messageSendingObject, 0);
			AssertEquals(0, entry.MergedLines.Count);
			AssertNotNull("EntryLines can be not null.", header.Lines);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 1;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_InvoiceQuantity = 5m;
			invoiceLine2.JI_ScheduledReExportDate = new ZDateTime("2024-01-01");

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_InvoiceQuantity = 10m;
			invoiceLine1.JI_ScheduledReExportDate = new ZDateTime("2024-01-02");

			messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, new ZDateTime("2024-01-01"));
			messageSendingObject.NewReExportDate = new ZDateTime("2024-01-01");
			AssertEquals(1, messageSendingObject.D72EntryLines.Count);
			
			var messageSendingInvoiceLineObject = messageSendingObject.MessageSendingInvoiceLines;
			AssertEquals(1, messageSendingInvoiceLineObject.Count);
			AssertEquals(2u, messageSendingInvoiceLineObject[0].EntryLineNo);
			AssertEquals(1u, messageSendingInvoiceLineObject[0].InvoiceLineNo);
			AssertEquals(5m, messageSendingInvoiceLineObject[0].Quantity);

			header = new ImportD72Creator().Create(entry, messageSendingObject, 0);
			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(1, header.Lines.Length);
			AssertEquals(2, header.Lines[0].EntryLineNo);
			AssertEquals(1, header.Lines[0].DetailLineNo);
			AssertEquals(5m, header.Lines[0].Quantity);
		}

		public void TestWithDeclarant()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var broker = declaration.Company.OrgProxy;
			broker.OH_Category = "BUS";
			broker.OH_FullName = "성신관세사무소";
			broker.OH_IsBroker = true;

			branch.GB_OH_OrgProxy = broker.PK;
			declaration.JE_GB = branch.PK;
			var brokercontact = broker.Contacts.AddNew();
			brokercontact.OC_ContactName = "이상규";
			var brokerallocation = brokercontact.Allocations.AddNew();
			brokerallocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Category = OrgConstants.Category.Business;
			importer.OH_FullName = "Test";
			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "Name";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var realSetCusCode = importer.CustomsCodes.AddNew();
			realSetCusCode.OK_CodeType = Constants.IdentificationType.BusinessRegNo;
			realSetCusCode.OK_CustomsRegNo = "4";
			realSetCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var notSetCusCode = importer.CustomsCodes.AddNew();
			notSetCusCode.OK_CodeType = Constants.IdentificationType.KoreanRegNoForResident;
			notSetCusCode.OK_CustomsRegNo = "1";
			notSetCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "62345"))
			{
				var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty);
				var header = new ImportD72Creator().Create(entry, messageSendingObject, 0);
				AssertNull(header.Declarant);

				declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
				header = new ImportD72Creator().Create(entry, messageSendingObject, 0);
				AssertNotNull(header.Declarant);
				AssertEquals("Test", header.Declarant.CompanyName);
				AssertEquals("Name", header.Declarant.RepresentativeName);
				AssertEquals("4", header.Declarant.BusinessRegNo);
			}

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty);
				var header = new ImportD72Creator().Create(entry, messageSendingObject, 0);
				AssertNotNull(header.Declarant);
				AssertEquals("성신관세사무소", header.Declarant.CompanyName);
				AssertEquals("이상규", header.Declarant.RepresentativeName);
			}
		}

		public void TestWithLinesOrderBy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "999999999999";

			var entryNum5FN_001 = entry.EntryNumbers.AddNew();
			entryNum5FN_001.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum5FN_001.CE_EntryLineReference = "1";
			var entryNum5FN_002 = entry.EntryNumbers.AddNew();
			entryNum5FN_002.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum5FN_002.CE_EntryLineReference = "2";

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 2;
			var invoiceLine1_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_SequenceNumber = 2;
			invoiceLine1_1.JI_CL = entryLine1.PK;
			invoiceLine1_1.JI_ScheduledReExportDate = new ZDateTime("2024-01-01");
			var invoiceLine1_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_SequenceNumber = 1;
			invoiceLine1_2.JI_CL = entryLine1.PK;
			invoiceLine1_2.JI_ScheduledReExportDate = new ZDateTime("2024-01-01");

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			var invoiceLine2_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2_1.JI_SequenceNumber = 2;
			invoiceLine2_1.JI_CL = entryLine2.PK;
			invoiceLine2_1.JI_ScheduledReExportDate = new ZDateTime("2024-01-01");
			var invoiceLine2_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2_2.JI_SequenceNumber = 1;
			invoiceLine2_2.JI_CL = entryLine2.PK;
			invoiceLine2_2.JI_ScheduledReExportDate = new ZDateTime("2024-01-01");

			var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, new ZDateTime("2024-01-01"));
			messageSendingObject.NewReExportDate = new ZDateTime("2024-01-01");

			AssertEquals(2, messageSendingObject.D72EntryLines.Count);
			
			var header = new ImportD72Creator().Create(entry, messageSendingObject, 0);
			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(4, header.Lines.Length);
			AssertNotNull("EntryLine is set. and CL_LineNumber is same, so Created a ImportD72Header.Lines", header.Lines);
			AssertEquals(1, header.Lines[0].EntryLineNo);
			AssertEquals(1, header.Lines[0].DetailLineNo);

			AssertEquals(1, header.Lines[1].EntryLineNo);
			AssertEquals(2, header.Lines[1].DetailLineNo);

			AssertEquals(2, header.Lines[2].EntryLineNo);
			AssertEquals(1, header.Lines[2].DetailLineNo);

			AssertEquals(2, header.Lines[3].EntryLineNo);
			AssertEquals(2, header.Lines[3].DetailLineNo);
		}

		public void TestDecimalplaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "TestDecimalplaces";

			var entryNum5FN = entry.EntryNumbers.AddNew();
			entryNum5FN.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum5FN.CE_EntryLineReference = "1";

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.UnitPrice = 5m;
			invoiceLine.JI_ScheduledReExportDate = ZDateTime.Today;

			var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Today);
			var messageSendingLineObject = messageSendingObject.MessageSendingInvoiceLines[0];
			messageSendingLineObject.EntryLineNo = 1;
			messageSendingLineObject.InvoiceLineNo = 1;
			messageSendingLineObject.Quantity = 1111.11111m;

			var header = new ImportD72Creator().Create(entry, messageSendingObject, 0);
			AssertEquals("TestDecimalplaces", header.ImportDeclarationNumber);
		}

		public void TestHasNotEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var importD72 = new ImportD72Creator().Create(entry, new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty), 0);
			AssertNotNull("EntryLines can be not null.", importD72.Lines);

			var result = new GOVCBRD72MessageBuilder(importD72).GenerateMessage();
			AssertNotNull(result.GoodsShipment);
		}
	}
}
