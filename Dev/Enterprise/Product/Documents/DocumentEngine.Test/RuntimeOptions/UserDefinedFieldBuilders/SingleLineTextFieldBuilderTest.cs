using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SingleLineTextFieldBuilder))]
	sealed class SingleLineTextFieldBuilderTest : UserDefinedFieldBuilderTest
	{
		protected override UserDefinedFieldBuilder GetFilterBuilderToTest()
		{
			return new SingleLineTextFieldBuilder(new ValidatorPack(), Factory, DummyEvaluator);
		}
	}
}
