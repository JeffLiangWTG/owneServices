using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;

public class AsycudaManifestHeaderDataObjectWriter : AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>
{
	public AsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager)
		: base(manager)
	{
	}
}
