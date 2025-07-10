namespace Enterprise.Customs.AE.Business;

public static class AEConstants
{
	public const string ClearanceLocationAirportFreeZone = "21";
	public const string DefaultDataGroupingForTariffs = Core.Constants.Customs.Universal.RefDataGrouping.Codes.GulfCooperationCouncil;
	public const string ExitPointAirportFreeZone = "AFZ";
	public const string PlaceOfDischargeAirportFreeZone = "F";

	public static class CusSupportingInfoTypes
	{
		public static class Codes
		{
			public const string DocumentAvailability = "DAV";
		}
	}

	public static class Messaging
	{
		public const string MessageRecipient = "NAIC";

		public static class EDIFACT
		{
			public const string MessageReleaseNumber = "23A";
			public const string Syntax = "UNOB";
			public const string SyntaxVersion = "4";
			public const string CharacterEncoding = "2";
			public const string SyntaxRelesaseNumber = "02";
			public const string ProcessingPriority = "A";
			public const string TestIndicatorValue = "0";
			public const string EDIRecipient = "UAENAIC";
		}

		public static class MessageTypes
		{
			public const string CUSCAR = "CAR";
			public const string CONTRL = "CTL";
			public const string CUSRES = "RES";
			public const string XTTERR = "XER";
			public const string DOCSUC = "DOC";
			public const string DOCERR = "ERR";
			public const string DOCXXX = "XXX";
		}

		public static class MessageFriendlyName
		{
			public const string XTTERROR = "XTTError";
		}

		public static class Placeholders
		{
			public const string InterchangeNumber = "<<AE-INT-NUM>>";
			public const string MessageNumber = "<<AE-MSG-NUM>>";
			public const string DateOfCreation = "<<DATE>>";
			public const string TimeOfCreation = "<TM>";
		}

		public static class StatusCodes
		{
			public const string ERR = "ERR";
			public const string Acknowledged = "ACK";
			public const string Unknown = "UNK";
		}

		public static class ActionCodedList
		{
			public const string ActionCoded4 = "4";
			public const string ActionCoded7 = "7";
			public const string ActionCoded8 = "8";
		}

		public static class ManifestNatureCodes
		{
			public const string Export22 = "22";
			public const string Import23 = "23";
			public const string Transhipment28 = "28";
			public const string Transit24 = "24";
		}
	}

	public static class RefCusCodeList
	{
		public static class CodeTypes
		{
			public const string ServiceRequirement = "SRV";
			public const string InvoiceType = "INVTP";
			public const string DelarationPurpose = "DECPR";
			public const string VehicleBrand = "VEHBR";
			public const string VehicleType = "VEHTP";
		}

		public static class Codes
		{
			public static class DeclarationPurpose
			{
				public const string Others = "4";
			}
		}

		public static class ServiceRequirementCode
		{
			public const string FullLoads = "2";
			public const string LessThanFullLoads = "3";
		}
	}
}
