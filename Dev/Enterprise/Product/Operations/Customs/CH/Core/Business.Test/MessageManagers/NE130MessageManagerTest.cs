using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE130MessageManager))]
sealed class NE130MessageManagerTest : BasePassarExportDeclarationMessageManagerTest
{
	protected override string DeclarationType => CHJobMessageTypeList.Codes.ExportDeclarationActivation;

	protected override string MessageType => PassarMessageTypeList.Codes.NE130;

	protected override string ExpectedMessageSubType => PassarDeclarationPhaseList.Codes.EDecToPassarDataTransfer;

	protected override string ExpectedEntryHeaderPhaseStatus => PassarDeclarationPhaseList.Codes.EDecToPassarDataTransfer;

	protected override Event ExpectedCustomsCommencedEvent => null;

	protected override ZString ExpectedDeclarationSentEventReference => PassarMessageTypeList.Codes.NE130;

	protected override Event ExpectedDeclarationSentEvent => Events.DeclarationActivationSent;

	protected override DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender) => new NE130MessageManager((ExportDeclarationMessageSendingObject)sender);
}
