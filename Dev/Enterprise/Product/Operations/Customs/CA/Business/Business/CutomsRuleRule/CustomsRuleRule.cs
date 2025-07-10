using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CustomsRuleRule : Customs.Business.CustomsRuleRule, Integration.Customs.CA.ICustomsRuleRule
	{
		public CustomsRuleRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CustomsRuleRuleLookups Lookups => (CustomsRuleRuleLookups)base.Lookups;

		protected override CusPermitRuleLookups GetNewLookups()
		{
			return new CustomsRuleRuleLookups(this);
		}
	}
}
