using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowTaskTypesRegistryItem))]
	sealed class WorkflowTaskTypesRegistryItemTest : StronglyTypedRegistryItemTestCase<CategorisedWorkflowTaskTypesCollection>
	{
		public void TestCheckingCompletionStatementsDoesNotAddNewItemsToThisCollection()
		{
			var count = Item.Value.Count;
			Item.Value.GetCompletionStatementTaskType("!@#");
			AssertEquals(count, Item.Value.Count);
		}

		public void TestProperties()
		{
			AssertNotNull(Item.DefaultValue);
			AssertEquals(typeof(WorkflowTaskTypesRegistryDataType), Item.DataType.GetType());
		}

		protected override StronglyTypedRegistryItem<CategorisedWorkflowTaskTypesCollection, CategorisedWorkflowTaskTypesCollection> GetNewRegistryItem()
		{
			return new WorkflowTaskTypesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		public void TestCacheCompletionStatementTaskTypes()
		{
			var registryItem = new WorkflowTaskTypesRegistryItem("ProcessManagerTaskTypes",
		RawDataRegistry.Categories.WorkflowManager,
		null,
		null,
		RegistryStorageFlags.System | RegistryStorageFlags.Company);

			var collection = registryItem.Value;
			var workflowTaskTypes = collection.GetTaskTypesFromWorkflowCode("WKI");
			var taskType = workflowTaskTypes.AddNew();
			taskType.Code = "BAM";
			taskType.IsCompletionStatementTaskType = true;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals("BAM", registryItem.GetCompletionStatementTaskType("WKI"));

			taskType.IsCompletionStatementTaskType = false;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(string.Empty, registryItem.GetCompletionStatementTaskType("WKI"));
		}
	}
}
