using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemPackagesAndContainersLayoutBuilder<NctsPackage>))]
	internal class Phase5GoodsItemPackagesAndContainersLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Phase5GoodsItemPackagesAndContainersLayoutBuilder<NctsPackage>, NctsPackage, Phase5GoodsItemPackagesAndContainersControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override Phase5GoodsItemPackagesAndContainersLayoutBuilder<NctsPackage> GetColumnLayoutBuilderForTesting() => new Phase5GoodsItemPackagesAndContainersLayoutBuilder<NctsPackage>();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
