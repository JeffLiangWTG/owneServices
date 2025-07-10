using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsLayoutBuilder))]
	class EntryInstructionDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionDetailsLayoutBuilder, CusEntryInstruction, EntryInstructionDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 3;

		protected override EntryInstructionDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new EntryInstructionDetailsLayoutBuilder();
	}
}
