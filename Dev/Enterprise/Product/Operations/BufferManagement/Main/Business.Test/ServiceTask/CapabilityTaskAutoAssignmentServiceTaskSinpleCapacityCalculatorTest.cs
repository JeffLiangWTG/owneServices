
using NUnit.Framework;
namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(CapabilityTaskAutoAssignmentServiceTask))]
	class CapabilityTaskAutoAssignmentServiceTaskSinpleCapacityCalculatorTest : CapabilityTaskAutoAssignmentServiceTaskOldCalculatorTest
	{
		protected override bool UsingSimpleQuery => true;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();
		}
	}
}
