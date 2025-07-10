using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFSA_RCF : CIMFSA
	{
		public CIMFSA_RCF(ICcsukCusAwb awb, ZString optionalOtherServiceInformation, ErrorCollector ec)
			: base(awb, optionalOtherServiceInformation, ec)
		{
		}

		internal static ZString ExplainInboundLine(ZString inboundText)
		{
			var elements = (inboundText + "/").Split('/');
			var flight = elements[1];
			var date = elements[2];
			var airport = elements[3];
			var pieces = elements[4];
			return string.Format("{3} Received on flight {0} at airport {1} at {2}. ", flight, airport, date, CIMFSA.ExplainPiecesAndMass(pieces));
		}
	}
}
