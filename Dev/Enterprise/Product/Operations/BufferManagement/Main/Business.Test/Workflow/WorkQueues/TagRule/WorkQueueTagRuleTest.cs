using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkQueueTagRule))]
	class WorkQueueTagRuleTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var tag = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(factory, "AAA"), "AAA");
			var rule = BMSTestHelper.CreateTagRule(tag, "AAA", TagRuleActionTypeList.Codes.MaintainMagnitude);
			var band = factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			return rule;
		}
	}
}
