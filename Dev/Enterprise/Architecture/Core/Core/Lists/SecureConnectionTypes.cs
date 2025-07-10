namespace Enterprise.ZArchitecture.Core
{
	public class SecureConnectionTypes : CodeDescriptionPairList
	{
		public const string None = "NO";
		public const string SSL = "SSL";
		public const string TLS = "TLS";

		public SecureConnectionTypes()
		{
			AddPair(None, SourceGenerated.ResString.GetMultilingualString("dba807fc-8637-4dc2-a3c3-29949da4dbca", "Unsecured Connection"));
			AddPair(SSL, "SSL");
			AddPair(TLS, "TLS");

			DefaultCode = TLS;
		}
	}
}
