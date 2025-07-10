namespace CWNUnit.TestAdapter
{
	public interface ITestOptions
	{
		bool Enabled { get; set; }
		string DatabaseName { get; set; }
		bool BreakOnPerformanceIssues { get; set; }
		bool IncludeAmnestyTests { get; set; }
		bool IncludeDeveloperOnlyTests { get; set; }
		bool EnableTaskTestListener { get; set; }
	}
}
