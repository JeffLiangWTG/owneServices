using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SingleAccountingPeriodStartDateBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new SingleAccountingPeriodStartDateBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			SingleAccountingPeriodStartDateBuilderForTesting filterBuilder = new SingleAccountingPeriodStartDateBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(SingleAccountingPeriodStartDateField), filterBuilder.GetFilterField().GetType());
		}

		public void TestCanBuild1()
		{
			SingleAccountingPeriodStartDateBuilderForTesting filterBuilder = new SingleAccountingPeriodStartDateBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("Period start date"));
		}

		public void TestCanBuild2()
		{
			SingleAccountingPeriodStartDateBuilderForTesting filterBuilder = new SingleAccountingPeriodStartDateBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("Accounting Period start date"));
		}

		public class SingleAccountingPeriodStartDateBuilderForTesting : SingleAccountingPeriodStartDateBuilder
		{
			public SingleAccountingPeriodStartDateBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
