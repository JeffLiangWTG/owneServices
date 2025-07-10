using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleTranslatableTextFilter))]
	sealed class ModuleTranslatableTextFilterTest : ModuleFilterTestCase<ModuleTranslatableTextFilter>
	{
		public void TestFilter()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (mockRes = Res.UseMockData())
			{
				AddData("05", "five", "五");
				AddData("10", "ten", "十");
				AddData("15", "fifteen", "十五");
				AddData("20", "twenty", "二十");
				AddData("25", "twenty-five", "二十五");
				AddData("50", "fifty", "五十");
				AddData("100", "one hundred", "百");
				Factory.Save();

				Filter.ComparisonOperator = "exact";
				Filter.Property = "十";
				var result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(Filter.Query);
				AssertContainsExactElementsInAnyOrder(new[] { "10" }, result.Select(x => (string)x.Z0_Code));

				Filter.ComparisonOperator = "starts with";
				Filter.Property = "十";
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(Filter.Query);
				AssertContainsExactElementsInAnyOrder(new[] { "10", "15" }, result.Select(x => (string)x.Z0_Code));

				Filter.ComparisonOperator = "contains";
				Filter.Property = "十";
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(Filter.Query);
				AssertContainsExactElementsInAnyOrder(new[] { "10", "15", "20", "25", "50" }, result.Select(x => (string)x.Z0_Code));

				Filter.ComparisonOperator = "not equal";
				Filter.Property = "十";
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(Filter.Query);
				AssertContainsExactElementsInAnyOrder(new[] { "05", "15", "20", "25", "50", "100" }, result.Select(x => (string)x.Z0_Code));

				Filter.ComparisonOperator = "not starting";
				Filter.Property = "十";
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(Filter.Query);
				AssertContainsExactElementsInAnyOrder(new[] { "05", "20", "25", "50", "100" }, result.Select(x => (string)x.Z0_Code));

				Filter.ComparisonOperator = "not contain";
				Filter.Property = "五";
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(Filter.Query);
				AssertContainsExactElementsInAnyOrder(new[] { "10", "20", "100" }, result.Select(x => (string)x.Z0_Code));

				Filter.ComparisonOperator = "is blank";
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(Filter.Query);
				AssertEquals(0, result.Length);

				Filter.ComparisonOperator = "is not blank";
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(Filter.Query);
				AssertContainsExactElementsInAnyOrder(new[] { "05", "10", "15", "20", "25", "50", "100" }, result.Select(x => (string)x.Z0_Code));
			}
		}

		public void TestModuleTranslatableTextFilterInAGroup()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (mockRes = Res.UseMockData())
			{
				AddData("05", "five", "五");
				AddData("10", "ten", "十");
				AddData("15", "fifteen", "十五");
				AddData("20", "twenty", "二十");
				AddData("25", "twenty-five", "二十五");
				AddData("50", "fifty", "五十");
				AddData("100", "one hundred", "百");
				Factory.Save();

				var moduleFilterCombiner = new ModuleFilterCombiner();

				var filter1 = GetNewModuleFilter();
				filter1.OrCategory = FilterOrCategory.Red;
				var filter2 = GetNewModuleFilter();
				filter2.OrCategory = FilterOrCategory.Red;

				filter1.ComparisonOperator = "contains";
				filter2.ComparisonOperator = "contains";

				filter1.Property = "AAAAA";
				filter2.Property = "CCCCC";
				var moduleFilters = new[] { filter1, filter2 };
				var resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);
				var result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(resultQuery);
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), result.Select(x => (string)x.Z0_Code));

				filter1.Property = "十五";
				filter2.Property = "五十";
				resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(resultQuery);
				AssertContainsExactElementsInAnyOrder(new[] { "15", "25", "50" }, result.Select(x => (string)x.Z0_Code));

				filter1.Property = "AAAAA";
				filter2.Property = "十五";
				resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(resultQuery);
				AssertContainsExactElementsInAnyOrder(new[] { "15", "25" }, result.Select(x => (string)x.Z0_Code));

				filter1.Property = "五十";
				filter2.Property = "CCCCC";
				resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(resultQuery);
				AssertContainsExactElementsInAnyOrder(new[] { "50" }, result.Select(x => (string)x.Z0_Code));

				filter1.ComparisonOperator = "starts with";
				filter2.ComparisonOperator = "starts with";

				filter1.Property = "AAAAA";
				filter2.Property = "CCCCC";
				moduleFilters = new[] { filter1, filter2 };
				resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(resultQuery);
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), result.Select(x => (string)x.Z0_Code));

				filter1.Property = "五";
				filter2.Property = "十";
				resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(resultQuery);
				AssertContainsExactElementsInAnyOrder(new[] { "05", "10", "15", "50" }, result.Select(x => (string)x.Z0_Code));

				filter1.Property = "AAAAA";
				filter2.Property = "十";
				resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(resultQuery);
				AssertContainsExactElementsInAnyOrder(new[] { "10", "15" }, result.Select(x => (string)x.Z0_Code));

				filter1.Property = "五";
				filter2.Property = "CCCCC";
				resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);
				result = Factory.Load<TranslatableDataFieldTestCase.DummyWithTranslatable>(resultQuery);
				AssertContainsExactElementsInAnyOrder(new[] { "05", "50" }, result.Select(x => (string)x.Z0_Code));
			}
		}

		void AddData(string number, string english, string chinese)
		{
			var dummy = Factory.New<TranslatableDataFieldTestCase.DummyWithTranslatable>();
			dummy.Z0_Code = number;
			dummy.Z0_Description = english;
			var key = dummy.Z0_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(null, english).ResourceKey;
			mockRes.Put(key, new ResourceStringData(key, chinese));
		}

		IMockResourceStringCache mockRes;

		public override void TestIsExpensiveQuery()
		{
			Assert(true);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override ModuleTranslatableTextFilter GetNewModuleFilter()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			{
				var collection = new ModuleFilterCollection();
				collection.AddFiltersForTranslatableText("Description", DummyBizoSchema.Z0_Description, typeof(TranslatableDataFieldTestCase.DummyWithTranslatable), (NoResString)"Description");
				return (ModuleTranslatableTextFilter)collection["Description_Local"];
			}
		}

		protected override ZString ExpectedDescription
		{
			get
			{
				return "Description_Local";
			}
		}
	}
}
