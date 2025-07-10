using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class InformationOnNonExitedExportMessageGenerationTest : MessageGenerationAbstractTest
{
	protected override ZString MessageType => "EXT";

	protected override ZString ExpectedMessage => MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.ExitInformationMessage.xml");

	protected override ZString ExpectedMessageSubType => "EXT";

	protected override ZString ExpectedPrettyMessage => "<font size='2' face='Courier New'>" + ExpectedMessage + "</font>";

	protected override ZString DeclarationMessageType => MessageTypeList.Codes.Export;

	protected override ZString ExpectedEntryHeaderPhaseStatus => CustomsEntryPhaseStatusList.Codes._583;

	protected override ZString ExpectedEntryHeaderStatus => NLConstants.StatusNew.SentToCustoms;
}
