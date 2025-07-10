
namespace Enterprise.Customs.AU.Module.AirCargo
{
	/// <summary>
	/// Summary description for AirCargoFilterConstants.
	/// </summary>
	public static class AirCargoFilterConstants
	{
		public static class NumberFilterTypes
		{
			public const string None = "None";
			public const string HouseBillNumber = "House Bill No.";
			public const string MasterBillNumber = "Master Bill No.";
			public const string JobNumber = "Job No.";
			public const string ConsolNumber = "Consol No.";
			public const string FlightNo = "Flight No.";
			public const string CoLoadMaster = "Co-Load Master No.";
			public const string ConRef = "Con Ref No.";
		}

		public static class CMROnlyTypes
		{
			public const string All = "ALL";
			public const string CMRonly = "CMR ONLY";
			public const string LegacyOnly = "LEGACY ONLY";
		}

		public static class PartyFilterTypes
		{
			public const string None = "None";
			public const string Consignor = "Consignor";
			public const string Consignee = "Consignee";
		}

		public static class PortFilterTypes
		{
			public const string None = "None";
			public const string OriginDestination = "Origin / Destination";
			public const string LoadDischarge = "Load / Discharge";
		}

		public static class StatusFilterType
		{
			public const string None = "None";
			public const string Outturn = "Customs Outturn Status";
			public const string OutturnBoth = "Outturn Status (Master or House)";
			public const string OutturnMaster = "Outturn Status (Master only)";
			public const string CMRUnderbond = "Customs Underbond Status";
			public const string CMRUnderbondBoth = "Underbond Status (Master or House)";
			public const string CMRUnderbondMaster = "Underbond Status (Master only)";
			public const string CMRCustoms = "Customs Cargo Status";
			public const string CMRMessage = "Customs Message Status";
			public const string InBondStore = "In Bond Store";
		}

		public static class EstablishmentTypes
		{
			public const string None = "None";
			public const string OriginAddress = "Establishment Origin Address";
			public const string OriginCode = "Establishment Origin Code";
			public const string DestinationAddress = "Establishment Destination Address";
			public const string DestinationCode = "Establishment Destination Code";
		}
	}
}
