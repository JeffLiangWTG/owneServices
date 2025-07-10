using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>))]
sealed class GoodsItemDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>, NctsDepartureCargoDesc, GoodsItemDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 3;

	protected override GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc> GetColumnLayoutBuilderForTesting() => new GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>();
}
