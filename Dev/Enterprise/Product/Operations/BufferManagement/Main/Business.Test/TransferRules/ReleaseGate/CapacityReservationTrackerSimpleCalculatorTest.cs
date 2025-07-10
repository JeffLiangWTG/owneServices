namespace Enterprise.BufferManagement.Business.Test
{
	class CapacityReservationTrackerSimpleCalculatorTest : CapacityReservationTrackerOldCalculatorTest
	{
		protected override bool UsingSimpleQuery => true;

		protected override void SetUp()
		{
			base.SetUp();
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();
		}
	}
}
