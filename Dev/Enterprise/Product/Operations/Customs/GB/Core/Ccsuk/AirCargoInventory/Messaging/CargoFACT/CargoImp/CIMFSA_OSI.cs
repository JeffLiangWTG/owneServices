using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFSA_OSI : CIMFSA
	{
		public CIMFSA_OSI(ZString formattedMawbNumber, ZString otherServiceInformation, ErrorCollector ec)
			: base(formattedMawbNumber, otherServiceInformation, ec)
		{
		}

		public CIMFSA_OSI(ICcsukCusAwb awb, ZString otherServiceInformation, ErrorCollector ec)
			: base(awb, otherServiceInformation, ec)
		{
		}

		protected override List<string> GetStatusCodeLine()
		{
			return new List<string>();
		}

		internal static ZString ExplainInboundLine(ZString inboundText)
		{
			var elements = (inboundText + "/").Split('/');
			var info1 = elements[1];
			var info2 = elements[2];
			ZString interpreted = (info1 + " " + info2).TrimEnd();
			interpreted = interpreted.Replace("SDC ", "Shipment description code = ");
			interpreted = interpreted.Replace(" PCS", " pieces");
			if (new Regex(" C.$").IsMatch(inboundText))
			{
				var cac = inboundText.Right(2);
				interpreted = interpreted.Replace(cac, new CustomsStatusCodes().GetDescriptionFromCode(cac));
			}
			return interpreted;
		}
	}
}
