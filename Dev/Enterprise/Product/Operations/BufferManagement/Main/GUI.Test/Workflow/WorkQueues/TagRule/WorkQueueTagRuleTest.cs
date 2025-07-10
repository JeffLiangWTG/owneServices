using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(WorkQueueTagRule))]
	class WorkQueueTagRuleRelatedFilterTest : RelatedModuleFilterSupportableTestCase<WorkQueueTagRule>
	{
		protected override IEnumerable<FilterRuleTestSet> GetFilterRules(WorkQueueTagRule businessObject)
		{
			return new[] { new FilterRuleTestSet(null, () => businessObject.Filter, "BMFilterRuleFilterBusinessObject") };
		}

		protected override void ValidateBusinessObject(WorkQueueTagRule businessObject)
		{
			businessObject.Validation.ValidateAll();
		}

		protected override WorkQueueTagRule GetNewBusinessObject()
		{
			var rule = base.GetNewBusinessObject();
			rule.TGR_IsSystem = false;

			return rule;
		}
	}
}
