using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface ITemporaryStoragePackedItemCollection<out TPackedItem, out TBill> : IAsycudaBillPackedItemCollection<TPackedItem, TBill>, IBusinessObjectCollection<TPackedItem>
		where TPackedItem : TemporaryStoragePackedItem
		where TBill : TemporaryStorageBill
	{
	}

	public class TemporaryStoragePackedItemCollection<TPackedItem, TBill> : AsycudaBillPackedItemCollection<TPackedItem, TBill>, ITemporaryStoragePackedItemCollection<TPackedItem, TBill>
		where TPackedItem : TemporaryStoragePackedItem
		where TBill : TemporaryStorageBill
	{
		public TemporaryStoragePackedItemCollection(TBill master) : base(master)
		{
			MaxCountValidationEnable(99999);
		}

		protected override bool AllowNewCore => Count < MaxCount;
	}
}
