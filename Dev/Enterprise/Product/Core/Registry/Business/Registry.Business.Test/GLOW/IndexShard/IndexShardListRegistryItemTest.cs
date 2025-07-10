using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IndexShardRegistryItem))]
	sealed class IndexShardListRegistryItemTest : StronglyTypedRegistryItemTestCase<IndexShardList>
	{
		StronglyTypedRegistryItem<IndexShardList, IndexShardList> GetRegistryItemWithDefaultValue()
		{
			return new IndexShardRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, new IndexShardList { new IndexShard() { IndexTableName = "CarrierShipmentHeader", ShardIndex = 1, IsOverridden = true } });
		}

		public void TestRegistryDefaultIndexShardList()
		{
			var list = GetRegistryItemWithDefaultValue().DefaultValue;
			AssertEquals(1, list.Count);
			var indexShard = list[0];
			AssertEquals("Default Table", "CarrierShipmentHeader", indexShard.IndexTableName);
			AssertEquals("IsOverrided", true, indexShard.IsOverridden);
			AssertEquals("ShardIndex", 1, indexShard.ShardIndex);
		}

		protected override StronglyTypedRegistryItem<IndexShardList, IndexShardList> GetNewRegistryItem()
		{
			var defaultCollection = new IndexShardList();
			return new IndexShardRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
