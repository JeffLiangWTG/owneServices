using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class LocationsCollection : List<FreightLocation>
	{
		public FreightLocation AirportOfArrival { get { return FindLocation(LocationTypes.Codes.AOA); } }
		public FreightLocation AirportOfDestination { get { return FindLocation(LocationTypes.Codes.AOD); } }
		public FreightLocation AirportOfOrigin { get { return FindLocation(LocationTypes.Codes.AOO); } }
		public FreightLocation CounrtyOfDestination { get { return FindLocation(LocationTypes.Codes.COD); } }
		public FreightLocation CountryOfOrigin { get { return FindLocation(LocationTypes.Codes.COO); } }
		public FreightLocation PortOfTranshipment { get { return FindLocation(LocationTypes.Codes.POS); } }

		FreightLocation FindLocation(string p)
		{
			return this.Find(x => x.Function == p && !x.IsEmpty);
		}

		public override string ToString()
		{
			var sb = new ZStringBuilder();
			foreach (var loc in new FreightLocation[] { AirportOfArrival, AirportOfDestination, AirportOfOrigin, CounrtyOfDestination, CountryOfOrigin, PortOfTranshipment })
			{
				if (loc != null && !loc.IsEmpty)
				{
					sb.Append(loc.ToString());
				}
			}
			return sb.ToStringWithDelimiterBetweenAppends("\r\n\t");
		}
	}
}
