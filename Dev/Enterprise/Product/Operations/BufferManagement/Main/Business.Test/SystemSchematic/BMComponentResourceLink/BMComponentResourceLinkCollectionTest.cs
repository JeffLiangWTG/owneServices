using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentResourceLinkCollection))]
	class BMComponentResourceLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<BMComponentResourceLinkCollection>
	{
		public void TestCollectionForComponents()
		{
			var system = Factory.New<BMSystem>();
			var component1 = system.Components.AddNew();
			var component2 = system.Components.AddNew();

			var link1 = component1.ResourceLinks.AddNew();
			AssertEquals(component1.PK, link1.FD_FC_Component);

			var link2 = component2.ResourceLinks.AddNew();
			AssertEquals(component2.PK, link2.FD_FC_Component);
		}

		public void TestCollectionForResources()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var collection1 = new BMComponentResourceLinkCollection(staff1);
			var collection2 = new BMComponentResourceLinkCollection(staff2);

			var link1 = collection1.AddNew();
			AssertEquals(staff1.GS_Code, link1.FD_GS_NKResource);

			var link2 = collection2.AddNew();
			AssertEquals(staff2.GS_Code, link2.FD_GS_NKResource);
		}
	}
}
