using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentCollection))]
	public class BMComponentCollectionTest : ActiveBusinessObjectCollectionTestCase<BMComponentCollection>
	{
		public void TestCollectionForBMSystem_ShouldNotContainChildComponents()
		{
			var system = Factory.New<BMSystem>();
			var cmp = system.Components.AddNew();
			var childComponent = system.Components.AddNew();
			childComponent.FC_FC_ParentComponent = cmp.PK;

			var collection = new BMComponentCollection(system);
			AssertEquals(1, collection.Count);
			AssertEquals(cmp.PK, collection[0].PK);
		}

		public void TestCollectionForParentComponent_ShouldOnlyContainChildComponents()
		{
			var system = Factory.New<BMSystem>();
			var cmp = system.Components.AddNew();
			var childComponent = cmp.ChildComponents.AddNew();

			var collection = new BMComponentCollection(cmp);
			AssertEquals(1, collection.Count);
			AssertEquals(childComponent.PK, collection[0].PK);
			AssertEquals(cmp.PK, childComponent.FC_FC_ParentComponent);
			AssertEquals(system.PK, childComponent.FC_FS_System);
			AssertEquals(BMComponentTypeList.Codes.Buffer, childComponent.FC_Type);
		}

		public void TestCollection_FilterRelationships()
		{
			var filteredComponents = CreateComponentsOfAllTypes().Where(component => component.FC_Type != BMComponentTypeList.Codes.ComponentRelationship);

			AssertContainsExactElementsInAnyOrder("This should find all BMComponents excluding component relationships, and yet...",
													filteredComponents, new BMComponentCollection(Factory));
			AssertContainsExactElementsInAnyOrder("This should find all BMComponents excluding component relationships, and yet...",
													filteredComponents, new BMComponentCollection(Factory, new ZQuery()));
		}

		public void TestCollection_CanIncludeRelationships()
		{
			var allComponents = CreateComponentsOfAllTypes();

			AssertContainsExactElementsInAnyOrder("This shouldn't filter by type, and yet...",
													allComponents, new BMComponentCollection(Factory, new ZQuery(), false));
		}

		#region Implementation

		IEnumerable<BMComponent> CreateComponentsOfAllTypes()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var components = new List<BMComponent>();

			foreach (var pair in new BMComponentTypeList().ToArray())
			{
				components.Add(BMSTestHelper.CreateComponent(Factory, pair.Code, name: pair.Description, system: system));
			}

			return components;
		}

		protected override BMComponentCollection GetCollectionToTest()
		{
			return new BMComponentCollection(Factory);
		}

		#endregion
	}
}
