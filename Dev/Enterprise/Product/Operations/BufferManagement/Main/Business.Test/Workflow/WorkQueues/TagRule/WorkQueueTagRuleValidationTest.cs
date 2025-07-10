using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkQueueTagRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestActionType()
		{
			var rule = Factory.New<WorkQueueTagRule>();

			foreach (ICodeDescription type in new TagRuleActionTypeList())
			{
				rule.TGR_ActionType = type.Code;
				if (type.Code == TagRuleActionTypeList.Codes.MaintainMagnitude)
				{
					AssertNoErrors(rule.TGR_ActionTypeInfo);
				}
				else
				{
					AssertHasError(rule.TGR_ActionTypeInfo, "Action Type for a Work Queue Tag Rule must be 'MAG'.");
				}
			}
		}
	}
}
