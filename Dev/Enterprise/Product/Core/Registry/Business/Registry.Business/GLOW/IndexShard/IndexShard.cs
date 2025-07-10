using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.CW1.Resources;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class IndexShard : RegistryBusinessObjectTemplate
	{
		#region Schema

		static class Schema
		{
			public const string IndexTableName = "IndexTableName";
			public const string ShardIndex = "ShardIndex";
			public const string IsOverridden = "IsOverridden";
		}

		#endregion

		#region Properties

		#region IndexTableName

		public ZString IndexTableName
		{
			get => indexTableName;
			set
			{
				SetNonPersistentPropertyValue<ZString>(IndexTableNameInfo, ref indexTableName, value);
				if (!IsValidationSuspended)
				{
					ValidateIndexTableName();
				}
			}
		}

		public ZPropertyInfo IndexTableNameInfo => GetZPropertyInfo(IndexShard.Schema.IndexTableName);

		ZString indexTableName;

		public bool IndexTableName_ReadOnly
		{
			get
			{
				return DefaultIndexShard.DefaultIndexShardMap.ContainsKey(IndexTableName);
			}
		}

		public CodeDescriptionPairList IndexTableNames
		{
			get
			{
				var tables = DefaultIndexShard.AllTableList.Select(ele => new CodeDescriptionPair(ele, ele)).ToList();
				var result = new CodeDescriptionPairList();
				result.AddRange(tables);
				return result;
			}
		}

		#endregion

		#region ShardIndex
		public ZInt ShardIndex
		{
			get => shardIndex;
			set
			{
				SetNonPersistentPropertyValue<ZInt>(ShardIndexInfo, ref shardIndex, value);
				if (!IsValidationSuspended)
				{
					ValidateShardIndex();
				}
			}
		}

		public ZPropertyInfo ShardIndexInfo => GetZPropertyInfo(IndexShard.Schema.ShardIndex);

		ZInt shardIndex;

		public bool ShardIndex_ReadOnly
		{
			get
			{
				return !IsOverridden;
			}
		}
		#endregion

		#region IsOverridden

		public ZBool IsOverridden
		{
			get => isOverridden;
			set
			{
				if (value == ZBool.False)
				{
					ShardIndex = DefaultIndexShard.DefaultIndexShardMap.TryGetValue(IndexTableName, out var result) ? result : 0;
					ShardIndexInfo.ClearAllNotifications();
				}
				SetNonPersistentPropertyValue<ZBool>(IsOverriddenInfo, ref isOverridden, value);
			}
		}

		public ZPropertyInfo IsOverriddenInfo => GetZPropertyInfo(IndexShard.Schema.IsOverridden);

		ZBool isOverridden;

		#endregion

		#endregion

		#region Validation
		public static ZString NegativeShard => Res.GetString("D23A2031-83C4-4B75-8558-EADDFF4D514B", "Please enter a non-negative shard.");
		public static ZString InvalidTable => Res.GetString("6342F8B0-701A-4BC3-B02B-70EE42EC05CF", "Please select a table from the list.");

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateIndexTableName();
			ValidateShardIndex();
		}

		protected void ValidateIndexTableName()
		{
			IndexTableNameInfo.ClearAllNotifications();
			if (!DefaultIndexShard.AllTableList.Contains(IndexTableName.ToString()))
			{
				IndexTableNameInfo.AddError(InvalidTable);
			}
			MandatoryValidation.CheckEntered(IndexTableNameInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(IndexTableNameInfo);
		}

		protected void ValidateShardIndex()
		{
			ShardIndexInfo.ClearAllNotifications();
			if (ShardIndex < 0)
			{
				ShardIndexInfo.AddError(NegativeShard);
			}
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IndexShard();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var mapEntry = (IndexShard)clone;
			mapEntry.ShardIndex = ShardIndex;
			mapEntry.IndexTableName = IndexTableName;
			mapEntry.IsOverridden = IsOverridden;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			if (IsOverridden)
			{
				base.WriteElements(writer);
				writer.WriteElementString("TableName", IndexTableName);
				writer.WriteElementString("ShardIndex", ShardIndex.ToString());
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IndexTableName = reader.ReadElementString("TableName");
			ShardIndex = int.Parse(reader.ReadElementString("ShardIndex"));
			IsOverridden = true;
		}

		#endregion
	}
}
