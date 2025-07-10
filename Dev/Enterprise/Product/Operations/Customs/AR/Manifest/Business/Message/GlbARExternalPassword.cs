using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public static class GlbARExternalPassword
	{
		public const string ConfigurationName = "ARClientCertificate";
		public const string InterchangeTypeForSending = "ARX";
		public const string EventTypeIRJ = "IRJ";
		public const string PasswordAwaitingCode = "AWA";
		public const string PasswordRegisteredCode = "REG";
		public static MultilingualString PasswordAwaiting => ResString.GetMultilingualString("E330E991-DB84-4B02-A3D9-3055D7F8C37C", "Awaiting Response");
		public static MultilingualString PasswordRegistered => ResString.GetMultilingualString("D5B16394-8BB3-4D58-9A8C-46BD3639BE5D", "Registered");
	}
}
