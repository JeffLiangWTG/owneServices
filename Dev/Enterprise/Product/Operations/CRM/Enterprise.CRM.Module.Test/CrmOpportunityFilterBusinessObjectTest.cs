using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CRM.Module.Test
{
	[TestedType(typeof(CrmOpportunityFilterBusinessObject))]
	public class CrmOpportunityFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override ZArchitecture.Business.FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CrmOpportunityFilterBusinessObject();
		}
	}
}
