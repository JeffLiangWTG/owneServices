using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class FilterModuleStrategyTest : TestCaseWithDummy
	{
		public void TestFiltersAreAddedAndDescriptionIsMadeUnique()
		{
			var strategy = new FilterModuleStrategyForTest();
			var filters = new ModuleFilterCollection();
			strategy.RunOnModuleFiltersCreated(filters, Dummy.GetType(), Factory);

			AssertEquals("Should contain 5 filters (2 being defaults)", 5, filters.Filter_List.Count);

			AssertNotNull("Should contain a filter called 'Z0_Description'" + FilterModuleStrategy.UniqueSuffix,
					filters["Z0_Description" + FilterModuleStrategy.UniqueSuffix]);

			AssertNotNull("Should contain a filter called 'Z0_AnotherDescription'" + FilterModuleStrategy.UniqueSuffix,
					filters["Z0_AnotherDescription" + FilterModuleStrategy.UniqueSuffix]);

			AssertNotNull("Should contain a filter called 'Z0_YetAnotherDescription'" + FilterModuleStrategy.UniqueSuffix,
					filters["Z0_YetAnotherDescription" + FilterModuleStrategy.UniqueSuffix]);
		}

		public void TestMultilingualDescription()
		{
			FilterModuleStrategyForTest strategy = new FilterModuleStrategyWithMultilinguageDescriptionForTest();
			var filters = new ModuleFilterCollection();
			strategy.RunOnModuleFiltersCreated(filters, Dummy.GetType(), Factory);

			AssertEquals("Wrong multilingual description", "I'm multilingual (System)", filters["Z0_Description" + FilterModuleStrategy.UniqueSuffix].MultilingualDescription);

			AssertEquals("Wrong multilingual description", "Me too! (System)", filters["Z0_AnotherDescription" + FilterModuleStrategy.UniqueSuffix].MultilingualDescription);

			AssertEquals("Wrong multilingual description", "Me three! (System)", filters["Z0_YetAnotherDescription" + FilterModuleStrategy.UniqueSuffix].MultilingualDescription);
		}

		public void TestMultilingualDescriptionDefaultsToDescriptionWhenEmpty()
		{
			var strategy = new FilterModuleStrategyForTest();
			var filters = new ModuleFilterCollection();
			strategy.RunOnModuleFiltersCreated(filters, Dummy.GetType(), Factory);

			AssertEquals("Wrong multilingual description", "Z0_Description (System)", filters["Z0_Description" + FilterModuleStrategy.UniqueSuffix].MultilingualDescription);

			AssertEquals("Wrong multilingual description", "Z0_AnotherDescription (System)", filters["Z0_AnotherDescription" + FilterModuleStrategy.UniqueSuffix].MultilingualDescription);

			AssertEquals("Wrong multilingual description", "Z0_YetAnotherDescription (System)", filters["Z0_YetAnotherDescription" + FilterModuleStrategy.UniqueSuffix].MultilingualDescription);
		}

		#region implementation
		class FilterModuleStrategyForTest : FilterModuleStrategy
		{
			public IFilterControl Control { get; set; }

			public override void RunOnFilterControlInitialisation(IFilterControl control, IBusinessObjectCollection gridCollection)
			{
			}

			protected override IEnumerable<ModuleFilter> FiltersToAdd
			{
				get
				{
					yield return new ModuleTextFilter("Z0_Description", DummyBizoSchema.Z0_VarCharMax);
					yield return new ModuleTextFilter("Z0_AnotherDescription", DummyBizoSchema.Z0_NVarChar);
					yield return new ModuleTextFilter("Z0_YetAnotherDescription", DummyBizoSchema.Z0_NVarCharMax);
				}
			}
		}

		class FilterModuleStrategyWithMultilinguageDescriptionForTest : FilterModuleStrategyForTest
		{
			protected override IEnumerable<ModuleFilter> FiltersToAdd
			{
				get
				{
					yield return new ModuleTextFilter("Z0_Description", DummyBizoSchema.Z0_VarCharMax) { MultilingualDescription = (NoResString)"I'm multilingual" };
					yield return new ModuleTextFilter("Z0_AnotherDescription", DummyBizoSchema.Z0_NVarChar) { MultilingualDescription = (NoResString)"Me too!" };
					yield return new ModuleTextFilter("Z0_YetAnotherDescription", DummyBizoSchema.Z0_NVarCharMax) { MultilingualDescription = (NoResString)"Me three!" };
				}
			}
		}
		#endregion
	}
}
