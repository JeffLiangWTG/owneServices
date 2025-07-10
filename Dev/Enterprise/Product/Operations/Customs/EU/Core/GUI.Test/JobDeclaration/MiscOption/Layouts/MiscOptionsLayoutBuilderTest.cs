using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayoutBuilder<BaseJobDeclaration>))]
	public class MiscOptionsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MiscOptionsLayoutBuilder<BaseJobDeclaration>, BaseJobDeclaration, CommonMiscOptionsControlBag>
	{
		protected override MiscOptionsLayoutBuilder<BaseJobDeclaration> GetColumnLayoutBuilderForTesting() => new MiscOptionsLayoutBuilder<BaseJobDeclaration>();

		protected override int ExpectedMaxColumns => 2;
	}
}
