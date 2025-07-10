using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class DateBasedAccountingPeriodBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new DateBasedAccountingPeriodBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			DateBasedAccountingPeriodBuilderForTesting filterBuilder = new DateBasedAccountingPeriodBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(DateBasedAccountingPeriodField), filterBuilder.GetFilterField().GetType());
		}

		public void TestCanBuild_PeriodDate()
		{
			DateBasedAccountingPeriodBuilderForTesting filterBuilder = new DateBasedAccountingPeriodBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("Period Date"));
		}

		public void TestCanBuild_AccountingPeriodDate()
		{
			DateBasedAccountingPeriodBuilderForTesting filterBuilder = new DateBasedAccountingPeriodBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("Accounting Period Date"));
		}

		public void TestCanBuild_AccountingDate()
		{
			DateBasedAccountingPeriodBuilderForTesting filterBuilder = new DateBasedAccountingPeriodBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(false, filterBuilder.CanBuild("Accounting Date"));
		}

		public void TestCanBuild_AccountingPeriod()
		{
			DateBasedAccountingPeriodBuilderForTesting filterBuilder = new DateBasedAccountingPeriodBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(false, filterBuilder.CanBuild("Accounting Period"));
		}

		public class DateBasedAccountingPeriodBuilderForTesting : DateBasedAccountingPeriodBuilder
		{
			public DateBasedAccountingPeriodBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
