using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFSA_TFD : CIMFSA
	{
		public CIMFSA_TFD(ICcsukCusAwb awb, ZString optionalOtherServiceInformation, ErrorCollector ec)
			: base(awb, optionalOtherServiceInformation, ec)
		{
		}

		internal static ZString ExplainInboundLine(ZString inboundText)
		{
			var elements = (inboundText + "/").Split('/');
			var carrier = elements[1];
			var date = elements[2];
			var airport = elements[3];
			var pieces = elements[4];
			var reference = elements[5];
			return string.Format("Transferred to carrier {0} at airport {1} at {2}. {3}. Reference:{4}.", carrier, airport, date, CIMFSA.ExplainPiecesAndMass(pieces), reference);
		}
	}
}

