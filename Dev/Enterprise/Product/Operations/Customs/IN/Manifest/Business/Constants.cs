namespace Enterprise.Customs.IN.Manifest.Business;

public static class Constants
{
	public static class RefCusCodeList
	{
		public static class Attributes
		{
			public const string EmailAddress = "EmailAddress";
		}
	}

	public static class Messaging
	{
		public const string INMessageNumPlaceHolder = "<<MSG>>";

		public static class IceGate
		{
			public const string TestName = "INCUSTOMSICEGATETEST";
			public const string ProdName = "INCUSTOMSICEGATEPROD";
		}

		public static class InterchangeHeaderAttributes
		{
			public const string SenderMailBoxKey = "custom.IN.FromMailBox";
			public const string DestinationMailBoxKey = "custom.IN.DestinationMailBox";
			public const string SubjectKey = "custom.IN.Subject";
			public const string FileNameKey = "custom.IN.FileName";
			public const string CopyToMailBoxKey = "custom.IN.CopyToMailBox";
		}
	}
}
