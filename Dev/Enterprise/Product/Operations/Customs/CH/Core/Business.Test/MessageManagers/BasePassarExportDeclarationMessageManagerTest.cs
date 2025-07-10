using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BasePassarExportDeclarationMessageManager))]
abstract class BasePassarExportDeclarationMessageManagerTest : DeclarationMessageManagerTest<ExportDeclarationMessageSendingObjectParent, ExportDeclarationMessageSendingObject>
{
	protected override string DeclarationType => CHJobMessageTypeList.Codes.Export;

	protected override string ExpectedEntryHeaderStatus => Common.Shared.MessageStatusList.Codes.Sent;

	protected override Event ExpectedCustomsCommencedEvent => Events.ExportCustomsCommenced;

	protected override string ExpectedApplicationCode => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override string ExpectedMessageType => MessageTypeCodeList.Codes.Export;

	protected override ExportDeclarationMessageSendingObjectParent CreateMessageSendingObjectParent(JobDeclaration declaration) => new ExportDeclarationMessageSendingObjectParent(declaration);

	protected override GlbExternalPassword CreateCurrentCompanyCredential() => CredentialsTestHelper.CreateCurrentCompanyTokenCredential();
}
