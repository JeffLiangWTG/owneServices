using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(IsReportCustomLabelHidden))]
	sealed class IsReportCustomLabelHiddenTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new IsReportCustomLabelHidden();
		}
	}
}
