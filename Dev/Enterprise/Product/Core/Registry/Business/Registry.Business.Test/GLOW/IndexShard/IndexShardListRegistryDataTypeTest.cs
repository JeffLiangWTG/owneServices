using System.Linq;
using CargoWise.Glow.Model.CW1.Resources;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IndexShardListRegistryDataType))]
	sealed class IndexShardListRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IndexShardListRegistryDataType>
	{
		#region Implementation

		protected override string ExpectedEditorName => "IndexShardRegistryItemEditor";

		protected override IndexShardListRegistryDataType GetNewDataType()
		{
			return new IndexShardListRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var indexShardList1 = GlowRegistry.Instance.GlowIndexerShards.DefaultValue;
			var entry1 = indexShardList1.AddNew();
			entry1.IndexTableName = DefaultIndexShard.AllTableList.Except(DefaultIndexShard.DefaultIndexShardMap.Keys).First();
			entry1.ShardIndex = 1;
			entry1.IsOverridden = true;

			var dataType = new IndexShardListRegistryDataType();
			var bytes1 = dataType.Serialise(indexShardList1);
			var bytes2 = dataType.Serialise(GlowRegistry.Instance.GlowIndexerShards.DefaultValue);

			return
			[
				new (indexShardList1, bytes1),
				new (GlowRegistry.Instance.GlowIndexerShards.DefaultValue, bytes2),
			];
		}

		#endregion
	}
}
