using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NC016MessageManager))]
sealed class NC016MessageManagerTest : BasePassarExportDeclarationMessageManagerTest
{
	protected override string MessageType => PassarMessageTypeList.Codes.NC016;

	protected override string ExpectedMessageSubType => MessageSubTypeCodeList.Codes.PassarRequestDataJourney;

	protected override string ExpectedEntryHeaderPhaseStatus => PassarDeclarationPhaseList.Codes.RequestDataJourney;

	protected override Event ExpectedCustomsCommencedEvent => null;

	protected override DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender) => new NC016MessageManager(sender);
}
