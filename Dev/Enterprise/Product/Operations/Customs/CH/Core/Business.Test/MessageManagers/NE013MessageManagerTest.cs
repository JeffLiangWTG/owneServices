using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE013MessageManager))]
sealed class NE013MessageManagerTest : BasePassarExportDeclarationMessageManagerTest
{
	protected override string MessageType => PassarMessageTypeList.Codes.NE013;

	protected override string ExpectedMessageSubType => PassarDeclarationPhaseList.Codes.Amendment;

	protected override string ExpectedEntryHeaderStatus => null;

	protected override string ExpectedEntryHeaderPhaseStatus => PassarDeclarationPhaseList.Codes.Amendment;

	protected override Event ExpectedCustomsCommencedEvent => null;

	protected override Event ExpectedDeclarationSentEvent => null;

	public void TestMessageStatusChangeEvent()
	{
		Sender.ShouldSend = true;

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
		{
			var entryHeader = (CusEntryHeader)Sender.Header;
			entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
			Sender.MessageType = MessageType;
			Factory.Save();

			var messages = Manager.GenerateMessages();

			Factory.Save();

			EventsTestHelper.AssertEventAdded(entryHeader, Events.MessageStatusChange, Common.Shared.MessageStatusList.Codes.Sent);
		}
	}

	protected override DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender) => new NE013MessageManager((ExportDeclarationMessageSendingObject)sender);
}
