using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>))]
	sealed class EntryInstructionsCoreDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>, CusEntryInstruction, Customs.GUI.EntryInstructionBasicDetailsControlBag>
	{
		protected override EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction> GetColumnLayoutBuilderForTesting() => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();

		protected override int ExpectedMaxColumns => 3;
	}
}
