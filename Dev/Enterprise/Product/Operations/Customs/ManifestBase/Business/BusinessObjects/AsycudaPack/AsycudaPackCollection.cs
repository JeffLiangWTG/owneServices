using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public interface IAsycudaPackCollection<out TPack, out TBill> : IDependentBusinessObjectCollection
		where TPack : AsycudaPack
		where TBill : AsycudaBill
	{
		new TPack this[int i] { get; }
		new TBill Master { get; }
		new int Count { get; }

		new TPack AddNew();
		TPack AddNew(Type bizOType);

		void Load();
		void RemoveAndDeleteAll();
		IDisposable SuspendSettingHasChanges();
		void RemoveAndDelete(BusinessObject packWithSg);
	}

	public class AsycudaPackCollection<TPack, TBill> : DependentBusinessObjectCollection<TPack, TBill>, IAsycudaPackCollection<TPack, TBill>
		where TPack : AsycudaPack
		where TBill : AsycudaBill
	{
		public AsycudaPackCollection(TBill master) : base(master)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => Master.GetPackType();

		protected override string FkColumnName => AsycudaPackSchema.APA_ABL_Bill.Name;

		protected override ZQuery CreateRelationshipFilter()
		{
			return Master == null
				? ZQuery.NoResultQuery
				: DataHelper.GenerateClusterKeyQuery(Master.ABL_ClusterKey, Master.PK, AsycudaPackSchema.APA_ClusterKey, FKSchemaColumnInDependent, !Master.IsInDatabase);
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			base.OnNonCommittedAdded(bizOAdded);
			var pack = (AsycudaPack)bizOAdded;
			pack.PackedItem?.ClearDetachedPack();
		}

		public new int Count => this.Cast<TPack>().Count();

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (child is AsycudaPack pack && pack.IsOnePackedItemRelationship)
			{
				_ = pack.PackedItem;
			}
		}
	}
}

