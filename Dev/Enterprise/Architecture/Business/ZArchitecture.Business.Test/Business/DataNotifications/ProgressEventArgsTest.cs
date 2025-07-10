using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ProgressEventArgsTest : TestCaseWithFactory
	{
		public void TestCurrentProgress()
		{
			ProgressEventArgs eventArgs = new ProgressEventArgs(5, 10);
			AssertEquals("CurrentProgress", 5, eventArgs.CurrentProgress);
		}

		public void TestTotal()
		{
			ProgressEventArgs eventArgs = new ProgressEventArgs(5, 10);
			AssertEquals("Total", 10, eventArgs.Total);
		}

		public void TestPercent()
		{
			ProgressEventArgs eventArgs = new ProgressEventArgs(5, 10);
			AssertEquals("Percentage", 50, eventArgs.PercentComplete);

			eventArgs = new ProgressEventArgs(3, 10);
			AssertEquals("Percentage", 30, eventArgs.PercentComplete);
		}
	}
}
