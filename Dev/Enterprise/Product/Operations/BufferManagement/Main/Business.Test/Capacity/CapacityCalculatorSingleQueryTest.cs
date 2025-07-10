namespace Enterprise.BufferManagement.Business.Test
{
	public class CapacityCalculatorSingleQueryTest : CapacityCalculatorTest
	{
		protected override void SetUp()
		{
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();

			base.SetUp();
		}
	}
}
