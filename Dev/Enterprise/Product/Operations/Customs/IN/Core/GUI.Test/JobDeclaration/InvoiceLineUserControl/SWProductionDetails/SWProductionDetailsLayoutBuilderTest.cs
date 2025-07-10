using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SWProductionDetailsLayoutBuilder<SWProduction>))]
sealed class SWProductionDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SWProductionDetailsLayoutBuilder<SWProduction>, SWProduction, SWProductionDetailsControlBag>
{
	protected override SWProductionDetailsLayoutBuilder<SWProduction> GetColumnLayoutBuilderForTesting() => new SWProductionDetailsLayoutBuilder<SWProduction>();

	protected override int ExpectedMaxColumns => 2;
}
