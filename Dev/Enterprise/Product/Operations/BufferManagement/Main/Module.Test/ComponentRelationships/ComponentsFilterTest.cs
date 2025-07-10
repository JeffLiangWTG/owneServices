using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module
{
	[TestedType(typeof(ComponentsFilter))]
	class ComponentsFilterTest : ModuleFilterTestCase<ComponentsFilter>
	{
		public void TestFilter_AnyMatch()
		{
			var componentView1 = CreateViewComponentAndRelatedComponent("ComponentView1", "ComponentRelated1A", "ComponentRelated1B");
			var componentView2 = CreateViewComponentAndRelatedComponent("ComponentView2", "ComponentRelated2A");
			var componentView3 = CreateViewComponentAndRelatedComponent("ComponentView3");

			var componentView4 = CreateViewComponentAndRelatedComponent("ComponentView4");
			var componentRelated12 = Factory.LoadTop1<BMComponent>(new ZQuery(BMComponentSchema.FC_Name, "ComponentRelated1B"));
			BMSTestHelper.CreateComponentRelationshipLink(Factory, componentFrom: componentView4, componentTo: componentRelated12);

			Factory.Save();

			var filter = GetNewModuleFilter();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result0 = Factory.Load<BMComponent>(filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"should result in all components",
				new[] { "ComponentView1", "ComponentView2", "ComponentView4" },
				result0.Select(c => c.FC_Name));

			var filter1 = GetNewModuleFilter();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter1.SelectedFilters.AddTextFilterStrip("Name", "Component Related ???");
			var result1 = Factory.Load<BMComponent>(filter1.Query);
			AssertContainsExactElementsInAnyOrder("should result in nothing", System.Array.Empty<BMComponent>(), result1);

			var filter2 = GetNewModuleFilter();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter2.SelectedFilters.AddTextFilterStrip("Name", "ComponentRelated1B");
			var result2 = Factory.Load<BMComponent>(filter2.Query);
			AssertContainsExactElementsInAnyOrder(
				"Component1B is related to componentView1 and ComponentView4",
				new[] { componentView1, componentView4 },
				result2);

			var filter3 = GetNewModuleFilter();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter3.SelectedFilters.AddTextFilterStrip("Name", "ComponentRelated2A");
			var result3 = Factory.Load<BMComponent>(filter3.Query);
			AssertContainsExactElementsInAnyOrder(
				"ComponentRelated2A is related to ComponentView2",
				new[] { componentView2 },
				result3);
		}

		public void TestFilter_NoneMatch()
		{
			var componentView1 = CreateViewComponentAndRelatedComponent("ComponentView1", "ComponentRelated1A", "ComponentRelated1B");
			var componentView2 = CreateViewComponentAndRelatedComponent("ComponentView2", "ComponentRelated2A");

			Factory.Save();

			var filter = GetNewModuleFilter();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			filter.SelectedFilters.AddTextFilterStrip("Name", "ComponentRelated1B");
			var result = Factory.Load<BMComponent>(filter.Query);
			AssertContainsExactElementsInAnyOrder(
				"ComponentRelated1A, ComponentRelated1B, ComponentView2, ComponentRelated2A don't have ComponentRelated1B",
				new[] { "ComponentRelated1A", "ComponentRelated1B", "ComponentView2", "ComponentRelated2A" },
				result.Select(c => c.FC_Name));
		}

		public void TestFilter_AllMatch()
		{
			var componentView1 = CreateViewComponentAndRelatedComponent("ComponentView1", "ComponentRelated1A", "ComponentRelated1B");
			var componentView2 = CreateViewComponentAndRelatedComponent("ComponentView2", "ComponentRelated2A");

			Factory.Save();

			var filter = GetNewModuleFilter();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			filter.SelectedFilters.AddTextFilterStrip("Name", "ComponentRelated1").SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var result = Factory.Load<BMComponent>(filter.Query);
			AssertContainsExactElementsInAnyOrder(
				"componentView1 has all relatedComponents are matched",
				new[] { "ComponentView1", "ComponentRelated1A", "ComponentRelated1B", "ComponentRelated2A" },
				result.Select(c => c.FC_Name));
		}

		ComponentRelationship CreateViewComponentAndRelatedComponent(string viewComponentName, params string[] relatedComponentNames)
		{
			var viewComponent = BMSTestHelper.CreateComponentRelationship(Factory, viewComponentName);

			foreach (var relatedComponentName in relatedComponentNames)
			{
				var relatedComponent = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, relatedComponentName, system: system);

				BMSTestHelper.CreateComponentRelationshipLink(Factory, componentFrom: viewComponent, componentTo: relatedComponent);
			}

			return viewComponent;
		}

		#region Implementation

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ComponentsFilter GetNewModuleFilter()
		{
			return new ComponentsFilter("Components", new BMComponentCollection(Factory));
		}

		protected override ZString ExpectedDescription => "Components";

		protected override void SetUp()
		{
			base.SetUp();
			system = Factory.NewWithValidTestData<BMSystem>();
		}

		BMSystem system;

		#endregion
	}
}
