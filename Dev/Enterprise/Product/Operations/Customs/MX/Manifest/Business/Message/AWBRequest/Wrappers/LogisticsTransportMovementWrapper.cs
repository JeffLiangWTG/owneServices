using System;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class LogisticsTransportMovementWrapper : ILogisticsTransportMovement
	{
		internal LogisticsTransportMovementWrapper(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, "asycudaManifestHeader cannot be null");
		}
		readonly AsycudaManifestHeader header;

		string ILogisticsTransportMovement.StageCode => header.AMA_Nature == ShipmentTypeList.Codes.Transhipment28 ? AWBRequestConstants.TransshipmentTransportMode : AWBRequestConstants.ImportExportTransportMode;

		string ILogisticsTransportMovement.ID => header.AMA_Voyage;

		string ILogisticsTransportMovement.NameOfTransport => header.Carrier?.CompanyName ?? ZString.Empty;

		IEvent ILogisticsTransportMovement.ArrivalEvent => arrivalEvent ?? (arrivalEvent = new EventWrapper(header.Factory, header.AMA_RL_NKPortOfDischarge, header.AMA_E_ARV));
		IEvent arrivalEvent;

		IEvent ILogisticsTransportMovement.DepartureEvent => departureEvent ?? (departureEvent = new EventWrapper(header.Factory, header.AMA_RL_NKPortOfLoading, header.AMA_E_DEP));
		IEvent departureEvent;
	}

	internal class EventWrapper : IEvent
	{
		internal EventWrapper(BusinessObjectFactory factory, string location, ZDateTime eventDate)
		{
			airportInfo = AWBRequestHelper.AirportInfo(factory, location);
			this.eventDate = eventDate;
		}
		readonly (string code, string name) airportInfo;
		readonly ZDateTime eventDate;

		DateTime IEvent.Date => MXMessageHelper.SafeDateTime(eventDate);

		string IEvent.LocationCode => airportInfo.code;

		string IEvent.LocationName => airportInfo.name;
	}
}
