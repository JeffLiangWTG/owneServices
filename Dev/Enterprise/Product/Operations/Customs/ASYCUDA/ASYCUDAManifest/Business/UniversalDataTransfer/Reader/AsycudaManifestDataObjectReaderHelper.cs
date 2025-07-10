using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer
{
	public class AsycudaManifestDataObjectReaderHelper : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestDataObjectReaderHelper
	{
		public AsycudaManifestDataObjectReaderHelper(ZString countryCode, BusinessObjectFactory factory)
			: base(countryCode, factory)
		{
		}

		protected override ASYCUDA.Business.UniversalDataTransfer.AsycudaBillDataObjectReader GetBillDataObjectReaderCore(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			return new AsycudaBillDataObjectReader(dataObject, logger, factory, (AsycudaManifestHeader)header, (AsycudaManifestDataObjectReaderHelper)helper, isUpdateEnabled);
		}
	}
}
