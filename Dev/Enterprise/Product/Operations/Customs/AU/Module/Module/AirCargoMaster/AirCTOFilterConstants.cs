namespace Enterprise.Customs.AU.Module.AirCargo;

public static class AirCTOFilterConstants
{
	public static class NumberFilterTypes
	{
		public const string None = "None";
		public const string MasterBillNumber = "Master Bill No.";
		public const string JobNumber = "Job No.";
		public const string FlightNo = "Flight No.";
	}

	public static class PortFilterTypes
	{
		public const string None = "None";
		public const string Origin = "Origin";
		public const string Destination = "Destination";
		public const string Load = "Load";
		public const string PortOfArrival = "Discharge";
		public const string PortOf1stArrival = "Port Of 1st Arrival";
	}

	public static class DateFilterType
	{
		public const string None = "None";
		public const string EstimatedArrival = "Estimated Arrival";
		public const string DateOf1stArrival = "1st Arrival";
	}
}
