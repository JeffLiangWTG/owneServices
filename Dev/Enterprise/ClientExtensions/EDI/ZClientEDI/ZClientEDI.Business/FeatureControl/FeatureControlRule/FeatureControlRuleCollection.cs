using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlRuleCollection : DependentBusinessObjectCollection<FeatureControlRule, FeatureControlHeader>
	{
		public FeatureControlRuleCollection(FeatureControlHeader parent) : base(parent)
		{
		}

		public FeatureControlRuleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => FeatureControlRuleSchema.FCR_FCM_FeatureControl;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}

	public class FeatureControlRuleNonDependentCollection : BusinessObjectCollection<FeatureControlRule>
	{
		public FeatureControlRuleNonDependentCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
