using Enterprise.BufferManagement.Business.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ReleaseGateKeeperSimpleCalculatorTest : ReleaseGateKeeperOldCalculatorTest
	{
		protected override bool UsingSimpleQuery => true;

		protected override void SetUp()
		{
			base.SetUp();
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();
		}
	}
}
