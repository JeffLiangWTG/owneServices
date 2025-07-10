using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	class ChiefJobComInvoiceLineValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSettingPreferenceCreatesA00()
		{
			PopulateTestReferenceDataAndSave();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var inv = dec.Invoices.AddNew();
			CombineAssertions("Invoice line A00", () =>
			{
				foreach (var trio in rateTriosAndExpectedResult)
				{
					var invLine = inv.InvoiceLines.AddNew();
					invLine.JI_Tariff = commCode;
					invLine.JI_CountryOfOrigin = trio.countryTradegroup;
					invLine.JI_PrimaryPreference = trio.preference.ZZS_Preference;
					var expectedA00 = trio.expectedA00Rate;
					if (expectedA00 == null)
					{
						AssertEquals("Should see no A00 for " + trio.countryTradegroup, 0, invLine.Taxes.Count);
					}
					else
					{
						var a00 = invLine.Taxes[0];
						AssertEquals("Should see A00 for " + trio.countryTradegroup, trio.expectedA00Rate, a00.G4_RateDuty);
					}
				}
			});
		}

		public void PopulateTestReferenceDataAndSave()
		{
			var yesterday = ZDateTime.MinSmallDateTimeValue;
			var tomorrow = ZDateTime.MaxSmallDateTimeValue;

			var gb = RefDataHelper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			var cds = RefDataHelper.CreateNewOrGetExistingDataGrouping("CDS", "CDS United Kingdom", gb);

			var impTariffType = RefDataHelper.CreateNewOrGetExistingTariffType("GB", "IMP");
			var preference100 = RefDataHelper.CreatePreferenceView("100", "One hundred", "GB");
			var preference101 = RefDataHelper.CreatePreferenceView("101", "Dalmatians", "GB");
			var preference200 = RefDataHelper.CreatePreferenceView("200", "Two hundred", "GB");
			var preference202 = RefDataHelper.CreatePreferenceView("202", "202 Accepted", "GB");
			var preference300 = RefDataHelper.CreatePreferenceView("300", "Three hundred", "GB");
			var preference303 = RefDataHelper.CreatePreferenceView("303", "Youngs 303 gun oil", "GB");
			var preference400 = RefDataHelper.CreatePreferenceView("400", "Four hundred", "GB");
			var preference420 = RefDataHelper.CreatePreferenceView("420", "Smoke time", "GB");
			Factory.Save();

			var tariff = RefDataHelper.CreateTariff("GB", impTariffType.PK, commCode, yesterday, tomorrow, taxOrFeeCode: "DTY");
			var dtyRateType = RefDataHelper.CreateNewOrGetExistingRateType("GB", "DTY", "Duty");
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);

			rateTriosAndExpectedResult = new List<(CusRefPreferenceView preference, string countryTradegroup, string formula, string expectedA00Rate)>();
			rateTriosAndExpectedResult.Add((preference100, "US", "Anything US", "F"));
			rateTriosAndExpectedResult.Add((preference100, "AU", "0", null));
			rateTriosAndExpectedResult.Add((preference100, "NZ", "VFD*0", null));
			rateTriosAndExpectedResult.Add((preference200, "IN", "Anything IN", "G"));
			rateTriosAndExpectedResult.Add((preference300, "FR", "Anything FR", "A"));
			rateTriosAndExpectedResult.Add((preference400, "TR", "Anything TR", "UT"));
			rateTriosAndExpectedResult.Add((preference400, "AD", "Anything AD", "U"));
			rateTriosAndExpectedResult.Add((preference420, "AQ", "Anything AQ", "AT"));
			rateTriosAndExpectedResult.Add((preference101, "CA", "Anything CA", "F"));
			rateTriosAndExpectedResult.Add((preference202, "PK", "Anything PK", "G"));
			rateTriosAndExpectedResult.Add((preference303, "DE", "Anything DE", "A"));

			foreach (var trio in rateTriosAndExpectedResult)
			{
				var tradeGroup = RefDataHelper.CreateTradeGroup("GB", trio.countryTradegroup, yesterday, tomorrow);
				RefDataHelper.AddCountry(tradeGroup, trio.countryTradegroup, yesterday.Date, tomorrow.Date);
				var rate = RefDataHelper.CreateRefCusRate(tariff.PK, rateCodeDtyA00.PK, yesterday, tomorrow, trio.formula, trio.preference.PK, dataGrouping: "GB");
				RefDataHelper.CreateCusApplicability(rate, tradeGroup, yesterday, tomorrow);
			}
			Factory.Save();
		}
		readonly string commCode = "6107110000";

		List<(CusRefPreferenceView preference, string countryTradegroup, string formula, string expectedA00Rate)> rateTriosAndExpectedResult;

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
