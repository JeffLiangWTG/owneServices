using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IConsignment
	{
		ZString ContainerCode { get; }
		ITransportMeans ArrivalTransportMeans { get; }
		ITransportMeans DepartureTransportMeans { get; }
		IGoodsLocation GoodsLocation { get; }
		ZString LoadingLocationID { get; }
		IEnumerable<ITransportEquipment> TransportEquipments { get; }
		IOrganisation Carrier { get; }
		ZString FreightPaymentMethodCode { get; }
		IEnumerable<ZString> ItineraryRoutingCountryCodes { get; }
		IOrganisation Consignor { get; }
		IConsignmentItem ConsignmentItem { get; }
	}
}
