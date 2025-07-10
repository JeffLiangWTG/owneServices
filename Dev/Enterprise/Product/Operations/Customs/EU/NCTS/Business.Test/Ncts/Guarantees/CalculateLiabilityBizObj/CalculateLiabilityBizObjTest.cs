using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CalculateLiabilityBizObj))]
	sealed class CalculateLiabilityBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCurrency()
		{
			PrepareTestData();
			AssertEquals("EUR", calculateLiabilityBizObj.Currency);
		}

		public void TestCurrency_Empty()
		{
			PrepareTestData();
			guarantee.PW_RX_NKCurrency = ZString.Empty;
			AssertEquals(ZString.Empty, calculateLiabilityBizObj.Currency);
		}

		public void TestCurrency_ReadOnly()
		{
			PrepareTestData();
			Assert(calculateLiabilityBizObj.CurrencyInfo.ReadOnly);
		}

		public void TestCurrency_ListAttribute() => AssertHasCustomAttribute<ListAttribute>(typeof(CalculateLiabilityBizObj), CalculateLiabilityBizObj.Schema.Currency, includesInherit: false, (x) => x.ListDataSourceMember == "Guarantee.Lookups.Currencies");

		public void TestDefaultValues()
		{
			PrepareTestData(setUpTariffAndAntidumpingAndCountervailing: true);
			CombineAssertions(() =>
			{
				AssertEquals("LiabilityPercentage", expected: 25, calculateLiabilityBizObj.LiabilityPercentage);
				AssertEquals("UseMonetaryValue", expected: false, calculateLiabilityBizObj.UseMonetaryValue);
				AssertEquals("UseDutiesAndTaxes", expected: true, calculateLiabilityBizObj.UseDutiesAndTaxes);
				AssertEquals("TotalValue", expected: 15.48m, calculateLiabilityBizObj.TotalValue);
			});
		}

		public void TestDefaultValues_TotalValue_UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods_false()
		{
			using (NctsConfigurationTestHelper.TemporarilySetUseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsConfigurationConfiguration(Factory, useDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods: false))
			{
				PrepareTestData(setUpTariffAndAntidumpingAndCountervailing: true);
				CombineAssertions(() =>
				{
					AssertEquals("PW_BondAmount", expected: 5.16m, guarantee.PW_BondAmount);
					AssertEquals("TotalValue", expected: 5.16m, calculateLiabilityBizObj.TotalValue);
					AssertEquals("UseDutiesAndTaxes", expected: false, calculateLiabilityBizObj.UseDutiesAndTaxes);
				});
			}
		}

		public void TestDefaultValues_TotalValue_UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods_True()
		{
			using (NctsConfigurationTestHelper.TemporarilySetUseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsConfigurationConfiguration(Factory, useDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods: true))
			{
				PrepareTestData(setUpTariffAndAntidumpingAndCountervailing: true);
				CombineAssertions(() =>
				{
					AssertEquals("PW_BondAmount", expected: 5.16m, guarantee.PW_BondAmount);
					AssertEquals("TotalValue", expected: 15.48m, calculateLiabilityBizObj.TotalValue);
					AssertEquals("UseDutiesAndTaxes", expected: true, calculateLiabilityBizObj.UseDutiesAndTaxes);
				});
			}
		}

		public void TestLiabilityAmount()
		{
			PrepareTestData();
			CombineAssertions(() =>
			{
				calculateLiabilityBizObj.TotalValue = 40;
				calculateLiabilityBizObj.LiabilityPercentage = 25;
				AssertEquals(10m, calculateLiabilityBizObj.LiabilityAmount);

				calculateLiabilityBizObj.LiabilityPercentage = 33;
				AssertEquals(13.2m, calculateLiabilityBizObj.LiabilityAmount);

				calculateLiabilityBizObj.TotalValue = 36.1111m;
				AssertEquals(11.9167m, calculateLiabilityBizObj.LiabilityAmount);
			});
		}

		public void TestLiabilityAmount_ReadOnly()
		{
			PrepareTestData();
			Assert(calculateLiabilityBizObj.LiabilityAmountInfo.ReadOnly);
		}

		public void TestUpdateLiabilityAmountOnGuarantee()
		{
			PrepareTestData();
			CombineAssertions(() =>
			{
				calculateLiabilityBizObj.LiabilityAmount = 10;
				calculateLiabilityBizObj.UpdateLiabilityAmountOnGuarantee();
				AssertEquals("10", expected: 10m, guarantee.PW_BondAmount);

				calculateLiabilityBizObj.LiabilityAmount = 1.234m;
				calculateLiabilityBizObj.UpdateLiabilityAmountOnGuarantee();
				AssertEquals("1.234", expected: 1.234m, guarantee.PW_BondAmount);
			});
		}

		public void TestUseDutiesAndTaxes_EUR_EUR()
		{
			PrepareTestData(setUpTariffAndAntidumpingAndCountervailing: true);
			CombineAssertions(() =>
			{
				calculateLiabilityBizObj.UseMonetaryValue = true;
				Assert("Precondition, different result", calculateLiabilityBizObj.TotalValue != 15.48m);
				var goodsItems = guarantee.NctsHeader.Bills.SelectMany(x => x.GoodsItems);
				AssertEquals("Precondition: VatAmount", 5.08m, goodsItems.Sum(x => x.VatAmount));
				AssertEquals("Precondition: DutyAmount", 1.8m, goodsItems.Sum(x => x.DutyAmount));
				AssertEquals("Precondition: AntiDumpingDutyAmount", 4.3m, goodsItems.Sum(x => x.AntiDumpingDutyAmount));
				AssertEquals("Precondition: CountervailingDutyAmount", 4.3m, goodsItems.Sum(x => x.CountervailingDutyAmount));

				calculateLiabilityBizObj.UseDutiesAndTaxes = true;
				AssertEquals("UseDutiesAndTaxes for calculation", expected: 15.48m, calculateLiabilityBizObj.TotalValue);
			});
		}

		public void TestUseDutiesAndTaxes_NotEUR_EUR()
		{
			PrepareTestData(setUpTariffAndAntidumpingAndCountervailing: true);
			var eur = GlbCompany.CurrentCompany.LocalCurrency;
			using var currencyDisposable = GlbCompany.CurrentCompany.TemporarilySetCurrency("XXX");
			var currencyConverter = (RefCurrencyCurrencyConverter)CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, roundToTargetCurrencyDecimals: false);
			var exchangeRate = currencyConverter.GetExchangeRateObject(eur);
			exchangeRate.RE_SellRate = 3m;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			goodsItem1.BY_RX_NKCurrency = "XXX";
			goodsItem2.BY_RX_NKCurrency = "XXX";

			CombineAssertions(() =>
			{
				calculateLiabilityBizObj.UseMonetaryValue = true;
				Assert("Precondition, different result", calculateLiabilityBizObj.TotalValue != 15.48m);

				var goodsItems = guarantee.NctsHeader.Bills.SelectMany(x => x.GoodsItems);
				AssertEquals("Precondition: VatAmount", 5.08m, goodsItems.Sum(x => x.VatAmount));
				AssertEquals("Precondition: DutyAmount", 1.8m, goodsItems.Sum(x => x.DutyAmount));
				AssertEquals("Precondition: AntiDumpingDutyAmount", 4.3m, goodsItems.Sum(x => x.AntiDumpingDutyAmount));
				AssertEquals("Precondition: CountervailingDutyAmount", 4.3m, goodsItems.Sum(x => x.CountervailingDutyAmount));

				calculateLiabilityBizObj.UseDutiesAndTaxes = true;
				AssertEquals("UseDutiesAndTaxes for calculation, ExchangeRate:EUR => XXX = 3", expected: 5.16m, calculateLiabilityBizObj.TotalValue);
			});
		}

		public void TestUseDutiesAndTaxes_NotEUR_NotEUR()
		{
			PrepareTestData(setUpTariffAndAntidumpingAndCountervailing: true);
			goodsItem1.BY_RX_NKCurrency = "XXX";
			goodsItem2.BY_RX_NKCurrency = "XXX";
			guarantee.PW_RX_NKCurrency = "YYY";
			CombineAssertions(() =>
			{
				calculateLiabilityBizObj.UseMonetaryValue = true;
				Assert("Precondition, different result", calculateLiabilityBizObj.TotalValue != 15.48m);
				var goodsItems = guarantee.NctsHeader.Bills.SelectMany(x => x.GoodsItems);
				AssertEquals("Precondition: VatAmount", 5.08m, goodsItems.Sum(x => x.VatAmount));
				AssertEquals("Precondition: DutyAmount", 1.8m, goodsItems.Sum(x => x.DutyAmount));
				AssertEquals("Precondition: AntiDumpingDutyAmount", 4.3m, goodsItems.Sum(x => x.AntiDumpingDutyAmount));
				AssertEquals("Precondition: CountervailingDutyAmount", 4.3m, goodsItems.Sum(x => x.CountervailingDutyAmount));

				calculateLiabilityBizObj.UseDutiesAndTaxes = true;
				AssertEquals("UseDutiesAndTaxes for calculation, ExchangeRate:XXX => YYY = 2", expected: 30.96m, calculateLiabilityBizObj.TotalValue);
			});
		}

		public void TestUseMonetaryValue_EUR_EUR()
		{
			PrepareTestData();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition UseMonetaryValue false", expected: false, calculateLiabilityBizObj.UseMonetaryValue);
				Assert("Precondition, different result", calculateLiabilityBizObj.TotalValue != 36m);

				calculateLiabilityBizObj.UseMonetaryValue = true;
				AssertEquals("UseMonetaryValue for calculation", expected: 36m, calculateLiabilityBizObj.TotalValue);
			});
		}

		public void TestUseMonetaryValue_NotEUR_EUR()
		{
			PrepareTestData();
			var eur = GlbCompany.CurrentCompany.LocalCurrency;
			using var currencyDisposable = GlbCompany.CurrentCompany.TemporarilySetCurrency("XXX");
			var currencyConverter = (RefCurrencyCurrencyConverter)CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, roundToTargetCurrencyDecimals: false);
			var exchangeRate = currencyConverter.GetExchangeRateObject(eur);
			exchangeRate.RE_SellRate = 3m;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			goodsItem1.BY_RX_NKCurrency = "XXX";
			goodsItem2.BY_RX_NKCurrency = "XXX";
			calculateLiabilityBizObj.UseMonetaryValue = true;
			AssertEquals(12m, calculateLiabilityBizObj.TotalValue);
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			calculateLiabilityBizObj = new CalculateLiabilityBizObj(Factory, guarantee);
			calculateLiabilityBizObj.UseMonetaryValue = true;
			AssertEquals(108m, calculateLiabilityBizObj.TotalValue);
		}

		public void TestUseMonetaryValue_NotEUR_NotEUR()
		{
			PrepareTestData();
			goodsItem1.BY_RX_NKCurrency = "XXX";
			goodsItem2.BY_RX_NKCurrency = "XXX";
			guarantee.PW_RX_NKCurrency = "YYY";
			calculateLiabilityBizObj.UseMonetaryValue = true;
			AssertEquals(24m, calculateLiabilityBizObj.TotalValue);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			return new CalculateLiabilityBizObj(Factory, guarantee);
		}

		void PrepareTestData(bool setUpTariffAndAntidumpingAndCountervailing = false)
		{
			(guarantee, goodsItem1, goodsItem2, calculateLiabilityBizObj) = CalculateLiabilityBizObjTestHelper.CreateCalculateLiabilityBizObj(Factory, setUpTariffAndAntidumpingAndCountervailing);
		}

		NctsDepartureCargoDesc goodsItem1;
		NctsDepartureCargoDesc goodsItem2;
		NctsGuarantee guarantee;
		CalculateLiabilityBizObj calculateLiabilityBizObj;
	}

	public static class CalculateLiabilityBizObjTestHelper
	{
		public static (NctsGuarantee guarantee, NctsDepartureCargoDesc goodsItem1, NctsDepartureCargoDesc goodsItem2) CreateGuaranteeForCalculateLiabilityBizObj(BusinessObjectFactory factory)
		{
			var newCurrency1 = RefCurrency.New(factory);
			newCurrency1.RX_Code = "XXX";
			newCurrency1.SetCustomsRate(ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, 3m);

			var newCurrency2 = RefCurrency.New(factory);
			newCurrency2.RX_Code = "YYY";
			newCurrency2.SetCustomsRate(ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, 2m);

			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_SubType = "1";
			guaranteeHeader.CPH_StartDate = ZDate.BrettsBirthday;

			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = "LAP";
			guaranteeRule.CPR_ValueFrom = "THI";

			var nctsHeader = factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_SystemCreateTimeUtc = ZDateTime.UtcNow;
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var goodsItem1 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem1.BY_MonetaryValue = 15;
			goodsItem1.BY_RX_NKCurrency = "EUR";

			var goodsItem2 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem2.BY_MonetaryValue = 21;
			goodsItem2.BY_RX_NKCurrency = "EUR";

			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_RX_NKCurrency = "EUR";
			guarantee.PW_BondAmount = 40m;

			return (guarantee, goodsItem1, goodsItem2);
		}

		public static (NctsGuarantee guarantee, NctsDepartureCargoDesc goodsItem1, NctsDepartureCargoDesc goodsItem2, CalculateLiabilityBizObj calculateLiabilityBizObj) CreateCalculateLiabilityBizObj(BusinessObjectFactory factory, bool setUpTariffAndAntidumpingAndCountervailing = false)
		{
			var (guarantee, goodsItem1, goodsItem2) = CreateGuaranteeForCalculateLiabilityBizObj(factory);
			if (setUpTariffAndAntidumpingAndCountervailing)
			{
				NCTSTestHelper.SetUpTariffAndAntidumpingAndCountervailing(factory);
				goodsItem1.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				goodsItem1.BY_ZZF_NKTaxType = ZString.Empty;
				goodsItem1.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;
				var supplementaryCode1 = goodsItem1.AdditionalSupplementaryCodes.AddNew();
				supplementaryCode1.CY_Code = "AC01";
			}
			var calculateLiabilityBizObj = new CalculateLiabilityBizObj(factory, guarantee);
			return (guarantee, goodsItem1, goodsItem2, calculateLiabilityBizObj);
		}
	}
}
