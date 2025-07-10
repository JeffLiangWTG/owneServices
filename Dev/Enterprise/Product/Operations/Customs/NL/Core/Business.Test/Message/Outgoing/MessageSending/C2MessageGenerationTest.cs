using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class C2MessageGenerationTest : MessageGenerationAbstractTest
{
	protected override ZString MessageType => "DEC";

	protected override ZString CustomsEntryInstructionStyle => "C2";

	protected override ZString DeclarationMessageType => MessageTypeList.Codes.Export;

	protected override ZString ExpectedMessage => MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.C2Message.xml");

	protected override ZString ExpectedPrettyMessage => "<font size='2' face='Courier New'>" + ExpectedMessage + "</font>";

	protected override ZString ExpectedEntryHeaderPhaseStatus => CustomsEntryPhaseStatusList.Codes._515;

	protected override ZString ExpectedEntryHeaderStatus => NLConstants.StatusNew.SentToCustoms;
}
