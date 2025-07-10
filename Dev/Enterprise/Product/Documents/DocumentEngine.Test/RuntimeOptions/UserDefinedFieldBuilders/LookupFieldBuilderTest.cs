using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(LookupFieldBuilder))]
	sealed class LookupFieldBuilderTest : UserDefinedFieldBuilderTest
	{
		protected override UserDefinedFieldBuilder GetFilterBuilderToTest()
		{
			return new LookupFieldBuilder(new ValidatorPack(), Factory, DummyEvaluator);
		}

		public void TestCanBuild()
		{
			Assert(FieldBuilder.CanBuild("address Lookup"));
			Assert(!FieldBuilder.CanBuild("Lookup"));
		}
	}
}
