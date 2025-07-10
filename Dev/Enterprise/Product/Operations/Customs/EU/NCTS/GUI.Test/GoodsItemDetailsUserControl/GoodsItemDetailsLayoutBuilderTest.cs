using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>))]
	class GoodsItemDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>, NctsDepartureCargoDesc, GoodsItemDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 3;

		protected override GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc> GetColumnLayoutBuilderForTesting() => new GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>();
	}
}
