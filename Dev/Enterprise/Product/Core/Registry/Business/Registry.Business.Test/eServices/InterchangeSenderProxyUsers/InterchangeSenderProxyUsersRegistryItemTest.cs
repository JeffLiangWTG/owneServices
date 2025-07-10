using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InterchangeSenderProxyUsersRegistryItem))]
	sealed class InterchangeSenderProxyUsersRegistryItemTest : StronglyTypedRegistryItemTestCase<InterchangeSenderProxyUserCollection>
	{
		public void TestProperties()
		{
			AssertNotNull(Item.DefaultValue);
			AssertEquals(typeof(InterchangeSenderProxyUserRegistryDataType), Item.DataType.GetType());
		}

		protected override StronglyTypedRegistryItem<InterchangeSenderProxyUserCollection, InterchangeSenderProxyUserCollection> GetNewRegistryItem()
		{
			return new InterchangeSenderProxyUsersRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new InterchangeSenderProxyUserCollection(new BusinessObjectFactory()));
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
