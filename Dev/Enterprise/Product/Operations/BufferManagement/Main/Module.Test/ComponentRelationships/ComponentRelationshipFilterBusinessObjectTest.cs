using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ComponentRelationshipFilterBusinessObject))]
	class ComponentRelationshipFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestNameFilter()
		{
			var bucketComponent = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Component Bucket", isActive: true, system);
			var viewComponent = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.ComponentRelationship, name: "Component View", isActive: true);

			var bizo = GetNewFilterStripBusinessObject();
			var nameFilter = (ModuleTextFilter)bizo["Name"];
			nameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			nameFilter.IsActive = true;
			nameFilter.Property = "Component";

			AssertContainsExactElementsInAnyOrder(
				"WHEN filtering 'name' THEN should only show view-component with exact name",
				new[] { viewComponent },
				Factory.Load<BMComponent>(bizo.Filter));

			nameFilter.Property = "Component ???";
			AssertContainsExactElementsInAnyOrder(
				"WHEN filtering non existence name THEN return empty",
				System.Array.Empty<BMComponent>(),
				Factory.Load<BMComponent>(bizo.Filter));
		}

		public void TestActiveStatusFilter()
		{
			var bucketComponent = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Component Bucket", isActive: true, system);
			var viewComponent = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.ComponentRelationship, name: "Component View", isActive: true);

			var bizo = GetNewFilterStripBusinessObject();
			var activeStatusFilter = (ModuleTextFilter)bizo["Active Status"];

			activeStatusFilter.IsActive = true;
			activeStatusFilter.Property = "Active";

			AssertContainsExactElementsInAnyOrder(
				"WHEN filtering 'IsActive' THEN should only show view-component with IsActive",
				new[] { viewComponent },
				Factory.Load<BMComponent>(bizo.Filter));

			activeStatusFilter.Property = "Inactive";
			AssertContainsExactElementsInAnyOrder(
				"WHEN filtering IsActive=false THEN return empty",
				System.Array.Empty<BMComponent>(),
				Factory.Load<BMComponent>(bizo.Filter));
		}

		public void TestComponents()
		{
			var viewComponent1 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.ComponentRelationship, name: "Component View 1", isActive: true);
			var relatedComponent1 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Related", isActive: true, system);

			BMSTestHelper.CreateComponentRelationshipLink(Factory, viewComponent1, relatedComponent1);

			var viewComponent2 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.ComponentRelationship, name: "Component View 2", isActive: true);

			Factory.Save();

			var bizo = GetNewFilterStripBusinessObject();
			var componentFilter = (ModuleGuidPivotFilter)bizo["Components"];
			componentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var textFilterStrip = componentFilter.SelectedFilters.AddTextFilterStrip("Name", "Related");
			componentFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(
				"WHEN filtering 'Components' THEN should only show view-component with related components",
				new[] { viewComponent1 },
				Factory.Load<BMComponent>(bizo.Filter));

			textFilterStrip.Property = "Component ???";
			AssertContainsExactElementsInAnyOrder(
				"WHEN filtering non existence name THEN return empty",
				System.Array.Empty<BMComponent>(),
				Factory.Load<BMComponent>(bizo.Filter));
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return module.FilterBusinessObject;
		}

		protected override void SetUp()
		{
			base.SetUp();
			system = Factory.NewWithValidTestData<BMSystem>();
			module = new ComponentRelationshipModule(); // ActiveStatus filter is added when ZFilterGridModule.FilterBusinessObject
		}
		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}

		BMSystem system;
		ComponentRelationshipModule module;

		#endregion
	}
}
