namespace Enterprise.Customs.AU.Module
{
	public static class VoyageManifestFilterConstants
	{
		public static class PortFilterTypes
		{
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string Load = "Load";
			public const string Discharge = "Discharge";
		}

		public static class NumberFilterTypes
		{
			public const string Voyage = "Voyage";
			public const string OceanBill = "Ocean Bill";
			public const string Container = "Container";
		}

		public static class PartyFilterTypes
		{
			public const string Consignee = "Consignee";
			public const string Consignor = "Consignor";
		}

		public static class StatusFilterTypes
		{
			public const string ImpendingArrival = "Impending Arrival";
			public const string ActualArrival = "Actual Arrival";
			public const string CargoReport = "Cargo Report";
			public const string CargoList = "Cargo List";
		}

		public static class DateFilterTypes
		{
			public const string ActualArrivalDate = "Actual Arrival Date";
			public const string EstimatedArrivalDate = "Estimated Arrival Date";
			public const string DepartureDate = "Last Foreign Port Departure Date";
		}
	}
}
