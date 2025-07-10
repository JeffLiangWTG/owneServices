using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlRuleLicenceDatabasePivotCollection : DependentBusinessObjectCollection<FeatureControlRuleLicenceDatabasePivot, FeatureControlRule>
	{
		public FeatureControlRuleLicenceDatabasePivotCollection(FeatureControlRule rule)
			: base(rule, rule.Factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => FeatureControlRuleLicenceDatabasePivotSchema.FCD_FCR_FeatureControlRule;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
