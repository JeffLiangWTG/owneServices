using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class FileProcessingTesting : TestCase
	{
		public void TestProcessedEventArgs()
		{
			ProcessedEventArgs args = new ProcessedEventArgs(10, 20, 30, "Test");
			AssertEquals(10, args.PercentageComplete);
			AssertEquals(20, args.ProcessedCount);
			AssertEquals(30, args.FailureCount);
			AssertEquals("Test", args.LogEntry);
		}

		public void TestProcessFileEventArgs()
		{
			ProcessFileEventArgs args = new ProcessFileEventArgs("Test");
			AssertEquals("Test", args.UnmappedFileName);
		}
	}
}
