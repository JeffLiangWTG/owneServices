using Enterprise.Customs.CN.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsLayoutBuilder))]
	class EntryInstructionDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionDetailsLayoutBuilder, CusEntryInstruction, EntryInstructionBasicDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 3;

		protected override EntryInstructionDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new EntryInstructionDetailsLayoutBuilder();
	}
}
