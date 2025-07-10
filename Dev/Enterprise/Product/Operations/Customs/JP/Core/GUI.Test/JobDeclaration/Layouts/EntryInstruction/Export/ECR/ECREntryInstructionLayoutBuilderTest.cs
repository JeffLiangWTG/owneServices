using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ECREntryInstructionLayoutBuilder))]
	sealed class ECREntryInstructionLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ECREntryInstructionLayoutBuilder, CusEntryInstruction, ECREntryInstructionControlBag>
	{
		protected override ECREntryInstructionLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new ECREntryInstructionLayoutBuilder();
		}

		protected override int ExpectedMaxColumns => 3;
	}
}
