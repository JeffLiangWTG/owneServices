using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EntryDetailsLayoutBuilder))]
sealed class EntryDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryDetailsLayoutBuilder, JobDeclaration, EU.GUI.CommonEntryDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 3;

	protected override EntryDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new EntryDetailsLayoutBuilder();
}
