using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportEntryInstructionLayoutBuilder))]
	sealed class ExportEntryInstructionLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ExportEntryInstructionLayoutBuilder, CusEntryInstruction, ExportEntryInstructionControlBag>
	{
		protected override ExportEntryInstructionLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new ExportEntryInstructionLayoutBuilder();
		}

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
