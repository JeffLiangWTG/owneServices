
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemeRuleCollection : BusinessObjectCollection<CMRPreferenceSchemeRule>
	{
		public CMRPreferenceSchemeRuleCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
