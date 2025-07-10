using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE069MessageManager))]
sealed class NE069MessageManagerTest : BasePassarExportDeclarationMessageManagerTest
{
	protected override string MessageType => PassarMessageTypeList.Codes.NE069;

	protected override string ExpectedMessageSubType => PassarDeclarationPhaseList.Codes.Rectification;

	protected override string ExpectedEntryHeaderStatus => CHLogicalStatusList.Codes.Sent;

	protected override string ExpectedEntryHeaderPhaseStatus => PassarDeclarationPhaseList.Codes.Rectification;

	protected override Event ExpectedDeclarationSentEvent => null;

	protected override Event ExpectedCustomsCommencedEvent => null;

	protected override DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender) => new NE069MessageManager((ExportDeclarationMessageSendingObject)sender);
}
