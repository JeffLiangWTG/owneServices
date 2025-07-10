using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(WorkQueuesFilterBusinessObject))]
	class WorkQueuesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestOwnerGroup()
		{
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Aaa");
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			queue1.TGM_GG_OwnerGroup = group1.PK;

			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "Bbb");
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			queue2.TGM_GG_OwnerGroup = group2.PK;

			Factory.Save();

			var bizo = new WorkQueuesFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo["Owner Group"];
			filterStrip.IsActive = true;
			filterStrip.Property = group1.PK;

			var rules = Factory.Load<WorkQueue>(bizo.Filter);
			AssertEquals(1, rules.Length);
			AssertCollectionContains(queue1, rules);
		}

		public void TestEmptyFilter_ShouldIncludeOnlyWorkQueues()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "Bbb");

			var bizo = new WorkQueuesFilterBusinessObject();
			var results = Factory.Load<WorkQueue>(bizo.Filter);

			AssertEquals(2, results.Length);
			AssertCollectionContains(queue1, results);
			AssertCollectionContains(queue2, results);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WorkQueuesFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
