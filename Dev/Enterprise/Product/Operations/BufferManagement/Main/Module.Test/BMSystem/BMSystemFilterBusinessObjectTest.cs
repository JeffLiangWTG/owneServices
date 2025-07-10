using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMSystemFilterBusinessObject))]
	class BMSystemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestNameFilter()
		{
			var system1 = Factory.NewWithValidTestData<BMSystem>();
			var system2 = Factory.NewWithValidTestData<BMSystem>();
			system1.FS_Name = "Name1";
			system2.FS_Name = "Name2";

			Factory.Save();

			var filter = new BMSystemFilterBusinessObject();
			((ModuleTextFilter)filter["Name"]).Property = "Name1";
			((ModuleTextFilter)filter["Name"]).IsActive = true;

			var results = Factory.Load<BMSystem>(filter.Filter);
			AssertCollectionContains(system1, results);
			AssertCollectionNotContains(system2, results);
		}

		public void TestDescriptionFilter()
		{
			var system1 = Factory.NewWithValidTestData<BMSystem>();
			var system2 = Factory.NewWithValidTestData<BMSystem>();
			system1.FS_Description = "Desc1";
			system2.FS_Description = "Desc2";

			Factory.Save();

			var filter = new BMSystemFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Desc1";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			var results = Factory.Load<BMSystem>(filter.Filter);
			AssertCollectionContains(system1, results);
			AssertCollectionNotContains(system2, results);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMSystemFilterBusinessObject();
		}

		#endregion
	}
}
