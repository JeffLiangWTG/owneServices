using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(NumberFieldBuilder))]
	sealed class NumberFieldBuilderTest : UserDefinedFieldBuilderTest
	{
		protected override UserDefinedFieldBuilder GetFilterBuilderToTest()
		{
			return new NumberFieldBuilder(new ValidatorPack(), Factory, DummyEvaluator);
		}
	}
}
