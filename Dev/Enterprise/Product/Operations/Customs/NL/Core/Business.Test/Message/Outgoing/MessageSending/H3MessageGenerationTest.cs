using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class H3MessageGenerationTest : MessageGenerationAbstractTest
{
	protected override ZString MessageType => "DEC";

	protected override ZString ExpectedMessage => MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.H3Message.xml");

	protected override ZString ExpectedPrettyMessage => "<font size='2' face='Courier New'>" + ExpectedMessage + "</font>";

	protected override ZString CustomsEntryInstructionStyle => "H3";
}
