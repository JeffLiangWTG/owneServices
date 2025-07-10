using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class PresentationMessageGenerationTest : MessageGenerationAbstractTest
{
	protected override ZString MessageType => "PRN";

	protected override ZString ExpectedMessage => ZString.Empty;

	protected override ZString ExpectedMessageSubType => "PRN";

	protected override ZString ExpectedPrettyMessage => "The generated message has no content.";

	protected override ZString DeclarationMessageType => MessageTypeList.Codes.Export;

	protected override ZString ExpectedEntryHeaderPhaseStatus => ZString.Empty;

	protected override ZString ExpectedEntryHeaderStatus => ZString.Empty;

	protected override ZString ExpectedEntryHeaderEntryStatus => ZString.Empty;
}
