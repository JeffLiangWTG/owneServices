using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusCalculationRuleRateCollection))]
	sealed class CusCalculationRuleRateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusCalculationRuleRateCollection>
	{
		protected override CusCalculationRuleRateCollection GetCollectionToTest()
		{
			return new CusCalculationRuleRateCollection(Factory, Factory.New<CusCalculationRule>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CusCalculationRuleRate(Factory);
		}

		public void TestMaxCountValidationEnable()
		{
			var collection = new CusCalculationRuleRateCollection(Factory, Factory.New<CusCalculationRule>());
			Enumerable.Range(0, 49).ForEach(_ => collection.AddNew());
			var item50 = collection.AddNew();
			var item51 = collection.AddNew();

			CombineAssertions(() =>
			{
				AssertNoRowErrorContaining(item50, "allowed a maximum");
				AssertHasRowErrorContaining(item51, "allowed a maximum");
			});
		}

		public void TestNewBusinessObjectIsValidated() => CombineAssertions(() =>
		{
			const string message = "Either Flat Rate or Uplift must have a value.";
			var collection = new CusCalculationRuleRateCollection(Factory, Factory.New<CusCalculationRule>());
			var rate = collection.AddNew();
			AssertHasWarning("FlatRate", rate.FlatRateInfo, message);
			AssertHasWarning("Uplift", rate.UpliftInfo, message);
		});
	}
}
