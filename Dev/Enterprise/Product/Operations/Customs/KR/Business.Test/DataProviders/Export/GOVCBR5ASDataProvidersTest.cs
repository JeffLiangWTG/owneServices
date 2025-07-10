using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5ASDataProvidersTest : TestCaseWithFactory
	{
		public void Test5ASHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var current5ASHeader = new Export5ASHeaderCreator().Create(entry, Array.Empty<AmendedItem>());

			AssertEquals("6N00221000025X", current5ASHeader.ExportDeclarationNumber);
			AssertEquals("130", current5ASHeader.DeclarationCustomsOffice);
			AssertEquals("10", current5ASHeader.DeclarationCustomsDivision);
			AssertEquals("6N002", current5ASHeader.UnipassDeclarantID);

			var exporter = current5ASHeader.Exporter;
			AssertEquals("레디코리아2", exporter.CompanyName);
			AssertEquals("서울특별시 서초구 동광로 41", exporter.AddressLine1);
			AssertEquals("레디인빌딩", exporter.AddressLine2);
			AssertEquals("레디코리1971018", exporter.UnipassIDForOrganization);
		}

		public void Test830HeaderIsChangedInAmendment()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "77777777", "레디", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = entry.Declaration;
			declaration.JE_ExportGoodsType = "12";
			declaration.JE_MessageSubType = "A";
			declaration.JE_GoodsDestination = "US";
			declaration.JE_RL_NKPortOfLoading = "KRPUS";
			declaration.JE_LocationQualifier = "08600";
			declaration.JE_LocationOfGoods = "서울 서초구 동광로41";
			declaration.JE_LocationOtherInformation = "77777777";
			declaration.JE_ExportDate = new ZDateTime(2022, 03, 30);
			declaration.JE_ExporterType = "A";
			declaration.JE_ReturnReason = "XX";
			declaration.JE_GoodsCondition = "X";
			declaration.JE_SimpleDRWApp = "AD";
			declaration.CusContainers.RemoveAll();
			declaration.JE_TradeIndicatorWithKP = "Y";
			declaration.JE_ContainerPackMode = "AB";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_CarrierCode = "KR";
			declaration.JE_VoyageFlightNo = "AA9997";
			declaration.JE_TotalNoOfPacksPackType = "EA";
			declaration.UnderbondMovementArrivalDate = new ZDateTime(2022, 01, 01);
			declaration.UnderbondMovementDepartureDate = new ZDateTime(2022, 04, 20);

			declaration.JE_ReturnType = "B";
			declaration.JE_TradeIDWithKP = "GS";
			declaration.JE_ProcedureType = "O";
			declaration.JE_LocationIDInBondedArea = "1234567890";

			declaration.InspectionDate = new ZDateTime(2022, 03, 31);

			JobComInvoiceHeader invoice = (JobComInvoiceHeader)declaration.Invoices.FirstOrDefault();
			invoice.JZ_PaymentTerms = "LC";
			invoice.JZ_LetterOfCreditNumber = "7654321";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 30000m;
			invoice.JZ_InvoiceCurrExRate = 1099.25m;
			invoice.JZ_NoOfPacks = 2;
			invoice.JZ_DRWApplicantType = "2";
			invoice.JZ_Remarks = @"신고인 기재";
			invoice.JZ_RX_NKInvoice_Currency = "KRW";

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertHeader(export5ASHeader, "A105", "11", "12");
			AssertHeader(export5ASHeader, "A106", "B", "A");
			AssertHeader(export5ASHeader, "A601", "HK", "US");
			AssertHeader(export5ASHeader, "A602", "KRINC", "PUS");
			AssertHeader(export5ASHeader, "A801", "08500", "08600");
			AssertHeader(export5ASHeader, "A805", "서울 금천구 가산디지털1로 119(SK트윈테크타워)", "서울 서초구 동광로41");
			AssertHeader(export5ASHeader, "A802", "서브", "레디");
			AssertHeader(export5ASHeader, "A901", "99999999", "77777777");
			AssertHeader(export5ASHeader, "AB01", "20210308", "20220331");
			AssertHeader(export5ASHeader, "A904", "20140101", "20220101");
			AssertHeader(export5ASHeader, "A905", "20140101", "20220420");
			AssertHeader(export5ASHeader, "A606", "20140101", "20220330");
			AssertHeader(export5ASHeader, "AC01", "1", "2");
			AssertHeader(export5ASHeader, "A104", "H", "O");
			AssertHeader(export5ASHeader, "A107", "TT", "LC");
			AssertHeader(export5ASHeader, "A203", "C", "A");
			AssertHeader(export5ASHeader, "AB03", "", "XX");
			AssertHeader(export5ASHeader, "A804", "O", "X");
			AssertHeader(export5ASHeader, "AC02", "NO", "AD");
			AssertHeader(export5ASHeader, "AB04", "True", "False");
			AssertHeader(export5ASHeader, "AB07", "N", "Y");
			AssertHeader(export5ASHeader, "A702", "AA", "AB");
			AssertHeader(export5ASHeader, "A803", "1234567", "7654321");
			AssertHeader(export5ASHeader, "A701", "10", "40");
			AssertHeader(export5ASHeader, "A604", "KE", "KR");
			AssertHeader(export5ASHeader, "A605", "AA9999", "AA9997");
			AssertHeader(export5ASHeader, "A705", "CFR", "FOB");
			AssertHeader(export5ASHeader, "A707", "USD", "KRW");
			AssertHeader(export5ASHeader, "A706", "27670", "30670");
			AssertHeader(export5ASHeader, "AB05", "1. 보세구역 반입 후 수출신고건은 보세구역 운영인, 컨테이너 작업업체 연락처 등 기재\r\n2. 보세구역 반입 후 수출신고건은 보세구역 운영인, 컨테이너 작업업체 연락처 등 기재", "신고인 기재");
			AssertHeader(export5ASHeader, "AA04", "3", "4");
			AssertHeader(export5ASHeader, "AA05", "OU", "EA");

			AssertHeader(export5ASHeader, "AB09", "A", "B");
			AssertHeader(export5ASHeader, "AB08", "02", "GS");
			AssertHeader(export5ASHeader, "A902", "9999999999", "1234567890");
		}
		public void Test830HeaderIsChangedInOrgHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			var seller = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "TEST1", "변경기업1");
			var sellerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리9999000" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller, sellerCodes);
			var sellerAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "11111"  }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller.MainAddress, sellerAddressCodes);

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "TEST2", "변경기업2");
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "김택윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "A1234", "BBBB99999");
			var supplierCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리9999000" },
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1248100111" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, supplierCodes);
			var supplierAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "99000" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier.MainAddress, supplierAddressCodes);

			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "TEST3", "변경기업3");
			TestOrgDataSetUpHelper.AddOrgAddress(manufacturer.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "04784", buildingNumber: "BBBB00000");
			var manufacturerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "제조미상1111222" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer, manufacturerCodes);
			var manufacturerAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00010" },
				new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "990" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer.MainAddress, manufacturerAddressCodes);

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "TEST4", "변경기업4");
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "HKBOARAM0001B" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);

			declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			declaration.JE_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			foreach (var invoice in declaration.Invoices)
			{
				invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
				invoice.JZ_OH_Buyer = importer.PK;
			}

			Factory.Save();
			declaration = Factory.Load<JobDeclaration>(declaration.PK);
			entry = declaration.CustomsEntryHeaders.First();
			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertHeader(export5ASHeader, "A202", "레디코리1971018", "레디코리9999000");
			AssertHeader(export5ASHeader, "A204", "00000", "11111");
			AssertHeader(export5ASHeader, "A302", "레디코리1971018", "레디코리9999000");
			AssertHeader(export5ASHeader, "A306", "101010", "A1234");
			AssertHeader(export5ASHeader, "A307", "020120", "BBBB99999");
			AssertHeader(export5ASHeader, "A310", "1028142299", "1248100111");
			AssertHeader(export5ASHeader, "A311", "00001", "99000");
			AssertHeader(export5ASHeader, "A402", "제조미상9999000", "제조미상1111222");
			AssertHeader(export5ASHeader, "A412", "00001", "00010");
			AssertHeader(export5ASHeader, "A502", "HKBOARAM0001A", "HKBOARAM0001B");
			AssertAmendItemIsNull(export5ASHeader, "A411");
			AssertAmendItemIsNull(export5ASHeader, "AB05");
		}

		public void Test830HeaderSumAndCalculated()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			var invoice1 = declaration.Invoices.FirstOrDefault();
			invoice1.JZ_Weight = 103m;
			var invoice2 = declaration.Invoices.LastOrDefault();
			invoice2.JZ_Weight = 20m;
			var invoiceLine1 = invoice1.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_SequenceNumber == 1);
			invoiceLine1.JI_Weight = 100m;
			var invoiceLine2 = invoice1.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_SequenceNumber == 2);
			invoiceLine2.JI_Weight = 20m;
			var invoiceLine3 = invoice2.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_SequenceNumber == 1);
			invoiceLine3.JI_Weight = 3m;

			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			var overseasFreight = invoiceLine1.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight);

			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			var overseasInsurance = invoiceLine1.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance);

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertHeader(export5ASHeader, "A703", "1300", "1310");
			AssertHeader(export5ASHeader, "A704", "2500", "2501");
			AssertHeader(export5ASHeader, "AA02", "29600", "123");
		}

		public void TestImportCargoManagementNumber()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			var invoice = (JobComInvoiceHeader)declaration.Invoices.FirstOrDefault();
			invoice.JZ_ImportCargoManagementNumber = "16HJSC0686I00080000";

			var amendManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			var amendedItems = amendManager.AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, ZString.Empty, 0, "F101", 0, "", "16HJSC0686I00080000");

			CreateLatestSnapshot(entry);
			invoice.JZ_ImportCargoManagementNumber = "16HJSC0686I00080001";
			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, ZString.Empty, 0, "F101", 0, "16HJSC0686I00080000", "16HJSC0686I00080001");

			CreateLatestSnapshot(entry);
			invoice.JZ_ImportCargoManagementNumber = "";
			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, ZString.Empty, 0, "F001", 0, "16HJSC0686I00080001", "");
		}

		public void TestWhenOrgIsDeleted()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			declaration.JE_OA_SellerAddress = ZGuid.Empty;

			var amendManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			var amendedItems = amendManager.AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(3, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, ZString.Empty, 0, "A201", 0, "레디코리아1", "");
			AssertAmendItem(export5ASHeader, ZString.Empty, 0, "A202", 0, "레디코리1971018", "");
			AssertAmendItem(export5ASHeader, ZString.Empty, 0, "A204", 0, "00000", "");
		}

		public void Test830EntryLineIsChanged()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var entryLine1 = entry.MergedLines[0];
			entryLine1.CL_AdValoremTariff = "899999999";
			entryLine1.CL_CustomsValue = 100m;
			var invoiceLine1 = entryLine1.RandomLine;
			invoiceLine1.JI_Tariff = "899999999";
			invoiceLine1.JI_Model = "HYUNDAI ROBEX3000LC-7A-CHANGED";
			invoiceLine1.JI_BrandName = "상표명2";
			invoiceLine1.InvoiceHeader.JZ_InvoiceNumber = "999999990";
			invoiceLine1.JI_CountryOfOrigin = "US";
			invoiceLine1.CertificateOfOriginData.CSI_SubType = "B";
			invoiceLine1.JI_COOLabelLocation = "Y";
			invoiceLine1.CertificateOfOriginData.CSI_Code = "N";
			invoiceLine1.JI_CustomsUnitQty = "CT";
			invoiceLine1.JI_NoOfPacks = 5;
			invoiceLine1.JI_PackType = "BO";
			invoiceLine1.JI_SkipManifestReport = "Y";

			invoiceLine1.JI_PrimaryPreference = "109";

			var entryLine2 = entry.MergedLines[1];
			var invoiceLine2 = entryLine2.RandomLine;
			invoiceLine2.JI_NetWeight = 400m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.JI_PreviousEntryNumber = "B";
			invoiceLine2.JI_PreviousEntryLineNumber = 2;

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(22, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "03", 1, "B104", 0, "8429521022", "899999999");
			AssertAmendItem(export5ASHeader, "03", 1, "B101", 0, "USED EXCAVATOR", "품명2");
			AssertAmendItem(export5ASHeader, "03", 1, "B102", 0, "HYUNDAI ROBEX3000LC-7A", "HYUNDAI ROBEX3000LC-7A-CHANGED");
			AssertAmendItem(export5ASHeader, "03", 1, "B103", 0, "상표명", "상표명2");
			AssertAmendItem(export5ASHeader, "03", 1, "B105", 0, "999999999", "999999990");
			AssertAmendItem(export5ASHeader, "03", 1, "B501", 0, "KR", "US");
			AssertAmendItem(export5ASHeader, "03", 1, "B502", 0, "A", "B");
			AssertAmendItem(export5ASHeader, "03", 1, "B503", 0, "N", "Y");
			AssertAmendItem(export5ASHeader, "03", 1, "B504", 0, "Y", "N");
			AssertAmendItem(export5ASHeader, "03", 2, "B201", 0, "299999.99", "400");
			AssertAmendItem(export5ASHeader, "03", 2, "B203", 0, "1235", "0");
			AssertAmendItem(export5ASHeader, "03", 2, "B204", 0, "CT", "");
			AssertAmendItem(export5ASHeader, "03", 2, "B203", 0, "1235", "0");
			AssertAmendItem(export5ASHeader, "03", 1, "B601", 0, "3", "5");
			AssertAmendItem(export5ASHeader, "03", 1, "B602", 0, "CT", "BO");
			AssertAmendItem(export5ASHeader, "03", 1, "B301", 0, "25987293", "100");
			AssertAmendItem(export5ASHeader, "03", 2, "B401", 0, "A", "B");
			AssertAmendItem(export5ASHeader, "03", 2, "B402", 0, "001", "002");
			AssertAmendItem(export5ASHeader, "03", 1, "B106", 0, "N", "Y");

			AssertAmendItem(export5ASHeader, "03", 1, "B505", 0, "", "109");
			AssertAmendItem(export5ASHeader, "03", 2, "B401", 0, "A", "B");
			AssertAmendItem(export5ASHeader, "03", 2, "B402", 0, "001", "002");
		}

		public void Test830EntryLineIsAdded()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_AdValoremTariff = "899999999";
			entryLine.CL_CustomsValue = 100m;
			var invoiceLine = entry.Declaration.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_Tariff = "899999999";
			invoiceLine.JI_Model = "HYUNDAI ROBEX3000LC-7A-CHANGED";
			invoiceLine.JI_BrandName = "상표명2";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.CertificateOfOriginIssueStatus = "N";
			invoiceLine.CriteriaForDeterminingCountryOfOrigin = "B";
			invoiceLine.JI_COOLabelLocation = "Y";
			invoiceLine.JI_NoOfPacks = 5;
			invoiceLine.JI_PackType = "BO";
			invoiceLine.JI_SkipManifestReport = "Y";
			invoiceLine.PRA_ReferenceNumber = "KR00101010000";
			invoiceLine.PRA_DateOfIssue = new ZDateTime(2020, 01, 01);
			invoiceLine.PRA_DateOfExpiry = new ZDateTime(2020, 09, 01);
			invoiceLine.JI_PreviousEntryNumber = "B";
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			invoiceLine.JI_NetWeight = 0m;

			invoiceLine.JI_Description = "모델규격3";
			invoiceLine.JI_Ingredient = "성분3";
			invoiceLine.JI_LotNumber = "ZZZZEEE";
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.UnitPrice = 200m;
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 1000m;

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(29, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "01", 3, "B104", 0, "", "899999999");
			AssertAmendItem(export5ASHeader, "01", 3, "B101", 0, "", "품명2");
			AssertAmendItem(export5ASHeader, "01", 3, "B102", 0, "", "HYUNDAI ROBEX3000LC-7A-CHANGED");
			AssertAmendItem(export5ASHeader, "01", 3, "B103", 0, "", "상표명2");
			AssertAmendItem(export5ASHeader, "01", 3, "B105", 0, "", "999999999");
			AssertAmendItem(export5ASHeader, "01", 3, "B501", 0, "", "US");
			AssertAmendItem(export5ASHeader, "01", 3, "B502", 0, "", "B");
			AssertAmendItem(export5ASHeader, "01", 3, "B503", 0, "", "Y");
			AssertAmendItem(export5ASHeader, "01", 3, "B504", 0, "", "N");
			AssertAmendItem(export5ASHeader, "01", 3, "B201", 0, null, "0");
			AssertAmendItem(export5ASHeader, "01", 3, "B202", 0, "", "KG");

			AssertAmendItem(export5ASHeader, "01", 3, "B601", 0, "", "5");
			AssertAmendItem(export5ASHeader, "01", 3, "B602", 0, "", "BO");
			AssertAmendItem(export5ASHeader, "01", 3, "B301", 0, "", "100");
			AssertAmendItem(export5ASHeader, "01", 3, "B401", 0, "", "B");
			AssertAmendItem(export5ASHeader, "01", 3, "B402", 0, "", "002");
			AssertAmendItem(export5ASHeader, "01", 3, "B106", 0, "", "Y");
			AssertAmendItem(export5ASHeader, "01", 3, "B701", 0, "", "N");

			AssertAmendItem(export5ASHeader, "01", 3, "H101", 0, "", "A");
			AssertAmendItem(export5ASHeader, "01", 3, "H102", 0, "", "KR00101010000");
			AssertAmendItem(export5ASHeader, "01", 3, "H103", 0, "", "20200101");
			AssertAmendItem(export5ASHeader, "01", 3, "H104", 0, "", "20200901");

			AssertAmendItem(export5ASHeader, "01", 3, "C201", 1, "", "10");
			AssertAmendItem(export5ASHeader, "01", 3, "C202", 1, "", "KG");
			AssertAmendItem(export5ASHeader, "01", 3, "C203", 1, "", "200");
			AssertAmendItem(export5ASHeader, "01", 3, "C204", 1, "", "2000");
			AssertAmendItem(export5ASHeader, "01", 3, "C102", 1, "", "성분3");
			AssertAmendItem(export5ASHeader, "01", 3, "C101", 1, "", "모델규격3");
			AssertAmendItem(export5ASHeader, "01", 3, "C103", 1, "", "ZZZZEEE");
		}

		public void Test830EntryLineIsDeleted()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			entry.MergedLines[0].Delete();

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "02", 1, "B001", 0, "N", "Y");
		}

		public void TestInvoiceLine()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var entryLine = entry.MergedLines[0];
			var invoiceLine = entry.Declaration.Invoices[0].JobComInvoiceLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			invoiceLine.JI_SequenceNumber = 3;
			invoiceLine.JI_Description = "모델규격3";
			invoiceLine.JI_Ingredient = "성분3";
			invoiceLine.JI_LotNumber = "ZZZZEEE";
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_LinePrice = 2000m;

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(7, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "01", 1, "C201", 3, "", "10");
			AssertAmendItem(export5ASHeader, "01", 1, "C202", 3, "", "KG");
			AssertAmendItem(export5ASHeader, "01", 1, "C203", 3, "", "200");
			AssertAmendItem(export5ASHeader, "01", 1, "C204", 3, "", "2000");
			AssertAmendItem(export5ASHeader, "01", 1, "C102", 3, "", "성분3");
			AssertAmendItem(export5ASHeader, "01", 1, "C101", 3, "", "모델규격3");
			AssertAmendItem(export5ASHeader, "01", 1, "C103", 3, "", "ZZZZEEE");

			CreateLatestSnapshot(entry);
			invoiceLine.JI_Description = "모델규격4";
			invoiceLine.JI_Ingredient = "성분4";
			invoiceLine.JI_LotNumber = "EISHCMD";
			invoiceLine.JI_InvoiceUQ = "G";
			invoiceLine.JI_InvoiceQuantity = 30m;
			invoiceLine.JI_LinePrice = 3000m;

			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(7, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "03", 1, "C201", 3, "10", "30");
			AssertAmendItem(export5ASHeader, "03", 1, "C202", 3, "KG", "G");
			AssertAmendItem(export5ASHeader, "03", 1, "C203", 3, "200", "100");
			AssertAmendItem(export5ASHeader, "03", 1, "C204", 3, "2000", "3000");
			AssertAmendItem(export5ASHeader, "03", 1, "C102", 3, "성분3", "성분4");
			AssertAmendItem(export5ASHeader, "03", 1, "C101", 3, "모델규격3", "모델규격4");
			AssertAmendItem(export5ASHeader, "03", 1, "C103", 3, "ZZZZEEE", "EISHCMD");

			CreateLatestSnapshot(entry);
			entry.Declaration.Invoices[0].JobComInvoiceLines.RemoveAndDelete(invoiceLine);
			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "02", 1, "C001", 3, "N", "Y");
		}

		public void TestPreApprovalData()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();

			var declaration = entry.Declaration;
			var invoice = (JobComInvoiceHeader)declaration.Invoices.FirstOrDefault();
			var invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.PRA_ReferenceNumber = "KR00101010000";
			invoiceLine.PRA_DateOfIssue = new ZDateTime(2020, 01, 01);
			invoiceLine.PRA_DateOfExpiry = new ZDateTime(2020, 09, 01);

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(4, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "01", 1, "H101", 0, "", "A");
			AssertAmendItem(export5ASHeader, "01", 1, "H102", 0, "", "KR00101010000");
			AssertAmendItem(export5ASHeader, "01", 1, "H103", 0, "", "20200101");
			AssertAmendItem(export5ASHeader, "01", 1, "H104", 0, "", "20200901");

			CreateLatestSnapshot(entry);
			invoiceLine.PRA_ReferenceNumber = "KR00101010001";
			invoiceLine.PRA_DateOfIssue = new ZDateTime(2020, 01, 02);
			invoiceLine.PRA_DateOfExpiry = new ZDateTime(2020, 09, 02);
			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(3, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "03", 1, "H102", 0, "KR00101010000", "KR00101010001");
			AssertAmendItem(export5ASHeader, "03", 1, "H103", 0, "20200101", "20200102");
			AssertAmendItem(export5ASHeader, "03", 1, "H104", 0, "20200901", "20200902");

			CreateLatestSnapshot(entry);
			invoiceLine.PreApprovalCollection.RemoveAndDeleteAll();
			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "02", 1, "H001", 0, "N", "Y");
		}

		public void TestGAApprovalDocumentData()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			var approvalDocument = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument.CSI_Procedure = "05";
			approvalDocument.CSI_LineNo = 3;
			approvalDocument.CSI_SubType = "2";
			approvalDocument.CSI_DateOfIssue = new ZDateTime(2021, 08, 16);
			approvalDocument.CSI_Code = "A";
			approvalDocument.CSI_ReferenceNumber = "NO3";
			approvalDocument.CSI_Description = "식품등의 수입신고확인증3";
			approvalDocument.CSI_AdditionalDescription = "테스트3";
			approvalDocument.CSI_ReferenceNumber2 = "TEST3";
			approvalDocument.CSI_ReferenceNumber2 = "TEST3";
			approvalDocument.CSI_Status = "01";
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2021, 08, 16);

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(11, export5ASHeader.AmendmentItems.Length);
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G106", "", "05");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G101", "", "03");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G105", "", "20210816");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G107", "", "2");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G103", "", "A");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G102", "", "NO3");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G104", "", "식품등의 수입신고확인증3");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G108", "", "테스트3");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G109", "", "TEST3");
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G110", "", "05201");

			CreateLatestSnapshot(entry);
			approvalDocument.CSI_Procedure = "06";
			approvalDocument.CSI_SubType = "3";
			approvalDocument.CSI_DateOfIssue = new ZDateTime(2021, 08, 17);
			approvalDocument.CSI_Code = "B";
			approvalDocument.CSI_ReferenceNumber = "NO4";
			approvalDocument.CSI_Description = "식품등의 수입신고확인증4";
			approvalDocument.CSI_AdditionalDescription = "테스트4";
			approvalDocument.CSI_ReferenceNumber2 = "TEST4";
			approvalDocument.CSI_Status = "02";

			entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2021, 08, 17);

			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(10, export5ASHeader.AmendmentItems.Length);
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G106", "05", "06");
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G105", "20210816", "20210817");
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G107", "2", "3");
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G103", "A", "B");
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G102", "NO3", "NO4");
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G104", "식품등의 수입신고확인증3", "식품등의 수입신고확인증4");
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G108", "테스트3", "테스트4");
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G109", "TEST3", "TEST4");
			AssertApprovalDocument(export5ASHeader, "03", 1, 1, 3, "G110", "05201", "06302");

			CreateLatestSnapshot(entry);
			invoiceLine.GAApprovalDataCollection.RemoveAndDelete(approvalDocument);

			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertApprovalDocument(export5ASHeader, "02", 1, 1, 3, "G001", "N", "Y");
		}

		public void TestGAApprovalDocumentDataWithInvalidDate()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			var approvalDocument = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument.CSI_LineNo = 3;

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertApprovalDocument(export5ASHeader, "01", 1, 1, 3, "G101", "", "03");
		}

		public void TestVehicleNumberData()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();

			var declaration = entry.Declaration;
			var invoice = (JobComInvoiceHeader)declaration.Invoices.FirstOrDefault();
			var invoiceLine = invoice.InvoiceLines[0];
			var vehicleNumber = invoiceLine.VehicleNumbers.AddNew();
			vehicleNumber.CY_Order = 3;
			vehicleNumber.CY_Data = "12345678909876543";

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertVehicleNumber(export5ASHeader, "01", 1, 1, 3, "E102", "", "12345678909876543");

			CreateLatestSnapshot(entry);
			vehicleNumber.CY_Data = "98765432101234567";

			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertVehicleNumber(export5ASHeader, "03", 1, 1, 3, "E102", "12345678909876543", "98765432101234567");

			CreateLatestSnapshot(entry);
			invoiceLine.VehicleNumbers.RemoveAndDelete(vehicleNumber);

			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertVehicleNumber(export5ASHeader, "02", 1, 1, 3, "E001", "N", "Y");
		}

		public void TestContainerData()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();

			var declaration = entry.Declaration;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "799999999999999";

			var containerLink = entry.PivotsToContainers.AddNew();
			containerLink.CCE_CO_Container = container.PK;
			containerLink.CCE_SequenceNumber = 3;

			declaration.JE_ContainerPackMode = "AA";

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertContainer(export5ASHeader, 3, "D101", "", "799999999999999");

			CreateLatestSnapshot(entry);
			container.CO_ContainerNumber = "699999999999999";

			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertContainer(export5ASHeader, 3, "D101", "799999999999999", "699999999999999");

			CreateLatestSnapshot(entry);
			entry.PivotsToContainers.RemoveAndDelete(containerLink);
			declaration.CusContainers.Delete(container);

			amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(1, export5ASHeader.AmendmentItems.Length);
			AssertContainer(export5ASHeader, 3, "D001", "", "");
		}

		public void TestOrganisation()
		{
			#region Organisation
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA2", "레디코리아2");
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "김택윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120");

			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA3", "제조미상");
			TestOrgDataSetUpHelper.AddOrgAddress(manufacturer.MainAddress, "", "", "04784");

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA4", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");

			Factory.Save();
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = importer.PK;
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "EXP";
			entry.EntryNumber = "6N00221000025X";
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");

			CreateLatestSnapshot(entry);

			#region Change Organisation
			supplier.OH_FullName = "레디코리아 Test";
			supplier.Contacts[0].OC_ContactName = "RepresentativeName Test";
			supplier.MainAddress.OA_Address1 = "Address1 Test";
			supplier.MainAddress.OA_Address2 = "Address2 Test2";
			supplier.MainAddress.OA_PostCode = "12345";
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(IdentificationType.RoadNameCode, "232323", Core.Constants.CountryCodes.KoreaSouth);
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(IdentificationType.BuildingNumber, "131231", Core.Constants.CountryCodes.KoreaSouth);

			manufacturer.OH_FullName = "제조미상 Test";
			manufacturer.MainAddress.OA_PostCode = "98765";

			importer.OH_FullName = "Company Name TEST";

			Factory.Save();
			#endregion

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(10, export5ASHeader.AmendmentItems.Length);
			AssertHeader(export5ASHeader, "A301", "레디코리아2", "레디코리아 Test");
			AssertHeader(export5ASHeader, "A308", "김택윤", "Representati");
			AssertHeader(export5ASHeader, "A303", "서울특별시 서초구 동광로 41", "Address1 Test");
			AssertHeader(export5ASHeader, "A304", "레디인빌딩", "Address2 Test2");
			AssertHeader(export5ASHeader, "A305", "06561", "12345");
			AssertHeader(export5ASHeader, "A306", "101010", "232323");
			AssertHeader(export5ASHeader, "A307", "020120", "131231");

			AssertHeader(export5ASHeader, "A401", "제조미상", "제조미상 Test");
			AssertHeader(export5ASHeader, "A406", "04784", "98765");

			AssertHeader(export5ASHeader, "A501", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED", "Company Name TEST");
		}

		public void TestNotAmendableItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			var before = new GOVCBR830MessageBuilder(new ExportEntryHeaderCreator().Create(entry)).GenerateMessage();

			entry.EntryNumber = "6N00221000125X";

			declaration.BrokerAddress.CompanyName = "변경된 신고인상호";

			var manufacturer = declaration.Invoices[0].ManufacturerAddress;
			manufacturer.CustomsCodes[1].OK_CustomsRegNo = "990";

			manufacturer = declaration.Invoices[1].ManufacturerAddress;
			manufacturer.CustomsCodes[1].OK_CustomsRegNo = "990";

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_AdValoremTariff = "899999999";
			entryLine.CL_CustomsValue = 100m;
			declaration.Invoices[0].JobComInvoiceLines[0].JI_CL = entryLine.PK;
			declaration.InvoiceLines[0].JI_SequenceNumber = 101;
			declaration.InvoiceLines[1].JI_SequenceNumber = 102;

			entry.ResetTotalsAndCachedValues();

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());
			var after = new GOVCBR830MessageBuilder(new ExportEntryHeaderCreator().Create(entry)).GenerateMessage();

			AssertNotEquals(before.Id.Value, after.Id.Value);
			AssertAmendItemIsNull(export5ASHeader, "A101");

			AssertNotEquals(before.Submitter.Name.Value, after.Submitter.Name.Value);
			AssertAmendItemIsNull(export5ASHeader, "A102");
			AssertAmendItemIsNull(export5ASHeader, "A103");

			AssertNotEquals(before.GoodsShipment.ApprovedEstablishmentPlace.Name.Value, after.GoodsShipment.ApprovedEstablishmentPlace.Name.Value);
			AssertAmendItemIsNull(export5ASHeader, "A411");

			AssertNotEquals(before.GoodsItemQuantity.Value, after.GoodsItemQuantity.Value);
			AssertAmendItemIsNull(export5ASHeader, "AA01");

			AssertNotEquals(before.InvoiceAmount.Value, after.InvoiceAmount.Value);
			AssertAmendItemIsNull(export5ASHeader, "AA06");

			AssertNotEquals(before.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].SequenceNumeric, after.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].SequenceNumeric);
			AssertAmendItemIsNull(export5ASHeader, "E101");
		}

		public void TestRoundedAmendmentItem()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();

			var entryLine = entry.MergedLines[0];
			entryLine.CL_CustomsValue = 25987200.9965222m;
			var invoiceLine = entryLine.InvoiceLines[0];
			invoiceLine.Charges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OverseasFreight).J7_Amount = 4000.7893333m;
			invoiceLine.Charges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OverseasInsurance).J7_Amount = 3000.4984222m;
			invoiceLine.JI_NetWeight = 300.2984913m;
			invoiceLine.JI_CustomsQuantity = 40.5689272m;
			invoiceLine.JI_InvoiceQuantity = 3.1234915m;
			invoiceLine.UnitPrice = 10000.4898366m;
			invoiceLine.JI_LinePrice = 31236.4265228m;
			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_InvoiceAmount = 20000.5894987m;
			invoice.JZ_InvoiceCurrExRate = 1384.2576367m;
			invoice.JZ_Weight = 29000.5784612m;

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var export5ASHeader = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());

			AssertEquals(10, export5ASHeader.AmendmentItems.Length);
			AssertAmendItem(export5ASHeader, "", 0, "A703", 0, "1300", "4301");
			AssertAmendItem(export5ASHeader, "", 0, "A704", 0, "2500", "3501");
			AssertAmendItem(export5ASHeader, "", 0, "A706", 0, "27670", "20670.59");
			AssertAmendItem(export5ASHeader, "", 0, "AA02", 0, "29600", "29000.578");
			AssertAmendItem(export5ASHeader, "03", 1, "B201", 0, "360", "360.298");
			AssertAmendItem(export5ASHeader, "03", 1, "B203", 0, "1235", "1241");
			AssertAmendItem(export5ASHeader, "03", 1, "B301", 0, "25987293", "25987201");
			AssertAmendItem(export5ASHeader, "03", 1, "C201", 1, "1", "3.1235");
			AssertAmendItem(export5ASHeader, "03", 1, "C203", 1, "27670", "10000.483433");
			AssertAmendItem(export5ASHeader, "03", 1, "C204", 1, "27670", "31236.43");
		}

		void AssertHeader(ExportAmendmentHeader export5ASHeader, ZString amendDataItemID, ZString beforeDescription, ZString afterDescription)
		{
			var amendItem = export5ASHeader.AmendmentItems.Cast<Export5ASItem>().FirstOrDefault(x => x.IsHeaderItem() && x.AmendDataItemID == amendDataItemID);
			AssertNotNull(amendItem);
			AssertEquals(ZString.Empty, amendItem.LineAmendType);
			AssertEquals(beforeDescription, amendItem.BeforeDescription);
			AssertEquals(afterDescription, amendItem.AfterDescription);
		}

		void AssertAmendItem(ExportAmendmentHeader export5ASHeader, ZString amendType, ZInt entryLineNo, ZString amendDataItemID, ZInt lineDetailNo, ZString beforeDescription, ZString afterDescription)
		{
			var amendItem = export5ASHeader.AmendmentItems.Cast<Export5ASItem>().FirstOrDefault(x => ZInt.ParseEmptyAsZero(x.EntryLineNo) == entryLineNo && x.AmendDataItemID == amendDataItemID && ZInt.ParseEmptyAsZero(x.LineDetailNo) == lineDetailNo);
			AssertNotNull(amendItem);
			AssertEquals(amendType, amendItem.LineAmendType);
			AssertEquals(beforeDescription, amendItem.BeforeDescription);
			AssertEquals(afterDescription, amendItem.AfterDescription);
		}

		void AssertAmendItemIsNull(ExportAmendmentHeader export5ASHeader, ZString amendDataItemID)
		{
			var amendItem = export5ASHeader.AmendmentItems.Cast<Export5ASItem>().FirstOrDefault(x => x.IsHeaderItem() && x.AmendDataItemID == amendDataItemID);
			AssertNull(amendItem);
		}

		void AssertApprovalDocument(ExportAmendmentHeader export5ASHeader, ZString amendType, ZInt entryLineNo, ZInt lineDetailNo, ZInt approvalDocumentNo, ZString amendDataItemID, ZString beforeDescription, ZString afterDescription)
		{
			var amendItem = export5ASHeader.AmendmentItems.Cast<Export5ASItem>().FirstOrDefault(x => ZInt.ParseEmptyAsZero(x.EntryLineNo) == entryLineNo && ZInt.ParseEmptyAsZero(x.LineDetailNo) == lineDetailNo && ZInt.ParseEmptyAsZero(x.RegulationCategorySequnceNo) == approvalDocumentNo && x.AmendDataItemID == amendDataItemID);
			AssertNotNull(amendItem);
			AssertEquals(amendType, amendItem.LineAmendType);
			AssertEquals(beforeDescription, amendItem.BeforeDescription);
			AssertEquals(afterDescription, amendItem.AfterDescription);
		}

		void AssertVehicleNumber(ExportAmendmentHeader export5ASHeader, ZString amendType, ZInt entryLineNo, ZInt lineDetailNo, ZInt vinSeqNo, ZString amendDataItemID, ZString beforeDescription, ZString afterDescription)
		{
			var amendItem = export5ASHeader.AmendmentItems.Cast<Export5ASItem>().FirstOrDefault(x => ZInt.ParseEmptyAsZero(x.EntryLineNo) == entryLineNo && ZInt.ParseEmptyAsZero(x.LineDetailNo) == lineDetailNo && ZInt.ParseEmptyAsZero(x.VINSequenceNo) == vinSeqNo && x.AmendDataItemID == amendDataItemID);
			AssertNotNull(amendItem);
			AssertEquals(amendType, amendItem.LineAmendType);
			AssertEquals(beforeDescription, amendItem.BeforeDescription);
			AssertEquals(afterDescription, amendItem.AfterDescription);
		}

		void AssertContainer(ExportAmendmentHeader export5ASHeader, ZInt containerSeqNo, ZString amendDataItemID, ZString beforeDescription, ZString afterDescription)
		{
			var amendItem = export5ASHeader.AmendmentItems.Cast<Export5ASItem>().FirstOrDefault(x => ZInt.ParseEmptyAsZero(x.ContainerSequenceNo) == containerSeqNo && x.AmendDataItemID == amendDataItemID);
			AssertNotNull(amendItem);
			AssertEquals("LineAmendType", ZString.Empty, amendItem.LineAmendType);
			AssertEquals("before Description", beforeDescription, amendItem.BeforeDescription);
			AssertEquals("After Description", afterDescription, amendItem.AfterDescription);
		}

		void CreateLatestSnapshot(CusEntryHeader entry)
		{
			var export830 = new ExportEntryHeaderCreator().Create(entry);
			export830.RoundDecimalValueRoundedWithDecimalPlaces();
			using (var stream = KRXmlObjectSerializer.Serialize(export830))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				Factory.Save();
			}
		}

		public void TestExport5ASHeaderCreatorByInterfaceData()
		{
			var message = Factory.New<EDIMessage>();
			var fileReader = new TestFileReader(typeof(ExportAmendmentDetailsTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing", "GOVCBR5AS_Extend.xml");
			message.EM_MessageText = messageText;
			Factory.Save();

			CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS.Declaration messageDeclaration5AS = null;
			if (!string.IsNullOrEmpty(message.EM_MessageText))
			{
				using (var textReader = message.GetEM_MessageTextReader())
				{
					messageDeclaration5AS = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS.Declaration>(textReader);
				}
			}
			var export5ASHeader = new Export5ASHeaderCreator().Create(messageDeclaration5AS);
			AssertEquals("040", export5ASHeader.DeclarationCustomsOffice);
			AssertEquals("15", export5ASHeader.DeclarationCustomsDivision);
			AssertEquals("1234520100523X", export5ASHeader.ExportDeclarationNumber);
			AssertEquals(null, export5ASHeader.AmendmentItems[0].LineAmendType);
			AssertEquals("20201230", export5ASHeader.AmendmentItems[0].BeforeDescription);
			AssertEquals("20201231", export5ASHeader.AmendmentItems[0].AfterDescription);
			AssertEquals("1", export5ASHeader.AmendmentItems[0].EntryLineNo);
			AssertEquals("111111", export5ASHeader.AmendmentItems[0].LineDetailNo);
			AssertEquals("A608", export5ASHeader.AmendmentItems[0].AmendDataItemID);
			AssertEquals("0", export5ASHeader.AmendmentItems[0].ContainerSequenceNo);
			AssertEquals("0", export5ASHeader.AmendmentItems[0].VINSequenceNo);
			AssertEquals("(주)레디코리아", export5ASHeader.Exporter.CompanyName);
			AssertEquals("레디코리1971018", export5ASHeader.Exporter.UnipassIDForOrganization);
			AssertEquals("신고인부호", export5ASHeader.UnipassDeclarantID);
		}

		public void TestNotAmendableItemCodeLists()
		{
			Assert(!DataItemIDList.ContainsCode("A101"));
			Assert(!DataItemIDList.ContainsCode("A102"));
			Assert(!DataItemIDList.ContainsCode("A103"));
			Assert(!DataItemIDList.ContainsCode("A411"));
			Assert(!DataItemIDList.ContainsCode("AA01"));
			Assert(!DataItemIDList.ContainsCode("AA06"));
			Assert(!DataItemIDList.ContainsCode("E101"));
		}

		CodeDescriptionPairList DataItemIDList => Factory.GetCachedValue<ExportAmendmentDataItemIDList>();
	}
}
