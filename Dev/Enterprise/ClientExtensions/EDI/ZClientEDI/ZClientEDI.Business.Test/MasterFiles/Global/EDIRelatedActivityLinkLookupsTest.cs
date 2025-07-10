using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	sealed class EDIRelatedActivityLinkLookupsTest : RelatedActivityLinkLookupsTest
	{
		protected override RelatedActivityLinkLookups GetNewLookups()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var inquiry = Factory.New<SalesEnquiry>();
			var pivot = ViewRelatedActivityPivot.Create(Factory, opportunity, inquiry);
			return EDIRelatedActivityLinkLookups.New(RelatedActivityLink.Get(pivot, opportunity));
		}
	}
}
