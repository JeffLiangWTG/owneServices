namespace CWNUnit.TestAdapter
{
	public class TestOptions : ITestOptions
	{
		public bool Enabled { get; set; }

		public string DatabaseName { get; set; }

		public bool BreakOnPerformanceIssues { get; set; }

		public bool IncludeAmnestyTests { get; set; }

		public bool IncludeDeveloperOnlyTests { get; set; }

		public bool EnableTaskTestListener { get; set; }
	}
}
