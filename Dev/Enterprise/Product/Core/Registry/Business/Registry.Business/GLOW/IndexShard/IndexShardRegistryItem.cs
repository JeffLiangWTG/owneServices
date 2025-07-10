using System.Linq;
using CargoWise.Glow.Model.CW1.Resources;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class IndexShardRegistryItem : StronglyTypedRegistryItem<IndexShardList>
	{
		public IndexShardRegistryItem(string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new IndexShardListRegistryDataType(), storage, options, new IndexShardList()))
		{
		}

		public IndexShardRegistryItem(string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			IndexShardList list)
			: base(new RegistryItemImpl(name, category, caption, hint, new IndexShardListRegistryDataType(), storage, options, list))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.IndexShardRegistryItemEditor, Enterprise.Registry.GUI")]
	public class IndexShardListRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IndexShardList>
	{
		protected override byte[] SerialiseCore(IndexShardList value)
		{
			var overridedItems = new IndexShardList();
			foreach (var item in value)
			{
				if (((IndexShard)item).IsOverridden)
				{
					overridedItems.Add(new IndexShard() { IndexTableName = ((IndexShard)item).IndexTableName, ShardIndex = ((IndexShard)item).ShardIndex, IsOverridden = true });
				}
			}
			return base.SerialiseCore(overridedItems);
		}

		protected override IndexShardList DeserialiseCore(byte[] value)
		{
			var deserializeObject = base.DeserialiseCore(value);
			var result = new IndexShardList();
			var defaultIndexShard = DefaultIndexShard.DefaultIndexShardMap;

			foreach (var keyValuePair in defaultIndexShard)
			{
				var newItem = new IndexShard() { IndexTableName = keyValuePair.Key, ShardIndex = keyValuePair.Value, IsOverridden = false };
				var existingItem = (IndexShard)deserializeObject.FirstOrDefault(item => ((IndexShard)item).IndexTableName == keyValuePair.Key);
				if (existingItem != null)
				{
					newItem.ShardIndex = existingItem.ShardIndex;
					newItem.IsOverridden = true;
				}
				result.Add(newItem);
			}

			foreach (var item in deserializeObject)
			{
				if(!defaultIndexShard.ContainsKey(((IndexShard)item).IndexTableName))
				{
					((IndexShard)item).IsOverridden = true; 
					result.Add(item);
				}
			}

			return result;
		}
	}
}
