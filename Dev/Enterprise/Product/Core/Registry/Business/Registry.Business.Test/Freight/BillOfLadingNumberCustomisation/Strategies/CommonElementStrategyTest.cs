using System;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CommonElementStrategyTest : TestCase
	{
		public void TestProperties()
		{
			var strategy1 = new CommonElementStrategy("key1", "name1", string.Empty, NumberCustomisationElementCategories.Standard, 5, null);
			var strategy2 = new CommonElementStrategy("key2", "name2", "description2", NumberCustomisationElementCategories.LinerAgency, 3, null);

			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element1 = new BillOfLadingNumberCustomisationElement(customisation, strategy1);
			BillOfLadingNumberCustomisationElement element2 = new BillOfLadingNumberCustomisationElement(customisation, strategy2);

			AssertEquals("strategy1.Key", "key1", strategy1.Key);
			AssertEquals("strategy1.Name", "name1", strategy1.Name);
			AssertEquals("strategy1.Description", "", strategy1.Description);
			AssertEquals("strategy1.Category", NumberCustomisationElementCategories.Standard, strategy1.Categories);
			AssertEquals("strategy1.MaxLength", 5, strategy1.CalcMaxGeneratedLength(element1));

			AssertEquals("strategy2.Key", "key2", strategy2.Key);
			AssertEquals("strategy2.Name", "name2", strategy2.Name);
			AssertEquals("strategy2.Description", "description2", strategy2.Description);
			AssertEquals("strategy2.Category", NumberCustomisationElementCategories.LinerAgency, strategy2.Categories);
			AssertEquals("strategy2.MaxLength", 3, strategy2.CalcMaxGeneratedLength(element2));
		}

		public void TestGetMaxGeneratedLengthFunction()
		{
			int getMaxGeneratedLengthFunc() => 1001;

			var strategy = new CommonElementStrategy("key1", "name1", "description1", NumberCustomisationElementCategories.LinerAgency, getMaxGeneratedLengthFunc, null);

			var customisation = new BillOfLadingNumberCustomisation();
			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);

			AssertEquals("strategy.Key", "key1", strategy.Key);
			AssertEquals("strategy.Name", "name1", strategy.Name);
			AssertEquals("strategy.Description", "description1", strategy.Description);
			AssertEquals("strategy.Category", NumberCustomisationElementCategories.LinerAgency, strategy.Categories);
			AssertEquals("strategy.MaxLength", 1001, strategy.CalcMaxGeneratedLength(element));
		}

		public void TestGetRegExForDataType()
		{
			AssertRegEx(string.Empty, CreateCommonElementWhenFixedMaxLengh(5, null));
			AssertRegEx("AAA", CreateCommonElementWhenFixedMaxLengh(5, (e) => "AAA"));
			AssertRegEx("[A-Z]{5}", CreateCommonElementWhenFixedMaxLengh(5, (e) => $"[A-Z]{{{e.Strategy.CalcMaxGeneratedLength(e)}}}"));

			AssertRegEx(string.Empty, CreateCommonElementWhenCalcMaxLengh(() => 7, null));
			AssertRegEx("BBB", CreateCommonElementWhenCalcMaxLengh(() => 7, (e) => $"BBB"));
			AssertRegEx("[A-Z]{7}", CreateCommonElementWhenCalcMaxLengh(() => 7, (e) => $"[A-Z]{{{e.Strategy.CalcMaxGeneratedLength(e)}}}"));
		}

		void AssertRegEx(string expectedRegEx, BillOfLadingNumberCustomisationElement element)
			=> AssertEquals(expectedRegEx, element.Strategy.GetRegExForDataType(element));

		BillOfLadingNumberCustomisationElement CreateCommonElementWhenFixedMaxLengh(int maxLength, Func<BillOfLadingNumberCustomisationElement, string> overrideRegEx)
		{
			var strategy = new CommonElementStrategy("key1", "name1", "description1", NumberCustomisationElementCategories.LinerAgency, maxLength, overrideRegEx);
			var customisation = new BillOfLadingNumberCustomisation();
			return new BillOfLadingNumberCustomisationElement(customisation, strategy);
		}

		BillOfLadingNumberCustomisationElement CreateCommonElementWhenCalcMaxLengh(Func<int> maxLength, Func<BillOfLadingNumberCustomisationElement, string> overrideRegEx)
		{
			var strategy = new CommonElementStrategy("key1", "name1", "description1", NumberCustomisationElementCategories.LinerAgency, maxLength, overrideRegEx);
			var customisation = new BillOfLadingNumberCustomisation();
			return new BillOfLadingNumberCustomisationElement(customisation, strategy);
		}
	}
}
