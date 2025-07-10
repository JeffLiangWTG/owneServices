using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class AcdAgrRequestImportInfoProviderTest : TestCaseWithFactory
	{
		public void TestNameOfMainGoods()
		{
			AssertEquals("Should be empty when there is no EntryLine", string.Empty, ImportInfo.NameOfMainGoods);
			TestData.InvoiceLine.JI_NameOfGoods = "Car";
			AssertEquals("Should be NameOfMainGoods of EntryLine", "Car", ImportInfoWithTestData.NameOfMainGoods);
		}

		public void TestTariffCode()
		{
			AssertEquals("Should be empty when there is no EntryLine", string.Empty, ImportInfo.TariffCode);
			TestData.InvoiceLine.JI_Tariff = "3005109000";
			AssertEquals("Should be TariffCode of EntryLine", "3005109000", ImportInfoWithTestData.TariffCode);
		}

		public void TestTotalPriceOfGoods()
		{
			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var exchangeRate = usdCurrency.ExchangeRates.FirstOrDefault(x => x.RE_RX_NKExCurrency == usdCurrency.Code && x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate && x.RE_GC == GlbCompany.CurrentCompany.PK && x.RE_StartDate <= ZDateTime.Today && x.RE_ExpiryDate >= ZDateTime.Today);
			if (exchangeRate == null)
			{
				exchangeRate = usdCurrency.ExchangeRates.AddNew();
				exchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = ZDateTime.Today.AddMonths(-1);
				exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			}
			exchangeRate.RE_SellRate = 7.32m;

			var invoice1 = TestData.InvoiceHeader;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine1 = TestData.InvoiceLine;
			invoiceLine1.JI_LinePrice = 50m;
			var entryLine2 = TestData.EntryHeader.MergedLines.AddNew();
			var invoice2 = TestData.JobDeclaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_LinePrice = 100m;
			AssertEquals("Should be 0 when CurrencyCode is empty", 0m, ImportInfo.TotalPriceOfGoods);

			invoice1.JZ_RX_NKInvoice_Currency = TestData.JobDeclaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("Should be sum of merged line total price", 782m, ImportInfoWithTestData.TotalPriceOfGoods);
		}

		[TestDate(2021, 08, 27)]
		public void TestImportExportDate()
		{
			var date = new DateTime(2021, 08, 17);
			TestData.JobDeclaration.JE_DateOfArrival = date;
			TestData.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Should be ImportExportDate of EntryHeader when import", date, ImportInfoWithTestData.ImportExportDate);

			TestData.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Should be Today when export", new DateTime(2021, 08, 27), ImportInfoWithTestData.ImportExportDate);

			TestData.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			TestData.JobDeclaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals("Should be null when ImportExportDate of EntryHeader is empty", null, ImportInfoWithTestData.ImportExportDate);
		}

		public void TestBillOfLading()
		{
			TestData.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			TestData.JobDeclaration.JE_TransitMode = TransitModeList.Codes.DirectTransition;
			TestData.JobDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			TestData.EntryInstruction.BillOfLading = "BOL123";
			AssertEquals("Should be BillOfLading of EntryHeader", "BOL123", ImportInfoWithTestData.BillOfLading);
		}

		public void TestTradeMode()
		{
			TestData.EntryInstruction.CEI_Style = "AB";
			AssertEquals("Should be CustomsProcedureCode of EntryHeader", "AB", ImportInfoWithTestData.TradeMode);
		}

		public void TestCurrencyCode()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CURR", "Currencies");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CURR", "USD", "USD currency", startDate, endDate);
			helper.CreateCusMapType("CURR", "BTH", "Currency Codes Mapping", true);
			helper.CreateCusMap("CURR", "USD", "502", startDate, endDate, Core.Constants.CountryCodes.China);
			Factory.Save();
			TestData.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			AssertEquals("Should be CURR MapCW1CodeToCustomsCode of EntryLine's CurrencyCode", "502", ImportInfoWithTestData.CurrencyCode);

			TestData.InvoiceHeader.JZ_RX_NKInvoice_Currency = string.Empty;
			AssertEquals("Should be empty when EntryLine's CurrencyCode is empty", string.Empty, ImportInfoWithTestData.CurrencyCode);

			AssertEquals("Should be empty when there is no EntryLine", string.Empty, ImportInfo.CurrencyCode);
		}

		public void TestGoodsOrigin()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", true);
			helper.CreateCusMap("CNTRY", "US", "502", startDate, endDate, Core.Constants.CountryCodes.China);
			Factory.Save();

			CNCusEntryHeaderHelper.SetDeclarationAndEntry(TestData.EntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			TestData.InvoiceLine.JI_CountryOfOrigin = "US";
			AssertEquals("Should be CNTRY MapCW1CodeToCustomsCode of EntryLine's GoodsOriginCode", "502", ImportInfoWithTestData.GoodsOrigin);

			TestData.InvoiceLine.JI_CountryOfOrigin = string.Empty;
			AssertEquals("Should be empty when EntryLine's GoodsOriginCode is empty", string.Empty, ImportInfoWithTestData.GoodsOrigin);

			AssertEquals("Should be empty when there is no EntryLine", string.Empty, ImportInfo.GoodsOrigin);
		}

		public void TestTraderCustomsCode()
		{
			var supplier = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Supplier Company", "CcdCode", "UscCode", "CiqCode").Header;
			TestData.JobDeclaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Should be TradePartyCCD of EntryHeader", "CcdCode", ImportInfoWithTestData.TraderCustomsCode);
		}

		public void TestDeclarantCustomsCode()
		{
			var proxy = Factory.New<OrgHeader>();
			var ccdCode = proxy.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = "CCD";
			ccdCode.OK_CustomsRegNo = "CCD123";
			TestData.JobDeclaration.Branch.GB_OH_OrgProxy = proxy.PK;
			AssertEquals("Should be DeclarantCCD of EntryHeader", "CCD123", ImportInfoWithTestData.DeclarantCustomsCode);
		}

		public void TestQuantityOrWeight()
		{
			TestData.EntryInstruction.CEI_Packages = 5;
			AssertEquals("Should be NoOfPacks of EntryHeader", 5m, ImportInfoWithTestData.QuantityOrWeight);
		}

		public void TestPackingInformation()
		{
			AssertEquals("Should be empty", string.Empty, ImportInfo.PackingInformation);
		}

		public void TestOtherNodes()
		{
			AssertEquals("Should be empty", string.Empty, ImportInfo.OtherNodes);
		}

		public void TestEntrustingPartyTelephone()
		{
			var importer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Importer Company Full Name", "ImporterCus1", "ImporterSocial1", "ImporterCIQ1", "Importer Company").Header;
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(TestData.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			TestData.JobDeclaration.ImporterDocumentaryAddress.OrganisationPK = importer.PK;
			TestData.EntryHeader.TradeParty.E2_Mobile_Formatted = "13245678901";
			AssertEquals("Should be MobilePhoneNumber of EntryHeader's TradeParty when PhoneNumber is empty", "13245678901", ImportInfoWithTestData.EntrustingPartyTelephone);

			TestData.EntryHeader.TradeParty.E2_Phone_Formatted = "01012345678";
			AssertEquals("Should be PhoneNumber of EntryHeader's TradeParty", "01012345678", ImportInfoWithTestData.EntrustingPartyTelephone);
		}

		public void TestEntryNumber()
		{
			AssertEquals("Should be empty", string.Empty, ImportInfo.EntryNumber);
		}

		[TestDate(2021, 08, 27)]
		public void TestReceivingDate()
		{
			AssertEquals("Should be today", new DateTime(2021, 08, 27), ImportInfo.ReceivingDate);
		}

		public void TestReceivingInformation()
		{
			AssertEquals("Should be empty", string.Empty, ImportInfo.ReceivingInformation);
		}

		public void TestOtherInformation()
		{
			AssertEquals("Should be empty", string.Empty, ImportInfo.OtherInformation);
		}

		public void TestAngencyFee()
		{
			AssertEquals("Should be 0", 0m, ImportInfo.AngencyFee);
		}

		public void TestPromiseNotes()
		{
			AssertEquals("Should be empty", string.Empty, ImportInfo.PromiseNotes);
		}

		public void TestEntrustedPartyTelephone()
		{
			AssertEquals("Should be empty when there is no contact", string.Empty, ImportInfo.EntrustedPartyTelephone);

			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var alloc = contact.Allocations.AddNew();
			alloc.PC_Type = OrgConstants.ContactAllocationType.CNCUS;
			TestData.JobDeclaration.Branch.GB_OH_OrgProxy = org.PK;
			contact.OC_Mobile = "13245678901";
			AssertEquals("Should be OC_Mobile of CNCUS contact when OC_Phone is empty", "13245678901", ImportInfoWithTestData.EntrustedPartyTelephone);

			contact.OC_Phone = "01012345678";
			AssertEquals("Should be OC_Phone of CNCUS contact", "01012345678", ImportInfoWithTestData.EntrustedPartyTelephone);
		}

		CNEntryHeaderTestData TestData => testData ?? (testData = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { }));
		CNEntryHeaderTestData testData;

		AcdAgrRequestImportInfoProvider ImportInfoWithTestData => importInfoWithTestData ?? (importInfoWithTestData = new AcdAgrRequestImportInfoProvider(TestData.EntryHeader));
		AcdAgrRequestImportInfoProvider importInfoWithTestData;

		AcdAgrRequestImportInfoProvider ImportInfo => importInfo ?? (importInfo = new AcdAgrRequestImportInfoProvider(Factory.New<CusEntryHeader>()));
		AcdAgrRequestImportInfoProvider importInfo;
	}
}
