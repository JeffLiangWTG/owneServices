using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.EU.H7.Business
{
	public interface IAsycudaBillCollection<out TBill, out TManifestHeader> : ManifestBase.IAsycudaBillCollection<TBill, TManifestHeader>
		where TBill : AsycudaBill
		where TManifestHeader : AsycudaManifestHeader
	{
	}

	public class AsycudaBillCollection<TBill, TManifestHeader> : ASYCUDA.Business.AsycudaBillCollection<TBill, TManifestHeader>, IAsycudaBillCollection<TBill, TManifestHeader>
		where TBill : AsycudaBill
		where TManifestHeader : AsycudaManifestHeader
	{
		public AsycudaBillCollection(TManifestHeader master)
			: base(master)
		{
		}

		public new IEnumerator<TBill> GetEnumerator() => Elements.Cast<TBill>().GetEnumerator();
	}
}
