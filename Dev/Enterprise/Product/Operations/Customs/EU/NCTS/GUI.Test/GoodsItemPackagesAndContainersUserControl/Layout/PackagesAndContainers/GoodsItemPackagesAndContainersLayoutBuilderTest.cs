using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemPackagesAndContainersLayoutBuilder<NctsDepartureCargoDesc>))]
	class GoodsItemPackagesAndContainersLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<GoodsItemPackagesAndContainersLayoutBuilder<NctsDepartureCargoDesc>, NctsDepartureCargoDesc, GoodsItemPackagesAndContainersControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override GoodsItemPackagesAndContainersLayoutBuilder<NctsDepartureCargoDesc> GetColumnLayoutBuilderForTesting() => new GoodsItemPackagesAndContainersLayoutBuilder<NctsDepartureCargoDesc>();
	}
}
