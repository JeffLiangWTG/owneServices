using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer
{
	public class EUH7AsycudaManifestHeaderDataObjectWriter : AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>
	{
		public EUH7AsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}
	}
}
