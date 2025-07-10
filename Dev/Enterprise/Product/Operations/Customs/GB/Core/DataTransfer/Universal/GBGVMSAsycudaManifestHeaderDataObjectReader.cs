using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.GB.GVMS;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class GBGVMSAsycudaManifestHeaderDataObjectReader : AsycudaManifestHeaderDataObjectReader<AsycudaManifestHeader>
	{
		public GBGVMSAsycudaManifestHeaderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.GvmsAsycudaManifestHeader;
	}
}
