using System.Linq;

namespace Enterprise.Registry.Business
{
	public class GlowOpportunityLeadSourceRegistryDataType : NonPersistentBusinessObjectRegistryDataTypeWithEnabledItem<CodeDescriptionBoolCollection>
	{
		public GlowOpportunityLeadSourceRegistryDataType()
		{
		}

		public GlowOpportunityLeadSourceRegistryDataType(CodeDescriptionBoolCollection defaultValue)
			: base(defaultValue)
		{
		}

		protected override bool HasEnabledItem(CodeDescriptionBoolCollection proposedValue)
		{
			return proposedValue.Any(x => ((CodeDescriptionBool)x).Bool);
		}
	}
}
