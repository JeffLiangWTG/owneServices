using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	[RowFetchStrategy(FetchStrategyType = typeof(AsycudaPackRowFetchStrategy))]
	[DependentBusinessObject(typeof(AsycudaBill), "Packs")]
	public class AsycudaPack : AutoAsycudaPack
		, Integration.Customs.ManifestBase.IAsycudaPack
		, IClusterKeyWorker
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly AsycudaPackTypeDecider TypeDecider = new AsycudaPackTypeDecider();

		public new class Schema : AutoAsycudaPack.Schema
		{
			public const string ContainerPK = "ContainerPK";
		}

		public Type GetPackedItemType() => GetPackedItemTypeCore();

		protected virtual Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		[ChildEditable]
		[ChildEditableTestExclude]
		public AsycudaPackPackedItemPivotCollection PackedItems
		{
			get
			{
				if (packedItems == null)
				{
					packedItems = CreateNewAsycudaPackCollection();
					packedItems.Load();
					if (!IsNonePackedItemRelationship)
					{
						RegisterEditableChildObject(packedItems);
					}
				}
				return packedItems;
			}
		}
		AsycudaPackPackedItemPivotCollection packedItems;

		protected virtual AsycudaPackPackedItemPivotCollection CreateNewAsycudaPackCollection() => new AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>(this);

		public bool IsManyPackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
		public bool IsNonePackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.None;
		public bool IsOnePackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		public virtual AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship
		{
			get
			{
#if DEBUG
				if (PackedItemRelationshipOverrideForTesting.HasValue)
				{
					return PackedItemRelationshipOverrideForTesting.Value;
				}
#endif
				var bill = Bill;
				return bill != null ? bill.PackedItemRelationship : AsycudaPackPackedItemPivotCollection.RelationshipType.None;
			}
		}

#if DEBUG
		public AsycudaPackPackedItemPivotCollection.RelationshipType? PackedItemRelationshipOverrideForTesting;
#endif

		public AsycudaPackedItem GetPackedItemFromCollection() => PackedItems.Cast<AsycudaPackPackedItemPivot>().OrderBy(x => x.APP_API_Item).FirstOrDefault()?.PackedItem;

		public AsycudaPackPackedItemPivot[] GetAllPackPackedItemPivots()
		{
			return Factory.Load<AsycudaPackPackedItemPivot>(DataHelper.GenerateClusterKeyQuery(APA_ClusterKey, PK, AsycudaPackPackedItemPivotSchema.APP_ClusterKey, AsycudaPackPackedItemPivotSchema.APP_APA_Pack, !IsInDatabase));
		}

		public AsycudaPackedItem[] GetAllPackedItems()
		{
			return Factory.Load(GetPackedItemType(), GetPackedItemsFromPivotQuery()).Cast<AsycudaPackedItem>().OrderBy(x => x.PK).ToArray();
		}

		AsycudaPackedItem[] GetAllPackedItemsToDelete()
		{
			return GetAllPackedItems().Where(x => !x.Bill.IsManyPackedItemRelationship).ToArray();
		}

		public ZQuery GetPackedItemsFromPivotQuery()
		{
			var zQuery = new ZQuery(AsycudaPackedItemSchema.API_ClusterKey, APA_ClusterKey);
			zQuery.AddToFilter(AsycudaPackedItemSchema.PK, GetAllPackPackedItemPivots().Select(x => x.APP_API_Item));
			zQuery.FetchOnlyFromLocalCache = !IsInDatabase;
			return zQuery;
		}

		public AsycudaPackedItem PackedItem
		{
			get
			{
				if (!IsOnePackedItemRelationship)
				{
					return (AsycudaPackedItem)Factory.GetNull(GetPackedItemType());
				}
				else if (IsOnePackedItemRelationship && !IsDeleted && (packItem == null || packItem.IsDeleted))
				{
					if (packItem != null)
					{
						UnRegisterEditableChildObject(packItem);
						packItem = null;
					}
					if (!IsDeleting)
					{
						LoadPackedItem();
					}
				}
				return packItem;
			}
		}
		AsycudaPackedItem packItem;

		void LoadPackedItem()
		{
			packItem = GetAllPackedItems().FirstOrDefault();
			if (packItem == null)
			{
				packItem = PackedItems.AddNewPackedItem();
			}
			RegisterEditableChildObject(packItem);
		}

		public AsycudaContainer Container => Factory.Load<AsycudaContainer>(ContainerPK);

		[ResourceStringData("Enterprise.Customs.ManifestBase.AsycudaPack|ContainerPK", Caption = "Container")]
		[List("Lookups.Containers")]
		[RelatedBusinessObject("Container")]
		[BusinessObjectTestExclude]
		public virtual ZGuid ContainerPK
		{
			get => Pivot?.APC_ACN_Container ?? ZGuid.Empty;
			set
			{
				var currentPivot = Pivot;
				var oldValue = currentPivot?.APC_ACN_Container ?? ZGuid.Empty;
				if (value.IsEmpty)
				{
					if (currentPivot != null)
					{
						currentPivot.Delete();
						pivot = null;
					}
				}
				else
				{
					if (currentPivot == null)
					{
						// use cached variable for performance
						var packageContainerLinkType = Bill?.GetPackageContainerLinkType() ?? typeof(AsycudaContainerBillOrPackageLink);
						pivot = (AsycudaContainerBillOrPackageLink)Factory.New(packageContainerLinkType);
						pivot.APC_APA_Pack = PK;
						RegisterEditableChildObject(pivot);
					}

					pivot.APC_ACN_Container = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateContainerPK();
				}
				ContainerPKInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ContainerPKInfo => GetZPropertyInfo(Schema.ContainerPK);

		public AsycudaContainerBillOrPackageLink Pivot
		{
			get
			{
				if (pivot == null || pivot.IsDeleted)
				{
					if (IsDeleted)
					{
						pivot = null;
					}
					else
					{
						pivot = Factory.Load<AsycudaContainerBillOrPackageLink>(DataHelper.GenerateClusterKeyQuery(APA_ClusterKey, PK, AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, !IsInDatabase)).OrderBy(x => x.PK).FirstOrDefault();
						RegisterEditableChildObject(pivot);
					}
				}
				return pivot;
			}
		}
		AsycudaContainerBillOrPackageLink pivot;

		public AsycudaBill Bill => Factory.Load<AsycudaBill>(APA_ABL_Bill);

		[RelatedBusinessObject("Bill")]
		public override ZGuid APA_ABL_Bill { get => base.APA_ABL_Bill; set => base.APA_ABL_Bill = value; }

		[List("Lookups.PackUQList")]
		public override ZString APA_PackUQ { get => base.APA_PackUQ; set => base.APA_PackUQ = value; }

		[List("Lookups.WeightUQList")]
		public override ZString APA_WeightUQ { get => base.APA_WeightUQ; set => base.APA_WeightUQ = value; }

		[List("Lookups.VolumeUQList")]
		public override ZString APA_VolumeUQ { get => base.APA_VolumeUQ; set => base.APA_VolumeUQ = value; }

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchStrategy.FetchForDelete();
				GetAllPackedItemsToDelete().DeleteAll(true);
				GetAllPackPackedItemPivots().DeleteAll();
				Pivot?.Delete();
			}
			base.Delete();
		}

		public override bool SupportsNotes => false;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaPackFetchStrategy(this);

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaBill);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)APA_ABL_BillInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(AsycudaPackPackedItemPivot), AsycudaPackPackedItemPivotSchema.APP_APA_Pack);
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)APA_ClusterKeyInfo;

		#endregion
	}
}

