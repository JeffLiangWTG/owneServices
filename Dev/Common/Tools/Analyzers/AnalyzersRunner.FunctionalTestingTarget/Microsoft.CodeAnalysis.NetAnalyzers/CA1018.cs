using System;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	// CA1018: Mark attributes with AttributeUsageAttribute
	public sealed class CA1018 : Attribute
	{
		// Custom attribute logic

		sealed class CA1018Private : Attribute
		{
			// Custom attribute logic
		}
	}
}
