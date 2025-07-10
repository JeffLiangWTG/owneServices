using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessTemplateReleaseGroupRuleCollection))]
	class ProcessTemplateReleaseGroupRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTemplateReleaseGroupRuleCollection>
	{
		public void TestItemsAddedToCollection_ShouldIncrementSequence()
		{
			var collection = GetCollectionToTest();

			var item1 = collection.AddNew();
			var item2 = collection.AddNew();
			var item3 = collection.AddNew();

			AssertEquals((ZByte)1, item1.PTR_Sequence);
			AssertEquals((ZByte)2, item2.PTR_Sequence);
			AssertEquals((ZByte)3, item3.PTR_Sequence);
		}

		public void TestItemAddedToCollection_WhenSequenceAtMaximum_ShouldNotIncrementSequence()
		{
			var collection = GetCollectionToTest();
			var item1 = collection.AddNew();

			item1.PTR_Sequence = byte.MaxValue;

			var item2 = collection.AddNew();

			AssertEquals(byte.MaxValue, item2.PTR_Sequence);
		}

		protected override ProcessTemplateReleaseGroupRuleCollection GetCollectionToTest()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);

			return new ProcessTemplateReleaseGroupRuleCollection(template);
		}
	}
}
