using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class TemporaryStorageMessagesMenu : EU.TemporaryStorage.GUI.TemporaryStorageMessagesMenu
	{
		public TemporaryStorageMessagesMenu(ZForm parentForm) : base(parentForm)
		{
			this.Name = nameof(TemporaryStorageMessagesMenu);
		}

		protected override MessageSendingFormWithValidationDetails GetNewMessageSendingForm(BaseMessageSendingObjectParent messageSendingObjectParent) => new UCC6TemporaryStorageMessageSendingForm(messageSendingObjectParent);

		protected override bool NeedToPreviewMessage => true;
	}
}
