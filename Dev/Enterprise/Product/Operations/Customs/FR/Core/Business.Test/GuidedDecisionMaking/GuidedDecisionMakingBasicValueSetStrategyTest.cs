using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.GDM.Testing
{
	sealed class GuidedDecisionMakingBasicValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestAdditionalCodesCanBeRefreshed_AfterRegionIsChanged()
		{
			SetUpTariffAndRate();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "FR";
			invoiceLine.ZG_CountryOfDestination = "FR";
			var gDMBasic = invoiceLine.GetGuidedDecisionMakingBasic() as GuidedDecisionMakingBasic;
			gDMBasic.TariffCode = "1122334455";
			gDMBasic.DataGrouping = GlbCompany.CurrentCompany.Country.Code;
			gDMBasic.Preference = "PRE";
			gDMBasic.EffectiveDate = ZDate.Today;
			gDMBasic.CountryOfOrigin = "CN";
			gDMBasic.CountryOfDestination = "CN";

			gDMBasic.RegionOrTerritoryOfDestination = "MARTI";
			AssertContainsExactElementsInAnyOrder("GDMBasic should contains additional codes for secondary group MARTI", new string[] { "Q003", "Q004" }, gDMBasic.AdditionalCodes.Select(x => x.AdditionalCode));

			gDMBasic.RegionOrTerritoryOfDestination = "CONTI";
			AssertContainsExactElementsInAnyOrder("GDMBasic should contains additional codes for secondary group CONTI", new string[] { "Q234", "Q235" }, gDMBasic.AdditionalCodes.Select(x => x.AdditionalCode));
		}

		public void TestVATApplicabilitiesCanBeRefreshed_AfterRegionIsChanged()
		{
			GuidedDecisionMakingVATCollectionTest.SetUpVATApplicabilities(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var gDMBasic = invoiceLine.GetGuidedDecisionMakingBasic() as GuidedDecisionMakingBasic;
			gDMBasic.TariffCode = "1111111111";
			gDMBasic.DataGrouping = GlbCompany.CurrentCompany.Country.Code;
			gDMBasic.EffectiveDate = ZDate.Today;
			gDMBasic.CountryOfOrigin = "CN";
			gDMBasic.RegionOrTerritoryOfDestination = "MGPRE";

			var vatApplicabilies = gDMBasic.VATApplicabilities;
			AssertArrayEqualsByElements(new ZString[] { "V002" }, vatApplicabilies.Select(x => x.AdditionalCode).ToArray());

			gDMBasic.RegionOrTerritoryOfDestination = "CONTI";
			vatApplicabilies = gDMBasic.VATApplicabilities;
			AssertArrayEqualsByElements(new ZString[] { "V001" }, vatApplicabilies.Select(x => x.AdditionalCode).ToArray());
		}

		void SetUpTariffAndRate()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var tradeGroup = RefDataHelper.CreateTradeGroup(dataGrouping, "GROUP", date1, date2);
			RefDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.China, date1, date2);

			var secondTradeGroupMARTI = RefDataHelper.CreateTradeGroup(dataGrouping, "MARTI", date1, date2);
			var secondTradeGroupCONTI = RefDataHelper.CreateTradeGroup(dataGrouping, "CONTI", date1, date2);
			var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "IMP");
			Factory.Save();

			var tariff = RefDataHelper.CreateTariff(dataGrouping, tariffType.PK, "1122334455", date1, date2, "Tariff1");
			var rateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty Desc.");
			rateType.ZZR_IsExport = true;
			var rateCode = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", rateType.PK);
			var preference = RefDataHelper.CreatePreferenceForCountry("PRE", "Preference", dataGrouping);
			var rate = RefDataHelper.CreateRate(tariff, rateCode.PK, date1, date2, "0", preference.PK);
			RefDataHelper.CreateCusApplicability(rate, tradeGroup, date1, date2, "Q003", secondTradeGroup: secondTradeGroupMARTI);
			RefDataHelper.CreateCusApplicability(rate, tradeGroup, date1, date2, "Q004", secondTradeGroup: secondTradeGroupMARTI);
			RefDataHelper.CreateCusApplicability(rate, tradeGroup, date1, date2, "Q234", secondTradeGroup: secondTradeGroupCONTI);
			RefDataHelper.CreateCusApplicability(rate, tradeGroup, date1, date2, "Q235", secondTradeGroup: secondTradeGroupCONTI);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
