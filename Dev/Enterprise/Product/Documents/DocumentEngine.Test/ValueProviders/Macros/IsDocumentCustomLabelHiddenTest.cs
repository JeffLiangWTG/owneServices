using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(IsDocumentCustomLabelHidden))]
	sealed class IsDocumentCustomLabelHiddenTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new IsDocumentCustomLabelHidden();
		}
	}
}
