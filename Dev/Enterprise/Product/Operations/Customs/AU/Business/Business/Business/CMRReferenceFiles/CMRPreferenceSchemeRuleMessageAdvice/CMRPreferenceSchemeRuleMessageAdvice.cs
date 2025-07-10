
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemeRuleMessageAdvice : AutoCMRPreferenceSchemeRuleMessageAdvice
	{
		public CMRPreferenceSchemeRuleMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPreferenceSchemeRuleMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPreferenceSchemeRuleMessageAdvice>();
		}
	}
}
