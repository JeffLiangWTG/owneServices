using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer
{
	public class AsycudaManifestHeaderDataObjectWriter : AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>
	{
		public AsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override IAsycudaBillDataObjectWriter CreateNewAsycudaBillDataObjectWriter(AsycudaManifestHeaderDataObjectWriterHelper headerHelper) => new AsycudaBillDataObjectWriter(writeManager, headerHelper);
	}
}
