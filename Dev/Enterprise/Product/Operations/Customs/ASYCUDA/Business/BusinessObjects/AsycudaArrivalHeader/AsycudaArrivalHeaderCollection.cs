using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface
		IAsycudaArrivalHeaderCollection<out TAsycudaArrivalHeader> : IActiveBusinessObjectCollection<TAsycudaArrivalHeader>
		where TAsycudaArrivalHeader : AsycudaArrivalHeader
	{
		new TAsycudaArrivalHeader this[int index] { get; }
	}

	public class AsycudaArrivalHeaderCollection<TAsycudaArrivalHeader> : ActiveBusinessObjectCollection<TAsycudaArrivalHeader>,
		IAsycudaArrivalHeaderCollection<TAsycudaArrivalHeader>,
		ISequenceNumberHeader
		where TAsycudaArrivalHeader : AsycudaArrivalHeader
	{
		public AsycudaArrivalHeaderCollection(AsycudaManifestHeader master)
			: base(master.Factory, master, new ZQuery(), AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader)
		{
		}

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this;
	}
}
