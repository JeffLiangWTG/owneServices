using Enterprise.Registry.Business;

namespace Enterprise.CRM.Common
{
	public class CrmOpportunityLookups : AutoCrmOpportunityLookups
	{
		public CrmOpportunityLookups(AutoCrmOpportunity parent) : base(parent)
		{
		}

		public ICodeDescriptionBoolList Statuses
		{
			get
			{
				return Factory.GetCachedValue<ICodeDescriptionBoolList>("CrmOpportunityLookups.Statuses", () =>
				{
					return OrganisationsDataRegistry.Instance.GlowOpportunityStatuses.Value;
				});
			}
		}
	}
}

