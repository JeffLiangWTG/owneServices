using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SingleAccountingPeriodEndDateBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new SingleAccountingPeriodBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			SingleAccountingPeriodEndDateBuilderForTesting filterBuilder = new SingleAccountingPeriodEndDateBuilderForTesting(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator), ReportRunningType.Report);
			AssertEquals(typeof(SingleAccountingPeriodEndDateField), filterBuilder.GetFilterField().GetType());
		}

		public void TestCanBuild1()
		{
			SingleAccountingPeriodEndDateBuilderForTesting filterBuilder = new SingleAccountingPeriodEndDateBuilderForTesting(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator), ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("Period end date"));
		}

		public void TestCanBuild2()
		{
			SingleAccountingPeriodEndDateBuilderForTesting filterBuilder = new SingleAccountingPeriodEndDateBuilderForTesting(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator), ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("Accounting Period end date"));
		}

		public class SingleAccountingPeriodEndDateBuilderForTesting : SingleAccountingPeriodEndDateBuilder
		{
			public SingleAccountingPeriodEndDateBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
