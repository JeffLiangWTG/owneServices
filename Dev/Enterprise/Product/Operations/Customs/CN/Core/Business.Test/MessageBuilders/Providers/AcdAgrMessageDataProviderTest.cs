using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class AcdAgrMessageDataProviderTest : TestCaseWithFactory
	{
		public void TestOperationInfo()
		{
			AssertType<AcdAgrRequestOperInfoProvider>("Should be an instance of AcdAgrRequestOperInfoProvider", Provider.OperationInfo);
		}

		public void TestImportInfo()
		{
			AssertType<AcdAgrRequestImportInfoProvider>("Should be an instance of AcdAgrRequestImportInfoProvider", Provider.ImportInfo);
		}

		public void TestValidateMandatoryFields()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", true);
			helper.CreateCusMap("CNTRY", "US", "502", startDate, endDate, Core.Constants.CountryCodes.China);
			Factory.Save();

			var entryHeader = TestData.EntryHeader;
			TestData.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertMissingMandatoryFields(@"CCD Registration Number of Import (on Declaration tab)
CCD Registration Number of Branch (on Misc tab)
Customs Procedure (on Entry Instruction tab)
Tariff (on Inv. Lines tab)
Trade Quantity (on Inv. Lines tab)
Name Of Goods (on Inv. Lines tab)
Goods Origin (on Inv. Lines tab)", entryHeader);

			TestData.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertMissingMandatoryFields(@"CCD Registration Number of Supplier (on Declaration tab)
CCD Registration Number of Branch (on Misc tab)
Customs Procedure (on Entry Instruction tab)
Tariff (on Inv. Lines tab)
Trade Quantity (on Inv. Lines tab)
Name Of Goods (on Inv. Lines tab)
Goods Origin (on Inv. Lines tab)", entryHeader);

			var supplier = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Supplier Company", "CcdCode", "UscCode", "CiqCode").Header;
			TestData.JobDeclaration.JE_OH_Supplier = supplier.PK;
			AssertMissingMandatoryFields(@"CCD Registration Number of Branch (on Misc tab)
Customs Procedure (on Entry Instruction tab)
Tariff (on Inv. Lines tab)
Trade Quantity (on Inv. Lines tab)
Name Of Goods (on Inv. Lines tab)
Goods Origin (on Inv. Lines tab)", entryHeader);

			var proxy = Factory.New<OrgHeader>();
			var ccdCode = proxy.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = "CCD";
			ccdCode.OK_CustomsRegNo = "CCD123";
			TestData.JobDeclaration.Branch.GB_OH_OrgProxy = proxy.PK;
			AssertMissingMandatoryFields(@"Customs Procedure (on Entry Instruction tab)
Tariff (on Inv. Lines tab)
Trade Quantity (on Inv. Lines tab)
Name Of Goods (on Inv. Lines tab)
Goods Origin (on Inv. Lines tab)", entryHeader);

			TestData.EntryInstruction.CEI_Style = "AB";
			AssertMissingMandatoryFields(@"Tariff (on Inv. Lines tab)
Trade Quantity (on Inv. Lines tab)
Name Of Goods (on Inv. Lines tab)
Goods Origin (on Inv. Lines tab)", entryHeader);

			TestData.InvoiceLine.JI_Tariff = "3005109000";
			AssertMissingMandatoryFields(@"Trade Quantity (on Inv. Lines tab)
Name Of Goods (on Inv. Lines tab)
Goods Origin (on Inv. Lines tab)", entryHeader);

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
			invoice1.JZ_RX_NKInvoice_Currency = TestData.JobDeclaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertMissingMandatoryFields(@"Name Of Goods (on Inv. Lines tab)
Goods Origin (on Inv. Lines tab)", entryHeader);

			TestData.InvoiceLine.JI_NameOfGoods = "Car";
			AssertMissingMandatoryFields(@"Goods Origin (on Inv. Lines tab)", entryHeader);

			CNCusEntryHeaderHelper.SetDeclarationAndEntry(TestData.EntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			TestData.InvoiceLine.JI_CountryOfOrigin = "US";
			AssertMissingMandatoryFields(ZString.Empty, entryHeader);
		}

		void AssertMissingMandatoryFields(string missingFields, CusEntryHeader entryHeader)
		{
			AssertContainsExactElementsInExactOrder(missingFields.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries), new AcdAgrMessageDataProvider(entryHeader).MissingMandatoryFields);
		}

		CNEntryHeaderTestData TestData => testData ?? (testData = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { }));
		CNEntryHeaderTestData testData;

		AcdAgrMessageDataProvider Provider => provider ?? (provider = new AcdAgrMessageDataProvider(Factory.New<CusEntryHeader>()));
		AcdAgrMessageDataProvider provider;
	}
}
