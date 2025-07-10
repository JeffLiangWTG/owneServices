using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business.Synchronisers
{
	class AsycudaBillAddressSynchroniser : BusinessObjectSynchroniser
	{
		public AsycudaBillAddressSynchroniser(IManifestBillAddress destination, JobDocAddress source)
		: base((BusinessObject)destination, source)
		{
		}

		public new AsycudaBillAddress Destination
		{
			get { return (AsycudaBillAddress)base.Destination; }
		}

		public new JobDocAddress Source
		{
			get { return (JobDocAddress)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(((IManifestBillAddress)Destination).OA_AddressInfo, Source.E2_OA_AddressInfo));
		}
	}
}
