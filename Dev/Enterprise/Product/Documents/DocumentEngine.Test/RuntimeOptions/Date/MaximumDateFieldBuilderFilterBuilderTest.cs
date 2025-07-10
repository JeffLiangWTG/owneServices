namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class MaximumDateFieldBuilderFilterBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest() => new MaximumDateFieldBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
	}
}
