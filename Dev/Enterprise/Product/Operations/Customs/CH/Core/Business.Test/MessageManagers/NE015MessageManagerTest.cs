using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE015MessageManager))]
sealed class NE015MessageManagerTest : BasePassarExportDeclarationMessageManagerTest
{
	protected override string MessageType => PassarMessageTypeList.Codes.NE015;

	protected override string ExpectedMessageSubType => PassarDeclarationPhaseList.Codes.Declaration;

	protected override string ExpectedEntryHeaderPhaseStatus => PassarDeclarationPhaseList.Codes.Declaration;

	protected override DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender) => new NE015MessageManager(sender);
}
