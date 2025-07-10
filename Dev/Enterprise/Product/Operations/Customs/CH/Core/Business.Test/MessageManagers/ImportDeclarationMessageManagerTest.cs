using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ImportDeclarationMessageManager))]
sealed class ImportDeclarationMessageManagerTest : DeclarationMessageManagerTest<ImportDeclarationMessageSendingObjectParent, ImportDeclarationMessageSendingObject>
{
	protected override string DeclarationType => JobMessageTypeList.Codes.Import;

	protected override string MessageType => PassarMessageTypeList.Codes.NI015;

	protected override string ExpectedMessageType => MessageTypeCodeList.Codes.Import;

	protected override string ExpectedMessageSubType => MessageSubTypeCodeList.Codes.ImportDeclaration;

	protected override string ExpectedEntryHeaderStatus => CHLogicalStatusList.Codes.Sent;

	protected override string ExpectedEntryHeaderPhaseStatus => PassarDeclarationPhaseList.Codes.Declaration;

	protected override Event ExpectedCustomsCommencedEvent => Events.CustomsCommenced;

	protected override string ExpectedApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override ImportDeclarationMessageSendingObjectParent CreateMessageSendingObjectParent(JobDeclaration declaration) => new ImportDeclarationMessageSendingObjectParent(declaration);

	protected override DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender) => new ImportDeclarationMessageManager(sender);

	protected override GlbExternalPassword CreateCurrentCompanyCredential() => CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

	public void TestEntryHeaderStatusChange() => CombineAssertions(() =>
	{
		Sender.ShouldSend = true;

		var testHelper = new SendingObjectsTestHelper();
		CreateCurrentCompanyCredential();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
		{
			var entryHeader = (CusEntryHeader)Sender.Header;

			Sender.MessageType = PassarMessageTypeList.Codes.NI015;
			var messages = Manager.GenerateMessages();
			Factory.Save();
			testHelper.AssertCusEntryHeader(entryHeader, CHLogicalStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Declaration);

			Sender.MessageType = PassarMessageTypeList.Codes.NI013;
			messages = Manager.GenerateMessages();
			Factory.Save();
			testHelper.AssertCusEntryHeader(entryHeader, CHLogicalStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Amendment);

			Sender.MessageType = PassarMessageTypeList.Codes.NI014;
			messages = Manager.GenerateMessages();
			Factory.Save();
			testHelper.AssertCusEntryHeader(entryHeader, CHLogicalStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Cancellation);

			entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
			entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Declaration;
			Sender.MessageType = PassarMessageTypeList.Codes.NI016;
			messages = Manager.GenerateMessages();
			Factory.Save();
			testHelper.AssertCusEntryHeader(entryHeader, CHLogicalStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Declaration);
		}
	});
}
