using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(CheckBoxFieldBuilder))]
	sealed class CheckBoxFieldBuilderTest : UserDefinedFieldBuilderTest
	{
		protected override UserDefinedFieldBuilder GetFilterBuilderToTest()
		{
			return new CheckBoxFieldBuilder(new ValidatorPack(), Factory, DummyEvaluator);
		}
	}
}
