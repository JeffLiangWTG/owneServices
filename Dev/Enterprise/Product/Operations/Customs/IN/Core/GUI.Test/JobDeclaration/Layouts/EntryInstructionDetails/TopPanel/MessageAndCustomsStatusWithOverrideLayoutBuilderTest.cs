using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(MessageAndCustomsStatusWithOverrideLayoutBuilder))]
sealed class MessageAndCustomsStatusWithOverrideLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MessageAndCustomsStatusWithOverrideLayoutBuilder, CusEntryInstruction, EntryInstructionDetailsControlBag>
{
	protected override MessageAndCustomsStatusWithOverrideLayoutBuilder GetColumnLayoutBuilderForTesting() => new MessageAndCustomsStatusWithOverrideLayoutBuilder();

	protected override int ExpectedMaxColumns => 1;
}
