namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule:	Test for empty strings using string length
	/// </summary>
	internal class CA1820
	{
		readonly string value = "test";

		public CA1820()
		{
			// CA1820: Test for empty strings using string length
			_ = value == "";
		}
	}
}
