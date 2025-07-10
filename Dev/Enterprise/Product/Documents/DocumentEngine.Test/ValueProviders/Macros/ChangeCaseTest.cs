using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ChangeCase))]
	sealed class ChangeCaseTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<ChangeCase>");
			AssertNotResponsibleForReplacing("<ChangeCase()>");
			AssertNotResponsibleForReplacing("<ChangeCase(,)>");
			AssertNotResponsibleForReplacing("<ChangeCase(\"Hello\")>");

			AssertIsResponsibleForReplacing("<ChangeCase(\"Hello\", L)>");
			AssertIsResponsibleForReplacing("<ChangeCase(\"Hello Hello\", u)>");
			AssertIsResponsibleForReplacing("<ChangeCase(\"<Z0_VarCharMax>\", t)>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("HELLO HELLO", "<ChangeCase(\"Hello Hello\", U)>");
			AssertIsReplacedWith("hello hello", "<ChangeCase(\"Hello Hello\", L)>");
			AssertIsReplacedWith("Hello Hello", "<ChangeCase(\"heLlo hEllo\", T)>");
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ChangeCase();
		}
	}
}
