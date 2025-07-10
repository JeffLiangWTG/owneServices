using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	[RowFetchStrategy(FetchStrategyType = typeof(AsycudaBillRowFetchStrategy))]
	[DependentBusinessObject(typeof(AsycudaManifestHeader), "Bills")]
	public class AsycudaBill : AutoAsycudaBill
		, Integration.Customs.ManifestBase.IAsycudaBill
		, IClusterKeyWorker
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly AsycudaBillTypeDecider TypeDecider = new AsycudaBillTypeDecider();

		public new partial class Schema : AutoAsycudaBill.Schema
		{
			public const string ContainerPK = "ContainerPK";
		}

		protected virtual ZString GetCountryCode() => Header?.AMA_RN_NKCountry ?? ZString.Empty;

		public AsycudaManifestHeader Header
		{
			get
			{
				if (header == null || header.IsDeleted || header.PK != ABL_AMA)
				{
					header = Factory.Load<AsycudaManifestHeader>(ABL_AMA);
				}
				return header;
			}
		}
		AsycudaManifestHeader header;

		public void SetHeader(AsycudaManifestHeader header) => this.header = header;

		[RelatedBusinessObject("Header")]
		public override ZGuid ABL_AMA
		{
			get { return base.ABL_AMA; }
			set { base.ABL_AMA = value; }
		}

		public Type GetPackageContainerLinkType() => GetPackageContainerLinkTypeCore();

		protected virtual Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);

		public Type GetPackType() => GetPackTypeCore();

		protected virtual Type GetPackTypeCore() => typeof(AsycudaPack);

		public Type GetPackedItemType() => GetPackedItemTypeCore();

		protected virtual Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		[ChildEditable]
		public IAsycudaPackCollection<AsycudaPack, AsycudaBill> Packs
		{
			get
			{
				if (packs == null)
				{
					packs = CreateNewAsycudaPackCollection();
					packs.Load();
					RegisterEditableChildObject(packs);
				}
				return packs;
			}
		}
		IAsycudaPackCollection<AsycudaPack, AsycudaBill> packs;

		protected virtual IAsycudaPackCollection<AsycudaPack, AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		[ChildEditable]
		public IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems
		{
			get
			{
				if (packedItems == null)
				{
					packedItems = CreateNewAsycudaBillPackedItemCollection();
					packedItems.Load();
					RegisterEditableChildObject(packedItems);
				}
				return packedItems;
			}
		}
		IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> packedItems;

		protected virtual IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> CreateNewAsycudaBillPackedItemCollection()
		{
			return new AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>(this);
		}

		public AsycudaContainer Container => Factory.Load<AsycudaContainer>(ContainerPK);

		[ResourceStringData("Enterprise.Customs.ManifestBase.AsycudaPack|ContainerPK", Caption = "Container")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Containers))]
		[RelatedBusinessObject(nameof(Container))]
		[BusinessObjectTestExclude]
		public ZGuid ContainerPK
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
						pivot = (AsycudaContainerBillOrPackageLink)Factory.New(typeof(AsycudaContainerBillOrPackageLink));
						pivot.APC_ABL_Bill = PK;
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
						pivot = Factory.Load<AsycudaContainerBillOrPackageLink>(DataHelper.GenerateClusterKeyQuery(ABL_ClusterKey, PK, AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, !IsInDatabase))
							.OrderBy(x => x.APC_SystemCreateTimeUtc)
							.FirstOrDefault();
						RegisterEditableChildObject(pivot);
					}
				}
				return pivot;
			}
		}
		AsycudaContainerBillOrPackageLink pivot;

		public ZPropertyInfo ContainerPKInfo => GetZPropertyInfo(Schema.ContainerPK);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchStrategy.FetchForDelete();
				Packs.RemoveAndDeleteAll();
				Pivot?.Delete();
				PackedItems.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		protected override void DeleteForDataRefresh()
		{
			Packs.RemoveAndDeleteAll();
			base.DeleteForDataRefresh();
		}

		public bool IsManyPackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
		public bool IsNonePackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.None;
		public bool IsOnePackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		public AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship
		{
			get
			{
#if DEBUG
				if (PackedItemRelationshipOverrideForTesting.HasValue)
				{
					return PackedItemRelationshipOverrideForTesting.Value;
				}
#endif
				var header = Header;
				return header != null ? header.PackedItemRelationship : AsycudaPackPackedItemPivotCollection.RelationshipType.None;
			}
		}

#if DEBUG
		public AsycudaPackPackedItemPivotCollection.RelationshipType? PackedItemRelationshipOverrideForTesting;
#endif

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaBillFetchStrategy(this);

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ABL_ClusterKey = Header?.AMA_ClusterKey ?? 1;
		}
#endif

		#region ICancellable

		public override bool IsCancelled
		{
			get => base.IsCancelled || (Header?.IsCancelled ?? false);
			set => base.IsCancelled = value;
		}

		public override string CanReactivate()
		{
			var header = Header;
			if (header != null && header.IsCancelled)
			{
				return ResString.GetMultilingualString("2EF1221F-F4FC-4665-9D86-B3B86EBC0216", "Cannot activate.  This Bill is on Canceled Manifest {0}.", header.AMA_JobReference);
			}

			return null;
		}

		#endregion

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaManifestHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)ABL_AMAInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(AsycudaPack), AsycudaPackSchema.APA_ABL_Bill);
				yield return new ClusterKeyChildInfo(typeof(AsycudaPackedItem), AsycudaPackedItemSchema.API_ABL_Bill);
				yield return new ClusterKeyChildInfo(typeof(AsycudaBillScreening), AsycudaBillScreeningSchema.ASR_ABL);
				yield return new ClusterKeyChildInfo(typeof(AsycudaTax), AsycudaTaxSchema.AET_ABL);
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)ABL_ClusterKeyInfo;

		#endregion

		public AsycudaBillAddress ShipperABLAddress => shipper ??= new AsycudaBillAddress(AsycudaBillAddress.AddressType.Shipper, ABL_OA_ShipperInfo, ABL_ShipperNameInfo, ABL_ShipperStreet1Info, ABL_ShipperStreet2Info, ABL_ShipperCityInfo, ABL_ShipperStateInfo, ABL_ShipperPostcodeInfo, ABL_RN_NKShipperCountryInfo, ABL_ShipperPhoneInfo);
		AsycudaBillAddress shipper;

		public AsycudaBillAddress ConsigneeABLAddress => consignee ??= new AsycudaBillAddress(AsycudaBillAddress.AddressType.Consignee, ABL_OA_ConsigneeInfo, ABL_ConsigneeNameInfo, ABL_ConsigneeStreet1Info, ABL_ConsigneeStreet2Info, ABL_ConsigneeCityInfo, ABL_ConsigneeStateInfo, ABL_ConsigneePostcodeInfo, ABL_RN_NKConsigneeCountryInfo, ABL_ConsigneePhoneInfo);
		AsycudaBillAddress consignee;

		public AsycudaBillAddress NotifyPartyABLAddress => notifyParty ??= new AsycudaBillAddress(AsycudaBillAddress.AddressType.NotifyParty, ABL_OA_NotifyPartyInfo, ABL_NotifyPartyNameInfo, ABL_NotifyPartyStreet1Info, ABL_NotifyPartyStreet2Info, ABL_NotifyPartyCityInfo, ABL_NotifyPartyStateInfo, ABL_NotifyPartyPostcodeInfo, ABL_RN_NKNotifyPartyCountryInfo, ABL_NotifyPartyPhoneInfo);
		AsycudaBillAddress notifyParty;
	}
}
