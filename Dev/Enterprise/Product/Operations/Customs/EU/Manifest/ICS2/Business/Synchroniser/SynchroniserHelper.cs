using System.Linq;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	static class SynchroniserHelper
	{
		public static Transport GetFirstSeaTransportWithDischargePortInEuCountry(ForwardingConsol consol) => GetSeaTransportsInLegOrder(consol).FirstOrDefault(IsDischargePortInEuOrNo);

		public static Transport GetFirstTransportOfFirstVesselArrivingInMemberOfICS2(ForwardingConsol consol) => GetFirstSeaTransportWithDischargePortInMemberOfICS2(consol) is { } transport
			? GetSeaTransportsInLegOrder(consol).FirstOrDefault(t => t.JW_Vessel == transport.JW_Vessel)
			: null;

		public static Transport GetLastSeaTransport(ForwardingConsol consol) => GetSeaTransportsInLegOrder(consol).LastOrDefault();

		static bool IsDischargePortInEuOrNo(Transport transport)
		{
			var discPort = transport.DiscPort;
			return discPort != null && (discPort.IsInEU || discPort.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Norway);
		}

		static IOrderedEnumerable<Transport> GetSeaTransportsInLegOrder(ForwardingConsol consol) => consol.Transports.Cast<Transport>().Where(t => t.IsSea).OrderBy(t => t.JW_LegOrder);

		public static Transport GetFirstSeaTransportWithDischargePortInMemberOfICS2(ForwardingConsol consol) => GetSeaTransportsInLegOrder(consol).FirstOrDefault(IsDischargePortIsMemberOfICS2);

		static bool IsDischargePortIsMemberOfICS2(Transport transport)
		{
			var discPort = transport.DiscPort;
			return discPort != null && (transport.Factory.IsMemberOfICS2(discPort.RL_RN_NKCountryCode) || discPort.IsInNorthernIreland);
		}
	}
}
