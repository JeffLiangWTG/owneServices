using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	class ColumnConfigurationFitlerBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new ColumnConfigurationFilterBuilder(new ValidatorPack(), Factory, DummyEvaluator, new ColumnConfigurationsManager(ZGuid.NewZGuid(), false), ReportRunningType.Report);
		}
	}
}
