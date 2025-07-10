using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer
{
	public class EUH7AsycudaManifestDataObjectReaderHelper : AsycudaManifestDataObjectReaderHelper
	{
		public EUH7AsycudaManifestDataObjectReaderHelper(ZString countryCode, BusinessObjectFactory factory) : base(countryCode, factory)
		{
		}

		protected override AsycudaBillDataObjectReader GetBillDataObjectReaderCore(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			var isHVLV = dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment) != null;
			return isHVLV ?
				new EUH7HVLVAsycudaBillDataObjectReader(dataObject, logger, factory, header, helper, isUpdateEnabled) :
				new AsycudaBillDataObjectReader(dataObject, logger, factory, header, helper, isUpdateEnabled);
		}

		protected override void FillManifestSpecificDataCore(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header, UniversalObjectFactory factory)
		{
			var headerBO = (AsycudaManifestHeader)header;
			var transportMode = shipmentDataObject.TransportMode;

			if (transportMode.Code.HasValue && transportMode.Code.Value == Core.Constants.TransportModes.Road)
			{
				headerBO.AMA_VehicleRegistration = shipmentDataObject?.VoyageFlightNo ?? ZString.Empty;
			}
			headerBO.AMA_AgentType = ZString.Empty;
		}
	}
}
