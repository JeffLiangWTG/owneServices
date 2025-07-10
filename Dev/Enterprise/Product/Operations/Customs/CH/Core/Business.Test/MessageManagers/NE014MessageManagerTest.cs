using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE014MessageManager))]
class NE014MessageManagerTest : BasePassarExportDeclarationMessageManagerTest
{
	protected override string MessageType => PassarMessageTypeList.Codes.NE014;

	protected override string ExpectedMessageSubType => PassarDeclarationPhaseList.Codes.Cancellation;

	protected override string ExpectedEntryHeaderPhaseStatus => PassarDeclarationPhaseList.Codes.Cancellation;

	protected override Event ExpectedDeclarationSentEvent => Events.DeclarationCancellationSent;

	protected override DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender) => new NE014MessageManager(sender);
}
