using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBufferTimespan))]
	class BMBufferTimespanTest : EnterpriseBusinessObjectTestCase
	{
		public void TestConvertToHours()
		{
			var timespan = Factory.New<BMBufferTimespan>();
			timespan.BMT_BufferTimespanInMinutes = 420;
			AssertEquals(7.0, timespan.BufferTimeSpanHours);
		}

		public void TestTimeDisplay()
		{
			var timespan = Factory.New<BMBufferTimespan>();
			timespan.BMT_Name = "Test Span";
			timespan.BMT_BufferTimespanInMinutes = 245;
			AssertEquals("004:05", timespan.TimeDisplay);
		}

		#region Delete

		public void TestLoadDelete()
		{
			var timespan = Factory.New<BMBufferTimespan>();
			timespan.BMT_Name = "Test Span";
			timespan.BMT_BufferTimespanInMinutes = 20;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			AssertNoExceptionThrown(() => newFactory.Load<BMBufferTimespan>(timespan.PK).Delete());
		}

		#endregion
	}
}
