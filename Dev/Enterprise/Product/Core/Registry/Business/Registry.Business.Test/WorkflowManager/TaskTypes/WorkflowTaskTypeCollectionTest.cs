using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowTaskTypeCollection))]
	sealed class WorkflowTaskTypeCollectionTest : CodeDescriptionBoolCollectionAbstractTest<WorkflowTaskTypeCollection>
	{
		public void TestWorkflowTaskTypeCollectionClone()
		{
			IRegistryBusiness clone = Collection.Clone(Collection.CurrentFallbackLevel, Collection.Factory);
			AssertNotNull(clone);
			AssertEquals(typeof(WorkflowTaskTypeCollection), clone.GetType());
		}

		public void TestAddNewAndIndexer()
		{
			var taskType = Collection.AddNew();
			AssertNotNull(taskType);
			AssertEquals(taskType, Collection[0]);
		}

		#region Implementation

		protected override WorkflowTaskTypeCollection GetCollectionToTest()
		{
			return new WorkflowTaskTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WorkflowTaskType();
		}

		#endregion
	}
}
