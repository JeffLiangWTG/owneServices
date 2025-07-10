using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.GUI;

public partial class SendMessageForm : MessageSendingFormWithValidationDetails
{
	[Obsolete("Do not call. Only for designer use.")]
	public SendMessageForm()
	{ }

	public SendMessageForm(BaseMessageSendingObjectParent messageParent) : base(messageParent)
	{
	}

	public MessageSendingContext Context { get; private set; }

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		DownloadButton.Enabled = SendButton.Enabled;
		SendButton.EnabledChanged += (s, e) => DownloadButton.Enabled = SendButton.Enabled;
	}

	protected override bool CheckIsOKToSend()
	{
		return base.CheckIsOKToSend() && SendMessageHelper.SetTokenPinIfNeeded(this, Context);
	}

	protected override void SendButton_Click(object sender, EventArgs e)
	{
		Context = sender == SendButton ? MessageSendingContext.EMAIL : MessageSendingContext.DOWNLOAD;
		base.SendButton_Click(sender, e);
	}
}
