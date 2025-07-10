using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public interface IAsycudaTransferHeaderCollection<out TAsycudaTransferHeader> : IBusinessObjectCollection<TAsycudaTransferHeader>
		where TAsycudaTransferHeader : AsycudaTransferHeader
	{
	}

	public class AsycudaTransferHeaderCollection<TAsycudaTransferHeader>
		: DependentBusinessObjectCollection<TAsycudaTransferHeader, AsycudaArrivalHeader>
		, IAsycudaTransferHeaderCollection<TAsycudaTransferHeader>
		where TAsycudaTransferHeader : AsycudaTransferHeader
	{
		public AsycudaTransferHeaderCollection(AsycudaArrivalHeader master) : base(master)
		{
		}

		public AsycudaTransferHeaderCollection(AsycudaArrivalHeader master, BusinessObjectFactory factory) : base(master, factory)
		{
		}

		public AsycudaTransferHeaderCollection(AsycudaArrivalHeader master, ZQuery additionalFilter) : base(master, additionalFilter)
		{
		}

		protected override string FkColumnName => AsycudaTransferHeaderSchema.ATF_ATH_ArrivalHeader.Name;

		public IEnumerator<TAsycudaTransferHeader> GetEnumerator() => Elements.Cast<TAsycudaTransferHeader>().GetEnumerator();
	}
}
