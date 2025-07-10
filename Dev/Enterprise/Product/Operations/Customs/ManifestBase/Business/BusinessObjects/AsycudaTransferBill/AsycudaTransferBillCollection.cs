using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public interface IAsycudaTransferBillCollection<out TAsycudaTransferBill> : IBusinessObjectCollection<TAsycudaTransferBill>
		where TAsycudaTransferBill : AsycudaTransferBill
	{
	}

	public class AsycudaTransferBillCollection<TAsycudaTransferBill>
		: DependentBusinessObjectCollection<TAsycudaTransferBill, AsycudaTransferHeader>
		, IAsycudaTransferBillCollection<TAsycudaTransferBill>
		where TAsycudaTransferBill : AsycudaTransferBill
	{
		public AsycudaTransferBillCollection(AsycudaTransferHeader master)
			: base(master)
		{
		}

		public AsycudaTransferBillCollection(AsycudaTransferHeader master, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
		}

		public override bool ReadOnly => base.ReadOnly || Master.ReadOnly;
		protected override string FkColumnName => AsycudaTransferBillSchema.ATB_ATF_TransferHeader.Name;

		public IEnumerator<TAsycudaTransferBill> GetEnumerator() => Elements.Cast<TAsycudaTransferBill>().GetEnumerator();
	}
}
