using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Module.Testing
{
	[TestedType(typeof(EDIGlbCompanyCampaignFilterBusinessObject))]
	class EDIGlbCompanyCampaignFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIGlbCompanyCampaignFilterBusinessObject();
		}
	}
}
