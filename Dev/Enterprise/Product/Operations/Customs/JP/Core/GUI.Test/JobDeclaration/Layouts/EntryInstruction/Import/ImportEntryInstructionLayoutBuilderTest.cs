using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportEntryInstructionLayoutBuilder))]
	sealed class ImportEntryInstructionLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ImportEntryInstructionLayoutBuilder, CusEntryInstruction, ImportEntryInstructionControlBag>
	{
		protected override ImportEntryInstructionLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new ImportEntryInstructionLayoutBuilder();
		}

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
