using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TaskAssignmentRestrictionsRegistryItem))]
	sealed class TaskTypeRestrictionsRegistryItemTest : StronglyTypedRegistryItemTestCase<TaskTypeRestrictionsCollection>
	{
		public void TestProperties()
		{
			AssertNotNull(Item.DefaultValue);
			AssertEquals(typeof(TaskAssignmentRestrictionsRegistryDataType), Item.DataType.GetType());
		}

		protected override StronglyTypedRegistryItem<TaskTypeRestrictionsCollection, TaskTypeRestrictionsCollection> GetNewRegistryItem()
		{
			return new TaskAssignmentRestrictionsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
