using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EXPProductsCustomDetailsLayoutBuilder))]
	sealed class EXPProductsCustomDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EXPProductsCustomDetailsLayoutBuilder, OrgSupplierPart, EXPProductsCustomDetailsControlBag>
	{
		protected override EXPProductsCustomDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new EXPProductsCustomDetailsLayoutBuilder();
	}
}
