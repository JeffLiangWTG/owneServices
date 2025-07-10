using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaPackPackedItemPivotCollection<TAsycudaPackedItem, TAsycudaPack> : AsycudaPackPackedItemPivotCollection
		where TAsycudaPackedItem : AsycudaPackedItem
		where TAsycudaPack : AsycudaPack
	{
		public AsycudaPackPackedItemPivotCollection(TAsycudaPackedItem packedItem)
			: base(packedItem)
		{
		}

		public AsycudaPackPackedItemPivotCollection(TAsycudaPack pack)
			: base(pack)
		{
		}

		public override AsycudaPackedItem AddNewPackedItem()
		{
			if (Master is TAsycudaPack pack)
			{
				var packedItem = (TAsycudaPackedItem)Factory.New(pack.GetPackedItemType());
				packedItem.API_ABL_Bill = pack.APA_ABL_Bill;
				packedItem.SetDetachedPack(pack);
				packedItem.DefaultFrom(pack);

				var pivot = AddNew();
				pivot.DefaultFrom(pack, packedItem);
				return packedItem;
			}

			return null;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			if (Master is TAsycudaPack pack && pack.IsNonePackedItemRelationship)
			{
				result = ZQuery.NoResultQuery;
			}
			return result;
		}
	}

	public abstract class AsycudaPackPackedItemPivotCollection : DependentBusinessObjectCollection<AsycudaPackPackedItemPivot, BusinessObject>
	{
		public AsycudaPackPackedItemPivotCollection(AsycudaPackedItem packedItem)
			: base(packedItem)
		{
			this.fkSchemaColumnInDependent = AsycudaPackPackedItemPivotSchema.APP_API_Item;
			IsManagedForDataRefresh = true;
		}

		public AsycudaPackPackedItemPivotCollection(AsycudaPack pack)
			: base(pack)
		{
			this.fkSchemaColumnInDependent = AsycudaPackPackedItemPivotSchema.APP_APA_Pack;
			IsManagedForDataRefresh = true;
		}

		public abstract AsycudaPackedItem AddNewPackedItem();

		public enum RelationshipType { Many, One, None }

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return fkSchemaColumnInDependent; }
		}
		readonly SchemaGuidColumn fkSchemaColumnInDependent;

		#region Get Pivot

		public AsycudaPackPackedItemPivot GetRelatedPivot(AsycudaPack package)
		{
			return package != null ? this.Cast<AsycudaPackPackedItemPivot>().FirstOrDefault(c => c.APP_APA_Pack == package.PK && !c.IsDeleted) : null;
		}

		#endregion

		#region Add Pivot

		public AsycudaPackPackedItemPivot AddPivotFor(AsycudaPack package)
		{
			var pivot = GetRelatedPivot(package);

			if (pivot == null)
			{
				pivot = AddNew();
				pivot.APP_APA_Pack = package.PK;
			}

			return pivot;
		}

		#endregion

		#region Delete Pivot

		public void DeletePivotFor(AsycudaPack package)
		{
			var pivot = GetRelatedPivot(package) as BusinessObject;

			if (pivot != null)
			{
				RemoveAndDelete(pivot);
			}
		}

		#endregion
	}
}
