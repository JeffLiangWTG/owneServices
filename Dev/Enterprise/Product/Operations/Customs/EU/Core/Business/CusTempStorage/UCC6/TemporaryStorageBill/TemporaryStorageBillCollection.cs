using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface ITemporaryStorageBillCollection<out TBill, out THeader> : ManifestBase.IAsycudaBillCollection<TBill, THeader>
		where TBill : TemporaryStorageBill
		where THeader : TemporaryStorageHeader
	{
	}

	public class TemporaryStorageBillCollection<TBill, THeader> : ManifestBase.AsycudaBillCollection<TBill, THeader>, ITemporaryStorageBillCollection<TBill, THeader>
		where TBill : TemporaryStorageBill
		where THeader : TemporaryStorageHeader
	{
		public TemporaryStorageBillCollection(THeader master)
			: base(master)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var temporaryStorageBill = (TemporaryStorageBill)child;
			temporaryStorageBill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
		}

		protected override bool AllowNewCore => Master.AMA_Calc_HasHouseConsignment;

		protected override ZQuery CreateAdditionalFilter()
		{
			var additionalFilter = base.CreateAdditionalFilter();
			if (Master.HasNoMasterBill)
			{
				additionalFilter.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, TemporaryStorageBill.ChildBolCode);
			}
			return additionalFilter;
		}
	}
}
