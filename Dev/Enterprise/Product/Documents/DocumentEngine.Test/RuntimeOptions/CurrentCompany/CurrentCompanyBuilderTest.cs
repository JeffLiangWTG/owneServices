using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class CurrentCompanyBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new CurrentCompanyBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			CurrentCompanyBuilderForTesting filterBuilder = new CurrentCompanyBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(CurrentCompanyField), filterBuilder.GetFilterField().GetType());
		}

		public void TestCanBuild()
		{
			CurrentCompanyBuilderForTesting filterBuilder = new CurrentCompanyBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("current company"));
		}

		public class CurrentCompanyBuilderForTesting : CurrentCompanyBuilder
		{
			public CurrentCompanyBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
