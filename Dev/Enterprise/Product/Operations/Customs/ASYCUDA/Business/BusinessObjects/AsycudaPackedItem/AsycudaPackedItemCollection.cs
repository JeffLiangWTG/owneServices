using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IAsycudaPackedItemCollection<out TChild, out TMaster> : IBusinessObjectCollection
		where TChild : AsycudaPackedItem
		where TMaster : AsycudaPack
	{
		new TChild this[int i] { get; }
		new int Count { get; }

		new TChild AddNew();

		void Load();
		void RemoveAndDeleteAll();
		IDisposable SuspendSettingHasChanges();
		void RemoveAndDelete(BusinessObject child);
	}

	public class AsycudaPackedItemCollection<TChild, TMaster> : BusinessObjectCollection<TChild>, IAsycudaPackedItemCollection<TChild, TMaster>
		where TChild : AsycudaPackedItem
		where TMaster : AsycudaPack
	{
		public AsycudaPackedItemCollection(TMaster pack)
			: base(pack.Factory)
		{
			this.pack = pack;
			Bill = pack.Bill;
		}
		protected readonly TMaster pack;
		AsycudaBill Bill { get; }

		protected override ZQuery CreateRelationshipFilter()
		{
			var pivotQuery = new ZDBOnlySubQuery(typeof(ManifestBase.AsycudaPackPackedItemPivot), AsycudaPackPackedItemPivotSchema.PK);
			pivotQuery.AddToFilter(AsycudaPackPackedItemPivotSchema.APP_APA_Pack, pack.PK);

			var packedItemQuery = new ZDBOnlyQuery(typeof(AsycudaPackedItem));
			packedItemQuery.AddSubQuery(AsycudaPackedItemSchema.PK, AsycudaPackPackedItemPivotSchema.APP_API_Item, pivotQuery, JoinCondition.And);
			return packedItemQuery;
		}
		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var packedItem = (TChild)child;
			packedItem.API_ABL_Bill = pack.APA_ABL_Bill;	
			var pivot = pack.PackedItems.AddNew();
			pivot.APP_API_Item = packedItem.PK;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			Bill?.PackedItems?.RemoveFromRelationship(bizO);
		}
	}
}
