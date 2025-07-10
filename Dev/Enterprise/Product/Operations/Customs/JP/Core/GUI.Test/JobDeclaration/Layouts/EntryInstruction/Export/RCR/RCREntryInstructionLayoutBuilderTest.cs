using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(RCREntryInstructionLayoutBuilder))]
	sealed class RCREntryInstructionLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<RCREntryInstructionLayoutBuilder, CusEntryInstruction, RCREntryInstructionControlBag>
	{
		protected override RCREntryInstructionLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new RCREntryInstructionLayoutBuilder();
		}
	}
}
