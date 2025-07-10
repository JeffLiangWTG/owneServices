using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemPackageLayoutBuilder<NctsPackage>))]
	class GoodsItemPackageLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<GoodsItemPackageLayoutBuilder<NctsPackage>, NctsPackage, GoodsItemPackageControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override GoodsItemPackageLayoutBuilder<NctsPackage> GetColumnLayoutBuilderForTesting() => new GoodsItemPackageLayoutBuilder<NctsPackage>();
	}
}
