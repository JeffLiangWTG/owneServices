using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(MonthYearPeriodBuilder))]
	sealed class MonthYearPeriodBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new MonthYearPeriodBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			var filterBuilder = new MonthYearPeriodBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(MonthYearPeriodField), filterBuilder.NewField().GetType());
		}

		public void TestCanBuild()
		{
			var filterBuilder = new MonthYearPeriodBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("MonthYearPeriod"));
		}
	}
}
