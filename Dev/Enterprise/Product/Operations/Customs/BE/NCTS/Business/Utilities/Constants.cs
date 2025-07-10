namespace Enterprise.Customs.BE.NCTS.Business;

public static class Constants
{
	public static class Security
	{
		public const string _0 = "0";
		public const string _1 = "1";
		public const string _2 = "2";
		public const string _3 = "3";
	}

	public static class ReleaseIndicator
	{
		public const string FullRelease = "1";
		public const string PartialRelease = "2";
		public const string PartialReleaseClosed = "3";
		public const string NoRelease = "4";
	}

	public static class ReleaseType
	{
		public const string PartialRelease = "1";
		public const string FullRelease = "2";
	}

	public static class NctsDepartureMovementHeaderAdditionalTextKeys
	{
		public const string InvalidationJustification = "InvalidationJustification";
	}

	public static class CusCodeDataTypes
	{
		public const string CountryOfRouting = "COR";
		public const string TransportInland = "TPI";
		public const string TRA = "TRA";
		public const string EXT = "EXT";
		public const string ITEM = "ITM";
		public const string LOC = "LOC";
	}

	public static class CusSupportingInfoTypes
	{
		public const string Other = "OTH";
		public const string PreviousDocument = "PRE";
		public const string SupportingDocument = "SUP";
	}

	public static class CusSupportingInfoSubTypes
	{
		public const string TransportDocument = "TRA";
		public const string AdditionalReference = "REF";
		public const string AdditionalInformation = "INF";
	}

	public static class CusPermitHeaderTypes
	{
		public const string TransitOperation = "TRD";
		public const string ACR = "ACR";
		public const string SSE = "SSE";
	}

	public static class OrgCusCodeTypes
	{
		public const string TransitOperationHolder = "TIR";
	}

	public static class AuthorisationTypes
	{
		public const string C520 = "C520";
		public const string C521 = "C521";
		public const string C522 = "C522";
		public const string C523 = "C523";
		public const string C524 = "C524";
	}

	public static class MessageTypes
	{
		public const string CC007C = "CC007C";
		public const string CC013C = "CC013C";
		public const string CC014C = "CC014C";
		public const string CC015C = "CC015C";
		public const string CC044C = "CC044C";
		public const string CC054C = "CC054C";
		public const string CC141C = "CC141C";
		public const string CC170C = "CC170C";
	}

	public static class MessageVersionRegistryDomainCodes
	{
		public const string NCTSP5 = "NCTSP5";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
	public static class MessageProcessingNotes
	{
		public const string ProcessingLog = "Processing Log";
	}

	public static class AdditionalDocumentTypes
	{
		public const string _4009 = "4009";
		public const string CBRNumber = "4008";
	}

	public static class PreviousDocumentTypes
	{
		public const string CargoManifest = "N785";
	}

	public static class ServiceTypes
	{
		public const string CTL = "CTL";
	}

	public static class RecepientTypes
	{
		public const string NTA = "NTA";
	}
}
