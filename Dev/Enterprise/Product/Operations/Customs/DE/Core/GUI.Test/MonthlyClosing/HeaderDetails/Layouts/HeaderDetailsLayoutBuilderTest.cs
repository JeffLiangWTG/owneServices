using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(HeaderDetailsLayoutBuilder<CusReconDeclaration>))]
	class HeaderDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<HeaderDetailsLayoutBuilder<CusReconDeclaration>, CusReconDeclaration, HeaderDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 3;

		protected override HeaderDetailsLayoutBuilder<CusReconDeclaration> GetColumnLayoutBuilderForTesting() => new HeaderDetailsLayoutBuilder<CusReconDeclaration>();
	}
}
