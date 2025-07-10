using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFSA_DLV : CIMFSA
	{
		public CIMFSA_DLV(ICcsukCusAwb awb, ZString optionalOtherServiceInformation, ErrorCollector ec)
			: base(awb, optionalOtherServiceInformation, ec)
		{
		}

		internal static ZString ExplainInboundLine(ZString inboundText)
		{
			var elements = (inboundText + "/").Split('/');
			var date = elements[1];
			var airport = elements[2];
			var pieces = elements[3];
			var receiver = elements[4];
			return string.Format("Delivered to consignee {0} at airport {1} at {2}. {3}.", receiver, airport, date, CIMFSA.ExplainPiecesAndMass(pieces));
		}
	}
}
