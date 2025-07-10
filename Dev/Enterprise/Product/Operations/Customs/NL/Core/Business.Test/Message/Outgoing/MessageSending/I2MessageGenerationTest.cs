using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class I2MessageGenerationTest : MessageGenerationAbstractTest
{
	protected override ZString MessageType => "PRE";

	protected override ZString ExpectedMessage => MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.I2Message.xml");

	protected override ZString ExpectedPrettyMessage => "<font size='2' face='Courier New'>" + ExpectedMessage + "</font>";

	protected override ZString ExpectedMessageSubType => "PRE";
}
