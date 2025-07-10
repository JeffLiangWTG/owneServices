
namespace Enterprise.Customs.CA.Business
{
	partial class PHACEndUseCodes
	{
		public static bool IsValidCategoryCode(string endUseCode, string categoryCode)
		{
			switch (endUseCode)
			{
				case Codes.PH01:
				case Codes.PH04:
				case Codes.PH06:
					return categoryCode == PHACCategories.Codes.PH01
							|| categoryCode == PHACCategories.Codes.PH02
							|| categoryCode == PHACCategories.Codes.PH05
							|| categoryCode == PHACCategories.Codes.PH06;
				case Codes.PH02:
				case Codes.PH03:
				case Codes.PH05:
					return categoryCode == PHACCategories.Codes.PH01
							|| categoryCode == PHACCategories.Codes.PH02
							|| categoryCode == PHACCategories.Codes.PH03
							|| categoryCode == PHACCategories.Codes.PH04
							|| categoryCode == PHACCategories.Codes.PH05
							|| categoryCode == PHACCategories.Codes.PH06;
				default:
					return false;
			}
		}

		public static bool IsPathogenToxinLicenceMandatory(string endUseCode, string categoryCode)
		{
			switch (endUseCode)
			{
				case Codes.PH01:
				case Codes.PH04:
				case Codes.PH06:
					switch (categoryCode)
					{
						case PHACCategories.Codes.PH02:
						case PHACCategories.Codes.PH05:
							return true;
						default:
							return false;
					}
				case Codes.PH02:
				case Codes.PH03:
				case Codes.PH05:
					switch (categoryCode)
					{
						case PHACCategories.Codes.PH02:
						case PHACCategories.Codes.PH03:
						case PHACCategories.Codes.PH04:
						case PHACCategories.Codes.PH05:
							return true;
						default:
							return false;
					}
				default:
					return false;
			}
		}
	}
}
