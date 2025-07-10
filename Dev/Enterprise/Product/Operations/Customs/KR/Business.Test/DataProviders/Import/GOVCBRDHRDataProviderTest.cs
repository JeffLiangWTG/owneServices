using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using UniversalConstants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRDHRDataProvidersTest : XMLMessageTestHelper<GOVCBRDHRDataProvidersTest>
	{
		[TestDate(2021, 01, 12)]
		public void TestRealData()
		{
			#region Sample Port Create
			var helperTest = new UniversalReferenceTestDataHelper(Factory);
			helperTest.CreateCusCodeType("PORT", "port");
			helperTest.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "PORT", "CNYAT", "YANTAI", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
			#endregion
			#region Importer
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Category = "BUS";
			importer.OH_FullName = "(주)노바복스코프";
			importer.OH_IsBroker = true;

			var importercontact = importer.Contacts.AddNew();
			importercontact.OC_ContactName = "전욱현";

			var importerallocation = importercontact.Allocations.AddNew();
			importerallocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var importerAddress = importer.MainAddress;
			importerAddress.OA_CompanyNameOverride = importer.OH_FullName;
			importerAddress.Address1 = "인천 남구 염전로 330 (주안 J TOWER1차 지식산업센터)";
			importerAddress.Address2 = "912호";
			importerAddress.OA_PostCode = "22126";
			importerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			importerAddress.OA_Fax = "0327143888";
			importerAddress.OA_Phone = "0325632745";
			importerAddress.OA_Email = "sales @novavox.net'@'";

			var importerCusCode1 = importer.MainAddress.CustomsCodes.AddNew();
			importerCusCode1.OK_CodeType = Constants.IdentificationType.BusinessRegNo;
			importerCusCode1.OK_CustomsRegNo = "2488600891";
			importerCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var importerCusCode2 = importer.MainAddress.CustomsCodes.AddNew();
			importerCusCode2.OK_CodeType = Constants.IdentificationType.UnipassIDForOrganization;
			importerCusCode2.OK_CustomsRegNo = "노바복스1181014";
			importerCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			#endregion
			#region Manufacturer
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Category = "BUS";
			manufacturer.OH_FullName = "ZHEJIANG KEEN LAB EQUIPMENTS CO LTD";
			manufacturer.OH_IsBroker = true;

			var manufacturercontact = manufacturer.Contacts.AddNew();
			manufacturercontact.OC_ContactName = ".";

			var manufzcturerallocation = manufacturercontact.Allocations.AddNew();
			manufzcturerallocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.OA_Address1 = "8888JINXINGXINCHANG COUNTY,ZHEJIANG PROVINCE CHINA";
			manufacturerAddress.OA_OH = manufacturer.PK;
			manufacturerAddress.OA_CompanyNameOverride = manufacturer.OH_FullName;
			manufacturerAddress.OA_Fax = ".";
			manufacturerAddress.OA_Phone = ".";
			#endregion
			#region Supplier
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Category = "BUS";
			supplier.OH_FullName = "ZHEJIANG KEEN LAB EQUIPMENTS CO LTD";
			supplier.OH_IsBroker = true;

			var suppliercontact = supplier.Contacts.AddNew();
			suppliercontact.OC_ContactName = ".";

			var supplierallocation = suppliercontact.Allocations.AddNew();
			supplierallocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Address1 = "8888JINXINGXINCHANG COUNTY,ZHEJIANG PROVINCE CHINA";
			supplierAddress.OA_CompanyNameOverride = supplier.OH_FullName;
			supplierAddress.OA_Fax = ".";
			supplierAddress.OA_Phone = ".";
			supplierAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			var supplierCusCode1 = supplier.MainAddress.CustomsCodes.AddNew();
			supplierCusCode1.OK_CodeType = Constants.IdentificationType.CertificateOfOriginExporterNumber;
			supplierCusCode1.OK_CustomsRegNo = "";
			supplierCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			#endregion

			#region Header
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2021, 01, 12);
			declaration.JE_RL_NKPortOfLoading = "CNYAT";
			declaration.JE_TransshipmentPort = ZString.Empty;
			declaration.JE_TransshipmentDate = ZDateTime.Empty;
			declaration.JE_OA_ImporterAddress = importerAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OA_ManufacturerAddress = manufacturerAddress.PK;
			declaration.JE_OH_Manufacturer = manufacturer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4225021000066M";

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_FTARelationArticleCode = "1";
			instruction.CEI_StatementNumber5WN = "192113334901920";
			entry.CH_CEI_Instruction = instruction.PK;
			#endregion

			#region Line1
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_OH_Supplier = supplier.PK;

			var invoiceLine1_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_Tariff = "8481800000";
			invoiceLine1_1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1_1.JI_NetWeight = 7.1;
			invoiceLine1_1.JI_NetWeightUQ = "KG";
			invoiceLine1_1.JI_PrimaryPreference = "FCN1";
			invoiceLine1_1.CertificateOfOriginIssueStatus = "B";
			invoiceLine1_1.JI_CustomsFifthQuantity = 1;
			var certificate1_1 = invoiceLine1_1.CertificateOfOriginData;
			certificate1_1.CSI_RN_NKCountryCode = ZString.Empty;
			certificate1_1.CSI_DateOfIssue = new ZDateTime(2021, 01, 11);
			certificate1_1.CSI_Quantity2 = 36;
			certificate1_1.CSI_UnitOfQuantity2 = "KG";
			certificate1_1.CSI_UnitOfQuantity = "KG";
			certificate1_1.CSI_ReferenceNumber = "1921133349019201";
			certificate1_1.CSI_IssuerType = "0";
			certificate1_1.CSI_Description = "중국국제무역촉진위원회(CCPIT)";
			certificate1_1.CSI_LineNo = 1;
			certificate1_1.CSI_UnitOfQuantity = "G";

			var invoiceLine1_2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_Tariff = "8481800000";
			invoiceLine1_2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1_2.JI_NetWeight = 7.2;
			invoiceLine1_2.JI_NetWeightUQ = "KG";
			invoiceLine1_2.JI_PrimaryPreference = "FCN1";
			invoiceLine1_2.CertificateOfOriginIssueStatus = "B";
			invoiceLine1_2.JI_CustomsFifthQuantity = 2;
			var certificate1_2 = invoiceLine1_2.CertificateOfOriginData;
			certificate1_2.CSI_RN_NKCountryCode = ZString.Empty;
			certificate1_2.CSI_DateOfIssue = new ZDateTime(2021, 01, 11);
			certificate1_2.CSI_Quantity2 = 35;
			certificate1_2.CSI_UnitOfQuantity2 = "KG";
			certificate1_2.CSI_UnitOfQuantity = "KG";
			certificate1_2.CSI_ReferenceNumber = "1921133349019201";
			certificate1_2.CSI_IssuerType = "0";
			certificate1_2.CSI_Description = "중국국제무역촉진위원회(CCPIT)";
			certificate1_2.CSI_LineNo = 2;

			var invoiceLine1_3 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1_3.JI_Tariff = "8481800000";
			invoiceLine1_3.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1_3.JI_NetWeight = 7.4;
			invoiceLine1_3.JI_NetWeightUQ = "KG";
			invoiceLine1_3.JI_PrimaryPreference = "FCN1";
			invoiceLine1_3.CertificateOfOriginIssueStatus = "B";
			invoiceLine1_3.JI_CustomsFifthQuantity = 3;
			var certificate1_3 = invoiceLine1_3.CertificateOfOriginData;
			certificate1_3.CSI_RN_NKCountryCode = ZString.Empty;
			certificate1_3.CSI_DateOfIssue = new ZDateTime(2021, 01, 11);
			certificate1_3.CSI_Quantity2 = 36;
			certificate1_3.CSI_UnitOfQuantity2 = "KG";
			certificate1_3.CSI_UnitOfQuantity = "KG";
			certificate1_3.CSI_ReferenceNumber = "1921133349019201";
			certificate1_3.CSI_IssuerType = "0";
			certificate1_3.CSI_Description = "중국국제무역촉진위원회(CCPIT)";
			certificate1_3.CSI_LineNo = 3;

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CH = entry.PK;
			entryLine1.CL_LineNumber = 001;
			entryLine1.CL_FTASequenceNumber = 001;
			entryLine1.CL_AdValoremTariff = "848180";

			invoiceLine1_1.JI_CL = entryLine1.PK;
			invoiceLine1_1.JI_SequenceNumber = 1;
			invoiceLine1_1.COOSplitOrder = ZInt.Zero;
			invoiceLine1_1.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine1_1.JI_COOSupportingDocType = "1";
			invoiceLine1_1.JI_CustomsFifthQuantity = 73m;

			invoiceLine1_2.JI_CL = entryLine1.PK;
			invoiceLine1_2.JI_SequenceNumber = 2;
			invoiceLine1_2.COOSplitOrder = ZInt.Zero;
			invoiceLine1_2.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine1_2.JI_COOSupportingDocType = "1";
			invoiceLine1_2.JI_CustomsFifthQuantity = 12m;

			invoiceLine1_3.JI_CL = entryLine1.PK;
			invoiceLine1_3.JI_SequenceNumber = 3;
			invoiceLine1_3.COOSplitOrder = ZInt.Zero;
			invoiceLine1_3.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine1_3.JI_COOSupportingDocType = "1";
			invoiceLine1_3.JI_CustomsFifthQuantity = 13m;
			#endregion

			#region Line2
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OH_Supplier = supplier.PK;

			var invoiceLine2_1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2_1.JI_Tariff = "8414900000";
			invoiceLine2_1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine2_1.JI_NetWeight = 15.3;
			invoiceLine2_1.JI_NetWeightUQ = "KG";
			invoiceLine2_1.JI_PrimaryPreference = "FCN1";
			invoiceLine2_1.CertificateOfOriginIssueStatus = "B";
			invoiceLine2_1.JI_CustomsFifthQuantity = 4;
			var certificate2_1 = invoiceLine2_1.CertificateOfOriginData;
			certificate2_1.CSI_RN_NKCountryCode = ZString.Empty;
			certificate2_1.CSI_DateOfIssue = new ZDateTime(2021, 01, 11);
			certificate2_1.CSI_Quantity2 = 54;
			certificate2_1.CSI_UnitOfQuantity2 = "KG";
			certificate2_1.CSI_UnitOfQuantity = "KG";
			certificate2_1.CSI_ReferenceNumber = "1921133349019201";
			certificate2_1.CSI_IssuerType = "0";
			certificate2_1.CSI_Description = "중국국제무역촉진위원회(CCPIT)";
			certificate2_1.CSI_LineNo = 4;

			var invoiceLine2_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2_2.JI_Tariff = "8414900000";
			invoiceLine2_2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine2_2.JI_NetWeight = 15;
			invoiceLine2_2.JI_NetWeightUQ = "KG";
			invoiceLine2_2.JI_PrimaryPreference = "FCN1";
			invoiceLine2_2.CertificateOfOriginIssueStatus = "B";
			invoiceLine2_2.JI_CustomsFifthQuantity = 5;
			var certificate2_2 = invoiceLine2_2.CertificateOfOriginData;
			certificate2_2.CSI_RN_NKCountryCode = ZString.Empty;
			certificate2_2.CSI_DateOfIssue = new ZDateTime(2021, 01, 11);
			certificate2_2.CSI_Quantity2 = 53;
			certificate2_2.CSI_UnitOfQuantity2 = "KG";
			certificate2_2.CSI_UnitOfQuantity = "KG";
			certificate2_2.CSI_ReferenceNumber = "1921133349019201";
			certificate2_2.CSI_IssuerType = "0";
			certificate2_2.CSI_Description = "중국국제무역촉진위원회(CCPIT)";
			certificate2_2.CSI_LineNo = 5;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CH = entry.PK;
			entryLine2.CL_LineNumber = 002;
			entryLine2.CL_FTASequenceNumber = 002;
			entryLine2.CL_AdValoremTariff = "841490";
			invoiceLine2_1.JI_CL = entryLine2.PK;
			invoiceLine2_1.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine2_1.JI_COOSupportingDocType = "1";
			invoiceLine2_1.JI_SequenceNumber = 1;

			invoiceLine2_2.JI_CL = entryLine2.PK;
			invoiceLine2_2.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine2_2.JI_COOSupportingDocType = "1";
			invoiceLine2_2.JI_SequenceNumber = 2;
			#endregion

			#region Test Serialisation With Xml 
			var importDHR = new ImportDHRCreator().Create(entry);
			var result = new GOVCBRDHRMessageBuilder(importDHR).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBRDHRDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBRDHR_Result_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
			#endregion

			#region Test Set Check With Header
			AssertEquals("4225021000066M", importDHR.ImportDeclarationNumber);
			AssertEquals("192113334901920", importDHR.StatementNumber5WN);
			AssertEquals("1", importDHR.LawCode);
			AssertEquals(new ZDateTime(2021, 01, 12), importDHR.DepartureDate);
			AssertEquals(Constants.YesNo.No, importDHR.TransshipmentYN);
			AssertEquals(ZDateTime.Empty, importDHR.TransshipmentDate);
			AssertEquals(ZString.Empty, importDHR.TransshipmentCountryCode);
			AssertEquals(ZString.Empty, importDHR.TransshipmentPort);
			AssertEquals("CN", importDHR.DepartureCountryCode);
			AssertEquals("Yantai", importDHR.DeparturePort);
			#endregion

			#region Test Set With Line
			var entryLines = importDHR.EntryLines;
			AssertNotNull(entryLines);
			AssertEquals(2, entryLines.Length);
			AssertEquals(new ZShort(001), entryLines[0].EntryLineNo);
			AssertEquals(new ZShort(001), entryLines[0].SequenceNo);
			AssertEquals("848180", entryLines[0].HSCode);
			AssertEquals(Core.Constants.CountryCodes.China, entryLines[0].CountryOfOrigin);
			AssertEquals("FCN1", entryLines[0].DutyRateCode);
			AssertEquals(ZString.Empty, entryLines[0].AdditionalInvoiceIssuingThirdCountryCode);
			AssertEquals(ZShort.Zero, entryLines[0].CertificateOfOriginSplitOrder);
			AssertEquals("1", entryLines[0].CountryOfOriginSupportingDocType);
			AssertEquals("B", entryLines[0].CertificateOfOriginProductType);
			AssertEquals(ZString.Empty, entryLines[0].AssociatedCOOIssuingCountryCode);
			AssertEquals(new ZDateTime(2021, 01, 11), entryLines[0].CertificateOfOriginIssueDate);
			AssertEquals("1921133349019201", entryLines[0].CertificateOfOriginNo);
			AssertEquals("중국국제무역촉진위원회(CCPIT)", entryLines[0].CertificateOfOriginAgencyName);
			AssertEquals("1", entryLines[0].CertifiticateOfOriginIssuerType);
			AssertEquals("1", entryLines[0].CertifiticateOfOriginIssuingAgencyType);

			AssertEquals(new ZShort(002), entryLines[1].EntryLineNo);
			AssertEquals(new ZShort(002), entryLines[1].SequenceNo);
			AssertEquals("841490", entryLines[1].HSCode);
			AssertEquals(Core.Constants.CountryCodes.China, entryLines[1].CountryOfOrigin);
			AssertEquals("FCN1", entryLines[1].DutyRateCode);
			AssertEquals(ZString.Empty, entryLines[1].AdditionalInvoiceIssuingThirdCountryCode);
			AssertEquals(ZShort.Zero, entryLines[1].CertificateOfOriginSplitOrder);
			AssertEquals("1", entryLines[1].CountryOfOriginSupportingDocType);
			AssertEquals("B", entryLines[1].CertificateOfOriginProductType);
			AssertEquals(ZString.Empty, entryLines[1].AssociatedCOOIssuingCountryCode);
			AssertEquals(new ZDateTime(2021, 01, 11), entryLines[1].CertificateOfOriginIssueDate);
			AssertEquals("1921133349019201", entryLines[1].CertificateOfOriginNo);
			AssertEquals("중국국제무역촉진위원회(CCPIT)", entryLines[1].CertificateOfOriginAgencyName);
			AssertEquals("1", entryLines[1].CertifiticateOfOriginIssuerType);
			AssertEquals("1", entryLines[1].CertifiticateOfOriginIssuingAgencyType);
			#endregion

			#region Test Set With Detail Line
			var invoiceLines = importDHR.DHRInvoiceLines;
			AssertNotNull(invoiceLines);
			AssertEquals(5, invoiceLines.Length);

			AssertEquals(new ZShort(1), invoiceLines[0].InvoiceLineNo);
			AssertEquals("1921133349019201", invoiceLines[0].CertificateOfOriginNo);
			AssertEquals(new ZShort(1), invoiceLines[0].CertificateOfOriginSeqNo);
			AssertEquals(new ZDecimal(73), invoiceLines[0].CertificateOfOriginUsedQuantity);
			AssertEquals("G", invoiceLines[0].CertificateOfOriginUsedUQ);

			AssertEquals(new ZShort(2), invoiceLines[1].InvoiceLineNo);
			AssertEquals("1921133349019201", invoiceLines[1].CertificateOfOriginNo);
			AssertEquals(new ZShort(2), invoiceLines[1].CertificateOfOriginSeqNo);
			AssertEquals(new ZDecimal(12), invoiceLines[1].CertificateOfOriginUsedQuantity);
			AssertEquals("KG", invoiceLines[1].CertificateOfOriginUsedUQ);

			AssertEquals(new ZShort(3), invoiceLines[2].InvoiceLineNo);
			AssertEquals("1921133349019201", invoiceLines[2].CertificateOfOriginNo);
			AssertEquals(new ZShort(3), invoiceLines[2].CertificateOfOriginSeqNo);
			AssertEquals(new ZDecimal(13), invoiceLines[2].CertificateOfOriginUsedQuantity);
			AssertEquals("KG", invoiceLines[2].CertificateOfOriginUsedUQ);

			AssertEquals(new ZShort(1), invoiceLines[3].InvoiceLineNo);
			AssertEquals("1921133349019201", invoiceLines[3].CertificateOfOriginNo);
			AssertEquals(new ZShort(4), invoiceLines[3].CertificateOfOriginSeqNo);
			AssertEquals(new ZDecimal(4), invoiceLines[3].CertificateOfOriginUsedQuantity);
			AssertEquals("KG", invoiceLines[3].CertificateOfOriginUsedUQ);

			AssertEquals(new ZShort(2), invoiceLines[4].InvoiceLineNo);
			AssertEquals("1921133349019201", invoiceLines[4].CertificateOfOriginNo);
			AssertEquals(new ZShort(5), invoiceLines[4].CertificateOfOriginSeqNo);
			AssertEquals(new ZDecimal(5), invoiceLines[4].CertificateOfOriginUsedQuantity);
			AssertEquals("KG", invoiceLines[4].CertificateOfOriginUsedUQ);
			#endregion

			#region Test Set Checj With Org
			var importerDHR = importDHR.Importer;
			AssertNotNull(importerDHR);
			AssertEquals("(주)노바복스코프", importerDHR.CompanyName);
			AssertEquals("전욱현", importerDHR.RepresentativeName);
			AssertEquals("인천 남구 염전로 330 (주안 J TOWER1차 지식산업센터)", importerDHR.AddressLine1);
			AssertEquals("912호", importerDHR.AddressLine2);
			AssertEquals("22126", importerDHR.Postcode);
			AssertEquals("0327143888", importerDHR.FaxNumber);
			AssertEquals("sales @novavox.net'@'", importerDHR.Email);
			AssertEquals("0325632745", importerDHR.PhoneNumber);
			AssertEquals(false, importerDHR.IsIndividual);
			AssertEquals("2488600891", importerDHR.BusinessRegNo);
			AssertEquals("노바복스1181014", importerDHR.UnipassIDForOrganization);

			var manufacturerDHR = importDHR.Manufacturer;
			AssertNotNull(manufacturerDHR);
			AssertEquals("ZHEJIANG KEEN LAB EQUIPMENTS CO LTD", manufacturerDHR.CompanyName);
			AssertEquals(".", manufacturerDHR.RepresentativeName);
			AssertEquals("8888JINXINGXINCHANG COUNTY,ZHEJIANG PROVINCE CHINA", manufacturerDHR.AddressLine1);
			AssertEquals(null, manufacturerDHR.AddressLine2);
			AssertEquals(null, manufacturerDHR.Postcode);
			AssertEquals(".", manufacturerDHR.FaxNumber);
			AssertEquals(null, manufacturerDHR.Email);
			AssertEquals(".", manufacturerDHR.PhoneNumber);
			AssertEquals(false, manufacturerDHR.IsIndividual);

			var supplierDHR = importDHR.Supplier;
			AssertNotNull(supplierDHR);
			AssertEquals("ZHEJIANG KEEN LAB EQUIPMENTS CO LTD", supplierDHR.CompanyName);
			AssertEquals(".", supplierDHR.RepresentativeName);
			AssertEquals("8888JINXINGXINCHANG COUNTY,ZHEJIANG PROVINCE CHINA", supplierDHR.AddressLine1);
			AssertEquals(null, supplierDHR.AddressLine2);
			AssertEquals(null, supplierDHR.Postcode);
			AssertEquals(".", supplierDHR.FaxNumber);
			AssertEquals(null, supplierDHR.Email);
			AssertEquals(".", supplierDHR.PhoneNumber);
			AssertEquals(false, supplierDHR.IsIndividual);
			#endregion
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		public void TestImporterRegistrationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Category = OrgConstants.Category.Business;
			var businessNoCode = importer.CustomsCodes.AddNew();
			businessNoCode.OK_CodeType = IdentificationType.BusinessRegNo;
			businessNoCode.OK_CustomsRegNo = "1";
			businessNoCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var residentRegNoCode = importer.CustomsCodes.AddNew();
			residentRegNoCode.OK_CodeType = IdentificationType.KoreanRegNoForResident;
			residentRegNoCode.OK_CustomsRegNo = "2";
			residentRegNoCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var unipassCode = importer.CustomsCodes.AddNew();
			unipassCode.OK_CodeType = IdentificationType.UnipassIDForOrganization;
			unipassCode.OK_CustomsRegNo = "3";
			unipassCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var individualCode = importer.CustomsCodes.AddNew();
			individualCode.OK_CodeType = IdentificationType.UnipassIDForIndividual;
			individualCode.OK_CustomsRegNo = "4";
			individualCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];

			var header = new ImportDHRCreator().Create(entry);
			AssertEquals("1", header.Importer.BusinessRegNo);
			AssertEquals(null, header.Importer.KoreanRegNoForResident);
			AssertEquals("3", header.Importer.UnipassIDForOrganization);
			AssertEquals(null, header.Importer.UnipassIDForIndividual);

			importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header = new ImportDHRCreator().Create(entry);
			AssertEquals(null, header.Importer.BusinessRegNo);
			AssertEquals("2", header.Importer.KoreanRegNoForResident);
			AssertEquals(null, header.Importer.UnipassIDForOrganization);
			AssertEquals("4", header.Importer.UnipassIDForIndividual);
		}

		public void TestTariffRate()
		{
			#region ZZ Data

			var zzDataSetUpper = new UniversalReferenceTestDataHelper(Factory);
			zzDataSetUpper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			var tariffType = zzDataSetUpper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);

			var dutyRateType = zzDataSetUpper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.RateTypes.Duty);
			var adValoremRateCode = zzDataSetUpper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, dutyRateType.PK);
			var specificRateCode = zzDataSetUpper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutySpecific, dutyRateType.PK);

			var cnTradeGroup = zzDataSetUpper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TradeGroups.China);
			zzDataSetUpper.AddCountry(cnTradeGroup, Core.Constants.CountryCodes.China);
			var fcn1Preference = zzDataSetUpper.CreatePreferenceForCountry("FCN1", "한ㆍ중국 FTA협정세율(선택1)", Core.Constants.CountryCodes.KoreaSouth);

			var allTradeGroup = zzDataSetUpper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TradeGroups.All);
			zzDataSetUpper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.China);
			var aPreference = zzDataSetUpper.CreatePreferenceForCountry("A", "기본세율", Core.Constants.CountryCodes.KoreaSouth);

			var hsTariff = zzDataSetUpper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101299000", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var allRate = zzDataSetUpper.CreateRate(hsTariff, adValoremRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "VFD * 0.08", aPreference.PK, "", Core.Constants.CountryCodes.KoreaSouth);
			var fcn1Rate = zzDataSetUpper.CreateRate(hsTariff, adValoremRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "VFD * 0.005", fcn1Preference.PK, "", Core.Constants.CountryCodes.KoreaSouth);

			var hsTariff2 = zzDataSetUpper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101299010", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var allRate2 = zzDataSetUpper.CreateRate(hsTariff2, specificRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "[KG] * 8000", aPreference.PK, "", Core.Constants.CountryCodes.KoreaSouth);
			var fcn1Rate2 = zzDataSetUpper.CreateRate(hsTariff2, specificRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "[KG] * 3000", fcn1Preference.PK, "", Core.Constants.CountryCodes.KoreaSouth);

			zzDataSetUpper.CreateCusApplicability(allRate, allTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			zzDataSetUpper.CreateCusApplicability(fcn1Rate, cnTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			zzDataSetUpper.CreateCusApplicability(allRate2, allTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			zzDataSetUpper.CreateCusApplicability(fcn1Rate2, cnTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			#endregion
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101299000";
			invoiceLine.JI_PrimaryPreference = "FCN1";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0101299010";
			invoiceLine2.JI_PrimaryPreference = "FCN1";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			invoiceLine.CusEntryLine.CL_FTASequenceNumber = 1;
			invoiceLine2.CusEntryLine.CL_FTASequenceNumber = 2;
			var header = new ImportDHRCreator().Create(invoiceLine.CusEntryLine.Header);
			AssertEquals(2, header.EntryLines.Length);
			AssertEquals(0.5m, header.EntryLines[0].TariffRate);
			AssertEquals("010129", header.EntryLines[0].HSCode);
			AssertEquals(3000m, header.EntryLines[1].TariffRate);
			AssertEquals("010129", header.EntryLines[1].HSCode);
		}

		public void TestThirdCountryData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CH = entry.PK;
			entryLine1.CL_LineNumber = 001;
			entryLine1.CL_FTASequenceNumber = 001;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_RN_NKSecondCommercialInvoiceCountry = "";

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CH = entry.PK;
			entryLine2.CL_LineNumber = 002;
			entryLine2.CL_FTASequenceNumber = 002;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_RN_NKSecondCommercialInvoiceCountry = Core.Constants.CountryCodes.Indonesia;

			var importDHR = new ImportDHRCreator().Create(entry);
			AssertNotNull(importDHR.EntryLines);
			AssertEquals(2, importDHR.EntryLines.Length);

			var entryLine = importDHR.EntryLines;
			AssertEquals("", entryLine[0].AdditionalInvoiceIssuingThirdCountryCode);
			AssertEquals("If AdditionalInvoiceIssuingThirdCountryCode value is Empty, It is N", "N", entryLine[0].AdditionalInvoiceIssuedInThirdCountryYN);

			AssertEquals("ID", entryLine[1].AdditionalInvoiceIssuingThirdCountryCode);
			AssertEquals("If AdditionalInvoiceIssuingThirdCountryCode value is Not Empty, It is Y", "Y", entryLine[1].AdditionalInvoiceIssuedInThirdCountryYN);
		}

		public void TestEntryLineOrdered()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var invoice = declaration.Invoices.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CH = entry.PK;
			entryLine1.CL_LineNumber = 002;
			entryLine1.CL_FTASequenceNumber = 002;

			var invoiceLine1_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine1.PK;
			var invoiceLine1_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine1.PK;
			var invoiceLine1_3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_3.JI_CL = entryLine1.PK;

			invoiceLine1_1.JI_SequenceNumber = 6;
			invoiceLine1_2.JI_SequenceNumber = 5;
			invoiceLine1_3.JI_SequenceNumber = 4;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CH = entry.PK;
			entryLine2.CL_LineNumber = 001;
			entryLine2.CL_FTASequenceNumber = 001;

			var invoiceLine2_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2_1.JI_CL = entryLine2.PK;
			var invoiceLine2_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2_2.JI_CL = entryLine2.PK;
			var invoiceLine2_3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2_3.JI_CL = entryLine2.PK;

			invoiceLine2_1.JI_SequenceNumber = 3;
			invoiceLine2_2.JI_SequenceNumber = 2;
			invoiceLine2_3.JI_SequenceNumber = 1;

			AssertEquals(2, entry.MergedLines.Count);
			var entryLine = entry.MergedLines;
			AssertEquals(new ZShort(002), entryLine[0].CL_LineNumber);
			AssertEquals(new ZShort(002), entryLine[0].CL_FTASequenceNumber);
			AssertEquals(new ZShort(001), entryLine[1].CL_LineNumber);
			AssertEquals(new ZShort(001), entryLine[1].CL_FTASequenceNumber);

			var invoiceLines1 = entryLine[0].InvoiceLines.Cast<JobComInvoiceLine>().ToList();
			AssertEquals(new ZShort(6), invoiceLines1[0].JI_SequenceNumber);
			AssertEquals(new ZShort(5), invoiceLines1[1].JI_SequenceNumber);
			AssertEquals(new ZShort(4), invoiceLines1[2].JI_SequenceNumber);

			var invoiceLines2 = entryLine[1].InvoiceLines.Cast<JobComInvoiceLine>().ToList();
			AssertEquals(new ZShort(3), invoiceLines2[0].JI_SequenceNumber);
			AssertEquals(new ZShort(2), invoiceLines2[1].JI_SequenceNumber);
			AssertEquals(new ZShort(1), invoiceLines2[2].JI_SequenceNumber);

			var importDHR = new ImportDHRCreator().Create(entry);
			AssertNotNull(importDHR.EntryLines);
			AssertEquals(2, importDHR.EntryLines.Length);

			var lines = importDHR.EntryLines;
			AssertEquals(1, lines[0].SequenceNo);
			AssertEquals(2, lines[1].SequenceNo);

			AssertNotNull(importDHR.DHRInvoiceLines);
			AssertEquals(6, importDHR.DHRInvoiceLines.Length);
			var invoiceLines = importDHR.DHRInvoiceLines;
			AssertEquals(1, invoiceLines[0].InvoiceLineNo);
			AssertEquals(2, invoiceLines[1].InvoiceLineNo);
			AssertEquals(3, invoiceLines[2].InvoiceLineNo);
			AssertEquals(4, invoiceLines[3].InvoiceLineNo);
			AssertEquals(5, invoiceLines[4].InvoiceLineNo);
			AssertEquals(6, invoiceLines[5].InvoiceLineNo);
		}

		public void TestEntryLineWithNotSetKR_FTASeqNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var invoice = declaration.Invoices.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CH = entry.PK;
			entryLine1.CL_LineNumber = 002;
			entryLine1.CL_FTASequenceNumber = 002;

			var invoiceLine1_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine1.PK;
			var invoiceLine1_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine1.PK;

			invoiceLine1_1.JI_SequenceNumber = 6;
			invoiceLine1_2.JI_SequenceNumber = 5;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CH = entry.PK;
			entryLine2.CL_LineNumber = 001;

			var invoiceLine2_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2_1.JI_CL = entryLine2.PK;
			invoiceLine2_1.JI_SequenceNumber = 3;

			AssertEquals(2, entry.MergedLines.Count);
			var entryLine = entry.MergedLines;
			AssertEquals(new ZShort(002), entryLine[0].CL_LineNumber);
			AssertEquals(new ZShort(2), entryLine[0].CL_FTASequenceNumber);
			AssertEquals(new ZShort(001), entryLine[1].CL_LineNumber);
			AssertEquals(new ZShort(0), entryLine[1].CL_FTASequenceNumber);

			var invoiceLines1 = entryLine[0].InvoiceLines.Cast<JobComInvoiceLine>().ToList();
			AssertEquals(new ZShort(6), invoiceLines1[0].JI_SequenceNumber);
			AssertEquals(new ZShort(5), invoiceLines1[1].JI_SequenceNumber);

			var invoiceLines2 = entryLine[1].InvoiceLines.Cast<JobComInvoiceLine>().ToList();
			AssertEquals(new ZShort(3), invoiceLines2[0].JI_SequenceNumber);

			var importDHR = new ImportDHRCreator().Create(entry);
			AssertNotNull(importDHR.EntryLines);
			AssertEquals(1, importDHR.EntryLines.Length);

			var lines = importDHR.EntryLines;
			AssertEquals(2, lines[0].SequenceNo);

			AssertNotNull(importDHR.DHRInvoiceLines);
			AssertEquals(2, importDHR.DHRInvoiceLines.Length);
			var invoiceLines = importDHR.DHRInvoiceLines;
			AssertEquals(5, invoiceLines[0].InvoiceLineNo);
			AssertEquals(6, invoiceLines[1].InvoiceLineNo);
		}

		public void TestFormattWithToDateTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Empty;
			declaration.JE_TransshipmentDate = ZDateTime.Empty;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CH = entry.PK;
			entryLine.CL_LineNumber = 001;
			entryLine.CL_FTASequenceNumber = 001;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.CertificateOfOriginIssueStatus = ZString.Empty;
			var certificate = invoiceLine.CertificateOfOriginData;
			certificate.CSI_ParentID = invoiceLine.PK;
			certificate.CSI_DateOfIssue = ZDateTime.Empty;
			certificate.CSI_LineNo = 1;

			var importDHR = new ImportDHRCreator().Create(entry);
			AssertEquals(ZDateTime.Empty, importDHR.DepartureDate);
			AssertEquals(ZDateTime.Empty, importDHR.TransshipmentDate);
			AssertNotNull(importDHR.EntryLines);
			AssertEquals(1, importDHR.EntryLines.Length);
			AssertEquals(ZDateTime.Empty, importDHR.EntryLines[0].CertificateOfOriginIssueDate);

			declaration.JE_ExportDate = new ZDateTime(2021, 01, 01);
			declaration.JE_TransshipmentDate = new ZDateTime(2021, 01, 02);
			certificate.CSI_DateOfIssue = new ZDateTime(2021, 01, 03);

			importDHR = new ImportDHRCreator().Create(entry);
			AssertEquals(new ZDateTime(2021, 01, 01), importDHR.DepartureDate);
			AssertEquals(new ZDateTime(2021, 01, 02), importDHR.TransshipmentDate);
			AssertNotNull(importDHR.EntryLines);
			AssertEquals(1, importDHR.EntryLines.Length);
			AssertEquals(new ZDateTime(2021, 01, 03), importDHR.EntryLines[0].CertificateOfOriginIssueDate);
		}

		public void TestNotSetOrg()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entry.MergedLines.AddNew().PK;

			var importDHR = new ImportDHRCreator().Create(entry);
			AssertNull(importDHR.Importer);
			AssertNull(importDHR.Supplier);
			AssertNull(importDHR.Manufacturer);

			#region Importer
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Category = "BUS";
			importer.OH_FullName = "Importer Company";
			importer.OH_IsBroker = true;

			var importercontact = importer.Contacts.AddNew();
			importercontact.OC_ContactName = "Importer Name";
			var importerallocation = importercontact.Allocations.AddNew();
			importerallocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var importerAddress = importer.MainAddress;
			importerAddress.OA_CompanyNameOverride = importer.OH_FullName;
			importerAddress.Address1 = "Importer Address1";
			importerAddress.Address2 = "Importer Address2";
			importerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var importerCusCode1 = importer.MainAddress.CustomsCodes.AddNew();
			importerCusCode1.OK_CodeType = Constants.IdentificationType.BusinessRegNo;
			importerCusCode1.OK_CustomsRegNo = "BusinessRegNo";
			importerCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var importerCusCode2 = importer.MainAddress.CustomsCodes.AddNew();
			importerCusCode2.OK_CodeType = Constants.IdentificationType.UnipassIDForOrganization;
			importerCusCode2.OK_CustomsRegNo = "UnipassIDForOrganization";
			importerCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			#endregion

			#region Manufacturer
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Category = "BUS";
			manufacturer.OH_FullName = "ManuFactuer Company";
			manufacturer.OH_IsBroker = true;

			var manufacturercontact = manufacturer.Contacts.AddNew();
			manufacturercontact.OC_ContactName = "ManuFactuer Name";
			var manufzcturerallocation = manufacturercontact.Allocations.AddNew();
			manufzcturerallocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var manufacturerAddress = manufacturer.MainAddress;
			manufacturerAddress.OA_Address1 = "ManuFactuer Address1";
			manufacturerAddress.OA_CompanyNameOverride = manufacturer.OH_FullName;
			manufacturerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			#endregion

			#region Supplier
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Category = "BUS";
			supplier.OH_FullName = "Supplier Company";
			supplier.OH_IsBroker = true;

			var suppliercontact = supplier.Contacts.AddNew();
			suppliercontact.OC_ContactName = "Supplier Name";
			var supplierallocation = suppliercontact.Allocations.AddNew();
			supplierallocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Address1 = "Supplier Address1";
			supplierAddress.OA_CompanyNameOverride = "Supplier Company2";
			supplierAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			#endregion

			declaration.JE_OA_ImporterAddress = importerAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_SupplierAddress = supplierAddress.PK;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;

			importDHR = new ImportDHRCreator().Create(entry);

			AssertNotNull(importDHR.Importer);
			var importerData = importDHR.Importer;
			AssertEquals("Importer Company", importerData.CompanyName);
			AssertEquals("Importer Name", importerData.RepresentativeName);
			AssertEquals("Importer Address1", importerData.AddressLine1);
			AssertEquals("Importer Address2", importerData.AddressLine2);
			AssertEquals("BusinessRegNo", importerData.BusinessRegNo);
			AssertEquals("UnipassIDForOrganization", importerData.UnipassIDForOrganization);

			AssertNotNull(importDHR.Manufacturer);
			var manufacturerData = importDHR.Manufacturer;
			AssertEquals("ManuFactuer Company", manufacturerData.CompanyName);
			AssertEquals("ManuFactuer Name", manufacturerData.RepresentativeName);
			AssertEquals("ManuFactuer Address1", manufacturerData.AddressLine1);

			AssertNotNull(importDHR.Supplier);
			var supplierData = importDHR.Supplier;
			AssertEquals("Supplier Company2", supplierData.CompanyName);
			AssertEquals("Supplier Name", supplierData.RepresentativeName);
			AssertEquals("Supplier Address1", supplierData.AddressLine1);
		}

		public void TestSupplierCustomsAddressOfRecord()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entry.MergedLines.AddNew().PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Category = "BUS";
			supplier.OH_FullName = "Supplier Company";
			
			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Address1 = "Supplier Address1";
			supplierAddress.OA_CompanyNameOverride = "Supplier Company Override1";
			supplierAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var supplierAddressOfRecord = supplier.Addresses.AddNew();
			supplierAddressOfRecord.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			supplierAddressOfRecord.OA_Address1 = "Supplier Address2";
			supplierAddressOfRecord.OA_CompanyNameOverride = "Supplier Company Override2";
			supplierAddressOfRecord.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			invoice.JZ_OH_Supplier = supplier.PK;

			var importDHR = new ImportDHRCreator().Create(entry);
			var supplierData = importDHR.Supplier;
			AssertEquals("Supplier Company Override2", supplierData.CompanyName);
			AssertEquals("Supplier Address2", supplierData.AddressLine1);
		}
	}
}
