using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusCalculationRuleRate))]
	sealed class CusCalculationRuleRateTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCannotDeleteFirstRate() => CombineAssertions(() =>
		{
			var rule = Factory.New<CusCalculationRule>();
			var calculationRuleRateCollection = rule.CalculationRuleRateCollection;
			AssertEquals("There's 1 rate automatically added to the collection", 1, calculationRuleRateCollection.Count);
			var rate1 = calculationRuleRateCollection[0];
			AssertEquals("First rate", true, rate1.IsFirstRate);
			AssertEquals("The first rate cannot be deleted", false, rate1.CanDelete);

			var rate2 = calculationRuleRateCollection.AddNew();
			AssertEquals("Second rate", false, rate2.IsFirstRate);
			AssertEquals("Any rate other than the first can be deleted", true, rate2.CanDelete);

			var rate3 = calculationRuleRateCollection.AddNew();
			AssertEquals("Third rate", false, rate3.IsFirstRate);
			AssertEquals("Any rate other than the first can be deleted", true, rate3.CanDelete);
		});

		public void TestValueFromReadOnlyForFirstRate() => CombineAssertions(() =>
		{
			var rule = Factory.New<CusCalculationRule>();
			var calculationRuleRateCollection = rule.CalculationRuleRateCollection;
			AssertEquals("There's 1 rate automatically added to the collection", 1, calculationRuleRateCollection.Count);
			var rate1 = calculationRuleRateCollection[0];
			AssertEquals("First rate", true, rate1.IsFirstRate);
			AssertEquals("Value From of the first rate is read only", true,  rate1.ValueFromInfo.ReadOnly);

			var rate2 = calculationRuleRateCollection.AddNew();
			AssertEquals("Second rate", false, rate2.IsFirstRate);
			AssertEquals("Value From of other rates are editable", false, rate2.ValueFromInfo.ReadOnly);

			var rate3 = calculationRuleRateCollection.AddNew();
			AssertEquals("Third rate", false, rate3.IsFirstRate);
			AssertEquals("Value From of other rates are editable", false, rate3.ValueFromInfo.ReadOnly);
		});

		public void TestReasonForNotAbleToDelete()
		{
			var rate = (CusCalculationRuleRate)GetNewBusinessObject();
			AssertEquals("Calculation Rule requires at least one Rate.", rate.ReasonForNotAbleToDelete);
		}

		public void TestProperties()
		{
			var rate = (CusCalculationRuleRate)GetNewBusinessObject();
			AssertType<ZDecimal>(rate.ValueFrom);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusCalculationRuleRate), nameof(CusCalculationRuleRate.ValueFrom), true, x => x.DecimalPlacesMember == nameof(CusCalculationRuleRate.CurrencyDecimalPlaces));
			AssertEquals("Value From (inclusive)", DataBoundResourceStrings.GetDataForProperty(rate.ValueFromInfo).Caption);

			AssertType<ZDecimal>(rate.FlatRate);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusCalculationRuleRate), nameof(CusCalculationRuleRate.FlatRate), true, x => x.DecimalPlacesMember == nameof(CusCalculationRuleRate.CurrencyDecimalPlaces));
			AssertEquals("Flat Rate", DataBoundResourceStrings.GetDataForProperty(rate.FlatRateInfo).Caption);

			AssertType<ZDecimal>(rate.ValueFrom);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusCalculationRuleRate), nameof(CusCalculationRuleRate.Uplift), true, x => x.DecimalPlaces == 5);
			AssertEquals("Uplift %", DataBoundResourceStrings.GetDataForProperty(rate.UpliftInfo).Caption);
		}

		public void TestCurrencyDecimalPlaces()
		{
			var currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "CU1";
			currency1.RX_SubUnitRatio = 10;
			var currency2 = Factory.New<RefCurrency>();
			currency2.RX_Code = "CU2";
			currency2.RX_SubUnitRatio = 1000;

			var rule = Factory.New<CusCalculationRule>();
			var rate = rule.CalculationRuleRateCollection[0];
			AssertEquals("Default 2 when no currency specified", 2, rate.CurrencyDecimalPlaces);

			rule.CCR_RX_NKCurrency = "CU1";
			AssertEquals("Currency with 1 decimal place", 1, rate.CurrencyDecimalPlaces);

			rule.CCR_RX_NKCurrency = "CU2";
			AssertEquals("Currency with 3 decimal places", 3, rate.CurrencyDecimalPlaces);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusCalculationRuleRate(Factory);
		}
	}
}
