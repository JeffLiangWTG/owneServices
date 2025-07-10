using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CommonEntryDetailsLayoutBuilder<JobDeclaration>))]
	class CommonEntryDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CommonEntryDetailsLayoutBuilder<JobDeclaration>, JobDeclaration, CommonEntryDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override CommonEntryDetailsLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new CommonEntryDetailsLayoutBuilder<JobDeclaration>();
	}
}
