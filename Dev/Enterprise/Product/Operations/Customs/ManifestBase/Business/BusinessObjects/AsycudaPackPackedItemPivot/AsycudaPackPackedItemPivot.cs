using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaPackPackedItemPivot : AutoAsycudaPackPackedItemPivot, Integration.Customs.ManifestBase.IAsycudaPackPackedItemPivot, IClusterKeyWorker
	{
		public AsycudaPackPackedItemPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		internal void DefaultFrom(AsycudaPack pack, AsycudaPackedItem packedItem)
		{
			this.pack = Argument.NotNull(pack, nameof(pack));
			this.packedItem = Argument.NotNull(packedItem, nameof(packedItem));

			APP_APA_Pack = pack.PK;
			APP_API_Item = packedItem.PK;
		}

		[RelatedBusinessObject(nameof(Pack))]
		public override ZGuid APP_APA_Pack
		{
			get => base.APP_APA_Pack;
			set
			{
				base.APP_APA_Pack = value;
			}
		}

		public AsycudaPack Pack
		{
			get
			{
				if (pack == null || pack.PK != APP_APA_Pack)
				{
					pack = APP_APA_Pack.IsEmpty ? null : Factory.Load<AsycudaPack>(APP_APA_Pack);
				}
				return pack;
			}
		}
		AsycudaPack pack;

		[RelatedBusinessObject(nameof(PackedItem))]
		public override ZGuid APP_API_Item
		{
			get => base.APP_API_Item;
			set => base.APP_API_Item = value;
		}

		public AsycudaPackedItem PackedItem
		{
			get
			{
				if (packedItem == null || packedItem.PK != APP_API_Item)
				{
					packedItem = APP_API_Item.IsEmpty ? null : Factory.Load<AsycudaPackedItem>(APP_API_Item);
				}
				return packedItem;
			}
		}
		AsycudaPackedItem packedItem;

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaPack);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)APP_APA_PackInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)APP_ClusterKeyInfo;

		#endregion
	}
}
