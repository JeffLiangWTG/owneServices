using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(EntryInstructionsDetailsLayoutBuilder))]
sealed class EntryInstructionsDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionsDetailsLayoutBuilder, CusEntryInstruction, Customs.GUI.EntryInstructionBasicDetailsControlBag>
{
	protected override EntryInstructionsDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new();

	protected override int ExpectedMaxColumns => 3;
}
