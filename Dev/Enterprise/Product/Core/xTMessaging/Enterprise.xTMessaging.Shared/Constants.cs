namespace Enterprise.xTMessaging.Shared
{
	public static class Constants
	{
		public const string CustomMsgAttributePrefix = "custom.";

		public static class CustomMsgAttributes
		{
			public const string ApplicationCode = "custom.ApplicationCode";
			public const string MessageTrackingID = "custom.MessageTrackingID";
			public const string MessageType = "custom.MessageType";
			public const string SourceParty = "custom.SourceParty";
			public const string DestinationParty = "custom.DestinationParty";
			public const string EDIMessageCreatorID = "custom.EDIMessageCreatorID";
			public const string CreateEDIMessage = "custom.CreateEDIMessage";
			public const string MessageSubType = "custom.MessageSubType";
			public const string ReceivingRetryCount = "custom.ReceivingCount";
			public const string ReferenceNumber = "custom.ReferenceNumber";
		}

		public enum MessageHandlingResultOperation
		{
			Success,
			Error,
			LeaveItToNextRun
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "xT system const")]
		public static class xTMsgAttributes
		{
			public const string cw1key = "cw1.key";
			public const string cw1certificate = "cw1.certificate";
			public const string refexternal = "refexternal";
			public const string FtpClientUser = "ftpclient.user";
			public const string FtpClientPassword = "ftpclient.password";
			public const string HttpClientUser = "httpclient.user";
			public const string HttpClientPassword = "httpclient.password";
			public const string anycertificate = "any.certificate";
			public const string MsgId = "msgid";
			public const string StdReceiver = "std.receiver";
			public const string Oauth2ClientID = "oauth2.client-id";
			public const string Oauth2ClientSecret = "oauth2.client-secret";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static class xTGRPCErrorStatusCodes
		{
			public const string Unavailable = "Unavailable";
			public const string InvalidArgument = "InvalidArgument";
			public const string Internal = "Internal";
		}

		public static class xTUniversalEventContextTypes
		{
			public const string OriginalMessage = "OriginalMessage";
			public const string ResponseMessage = "ResponseMessage";
			public const string OriginalAttributes = "OriginalAttributes";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static class xTNotificationErrorRegexStrings
		{
			public const string MessageTransmissionFailureRegex = "(?i)error description: message transmission to.*rejected by peer:(?!.*result code (401|403) not accepted).*";
			public const string MessageUnauthorizedFailureRegex = "(?i)error description: message transmission to.*rejected by peer: result code (401|403) not accepted";
			public const string ProcessingComponentStepErrorRegex = @"\bProcessing step execution error\b";
			public const string PreProcessingComponentStepErrorRegex = "fromobj : xt-application:/Framework/Direct Endpoints/";
		}

		public static class UniversalEventTypeErrorCategories
		{
			public const string PreProcessingError = "PreProcessingError";
			public const string PostProcessingError = "PostProcessingError";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
			public const string Unauthorized = "Unauthorized";
			public const string BusinessError = "BusinessError";
			public const string TransmissionError = "TransmissionError";
		}
	}
}
