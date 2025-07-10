using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>))]
sealed class GoodsItemDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>, NctsDepartureCargoDesc, GoodsItemDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 2;

	protected override GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc> GetColumnLayoutBuilderForTesting() => new GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>();
}
