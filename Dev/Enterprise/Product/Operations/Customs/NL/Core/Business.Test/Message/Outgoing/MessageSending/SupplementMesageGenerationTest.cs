using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class SupplementMessageGenerationTest : MessageGenerationAbstractTest
{
	protected override ZString MessageType => "SUP";

	protected override ZString ExpectedMessage => ZString.Empty;

	protected override ZString ExpectedMessageSubType => "SUP";

	protected override ZString ExpectedPrettyMessage => "The generated message has no content.";

	protected override ZString DeclarationMessageType => MessageTypeList.Codes.Export;

	protected override ZString InitialPhaseStatus => CustomsEntryPhaseStatusList.Codes.SUP;

	protected override ZString ExpectedEntryHeaderPhaseStatus => CustomsEntryPhaseStatusList.Codes.SUP;

	protected override ZString ExpectedEntryHeaderStatus => NLConstants.StatusNew.SentToCustoms;
}
