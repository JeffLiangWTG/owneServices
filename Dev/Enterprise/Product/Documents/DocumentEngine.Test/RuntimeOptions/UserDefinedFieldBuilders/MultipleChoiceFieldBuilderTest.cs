using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(MultipleChoiceFieldBuilder))]
	sealed class MultipleChoiceFieldBuilderTest : UserDefinedFieldBuilderTest
	{
		protected override UserDefinedFieldBuilder GetFilterBuilderToTest()
		{
			return new MultipleChoiceFieldBuilder(new ValidatorPack(), Factory, DummyEvaluator);
		}
	}
}
