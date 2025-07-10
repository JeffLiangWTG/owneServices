using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Modifiable))]
	sealed class ModifiableTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("<Modifiable(some content)>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(some content)>", Passes.FirstPass));
			Assert("<Modifiable(<Z0_VarCharMax>)>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(<Z0_VarCharMax>)>", Passes.FirstPass));
			Assert("<Modifiable(<Z0_VarCharMax>\n<ZD1_Text>)>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(<Z0_VarCharMax>\n<ZD1_Text>)>", Passes.FirstPass));
			Assert("<Modifiable(<Z0_VarCharMax>)   >", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(<Z0_VarCharMax>)   >", Passes.FirstPass));
			Assert("<Modifiable    (<Z0_VarCharMax>)>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable    (<Z0_VarCharMax>)>", Passes.FirstPass));
			Assert("<    Modifiable(<Z0_VarCharMax>)>", ValueProviderToTest.IsResponsibleForReplacing("<    Modifiable(<Z0_VarCharMax>)>", Passes.FirstPass));
			Assert("<  Modifiable  (<Z0_VarCharMax>)>", ValueProviderToTest.IsResponsibleForReplacing("<  Modifiable  (<Z0_VarCharMax>)>", Passes.FirstPass));
			Assert("<Modifiable(<Z0_VarCharMax>)>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(<Z0_VarCharMax>)>", Passes.FirstPass));
			Assert("<Modifiable   (<Z0_VarCharMax>)   >", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable   (<Z0_VarCharMax>)   >", Passes.FirstPass));
			Assert("<Modifiable(<Z0_VarCharMax>)>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(<Z0_VarCharMax>)>", Passes.FirstPass));
			Assert("<  Modifiable(<Z0_VarCharMax>)  >", ValueProviderToTest.IsResponsibleForReplacing("<  Modifiable(<Z0_VarCharMax>)  >", Passes.FirstPass));
			Assert("<Modifiable(  <Z0_VarCharMax>  )>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(  <Z0_VarCharMax>  )>", Passes.FirstPass));
			Assert("<Modifiable(<Z0_VarCharMax>)>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(<Z0_VarCharMax>)>", Passes.FirstPass));
			Assert("<Modifiable(<Z0_VarCharMax> <Z0_VarCharMax  )>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(<Z0_VarCharMax> <Z0_VarCharMax  )>", Passes.FirstPass));
			Assert("<Modifiable(<Z0_VarCharMax> <Z0_VarCharMax  )>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(<Z0_VarCharMax> <Z0_VarCharMax  )>", Passes.FirstPass));
			Assert("<Modifiable()())()(())()>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable()())()(())()>", Passes.FirstPass));
			Assert("Not responsible: <Modifiable()>", !ValueProviderToTest.IsResponsibleForReplacing("<Modifiable()>", Passes.FirstPass));
			Assert("<Modifiable( )>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable( )>", Passes.FirstPass));
			Assert("<Modifiable(s)>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable(s)>", Passes.FirstPass));
			Assert("<Modifiable()))))>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable()))))>", Passes.FirstPass));
			Assert("<Modifiable((((()(((()>", ValueProviderToTest.IsResponsibleForReplacing("<Modifiable((((()(((()>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("some content", ValueProviderToTest.GetReplacement("<Modifiable(some content)>", Report));
			AssertEquals(" ", ValueProviderToTest.GetReplacement("<Modifiable( )>", Report));
			AssertEquals("SDEF35wV%2353%%v@#%q@5", ValueProviderToTest.GetReplacement("<Modifiable(SDEF35wV%2353%%v@#%q@5)>", Report));
			AssertEquals("some content", ValueProviderToTest.GetReplacement("<  Modifiable(some content)>", Report));
			AssertEquals("some content", ValueProviderToTest.GetReplacement("<Modifiable(some content)  >", Report));
			AssertEquals("the answer: 42", ValueProviderToTest.GetReplacement("<Modifiable(the answer: 42)  >", Report));
			AssertEquals("<Z0_VarCharMax>", ValueProviderToTest.GetReplacement("<Modifiable  (<Z0_VarCharMax>)  >", Report));
			AssertEquals("<Z0_VarCharMax> \n <ZD1_Text>", ValueProviderToTest.GetReplacement("<Modifiable  (<Z0_VarCharMax> \n <ZD1_Text>)  >", Report));
			AssertEquals("085137+/-+", ValueProviderToTest.GetReplacement("<Modifiable(085137+/-+)  >", Report));
			AssertEquals("\"blah\"", ValueProviderToTest.GetReplacement("<Modifiable(\"blah\")  >", Report));
			AssertEquals("$42 (AUD)", ValueProviderToTest.GetReplacement("<Modifiable($42 (AUD))  >", Report));
			AssertEquals(")()()()()()()()(", ValueProviderToTest.GetReplacement("<Modifiable()()()()()()()()()  >", Report));
			AssertEquals("()()()", ValueProviderToTest.GetReplacement("<Modifiable(()()())  >", Report));
			AssertEquals("(AU)$42", ValueProviderToTest.GetReplacement("<Modifiable((AU)$42)  >", Report));
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Modifiable();
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			AssertEquals(expectedResult, ValueProviderToTest.GetReplacement(example, Report));
		}
	}
}
