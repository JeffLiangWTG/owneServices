using System;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	// CA1813: Avoid unsealed attributes
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class CA1813 : Attribute
	{
		// Custom attribute logic

		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
		class CA1813Private : Attribute
		{
			// Custom attribute logic
		}

		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
		protected class CA1813Protected : Attribute
		{
			// Custom attribute logic
		}
	}
}
