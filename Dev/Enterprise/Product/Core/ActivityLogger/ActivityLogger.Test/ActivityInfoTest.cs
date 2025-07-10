using NUnit.Framework;

namespace Enterprise.ActivityLogger.Test
{
	sealed class ActivityInfoTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Enterprise.ActivityLogger.ActivityInfo")]
		public void TestInvalidProcessId()
		{
			AssertNoExceptionThrown(() => new ActivityInfo(12345));
		}
	}
}
