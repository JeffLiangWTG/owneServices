using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(FilterRuleWorkflowCollection))]
	public class FilterRuleWorkflowCollectionTest : ActiveBusinessObjectCollectionTestCase<FilterRuleWorkflowCollection>
	{
		protected override FilterRuleWorkflowCollection GetCollectionToTest()
		{
			return new FilterRuleWorkflowCollection(Factory, new ZQuery());
		}
	}
}
