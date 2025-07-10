using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessTemplateReleaseGateRuleCategory))]
	class ProcessTemplateReleaseGateRuleCategoryTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var rule = Factory.New<ProcessTemplateReleaseGateRule>();
			var ruleCategory = Factory.New<ProcessTemplateReleaseGateRuleCategory>();
			ruleCategory.RGC_RGR_Rule = rule.PK;

			return ruleCategory;
		}

		#endregion
	}
}
