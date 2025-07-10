using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CDB01EntryInstructionLayoutBuilder))]
	sealed class CDB01EntryInstructionLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CDB01EntryInstructionLayoutBuilder, CusEntryInstruction, CDB01EntryInstructionControlBag>
	{
		protected override CDB01EntryInstructionLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new CDB01EntryInstructionLayoutBuilder();
		}

		protected override int ExpectedMaxColumns => 3;
	}
}
