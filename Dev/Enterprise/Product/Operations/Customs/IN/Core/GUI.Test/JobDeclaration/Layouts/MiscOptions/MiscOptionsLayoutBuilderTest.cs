using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(MiscOptionsLayoutBuilder))]
sealed class MiscOptionsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MiscOptionsLayoutBuilder, JobDeclaration, CommonMiscOptionsControlBag>
{
	protected override MiscOptionsLayoutBuilder GetColumnLayoutBuilderForTesting() => new();

	protected override int ExpectedMaxColumns => 2;
}
