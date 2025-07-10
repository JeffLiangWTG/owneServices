using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowIterationReasonsRegistryItem))]
	sealed class WorkflowIterationReasonsRegistryItemTest : StronglyTypedRegistryItemTestCase<CategorisedWorkflowIterationReasonsCollection>
	{
		public void TestProperties()
		{
			AssertNotNull(Item.DefaultValue);
			AssertEquals(typeof(WorkflowIterationReasonsRegistryDataType), Item.DataType.GetType());
		}

		protected override StronglyTypedRegistryItem<CategorisedWorkflowIterationReasonsCollection, CategorisedWorkflowIterationReasonsCollection> GetNewRegistryItem()
		{
			return new WorkflowIterationReasonsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
