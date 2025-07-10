using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class ConsignmentLegFlight
	{
		public enum LegTypes
		{
			Inward = 20,
			Onward = 12
		}

		public LegTypes LegType { get; set; }

		public ZString FlightNumber { get; set; }

		public ZString Awb { get; set; }

		public ZString Carrier { get; set; }

		public ZString Mode { get; set; }

		public ZDateTime Date { get; set; }

		public LocationsCollection Locations { get; set; }

		public override string ToString()
		{
			ZString tdtSectionData = string.Format("{0} {1} {2} {3}", Awb, Carrier, FlightNumber, Date).Trim();
			if (!tdtSectionData.IsEmpty)
			{ tdtSectionData += "\r\n"; }
			return string.Format("{0} {1}", tdtSectionData, Locations.ToString());
		}
	}
}
