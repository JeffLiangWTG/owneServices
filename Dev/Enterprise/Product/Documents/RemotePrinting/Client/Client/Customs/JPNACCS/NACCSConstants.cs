namespace Enterprise.RemotePrinting.Client
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer")]
	public static class NACCSConstants
	{
		public static class Encodings
		{
			public const string DefaultEncoding = "EUC-JP";
		}

		public static class ErrorTypes
		{
			public const string TransmissionError = "TransmissionError";
			public const string Unauthorized = "Unauthorized";
		}

		public static class Actions
		{
			public const string Authenticate = "Authenticate";
			public const string Transmit = "Transmit";
		}

		public static class Attributes
		{
			public const string ProtocolType = "custom.NACCS.ProtocolType";
			public const string CompanyCode = "custom.NACCS.CompanyCode";
			public const string ClientMailbox = "custom.NACCS.ClientMailbox";
			public const string Domain = "custom.NACCS.Domain";
			public const string MachineID = "custom.NACCS.MachineID";
			public const string ServerMailbox = "custom.NACCS.ServerMailbox";
			public const string MessageId = "custom.NACCS.MessageId";
		}

		public const string ApplicationCode = "JPC";
		public const string ErrorMessageType = "XER";

		public const string WebPrintParty = "WebPrint";
		public const string CW1Party = "CW1";
		public const string CustomsParty = "NACCS";

		/// <summary>
		/// BrettsGuid
		/// </summary>
		public const string EmptyTrackingId = "20DD961B-3E62-40E5-B60A-B1312B70F5EE";

		public static class ErrorMessages
		{
			public const string ErrorSettingLog = "Failed to retrieve valid settings. Please check your configuration in CargoWise by navigating to Maintain > System > Registry > Customs > Country or Region Specific > Japan > NACCS Messaging > Remote WebPrint Client Configurations. Ensure that the Status is set to REG – Registered to xT, and the Local Computer Alias (Machine ID) matches the one for WebPrint.";
		}
	}
}
