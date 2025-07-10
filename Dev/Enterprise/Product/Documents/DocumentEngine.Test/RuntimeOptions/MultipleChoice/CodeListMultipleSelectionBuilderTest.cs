using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class CodeListMultipleSelectionBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new CodeListMultipleSelectionBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			CodeListMultipleSelectionBuilderForTest filterBuilder = new CodeListMultipleSelectionBuilderForTest(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator), ReportRunningType.Report);
			AssertEquals(typeof(OptionGroup), filterBuilder.GetFilterField().GetType());
		}

		public void TestCanBuild()
		{
			CodeListMultipleSelectionBuilder filterBuilder = new CodeListMultipleSelectionBuilder(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator), ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("codelistmultipleselect"));
		}

		public class CodeListMultipleSelectionBuilderForTest : CodeListMultipleSelectionBuilder
		{
			public CodeListMultipleSelectionBuilderForTest(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
