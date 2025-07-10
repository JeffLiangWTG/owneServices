using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRD87DataProvidersTest : XMLMessageTestHelper<GOVCBRD87DataProvidersTest>
	{
		public const string BUS = OrgConstants.Category.Business;
		public const string PUB = StmNoteDescription.Pub;
		public const string MB = BillTypeList.Codes.MasterBill;
		public const string HB = BillTypeList.Codes.HouseBill;

		[TestDate(2021, 04, 06)]
		public void TestSerialisationAndDeserialisationRealData()
		{
			#region OrgHeader
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, BUS, "BBB", "Festel");
			supplier.OH_IsConsignor = true;
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 동광로 41");

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, BUS, "CCC", "케이피엘써키트(주)");
			TestOrgDataSetUpHelper.AddOrgAddress(importer.MainAddress, "인천 서구 가좌동");
			importer.MainAddress.OA_Phone = "032-583-6480";
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_ExportGoodsType = "C";
			declaration.JE_TotalNoOfPieces = 2;
			declaration.JE_TotalWeight = 20;
			declaration.JE_TotalWeightUnit = "M";
			declaration.JE_TotalNoOfPacks = 2;
			declaration.JE_TotalNoOfPacksPackType = "BL";
			declaration.JE_LocationOtherInformation = "01002038";
			declaration.JE_AgentsReference = "US899916191";
			declaration.JE_EntryDate = new ZDate(2021, 12, 16);

			var declarationNote = declaration.Notes.AddNew();
			declarationNote.ST_NoteType = PUB;
			declarationNote.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			declarationNote.ST_NoteText = "VISAGE COSMETIC SURGERY SYSTEM";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = MB;
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			masterBill.CU_BillSeqNo = "0180";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillType = HB;
			houseBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			houseBill.CU_BillSeqNo = "001";

			var blNo = declaration.DeclarationRefs.AddNew();
			blNo.J3_ReferenceType = Constants.MRN;
			blNo.J3_ReferenceNumber = "20HDMUA820I4";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000000m;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice.JZ_ImportCargoManagementNumber = "20HDMUA820I40180001";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._D87);
			entryNum.CE_IssueDate = new ZDateTime(2021, 04, 06);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");

			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var importD87Header = new ImportD87HeaderCreator().Create(entry);
			Factory.Save();

			var result = new GOVCBRD87MessageBuilder(importD87Header).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBRD87DataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBRD87_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}

			AssertEquals(false, importD87Header.HouseBillSplitDeclarationIndicator);
			AssertEquals("US899916191", importD87Header.CarnetCertificateNumber);
			AssertEquals("VISAGE COSMETIC SURGERY SYSTEM", importD87Header.RepresentativeProductName);
			AssertEquals("01002038", importD87Header.BondedAreaCode);
			AssertEquals("C", importD87Header.CarnetUseCode);
			AssertEquals("010", importD87Header.DeclarationCustomsOffice);
			AssertEquals("10", importD87Header.DeclarationCustomsDivision);
			AssertEquals("20211216", importD87Header.EffectiveToDate.ToString("yyyyMMdd"));
			AssertEquals(3000000m, importD87Header.TotalInvoiceAmount);
			AssertEquals("KRW", importD87Header.InvoiceCurrency);
			AssertEquals(2m, importD87Header.TotalQty);
			AssertEquals(20m, importD87Header.TotalGrossWeight);
			AssertEquals("M", importD87Header.TotalGrossWeighUnit);
			AssertEquals(2m, importD87Header.TotalPackQty);
			AssertEquals("BL", importD87Header.PackType);
			AssertEquals("20HDMUA820I40180001", importD87Header.CargoManagementNo);
			AssertEquals("Festel", importD87Header.Supplier.CompanyName);
			AssertEquals("케이피엘써키트(주)", importD87Header.Importer.CompanyName);
			AssertEquals("인천 서구 가좌동", importD87Header.Importer.AddressLine1);
			AssertEquals("", importD87Header.Importer.AddressLine2);
			AssertEquals("032-583-6480", importD87Header.Importer.PhoneNumber);
			AssertEquals("6N002", importD87Header.UnipassDeclarantID);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		[TestDate(1999, 12, 27)]
		public void TestSerialisationAndDeserialisation()
		{
			#region OrgHeader	
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, BUS, "BBB", "명의인명");
			supplier.OH_IsConsignor = true;
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울시 서초구 동광로 빌딩 123");

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, BUS, "CCC", "수입자상호");
			TestOrgDataSetUpHelper.AddOrgAddress(importer.MainAddress, "수입자주소");
			importer.MainAddress.OA_Phone = "02-541-1834";
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_ExportGoodsType = "A";
			declaration.JE_TotalNoOfPieces = 9;
			declaration.JE_TotalWeight = 99;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 999;
			declaration.JE_TotalNoOfPacksPackType = "CT";
			declaration.JE_LocationOtherInformation = "A1234567";
			declaration.JE_AgentsReference = "US899916191";
			declaration.JE_EntryDate = new ZDate(2013, 12, 12);

			var declarationNote = declaration.Notes.AddNew();
			declarationNote.ST_NoteType = PUB;
			declarationNote.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			declarationNote.ST_NoteText = "VISAGE COSMETIC SURGERY SYSTEM";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			masterBill.CU_BillSeqNo = "0000";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;
			houseBill.CU_BillSeqNo = "000";

			var blNo = declaration.DeclarationRefs.AddNew();
			blNo.J3_ReferenceType = Constants.MRN;
			blNo.J3_ReferenceNumber = "00000000";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 9999m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice.JZ_ImportCargoManagementNumber = "0000000000000000";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._D87);
			entryNum.CE_IssueDate = new ZDateTime(1999, 12, 27);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var importD87Header = new ImportD87HeaderCreator().Create(entry);
			Factory.Save();

			var result = new GOVCBRD87MessageBuilder(importD87Header).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBRD87DataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBRD87_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}

			AssertEquals(true, importD87Header.HouseBillSplitDeclarationIndicator);
			AssertEquals("US899916191", importD87Header.CarnetCertificateNumber);
			AssertEquals("VISAGE COSMETIC SURGERY SYSTEM", importD87Header.RepresentativeProductName);
			AssertEquals("A1234567", importD87Header.BondedAreaCode);
			AssertEquals("A", importD87Header.CarnetUseCode);
			AssertEquals("010", importD87Header.DeclarationCustomsOffice);
			AssertEquals("10", importD87Header.DeclarationCustomsDivision);
			AssertEquals("20131212", importD87Header.EffectiveToDate.ToString("yyyyMMdd"));
			AssertEquals(9999m, importD87Header.TotalInvoiceAmount);
			AssertEquals("USD", importD87Header.InvoiceCurrency);
			AssertEquals(9m, importD87Header.TotalQty);
			AssertEquals(99m, importD87Header.TotalGrossWeight);
			AssertEquals("KG", importD87Header.TotalGrossWeighUnit);
			AssertEquals(999m, importD87Header.TotalPackQty);
			AssertEquals("CT", importD87Header.PackType);
			AssertEquals("0000000000000000", importD87Header.CargoManagementNo);
			AssertEquals("명의인명", importD87Header.Supplier.CompanyName);
			AssertEquals("수입자상호", importD87Header.Importer.CompanyName);
			AssertEquals("수입자주소", importD87Header.Importer.AddressLine1);
			AssertEquals("", importD87Header.Importer.AddressLine2);
			AssertEquals("02-541-1834", importD87Header.Importer.PhoneNumber);
			AssertEquals("12345", importD87Header.UnipassDeclarantID);
		}

		public void TestWhenJE_EntryDateIsEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Carnet;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertNoExceptionThrown(() => new ImportD87HeaderCreator().Create(entry));
		}
	}
}
