using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	class ComponentRelationshipLinkLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestComponentFroms_FilterComponentRelationship()
		{
			var disconnectedLinkLookups = new ComponentRelationshipLinkLookups(Factory.New<ComponentRelationshipLink>());

			AssertNotNull("Precondition", disconnectedLinkLookups);
			AssertContainsExactElementsInAnyOrder("ComponentFroms should only list ComponentRelationships", new[] { componentRelationship }, disconnectedLinkLookups.ComponentFroms.ToArray());
		}

		public void TestComponentTos_FilterTypesAndSubcomponents()
		{
			var disconnectedLinkLookups = new ComponentRelationshipLinkLookups(Factory.New<ComponentRelationshipLink>());

			AssertNotNull("Precondition", link1.Lookups.ComponentTos);
			AssertContainsExactElementsInAnyOrder("ComponentTos should include buckets and buffers, and exclude subcomponents.", new[] { bucket, buffer1, buffer2 }, link1.Lookups.ComponentTos.ToArray());
		}

		public void TestComponentTos()
		{
			var bucketFilter = new FilterBusinessObjectDefault("Component Type", "Property", (ZString)BMComponentTypeList.Codes.Bucket, true);
			var bufferFilter = new FilterBusinessObjectDefault("Component Type", "Property", (ZString)BMComponentTypeList.Codes.Buffer, true);

			link2.FL_FC_ComponentTo = bucket.PK;

			var defaults1 = link1.Lookups.ComponentTos.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			var defaults2 = link2.Lookups.ComponentTos.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();

			AssertContainsExactElementsInAnyOrder("If all other components in the relationship share a type, default filters should include that type.", new[] { bucketFilter }, defaults1);
			AssertEquals("If this is the only link to a component in the relationship, there should have no default filters.", 0, defaults2.Count());

			link2.FL_FC_ComponentTo = buffer1.PK;

			defaults1 = link1.Lookups.ComponentTos.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			defaults2 = link2.Lookups.ComponentTos.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();

			AssertContainsExactElementsInAnyOrder("If all other components in the relationship share a type, default filters should include that type.", new[] { bufferFilter }, defaults1);
			AssertEquals("If this is the only link to a component in the relationship, there should have no default filters.", 0, defaults2.Count());

			BMSTestHelper.CreateComponentRelationshipLink(Factory, componentRelationship, bucket);

			defaults1 = link1.Lookups.ComponentTos.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			defaults2 = link2.Lookups.ComponentTos.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();

			AssertEquals("If other components in the relationship have different types, there should be no default filters.", 0, defaults1.Count());
			AssertContainsExactElementsInAnyOrder("If all other components in the relationship share a type, default filters should include that type.", new[] { bucketFilter }, defaults2);
		}

		BMSystem system;
		ComponentRelationship componentRelationship;
		ComponentRelationshipLink link1;
		ComponentRelationshipLink link2;
		BMComponent bucket;
		BMComponent buffer1;
		BMComponent buffer2;

		protected override void SetUp()
		{
			base.SetUp();

			system = BMSTestHelper.CreateSystem(Factory);
			bucket = BMSTestHelper.CreateBucket(system);
			buffer1 = BMSTestHelper.CreateBuffer(system);
			buffer2 = BMSTestHelper.CreateBuffer(system);
			var childComponent = BMSTestHelper.CreateSubBuffer(buffer1);
			componentRelationship = BMSTestHelper.CreateComponentRelationship(Factory);

			link1 = BMSTestHelper.CreateComponentRelationshipLink(Factory, componentFrom: componentRelationship);
			link2 = BMSTestHelper.CreateComponentRelationshipLink(Factory, componentFrom: componentRelationship);
		}
	}
}
