using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(UNDGLayoutBuilder<JobDeclaration>))]
	sealed class UNDGLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<UNDGLayoutBuilder<JobDeclaration>, JobDeclaration, UNDGUserControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override UNDGLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new UNDGLayoutBuilder<JobDeclaration>();
	}
}
