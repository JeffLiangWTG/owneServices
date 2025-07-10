using NUnit.Framework;

namespace Enterprise.ZArchitecture.Benchmark.Framework.TestInterfaces
{
	public interface ITestContext
	{
		bool IsRunningOnDAT { get; }

		/// <summary>
		/// Passes the test. Designed to be called when running on DAT.
		/// </summary>
		void PassWithMessage(string message);

		/// <summary>
		/// Reports test results via a test failure / warning message. Designed to be called when running on local.
		/// </summary>
		void ReportTestResults(string message);
	}

	public sealed class NUnit1TestContext : ITestContext
	{
		public bool IsRunningOnDAT => TestingState.IsRunningOnDAT;

		public void PassWithMessage(string message)
			=> Assertion.Assert(message, true);

		public void ReportTestResults(string message)
			=> Assertion.Fail(message);
	}

	public sealed class NUnit4TestContext : ITestContext
	{
		// https://devops.wisetechglobal.com/wtg/DevTools/_wiki/wikis/DevTools.wiki/11015/Known-Environment-Variables
		public bool IsRunningOnDAT
			=> bool.TryParse(System.Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var onDat) && onDat;

		public void PassWithMessage(string message)
			=> Assert.Pass(message);

		public void ReportTestResults(string message)
			=> Assert.Warn(message);  // Note: this will cause the test to fail on DAT; because DAT only has Pass / Fail outcomes for tests.
	}
}
