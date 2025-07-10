using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class CNCountryCodeMapper
	{
		const string UnknownCountryCode = "ZZZ";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Show in Chinese")]
		const string UnknownCountryName = "国(地)别不详";

		public static ZString GetCNCountryCode(this RefCountry country)
		{
			return country?.RN_IsoAlpha3Code ?? UnknownCountryCode;
		}

		public static ZString GetCNCountryName(this RefCountry country)
		{
			return country?.RN_DescMultilingual.ToString(Core.Constants.Languages.ChineseSimplified) ?? UnknownCountryName;
		}

		public static CodeAndDescriptionWrapper CreateCodeAndDescriptionWrapper(this RefCountry country, BusinessObjectFactory factory)
		{
			return CodeAndDescriptionWrapper.New(country.GetCNCountryCode(), country.GetCNCountryName(), factory);
		}
	}
}
