namespace Enterprise.Customs.ASYCUDA.Business
{
	public static class Constants
	{
		public static class Containers
		{
			public const string _20NOR = "20NOR";   //  Twenty foot non-operating reefer
			public const string _20HC = "20HC"; // 	Twenty foot high cube
			public const string _20GP = "20GP"; //  Twenty foot general purpose
			public const string _CARH4 = "CARH4";   // 	CAR HAULER 20
			public const string _20PL = "20PL"; //  Twenty foot platform

			public const string _20FR = "20FR"; //  Twenty foot flatrack
			public const string _20OT = "20OT"; //  Twenty foot open top
			public const string _BELLY = "BELLY";   // 	BELLY DUMP TRAILER 40
			public const string _STEEL1 = "STEEL1"; // 	STEEL DUMP TRAILER 40
			public const string _40HC = "40HC"; //  Forty foot high cube
			public const string _40FR = "40FR"; //  Forty foot flatrack
			public const string _GRAI7 = "GRAI7";   // 	GRAIN HAULER 40
			public const string _40NOR = "40NOR";   //  Forty foot non-operating reefer
			public const string _45HC = "45HC"; //  Forty Five foot high cube
			public const string _40GP = "40GP"; //  Forty foot general purpose

			public const string _40OT = "40OT"; //  Forty foot open top
			public const string _LOG1 = "LOG1"; // 	LOGGING 40
			public const string _40PL = "40PL"; //  Forty foot platform
			public const string _40RE = "40RE"; //  Forty foot reefer
			public const string _20RE = "20RE"; //  Twenty foot reefer
			public const string _40REHC = "40REHC"; // 	Forty foot high cube reefer
		}

		public static class RegistrationTypes
		{
			public const string BillIssuer = "BIL";
		}

		public static class ServiceLevel
		{
			public const string Standard = "STD";
		}

		public static class GlobalManifestApplicationProvider
		{
			public const string Default = "Default";
		}

		public static class ASYCUDAManifestTypesCodes
		{
			public const string ASY = "ASY";
		}

		public static class AddressType
		{
			public const string Branch = "Branch";
		}

		public static class CustomsChargeType
		{
			public const string CustomsChargeCode = "CUS";
			public const string CustomsChargeDescription = "Customs Value";
			public const string CustomsDutyCode = "CDU";
			public const string CustomsDutyCodeDescription = "Customs Duty Payable";
			public const string GSTCode = "GST"; // TODO: Should change to TAX
			public const string GSTDescription = "Goods and Services Tax"; // TODO: Should change to TAX Value
		}

		public static class CustomsEntryType
		{
			public const string TradeNetPermit = "TNP";
			public const string ACCESSPermit = "ASY";
			public const string SAD = "SAD";
		}

		public static class CodeTypeDescription
		{
			public const string AsycudaRegistrationDescription = "Asycuda Registration";
			public const string BillIssuerDescription = "Bill Issuer";
		}

		public static class CustomsReferenceType
		{
			public const string ABL_SenderReferenceType = "SRF";
		}

		public static class DataProviders
		{
			public const string SGAccess = "SGA";
		}

		public static class EventContext
		{
			public const string MasterBill = "MasterBill";
			public const string HouseBill = "HouseBill";
			public const string ConsignmentReference = "ConsignmentReference";
			public const string ConsignmentStatus = "ConsignmentStatus";
			public const string ErrorCode = "ErrorCode";
			public const string MessageStatusCode = "MessageStatusCode";
			public const string ManifestPermitNumber = "ManifestPermitNumber";
			public const string TradeNetPermitNumber = "TradeNetPermitNumber";
			public const string TradeNetPermitStatus = "TradeNetPermitStatus";
			public const string CustomsEntryNumber = "CustomsEntryNumber";
			public const string CustomsEntryStatus = "CustomsEntryStatus";
			public const string MessageType = "MessageType";
		}
	}
}
