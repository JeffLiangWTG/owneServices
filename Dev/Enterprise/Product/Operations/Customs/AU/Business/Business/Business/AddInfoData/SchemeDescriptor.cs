using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class SchemeDescriptor
	{
		public static string GetDescriptionIncludingDutyRate(CMRSchemeList schemeList, CMRTariffRatePeriodSnapshot tariffRate)
		{
			ZStringBuilder result = new ZStringBuilder();
			ZString description = schemeList.GetDescriptionFromCode(tariffRate.TT_PreferenceSchemeType);
			result.Append(description);
			result.Append("(");
			result.Append(tariffRate.GetDutyRateDescription());
			result.Append(")");
			return result.ToString();
		}
	}
}
