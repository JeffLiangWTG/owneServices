using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string AusIndustryDetermination = "AD";
			public const string ByLaw = "BL";
			public const string Determination = "DE";
			public const string ImportCreditNumber = "ICN";
			public const string TariffConcessionOrder = "TC";
			public const string TariffQuota = "TFQ";
			public const string TradexOrder = "TX";
		}

		public static class Descriptions
		{
			public const string AusIndustryDetermination = "Aust. Industry Determination";
			public const string ByLaw = "By Law";
			public const string Determination = "Determination";
			public const string ImportCreditNumber = "Import Credit Number";
			public const string TariffConcessionOrder = "Tariff Concession Order";
			public const string TariffQuota = "Tariff Quota";
			public const string TradexOrder = "Tradex Order";
		}

		public CMRInstrumentTypeList()
		{
			AddPair(Codes.AusIndustryDetermination, Descriptions.AusIndustryDetermination);
			AddPair(Codes.ByLaw, Descriptions.ByLaw);
			AddPair(Codes.Determination, Descriptions.Determination);
			AddPair(Codes.ImportCreditNumber, Descriptions.ImportCreditNumber);
			AddPair(Codes.TariffConcessionOrder, Descriptions.TariffConcessionOrder);
			AddPair(Codes.TariffQuota, Descriptions.TariffQuota);
			AddPair(Codes.TradexOrder, Descriptions.TradexOrder);
		}

		public static ZString GetInstrumentTypeMappedFromLegacy(ZString legacyType)
		{
			ZString result = legacyType;
			switch (legacyType)
			{
				case CustomsInstrumentTypeList.Codes.TariffConcession:
					result = CMRInstrumentTypeList.Codes.TariffConcessionOrder;
					break;
				case CustomsInstrumentTypeList.Codes.MinisterialDetermination:
					result = CMRInstrumentTypeList.Codes.Determination;
					break;
			}
			return result;
		}

		public static ZString GetInstrumentTypeMappedFromCMR(ZString cMRType)
		{
			ZString result = cMRType;
			switch (cMRType)
			{
				case CMRInstrumentTypeList.Codes.TariffConcessionOrder:
					result = CustomsInstrumentTypeList.Codes.TariffConcession;
					break;
				case CMRInstrumentTypeList.Codes.Determination:
					result = CustomsInstrumentTypeList.Codes.MinisterialDetermination;
					break;
			}
			return result;
		}
	}
}
