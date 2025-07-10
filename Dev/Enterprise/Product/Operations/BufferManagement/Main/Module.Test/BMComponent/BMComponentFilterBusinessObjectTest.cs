using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMComponentFilterBusinessObject))]
	public class BMComponentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestNameFilter()
		{
			var cmp1 = Factory.NewWithValidTestData<BMComponent>();
			var cmp2 = Factory.NewWithValidTestData<BMComponent>();
			cmp1.FC_Name = "Name1";
			cmp2.FC_Name = "Name2";

			Factory.Save();

			var filter = new BMComponentFilterBusinessObject();
			((ModuleTextFilter)filter["Name"]).Property = "Name1";
			((ModuleTextFilter)filter["Name"]).IsActive = true;

			var results = Factory.Load<BMComponent>(filter.Filter);
			AssertCollectionContains(cmp1, results);
			AssertCollectionNotContains(cmp2, results);
		}

		public void TestComponentTypeFilter()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var cmp1 = BMSTestHelper.CreateBuffer(system, "Buffer", 10);
			var cmp2 = BMSTestHelper.CreateBucket(system, "Bucket");

			Factory.Save();

			var filter = new BMComponentFilterBusinessObject();
			((ModuleTextFilter)filter["Component Type"]).Property = "BUF";
			((ModuleTextFilter)filter["Component Type"]).IsActive = true;

			var results = Factory.Load<BMComponent>(filter.Filter);
			AssertCollectionContains(cmp1, results);
			AssertCollectionNotContains(cmp2, results);
		}

		public void TestSystemFilter()
		{
			var system1 = Factory.NewWithValidTestData<BMSystem>();
			var cmp1 = Factory.NewWithValidTestData<BMComponent>();
			system1.Components.Add(cmp1);

			var system2 = Factory.NewWithValidTestData<BMSystem>();
			var cmp2 = Factory.NewWithValidTestData<BMComponent>();
			system2.Components.Add(cmp2);

			Factory.Save();

			var filter = new BMComponentFilterBusinessObject();
			((ModuleGuidFilter)filter["BMSystem"]).Property = system1.PK;
			((ModuleGuidFilter)filter["BMSystem"]).IsActive = true;

			var results = Factory.Load<BMComponent>(filter.Filter);
			AssertCollectionContains(cmp1, results);
			AssertCollectionNotContains(cmp2, results);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMComponentFilterBusinessObject();
		}

		#endregion
	}
}
