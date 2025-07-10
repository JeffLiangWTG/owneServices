using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeValidationRuleCollection : ActiveBusinessObjectCollection<BarcodeValidationRule>
	{
		public BarcodeValidationRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BarcodeValidationRuleCollection(BarcodeRuleSet ruleSet)
			: base(ruleSet.Factory, ruleSet, null, BarcodeValidationRuleSchema.BVR_BRS_RuleSet)
		{
		}

		protected override bool AllowNew => base.AllowNew && (Relationship.Master == null || !Relationship.Master.ReadOnly);
	}
}
