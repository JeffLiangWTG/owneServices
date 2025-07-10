using System;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Properties should not return arrays
	/// </summary>
	public class CA1819
	{
		// CA1819: Properties should not return arrays
		public string[] Pages => Array.Empty<string>();
	}

	/// <summary>
	/// Rule: Properties should not return arrays
	/// </summary>
	class CA1819Internal
	{
		// CA1819: Properties should not return arrays
		public string[] Pages => Array.Empty<string>();

		class CA1819Private
		{
			// CA1819: Properties should not return arrays
			public string[] Pages => Array.Empty<string>();
		}
	}
}
