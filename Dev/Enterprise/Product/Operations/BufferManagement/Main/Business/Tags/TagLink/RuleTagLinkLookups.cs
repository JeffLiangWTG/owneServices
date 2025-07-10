using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class RuleTagLinkLookups : TagLinkLookups
	{
		public RuleTagLinkLookups(AutoTagLink parent)
			: base(parent)
		{
		}

		protected override ZQuery GetDefinitionUsageScope()
		{
			return new ZQuery(TagDefinitionSchema.TGD_UsageScope, SQLComparisonOperator.NotEqual, TagUsageScopeList.Codes.User);
		}
	}
}
