
using NUnit.Framework;
namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(CapacityConstrainedResourceStatusServiceTask))]
	class CapacityConstrainedResourceStatusServiceTaskSimpleCalculatorTest : CapacityConstrainedResourceStatusServiceTaskOldCalculatorTest
	{
		protected override bool UsingSimpleQuery => true;

		protected override void LocalSetUp()
		{
			base.LocalSetUp();
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();
		}
	}
}
