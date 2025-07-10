using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessTemplateReleaseGateRuleMapping))]
	class ProcessTemplateReleaseGateRuleMappingTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var rule = Factory.New<ProcessTemplateReleaseGateRule>();
			var ruleMapping = Factory.New<ProcessTemplateReleaseGateRuleMapping>();
			ruleMapping.RGM_RGR_Rule = rule.PK;

			return ruleMapping;
		}

		#endregion
	}
}
