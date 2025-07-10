using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class LogisticsTransportMovementWrapper : ILogisticsTransportMovement
	{
		internal LogisticsTransportMovementWrapper(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, "asycudaManifestHeader cannot be null");
		}
		readonly AsycudaManifestHeader header;

		string ILogisticsTransportMovement.StageCode => header.AMA_Nature == ShipmentTypeList.Codes.Transhipment28 ? ARAWBMessageConstants.TransshipmentTransportMode : ARAWBMessageConstants.ImportExportTransportMode;

		string ILogisticsTransportMovement.ID => header.AMA_Voyage;

		string ILogisticsTransportMovement.NameOfTransport => header.Carrier?.CompanyName ?? ZString.Empty;

		IEvent ILogisticsTransportMovement.ArrivalEvent => arrivalEvent ?? (arrivalEvent = new EventWrapper(header.Factory, header.AMA_RL_NKPortOfDischarge, header.AMA_E_ARV));
		IEvent arrivalEvent;

		IEvent ILogisticsTransportMovement.DepartureEvent => departureEvent ?? (departureEvent = new EventWrapper(header.Factory, header.AMA_RL_NKPortOfLoading, header.AMA_E_DEP));
		IEvent departureEvent;
	}
}
