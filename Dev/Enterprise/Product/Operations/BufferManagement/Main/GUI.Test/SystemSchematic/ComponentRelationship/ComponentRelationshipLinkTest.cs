using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(ComponentRelationshipLink))]
	public class ComponentRelationshipLinkRelatedFilterTest : RelatedModuleFilterSupportableTestCase<ComponentRelationshipLink>
	{
		protected override IEnumerable<FilterRuleTestSet> GetFilterRules(ComponentRelationshipLink businessObject)
		{
			return new[] { new FilterRuleTestSet(null, () => businessObject.FilterRule, "BMFilterRuleFilterBusinessObject") };
		}

		protected override void ValidateBusinessObject(ComponentRelationshipLink businessObject)
		{
			businessObject.Validation.ValidateAll();
		}

		protected override ComponentRelationshipLink GetNewBusinessObject()
		{
			return BMSTestHelper.CreateComponentRelationshipLink(Factory, Factory.NewWithValidTestData<ComponentRelationship>(), Factory.NewWithValidTestData<BMComponent>());
		}
	}
}
