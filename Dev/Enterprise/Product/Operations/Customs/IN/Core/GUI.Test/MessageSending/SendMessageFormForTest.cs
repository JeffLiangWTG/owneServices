using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI.Testing;

class SendMessageFormForTest : SendMessageForm
{
	public SendMessageFormForTest(BaseMessageSendingObjectParent messageSendingObjectParent) : base(messageSendingObjectParent)
	{
	}

	public ZButton SendButtonExposed => base.GetEffectiveSendButton();

	public ZButton DownloadButtonExposed => base.DownloadButton;
}
