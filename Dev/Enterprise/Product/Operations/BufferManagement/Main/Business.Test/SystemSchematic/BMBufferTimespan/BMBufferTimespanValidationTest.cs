using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBufferTimespanValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNameAndTimespanValidation()
		{
			var timespan = Factory.New<BMBufferTimespan>();
			timespan.Validation.ValidateAll();

			AssertHasError(timespan.BMT_NameInfo, "Please enter a Name.");
			AssertHasError(timespan.BufferTimespanInfo, "Please enter a Buffer Timespan.");

			timespan.BMT_Name = "Test Span";
			timespan.BMT_BufferTimespanInMinutes = 1;
			AssertNoErrors(timespan.BMT_NameInfo);
			AssertHasError(timespan.BufferTimespanInfo, "The buffer timespan should be at least three minutes, since there are always three zones in a buffer, which are each one minute at the smallest.");

			timespan.BMT_BufferTimespanInMinutes = 3;
			AssertNoErrors(timespan.BMT_NameInfo);
			AssertNoErrors(timespan.BufferTimespanInfo);
		}
	}
}
