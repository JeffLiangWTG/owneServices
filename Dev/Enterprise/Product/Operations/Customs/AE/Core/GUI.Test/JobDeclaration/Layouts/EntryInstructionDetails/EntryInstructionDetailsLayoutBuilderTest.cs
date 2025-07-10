using Enterprise.Customs.AE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsLayoutBuilder))]
sealed class EntryInstructionDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionDetailsLayoutBuilder, CusEntryInstruction, Customs.GUI.EntryInstructionBasicDetailsControlBag>
{
	protected override EntryInstructionDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new EntryInstructionDetailsLayoutBuilder();

	protected override int ExpectedMaxColumns => 3;
}
