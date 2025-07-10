using System.Threading;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Startup.Tools.Testing
{
	sealed class ThreadMonitorValidationTest : TestCaseWithFactory
	{
		public void TestValidateIntervalNumber()
		{
			var monitor = new ThreadMonitor(Thread.CurrentThread)
			{
				IntervalType = IntervalTypeConstant.Minute
			};

			AssertNoErrors(monitor.IntervalNumberInfo);

			monitor.IntervalNumber = -1;
			AssertHasError(monitor.IntervalNumberInfo, "Please enter an 'Interval Number' greater than 0.");

			monitor.IntervalNumber = 70;
			AssertHasError(monitor.IntervalNumberInfo, "Please enter an 'Interval Number' less than or equal to 60.");

			monitor.IntervalType = IntervalTypeConstant.Second;
			AssertNoErrors(monitor.IntervalNumberInfo);

			monitor.IntervalNumber = 3800;
			AssertHasError(monitor.IntervalNumberInfo, "Please enter an 'Interval Number' less than or equal to 3600.");
		}

		public void TestValidateIntervalType()
		{
			var monitor = new ThreadMonitor(Thread.CurrentThread);

			AssertNoErrors(monitor.IntervalTypeInfo);

			monitor.IntervalType = "XXX";
			AssertHasError(monitor.IntervalTypeInfo, "Enter a valid selection.");
		}

		public void TestValidateSelectedThreadId()
		{
			var monitor = new ThreadMonitor(Thread.CurrentThread);
			AssertNoErrors(monitor.SelectedThreadIdInfo);

			monitor.SelectedThreadId++;
			AssertHasError(monitor.SelectedThreadIdInfo, "The selected thread ID does not correspond to an active thread. Please select an active thread.");

			monitor.SelectedThreadId = 0;
			AssertHasError(monitor.SelectedThreadIdInfo, "Please enter a 'Selected Thread Id' greater than 0.");
		}
	}
}
