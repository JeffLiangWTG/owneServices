namespace Enterprise.Customs.CH.Business;

public static class MessagingConstants
{
	public static class CustomsDestinationCodes
	{
		public const string CustomsEdecSoap = "CHCustomsEdecSOAP";
		public const string CustomsEdecEmail = "CHCustomsEdecEmail";
		public const string CustomsEbdSoap = "CHCustomsEbdSOAP";
		public const string CustomsEvvSoap = "CHCustomsEvvSOAP";
		public const string CustomsKeyMan = "CHCustomsKeyMan";
		public const string CustomsEComSoap = "CHCustomsEComSOAP";
		public const string CustomsEComEmail = "CHCustomsEComEmail";
		public const string CustomsBordereauSoap = "CHCustomsBordereauSOAP";
		public const string CustomsPassar = "CHCustomsPassar";
		public const string CustomsCharteraOutput = "CHCustomsCharteraOutput";
	}

	internal class CustomsCorrectionCode
	{
		internal const string Original = "1";
		internal const string Correction = "2";
		internal const string Cancellation = "0";
		internal const string RequestLastResponse = "3";
	}

	public static class CustomMsgAttributes
	{
		public const string BpId = "custom.CH.bpId";
		public const string MessageType = "custom.CH.messageType";
		public const string MessageID = "custom.CH.messageId";
		public const string LastMessageID = "custom.CH.lastMessageId";
		public const string AccessToken = "custom.CH.accessToken";
		public const string PartnerTopic = "custom.CH.partnerTopic";

		public static class MessageTypes
		{
			public const string DocumentSearchRequest = "DocumentSearchRequest";
			public const string DocumentDeliveryRequest = "DocumentDeliveryRequest";
		}

		public static class MessageTypePrefixs
		{
			public const string NationalTransit = "NT";
			public const string NationalCommon = "NC";
		}
	}

	public static class xTCustomConfiguration
	{
		public const string InterchangeTypes = "CHC";

		public static class Names
		{
			public const string RegistrationNumber = "CHCustomId";
		}

		public static class Status
		{
			public const string Update = "VAL";
			public const string Delete = "DEL";
		}
	}
}
