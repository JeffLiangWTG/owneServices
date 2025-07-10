using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	public class AsycudaPackedItem : AutoAsycudaPackedItem
		, Integration.Customs.ManifestBase.IAsycudaPackedItem
		, IHugeSequenceNumberLine
		, IClusterKeyWorker
		, IAsycudaTaxTypeSupporter
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly AsycudaPackedItemTypeDecider TypeDecider = new AsycudaPackedItemTypeDecider();

		[RelatedBusinessObject(nameof(Bill))]
		public override ZGuid API_ABL_Bill
		{
			get => base.API_ABL_Bill;
			set
			{
				var oldValue = API_ABL_Bill;
				base.API_ABL_Bill = value;
				if (!IsCopying && oldValue != API_ABL_Bill)
				{
					DetachedFromBill(oldValue);
					var bill = Bill;
					if (bill != null)
					{
						API_ClusterKey = bill.ABL_ClusterKey;
						AttachToBill(bill);
					}
				}
			}
		}

		void AttachToBill(AsycudaBill bill)
		{
			bill.PackedItems.Reload(false);
			bill.PackedItems.SequenceNumberCalculator.RecalculateWhenAdded(this);
		}

		void DetachedFromBill(ZGuid billPK)
		{
			var bill = Factory.Load<AsycudaBill>(billPK);
			if (bill != null)
			{
				bill.PackedItems.Reload(false);
				bill.PackedItems.SequenceNumberCalculator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		[ChildEditable(true)]
		public AsycudaPackPackedItemPivotCollection AsycudaPackPackedItemPivots
		{
			get
			{
				if (asycudaPackPackedItemPivots == null)
				{
					asycudaPackPackedItemPivots = new AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>(this);
					RegisterEditableChildObject(asycudaPackPackedItemPivots);
				}
				asycudaPackPackedItemPivots.Load();
				return asycudaPackPackedItemPivots;
			}
		}
		AsycudaPackPackedItemPivotCollection asycudaPackPackedItemPivots;

		public ZInt SequenceNumber { get => API_LineNo; set => API_LineNo = value; }

		public ZGuid FKToHeader => API_ABL_Bill;

		public AsycudaBill Bill => Factory.Load<AsycudaBill>(API_ABL_Bill);

		public AsycudaPack Pack => IsDetachedPack(detachedPack) ? detachedPack : ((AsycudaPackPackedItemPivot)AsycudaPackPackedItemPivots.FirstOrDefault())?.Pack;

		internal void ClearDetachedPack()
		{
			detachedPack = null;
		}

		internal void SetDetachedPack(AsycudaPack pack)
		{
			if (IsDetachedPack(pack))
			{
				detachedPack = pack;
			}
		}
		AsycudaPack detachedPack;

		bool IsDetachedPack(AsycudaPack pack)
		{
			var packIsLinkedToThisPackedItem = pack != null && AsycudaPackPackedItemPivots.Cast<AsycudaPackPackedItemPivot>().Any(x => x.APP_APA_Pack == pack.PK);
			return packIsLinkedToThisPackedItem && ((INeedRow)pack).Row.RowState == DataRowState.Detached;
		}

		internal void DefaultFrom(AsycudaPack pack)
		{
			DefaultFromCore(pack);
		}

		protected virtual void DefaultFromCore(AsycudaPack pack)
		{
		}

		public override bool SupportsNotes => false;

		public override void OnSaving()
		{
			base.OnSaving();
			if (ShouldDelete)
			{
				Delete();
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				AsycudaPackPackedItemPivots.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		bool ShouldDelete
		{
			get
			{
				var result = false;
				var pack = Pack;
				if (pack != null)
				{
					switch (pack.PackedItemRelationship)
					{
						case AsycudaPackPackedItemPivotCollection.RelationshipType.None:
							result = true;
							break;
						case AsycudaPackPackedItemPivotCollection.RelationshipType.One:
							result = (pack.PackedItem?.PK ?? ZGuid.Empty) != PK;
							break;
					}
				}
				return result;
			}
		}

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaBill);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)API_ABL_BillInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(AsycudaTax), AsycudaTaxSchema.AET_API_AsycudaPackedItem);
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)API_ClusterKeyInfo;

		#endregion

		[ChildEditable]
		public IAsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem> AsycudaTaxes
		{
			get
			{
				if (asycudaTaxes == null)
				{
					asycudaTaxes = CreateNewAsycudaPackedItemTaxCollection();
					asycudaTaxes.Load();
					RegisterEditableChildObject(asycudaTaxes);
				}
				return asycudaTaxes;
			}
		}
		IAsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem> asycudaTaxes;

		protected virtual IAsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem> CreateNewAsycudaPackedItemTaxCollection() => new AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem>(this);

		public Type GetAsycudaTaxType() => GetAsycudaTaxTypeCore();
		protected virtual Type GetAsycudaTaxTypeCore() => typeof(AsycudaTax);
	}
}

