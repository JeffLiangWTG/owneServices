using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
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
			AssertEquals("Value From of the first rate is read only", true, rate1.ValueFromInfo.ReadOnly);

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

		public void TestUpliftCaption()
		{
			var rate = (CusCalculationRuleRate)GetNewBusinessObject();
			AssertEquals("Uplift %", DataBoundResourceStrings.GetDataForProperty(rate.UpliftInfo).Caption);
		}

		public void TestUpliftDecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusCalculationRuleRate), nameof(CusCalculationRuleRate.Uplift), true, x => x.DecimalPlaces == 5);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusCalculationRuleRate(Factory);
		}
	}
}
