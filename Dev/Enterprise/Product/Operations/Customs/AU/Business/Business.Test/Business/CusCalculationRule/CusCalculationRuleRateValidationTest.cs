using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusCalculationRuleRateValidation))]
	sealed class CusCalculationRuleRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckValueFrom() => CombineAssertions(() =>
		{
			var rule = Factory.New<CusCalculationRule>();
			var rateCollection = rule.CalculationRuleRateCollection;
			AssertEquals("rateCollection have a default rate", 1, rateCollection.Count);
			var rate1 = rateCollection[0];
			AssertEquals("default rate value from has set to zero", 0m, rate1.ValueFrom);
			var rate2 = rateCollection.AddNew();
			rate2.ValueFrom = -1;
			AssertHasError(rate2.ValueFromInfo, "Value From must be greater than zero.");
			rate2.ValueFrom = 0;
			AssertHasError(rate2.ValueFromInfo, "The Value From (inclusive) has been duplicated and must be unique.");
			rate1.Validation.ValidateValueFrom();
			AssertNoErrors("No validation done for ValueFrom for the first rate", rate1.ValueFromInfo);
		});

		public void TestCheckFlatRate()
		{
			var rate = new CusCalculationRuleRate(Factory);
			rate.Validation.ValidateFlatRate();
			AssertHasWarning(rate.FlatRateInfo, "Either Flat Rate or Uplift must have a value.");
		}

		public void TestCheckUplift()
		{
			var rate = new CusCalculationRuleRate(Factory);
			rate.Validation.ValidateUplift();
			AssertHasWarning(rate.UpliftInfo, "Either Flat Rate or Uplift must have a value.");
		}
	}
}
