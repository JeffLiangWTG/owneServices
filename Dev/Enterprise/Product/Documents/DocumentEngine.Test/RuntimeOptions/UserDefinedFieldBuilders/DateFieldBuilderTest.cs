using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(DateFieldBuilder))]
	sealed class DateFieldBuilderTest : UserDefinedFieldBuilderTest
	{
		protected override UserDefinedFieldBuilder GetFilterBuilderToTest()
		{
			return new DateFieldBuilder(new ValidatorPack(), Factory, DummyEvaluator);
		}
	}
}
