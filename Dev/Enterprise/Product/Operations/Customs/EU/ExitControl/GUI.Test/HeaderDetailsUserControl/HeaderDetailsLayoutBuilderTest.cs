using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(HeaderDetailsLayoutBuilder<CusExitHeader>))]
	sealed class HeaderDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<HeaderDetailsLayoutBuilder<CusExitHeader>, CusExitHeader, HeaderDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override HeaderDetailsLayoutBuilder<CusExitHeader> GetColumnLayoutBuilderForTesting() => new HeaderDetailsLayoutBuilder<CusExitHeader>();
	}
}
