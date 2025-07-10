using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;

public class AsycudaManifestHeaderDataObjectReader : AsycudaManifestHeaderDataObjectReader<AsycudaManifestHeader>
{
	public AsycudaManifestHeaderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
	{
	}
}
