using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class KafkaSecurityProtocolOptions : CodeDescriptionPairList
	{
		public const string PLAINTEXT = "PLAINTEXT";
		public const string SASL_SSL = "SASL_SSL";
		public const string SASL_PLAINTEXT = "SASL_PLAINTEXT";
		public const string SSL = "SSL";

		public KafkaSecurityProtocolOptions()
		{
			AddPair(PLAINTEXT, ResString.GetMultilingualString("14360584-D9BE-46EB-8F66-151E644B9186", "NO Encryption Or Authentication"));
			AddPair(SASL_SSL, ResString.GetMultilingualString("FE47E0B4-F307-4ED1-8F90-D09E242E0B17", "SASL Authentication And SSL Encryption"));
			AddPair(SASL_PLAINTEXT, ResString.GetMultilingualString("3D1CB877-3F4E-4D0C-A514-45C7C2A82F5B", "SASL Authentication"));
			AddPair(SSL, ResString.GetMultilingualString("93F3E3F2-970B-4329-A5F4-B73A243D1DF7", "SSL Encryption"));

			DefaultCode = PLAINTEXT;
		}
	}
}
