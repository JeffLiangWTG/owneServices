using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class CusEntryLineTest : TestCaseWithFactory
	{
		public void TestDomesticRegion()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CIQDT", "101011", "Region1");
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CIQDT", "101022", "Region2");
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeaderItems.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			testInvoiceLine.JI_DestinationRegion = "101011";
			testInvoiceLine.JI_OriginRegion = "101022";
			AssertEquals("101011", testCusEntryLine.DomesticRegionCode);
			AssertEquals("Region1", testCusEntryLine.DomesticRegionName);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeaderItems.EntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("101022", testCusEntryLine.DomesticRegionCode);
			AssertEquals("Region2", testCusEntryLine.DomesticRegionName);
		}

		public void TestDomesticDistrict()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "DISTR", "NJ", "南京");
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "DISTR", "LD", "伦敦");
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeaderItems.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			testInvoiceLine.JI_DestinationDistrict = "NJ";
			testInvoiceLine.JI_OriginDistrict = "LD";
			AssertEquals("NJ", testCusEntryLine.DomesticDistrictCode);
			AssertEquals("南京", testCusEntryLine.DomesticDistrictName);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeaderItems.EntryHeader, true, EntryTypeList.Codes.RecordListing);
			AssertEquals("NJ", testCusEntryLine.DomesticDistrictCode);
			AssertEquals("南京", testCusEntryLine.DomesticDistrictName);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeaderItems.EntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			testInvoiceLine.JI_DestinationDistrict = "NJ";
			testInvoiceLine.JI_OriginDistrict = "LD";
			AssertEquals("LD", testCusEntryLine.DomesticDistrictCode);
			AssertEquals("伦敦", testCusEntryLine.DomesticDistrictName);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeaderItems.EntryHeader, false, EntryTypeList.Codes.RecordListing);
			AssertEquals("LD", testCusEntryLine.DomesticDistrictCode);
			AssertEquals("伦敦", testCusEntryLine.DomesticDistrictName);
		}

		public void TestEntryLineNo()
		{
			testCusEntryLine.CL_LineNumber = 10001;
			AssertEquals((short)10001, testCusEntryLine.EntryLineNo);
		}

		public void TestProductManualNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			instruction2.CEI_CEI_Parent = instruction1.PK;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			invoiceLine.JI_ProductManualNo = 1;
			invoiceLine.JI_ProductManualNo2 = 2;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var cusEntryHeader = declaration.CustomsEntryHeaders.First(x => x.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			var recEntryHeader = declaration.CustomsEntryHeaders.First(x => x.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals("ProductManualNo for CUS entry", (ZInt)1, cusEntryHeader.MergedLines[0].ProductManualNo);
			AssertEquals("ProductManualNo for REC entry", (ZInt)2, recEntryHeader.MergedLines[0].ProductManualNo);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.DoMerge();
			cusEntryHeader = declaration.CustomsEntryHeaders.First(x => x.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			recEntryHeader = declaration.CustomsEntryHeaders.First(x => x.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals("ProductManualNo for REC entry", (ZInt)1, recEntryHeader.MergedLines[0].ProductManualNo);
			AssertEquals("ProductManualNo for CUS entry", (ZInt)2, cusEntryHeader.MergedLines[0].ProductManualNo);
		}

		public void TestTariff()
		{
			testInvoiceLine.JI_Tariff = "12345678";
			AssertEquals("12345678", testCusEntryLine.TariffCode);
			testInvoiceLine.JI_Tariff = "1234567891";
			AssertEquals("1234567891", testCusEntryLine.TariffCode);
		}

		public void TestCIQTariff()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff).PK;
			Factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "2009891200101", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "未混合芒果汁");
			Factory.Save();
			AssertEquals("", testCusEntryLine.CIQTariffCode);
			AssertEquals("", testCusEntryLine.CIQSupplementCode);
			AssertEquals("", testCusEntryLine.CIQTariffDescription);
			testInvoiceLine.JI_CIQTariff = "2009891200101";
			AssertEquals("2009891200101", testCusEntryLine.CIQTariffCode);
			AssertEquals("101", testCusEntryLine.CIQSupplementCode);
			AssertEquals("未混合芒果汁", testCusEntryLine.CIQTariffDescription);
		}

		public void TestNameOfGoods()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000");
			Factory.Save();
			var entryHeader = testCusEntryHeaderItems.EntryHeader;
			var entryLine = testCusEntryHeaderItems.EntryLine;
			var invoiceHeader = testCusEntryHeaderItems.InvoiceHeader;
			var invoiceLine = testCusEntryHeaderItems.InvoiceLine;
			entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.JI_NameOfGoods = "产品A";
			AssertEquals("NameOfGoods", "产品A", testCusEntryLine.NameOfGoods);
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_NameOfGoods = "产品C";
			entryLine.InvoiceLines.Add(invoiceLine2);
			AssertEquals("NameOfGoods", "产品A等", testCusEntryLine.NameOfGoods);
		}

		public void TestGoodsSpecModel()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "99997", "00423", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("99997", "包装规格");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var entryHeader = testCusEntryHeaderItems.EntryHeader;
			var entryLine = testCusEntryHeaderItems.EntryLine;
			var invoiceHeader = testCusEntryHeaderItems.InvoiceHeader;
			var invoiceLine = testCusEntryHeaderItems.InvoiceLine;
			entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX|无其他";
			invoiceLine.XC_GoodsSpecModel2 = "2千克/箱";
			invoiceLine.JI_Tariff = "2713200000";
			AssertEquals("GoodsSpecModel", "1千克/箱|XXXXX|无其他", testCusEntryLine.GoodsSpecModel);
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_Tariff = "2713200000";
			invoiceLine2.XC_GoodsSpecModel = "|LLLLL|其他";
			entryLine.InvoiceLines.Add(invoiceLine2);
			AssertEquals("GoodsSpecModel", "1千克/箱|XXXXX/LLLLL|其他", testCusEntryLine.GoodsSpecModel);
		}

		public void TestQuantities()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "035", "千克", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "002", "座", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "003", "辆", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			testInvoiceLine.JI_TradeQuantity = 111.11m;
			testInvoiceLine.JI_TradeUnitQty = "035";
			testInvoiceLine.JI_CustomsQuantity = 1m;
			testInvoiceLine.JI_CustomsUnitQty = "002";
			testInvoiceLine.JI_CustomsSecondQuantity = 2m;
			testInvoiceLine.JI_CustomsSecondUnitQty = "003";
			var newInvoiceLine = testCusEntryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
			newInvoiceLine.JI_TradeQuantity = 111.11m;
			newInvoiceLine.JI_TradeUnitQty = "035";
			newInvoiceLine.JI_CustomsQuantity = 2m;
			newInvoiceLine.JI_CustomsUnitQty = "002";
			newInvoiceLine.JI_CustomsSecondQuantity = 3m;
			newInvoiceLine.JI_CustomsSecondUnitQty = "003";
			AssertEquals(222.22m, testCusEntryLine.TradeQuantity);
			AssertEquals("035", testCusEntryLine.TradeUnitQty);
			AssertEquals("千克", testCusEntryLine.TradeUnitQtyDesc);
			AssertEquals(3m, testCusEntryLine.CustomsQuantity);
			AssertEquals("002", testCusEntryLine.CustomsUnitQty);
			AssertEquals("座", testCusEntryLine.CustomsUnitQtyDescription);
			AssertEquals(5m, testCusEntryLine.CustomsSecondQuantity);
			AssertEquals("003", testCusEntryLine.CustomsSecondUnit);
			AssertEquals("辆", testCusEntryLine.CustomsSecondUnitDesc);
		}

		public void TestGoodsOriginOrDest()
		{
			var entryHeader = testCusEntryHeaderItems.EntryHeader;
			var invoiceLine = testCusEntryHeaderItems.InvoiceLine;
			invoiceLine.JI_CountryOfOrigin = "GB";
			invoiceLine.JI_RN_NKCountryOfExport = "US";
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(entryHeader, true, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("GBR", testCusEntryLine.GoodsOriginCode);
			AssertEquals("英国", testCusEntryLine.GoodsOriginName);
			AssertEquals("USA", testCusEntryLine.GoodsDestCode);
			AssertEquals("美国", testCusEntryLine.GoodsDestName);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(entryHeader, false, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("GBR", testCusEntryLine.GoodsOriginCode);
			AssertEquals("英国", testCusEntryLine.GoodsOriginName);
			AssertEquals("USA", testCusEntryLine.GoodsDestCode);
			AssertEquals("美国", testCusEntryLine.GoodsDestName);
		}

		public void TestUnitPrice()
		{
			var invoiceHeader = testCusEntryHeaderItems.InvoiceHeader;
			invoiceHeader.JZ_RX_NKInvoice_Currency = testCusEntryHeaderItems.JobDeclaration.LocalCurrencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine = testCusEntryHeaderItems.InvoiceLine;
			invoiceLine.JI_LinePrice = 50.01m;
			invoiceLine.JI_TradeQuantity = 1;
			AssertEquals(50.01m, testCusEntryLine.UnitPrice);
		}

		public void TestTotalPrice()
		{
			var invoiceHeader = testCusEntryHeaderItems.InvoiceHeader;
			invoiceHeader.JZ_RX_NKInvoice_Currency = testCusEntryHeaderItems.JobDeclaration.LocalCurrencyCode;
			var invoiceLine = testCusEntryHeaderItems.InvoiceLine;
			invoiceLine.JI_LinePrice = 50.01m;
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsStatisticalValueApplicable = false;
			charge.J7_Amount = 2m;
			var charge1 = invoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = "OTH";
			charge1.J7_IsDutiable = true;
			charge1.J7_IsGSTApplicable = false;
			charge1.J7_Amount = 5m;
			var invoice2 = testCusEntryHeaderItems.JobDeclaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_RX_NKInvoice_Currency = testCusEntryHeaderItems.JobDeclaration.LocalCurrencyCode;
			var invoiceLine2 = testCusEntryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_JZ = invoice2.PK;
			invoiceLine2.JI_LinePrice = 51.11m;
			var charge2 = invoiceLine2.Charges.AddNew();
			charge2.J7_ChargeType = "ONS";
			charge.J7_IsDutiable = true;
			charge.J7_IsGSTApplicable = false;
			charge.J7_IsStatisticalValueApplicable = false;
			charge2.J7_Amount = 8m;
			var charge3 = invoiceLine2.Charges.AddNew();
			charge3.J7_ChargeType = "OTH";
			charge3.J7_IsDutiable = false;
			charge3.J7_IsGSTApplicable = true;
			charge3.J7_Amount = 11m;
			var charge4 = invoiceLine2.Charges.AddNew();
			charge4.J7_ChargeType = "OFT";
			charge4.J7_IsDutiable = false;
			charge4.J7_IsGSTApplicable = true;
			charge4.J7_IsStatisticalValueApplicable = false;
			charge4.J7_Amount = 12m;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals(132.12m, testCusEntryLine.TotalPrice);
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals(108.12m, testCusEntryLine.TotalPrice);
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			AssertEquals(120.12m, testCusEntryLine.TotalPrice);
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			AssertEquals(116.12m, testCusEntryLine.TotalPrice);
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			AssertEquals(108.12m, testCusEntryLine.TotalPrice);
		}

		public void TestCurrency()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CURR", "GBP", "英镑");
			var invoiceHeader = testCusEntryHeaderItems.InvoiceHeader;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "GBP";
			AssertEquals("GBP", testCusEntryLine.CurrencyCode);
			AssertEquals("英镑", testCusEntryLine.CurrencyDesc);
		}

		public void TestDutyMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			instruction2.CEI_CEI_Parent = instruction1.PK;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			invoiceLine.JI_DutyMode = DutyModeList.Codes._1;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var cusEntryHeader = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			var recEntryHeader = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals("ProductManualNo for CUS entry", (ZString)"1", cusEntryHeader.MergedLines[0].DutyModeCode);
			AssertEquals("ProductManualNo for CUS entry", (ZString)"照章征税", cusEntryHeader.MergedLines[0].DutyModeDesc);
			AssertEquals("ProductManualNo for REC entry", (ZString)"3", recEntryHeader.MergedLines[0].DutyModeCode);
			AssertEquals("ProductManualNo for REC entry", (ZString)"全免", recEntryHeader.MergedLines[0].DutyModeDesc);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.DoMerge();
			cusEntryHeader = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			recEntryHeader = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals("ProductManualNo for CUS entry", (ZString)"1", cusEntryHeader.MergedLines[0].DutyModeCode);
			AssertEquals("ProductManualNo for CUS entry", (ZString)"照章征税", cusEntryHeader.MergedLines[0].DutyModeDesc);
			AssertEquals("ProductManualNo for REC entry", (ZString)"3", recEntryHeader.MergedLines[0].DutyModeCode);
			AssertEquals("ProductManualNo for REC entry", (ZString)"全免", recEntryHeader.MergedLines[0].DutyModeDesc);
		}

		public void TestProcudtCode()
		{
			var invoiceLine = testCusEntryHeaderItems.InvoiceLine;
			invoiceLine.JI_PartNo = "PROCUDTCD";
			AssertEquals("PROCUDTCD", testCusEntryLine.ProductCode);
		}

		public void TestProcuctVersion()
		{
			var invoiceLine = testCusEntryHeaderItems.InvoiceLine;
			invoiceLine.JI_ProductVersion = "PRODVER";
			AssertEquals("PRODVER", testCusEntryLine.ProductVersion);
		}

		public void TestCertOfOriginFields()
		{
			testInvoiceLine.Declaration.JE_MessageType = "IMP";
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CNPTA", "AU1", "CN-AU free trade agreement 1", ("ApplicableCountry", "AU"));
			testInvoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			testInvoiceLine.CertificateOfOriginType = "C";
			testInvoiceLine.CertificateOfOrigin = "C12345678";
			testInvoiceLine.TradeAgreementCode = "AU1";
			testInvoiceLine.ItemNoOnCertOfOrigin = 2;
			testInvoiceLine.CertificateOfOriginCountry = "AU";

			CombineAssertions("Cert Of Origin Fields", () =>
			{
				AssertEquals("CertOfOriginNumber", "C12345678", testCusEntryLine.CertOfOriginNumber);
				AssertEquals("TradeAgreementCode", "AU1", testCusEntryLine.TradeAgreementCode);
				AssertEquals("TradeAgreementDesctiption", "CN-AU free trade agreement 1", testCusEntryLine.TradeAgreementDesctiption);
				AssertEquals("ItemNoOnCertOfOrigin", new ZShort(2), testCusEntryLine.ItemNoOnCertOfOrigin);
				AssertEquals("CertOfOriginType", "C", testCusEntryLine.CertOfOriginType);
				AssertEquals("CertOfOriginTypeDescription", "原产地证书", testCusEntryLine.CertOfOriginTypeDescription);
				AssertEquals("CertOfOriginCountry", "AU", testCusEntryLine.CertOfOriginCountry);
			});
		}

		public void TestCusEntryWithIMPAndCIF()
		{
			var entryHeader = SetupCusEntryWithIMPAndCIF();
			AssertNotNull(entryHeader);
			AssertEquals(entryHeader.IncoTermCode, "1");
			AssertEquals(3, entryHeader.AllEntryLines.Count);
			CombineAssertions(() =>
			{
				Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 32.69 && x.UnitPrice == 16.345));
				Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 32.69 && x.UnitPrice == 10.8966));
				Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 43.58 && x.UnitPrice == 14.5266));
			});
		}

		public void TestCusEntryWithIMPAndFOB()
		{
			var entryHeader = SetupCusEntryWithIMPAndFOB();
			CombineAssertions(() =>
			{
				AssertNotNull(entryHeader);
				AssertEquals(entryHeader.IncoTermCode, "3");
				AssertEquals(3, entryHeader.AllEntryLines.Count);
				Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30 && x.UnitPrice == 15));
				Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30 && x.UnitPrice == 15));
				Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 40 && x.UnitPrice == 13.3333));
			});
		}

		public void TestCusEntryWithIMPAndCFR()
		{
			var entryHeader = SetupCusEntryWithIMPAndCFR();
			AssertNotNull(entryHeader);
			AssertEquals(entryHeader.IncoTermCode, "2");
			AssertEquals(3, entryHeader.AllEntryLines.Count);
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30 && x.UnitPrice == 15));
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30 && x.UnitPrice == 10));
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 40 && x.UnitPrice == 13.3333));
		}

		public void TestCusEntryWithEXPAndCIF()
		{
			var entryHeader = SetupCusEntryWithEXPAndCIF();
			AssertNotNull(entryHeader);
			AssertEquals(entryHeader.IncoTermCode, "1");
			AssertEquals(3, entryHeader.AllEntryLines.Count);
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30.3 && x.UnitPrice == 15.15));
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30.3 && x.UnitPrice == 10.1));
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 40.4 && x.UnitPrice == 13.4666));
		}

		public void TestCusEntryWithEXPAndFOB()
		{
			var entryHeader = SetupCusEntryWithEXPAndFOB();
			AssertNotNull(entryHeader);
			AssertEquals(entryHeader.IncoTermCode, "3");
			AssertEquals(3, entryHeader.AllEntryLines.Count);
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30.3 && x.UnitPrice == 15.15));
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30.3 && x.UnitPrice == 10.1));
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 40.4 && x.UnitPrice == 13.4666));
		}

		public void TestCusEntryWithEXPAndCFR()
		{
			var entryHeader = SetupCusEntryWithEXPAndCFR();
			AssertNotNull(entryHeader);
			AssertEquals(entryHeader.IncoTermCode, "2");
			AssertEquals(3, entryHeader.AllEntryLines.Count);
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30.30 && x.UnitPrice == 15.15));
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 30.30 && x.UnitPrice == 10.10));
			Assert(entryHeader.AllEntryLines.Cast<CusEntryLine>().Any(x => x.TotalPrice == 40.40 && x.UnitPrice == 13.4666));
		}

		public CusEntryHeader SetupCusEntryWithIMPAndCIF()
		{
			SetupRefCusMapAndRefCusMapType();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = "CIF";
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			SetupInvoiceLines(invoiceHeader, instruction);
			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();
			var charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 4;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge.J7_DistributeBy = "VAL";
			charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_Amount = 5;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_Percentage = 5;
			charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "RYT";
			charge.J7_Amount = 1;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_DistributeBy = "VAL";
			Factory.Save();
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(notifier);
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			return entryHeader;
		}

		public CusEntryHeader SetupCusEntryWithIMPAndFOB()
		{
			SetupRefCusMapAndRefCusMapType();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			SetupInvoiceLines(invoiceHeader, instruction);
			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();
			var charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 4;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_FullOrPartialApportionment = "PAA";
			charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Percentage = 5;
			charge.J7_DistributeBy = "VAL";
			charge.J7_FullOrPartialApportionment = "PAA";
			charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "RYT";
			charge.J7_Amount = 1;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_DistributeBy = "VAL";
			charge.J7_FullOrPartialApportionment = "PAA";
			Factory.Save();
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(notifier);
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			return entryHeader;
		}

		public CusEntryHeader SetupCusEntryWithIMPAndCFR()
		{
			SetupRefCusMapAndRefCusMapType();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = "CFR";
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			SetupInvoiceLines(invoiceHeader, instruction);
			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();
			var charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 4;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsIncludedInITOT = true;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge.J7_DistributeBy = "VAL";
			charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_Amount = 5;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_Percentage = 5;
			charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "RYT";
			charge.J7_Amount = 1;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_DistributeBy = "VAL";
			Factory.Save();
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(notifier);
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			return entryHeader;
		}

		public CusEntryHeader SetupCusEntryWithEXPAndCIF()
		{
			SetupRefCusMapAndRefCusMapType();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = "CIF";
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			SetupInvoiceLines(invoiceHeader, instruction);
			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();
			var charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 4;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge.J7_DistributeBy = "VAL";
			charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_Amount = 5;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_Percentage = 5;
			charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "OTH";
			charge.J7_Amount = 1;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsDutiable = true;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge.J7_DistributeBy = "VAL";
			Factory.Save();
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(notifier);
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			return entryHeader;
		}

		public CusEntryHeader SetupCusEntryWithEXPAndFOB()
		{
			SetupRefCusMapAndRefCusMapType();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			SetupInvoiceLines(invoiceHeader, instruction);
			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();
			var charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 4;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_FullOrPartialApportionment = "PAA";
			charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Percentage = 5;
			charge.J7_DistributeBy = "VAL";
			charge.J7_FullOrPartialApportionment = "PAA";
			charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "OTH";
			charge.J7_Amount = 1;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsDutiable = true;
			charge.J7_IsGSTApplicable = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_FullOrPartialApportionment = "PAA";
			Factory.Save();
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(notifier);
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			return entryHeader;
		}

		public CusEntryHeader SetupCusEntryWithEXPAndCFR()
		{
			SetupRefCusMapAndRefCusMapType();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = "CFR";
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			SetupInvoiceLines(invoiceHeader, instruction);
			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();
			var charge = invoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_Amount = 5;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_Percentage = 5;
			var charge1 = invoiceHeader.Charges.AddNew();
			charge1.J7_ChargeType = "OTH";
			charge1.J7_Amount = 1;
			charge1.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge1.J7_IsIncludedInITOT = false;
			charge1.J7_IsDutiable = true;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge1.J7_DistributeBy = "VAL";
			var charge2 = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge2.J7_ChargeType = "OFT";
			charge2.J7_Amount = 4;
			charge2.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge2.J7_IsDutiable = false;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_DistributeBy = "VAL";
			charge2.J7_FullOrPartialApportionment = "PAA";
			Factory.Save();
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(notifier);
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			return entryHeader;
		}

		public void TestTypeOfFees()
		{
			AssertEquals("Fees' type should be expected.", typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>), Factory.New<CusEntryLine>().Fees.GetType());
		}

		void SetupInvoiceLines(JobComInvoiceHeader invoiceHeader, CusEntryInstruction instruction)
		{
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_TradeQuantity = 2;
			invoiceLine.JI_LinePrice = 30;
			invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LineNo = 2;
			invoiceLine.JI_TradeQuantity = 3;
			invoiceLine.JI_LinePrice = 30;
			invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LineNo = 3;
			invoiceLine.JI_TradeQuantity = 3;
			invoiceLine.JI_LinePrice = 40;
		}

		void SetupRefCusMapAndRefCusMapType()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("CURR", "BTH", "BTH", true);
			helper.CreateCusMapType("CURR", "OUT", "OUT", true);
			helper.CreateCusMap("CURR", "CNY", "142", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), Core.Constants.CountryCodes.China);
			helper.CreateCusMap("CURR", "USD", "101", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), Core.Constants.CountryCodes.China);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			testCusEntryHeaderItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				Factory.Save();
			});
			testCusEntryLine = testCusEntryHeaderItems.EntryLine;
			testInvoiceLine = testCusEntryHeaderItems.InvoiceLine;
		}

		CNEntryHeaderTestData testCusEntryHeaderItems;
		CusEntryLine testCusEntryLine;
		JobComInvoiceLine testInvoiceLine;
	}
}
