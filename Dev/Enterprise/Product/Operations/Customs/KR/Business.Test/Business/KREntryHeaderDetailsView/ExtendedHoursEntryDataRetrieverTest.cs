using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ExtendedHoursEntryDataRetrieverTest : TestCaseWithFactory
	{
		public void TestKREntryHeaderDetailsViews()
		{
			var declaration = SetUpData();

			ZString[] entryNumbers = { "2362520050702X", "2362520042782X" };
			var krEntryHeaderDetailsViews = ExtendedHoursEntryDataRetriever.GetEntryHeaderDetailsForExtendedHoursRequest(Factory, declaration.JE_GC, entryNumbers);
			AssertEquals("View Count", 2, krEntryHeaderDetailsViews.Length);
			AssertEquals("EntryNum has 2362520042782X", true, krEntryHeaderDetailsViews.Any(x => x.KEH_EntryNum == "2362520042782X"));
			AssertEquals("EntryNum has 2362520050702X", true, krEntryHeaderDetailsViews.Any(x => x.KEH_EntryNum == "2362520050702X"));
		}

		public JobDeclaration SetUpData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);

			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521000", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "USED EXCAVATOR");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9529521022", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "PAPER CALENDARS");

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 1000m, usdCurrency);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 2000m, usdCurrency);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TotalNoOfPacksPackType = "OU";
			declaration.JE_LocationOtherInformation = "99999999";
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA2", "레디코리아");
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "모나리자(주)");
			declaration.JE_OH_DutyPayer = payer.PK;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "2362520050702X";
			entry1.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_NoOfPacks = 100m;
			invoice1.JZ_Weight = 120m;
			invoice1.JZ_WeightUQ = "KG";

			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "8429521000";
			entryLine1.CL_CustomsValue = 100m;
			var entryLine2 = entry1.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "9529521022";
			entryLine2.CL_CustomsValue = 200m;

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "2362520042782X";
			entry2.CusEntryNumber.CE_IssueDate = ZDateTime.Today.AddDays(1);

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_NoOfPacks = 200m;
			invoice2.JZ_Weight = 100m;
			invoice2.JZ_WeightUQ = "KG";

			var entryLine = entry2.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "9529521022";
			entryLine.CL_CustomsValue = 100m;

			var invoiceLine = invoice2.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();

			return declaration;
		}
		void SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();
		}
	}
}
