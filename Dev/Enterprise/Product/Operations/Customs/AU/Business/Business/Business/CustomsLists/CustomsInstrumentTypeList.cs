using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsInstrumentTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string MinisterialDetermination = "MD1";
			public const string TariffQuota = "TFQ";
			public const string TariffConcession = "TC1";
			public const string ByLaw = "BL";
		}

		public static class Descriptions
		{
			public const string MinisterialDetermination = "Ministerial Determination";
			public const string TariffQuota = "Tariff Quota";
			public const string TariffConcession = "Tariff Concession";
			public const string ByLaw = "By-Law";
		}

		public CustomsInstrumentTypeList()
		{
			AddPair(Codes.MinisterialDetermination, Descriptions.MinisterialDetermination);
			AddPair(Codes.TariffQuota, Descriptions.TariffQuota);
			AddPair(Codes.TariffConcession, Descriptions.TariffConcession);
			AddPair(Codes.ByLaw, Descriptions.ByLaw);
		}
	}
}
