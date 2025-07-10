namespace Enterprise.Customs.AU.Module
{
	public static class SeaCargoFilterConstants
	{
		public static class NumberFilterTypes
		{
			public const string None = "None";
			public const string OceanBillNumber = "Ocean Bill";
			public const string HouseBillNumber = "House Bill";
			public const string ParentBillNumber = "Parent Bill";
			public const string ContainerNumber = "Container #";
			public const string ResponsiblePartyID = "Responsible Party ID";
			public const string VesselVoyage = "Vessel / Voyage";
		}

		public static class DateFilterTypes
		{
			public const string None = "None";
			public const string ETD = "ETD";
			public const string ArrivalDate = "Arrival";
			public const string FirstArrivalDate = "First Arrival";
			public const string DepartureDate = "Departure";
		}

		public static class PortFilterTypes
		{
			public const string None = "None";
			public const string All = "Load,Origin / Discharge,Destination";
			public const string LoadingDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";
		}

		public static class StatusFilterTypes
		{
			public const string CustomsStatus = "Customs Cargo Status";
			public const string MessageStatus = "Customs Message Status";
			public const string UnderbondStatus = "Underbond Status";
			public const string OutturnStatus = "Outturn Status";
		}

		public static class EstablishmentTypes
		{
			public const string OriginAddress = "Establishment Origin Address";
			public const string OriginCode = "Establishment Origin Code";
			public const string DestinationAddress = "Establishment Destination Address";
			public const string DestinationCode = "Establishment Destination Code";
		}
	}
}
