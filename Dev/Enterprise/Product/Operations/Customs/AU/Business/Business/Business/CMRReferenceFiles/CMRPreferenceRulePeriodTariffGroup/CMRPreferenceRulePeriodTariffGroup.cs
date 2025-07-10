
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodTariffGroup : AutoCMRPreferenceRulePeriodTariffGroup
	{
		public CMRPreferenceRulePeriodTariffGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPreferenceRulePeriodTariffGroup New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPreferenceRulePeriodTariffGroup>();
		}
	}
}
