
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodCountry : AutoCMRPreferenceRulePeriodCountry
	{
		public CMRPreferenceRulePeriodCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPreferenceRulePeriodCountry New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPreferenceRulePeriodCountry>();
		}
	}
}
