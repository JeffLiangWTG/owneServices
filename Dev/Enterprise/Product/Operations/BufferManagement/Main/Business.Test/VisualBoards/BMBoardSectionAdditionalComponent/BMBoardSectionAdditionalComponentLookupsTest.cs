using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardSectionAdditionalComponentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalComponentLookupWhenSystemIsNull_ShouldNotThrowAnException()
		{
			var board = Factory.New<BMBoard>();
			var section = board.Sections.AddNew();
			var additionalComponent = Factory.New<BMComponent>();
			AssertNotNull("Precondition", section.Board);
			AssertNull("Precondition", section.Board.System);

			var message = "Given section.Board.System is null, when adding an additional component, then should not throw null reference exception.";
			AssertNoExceptionThrown(message, () => BMSTestHelper.CreateAdditionalComponent(section, additionalComponent));
		}

		public void TestComponents()
		{
			var lookups = new BMBoardSectionAdditionalComponentLookups(Factory.New<BMBoardSectionAdditionalComponent>());
			AssertNotNull(lookups.Components);

			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();

			var otherSystem = Factory.New<BMSystem>();
			var otherComponent = otherSystem.Components.AddNew();
			otherComponent.FC_Name = "Other";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var addComp = Factory.NewWithValidTestData<BMComponent>();
			addComp.FC_Name = "addComp";
			var additionalComponent = BMSTestHelper.CreateAdditionalComponent(section, addComp);

			AssertEquals("Should return all components since no component is selected in section", 3, additionalComponent.Lookups.Components.Count);

			var defaults = additionalComponent.Lookups.Components.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			AssertEquals(1, defaults.Count());
			AssertEquals("BMSystem filter should be defaulted to addComp's system", addComp.System.PK, defaults.First().Value);

			var otherAdditionalComponent = BMSTestHelper.CreateAdditionalComponent(section, otherComponent);
			defaults = otherAdditionalComponent.Lookups.Components.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			AssertEquals(1, defaults.Count());
			AssertEquals("BMSystem filter should be defaulted to 'otherSystem'", otherSystem.PK, defaults.First().Value);

			otherAdditionalComponent.BSA_FC_Component = ZGuid.Empty;
			defaults = otherAdditionalComponent.Lookups.Components.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			AssertEquals(1, defaults.Count());
			AssertEquals("BMSystem filter should be defaulted to 'system'", system.PK, defaults.First().Value);
		}

		public void TestComponents_ShouldIncludeComponentRelationships()
		{
			var system = BMSTestHelper.CreateSystem(Factory);

			var buffer = BMSTestHelper.CreateBuffer(system);
			var bucket = BMSTestHelper.CreateBucket(system);
			var relationship = BMSTestHelper.CreateComponentRelationship(Factory);

			var section = BMSTestHelper.CreateBoardSection(buffer);
			var additionalComponent = section.SectionConfiguration.AdditionalComponents.AddNew();

			AssertContainsExactElementsInAnyOrder("Buffers, buckets, and relationships may all be valid additional components.", new[] { buffer, bucket, relationship }, additionalComponent.Lookups.Components);
		}
	}
}
