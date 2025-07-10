using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

public partial class SendMessageForm : IN.GUI.SendMessageForm
{
	[System.Obsolete("Do not call. Only for designer use.")]
	public SendMessageForm()
	{
	}

	public SendMessageForm(ManifestMessageSendingObjectParent messageParent) : base(messageParent)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}

	protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => new SendMessageGridColumnLayout((ManifestMessageSendingObjectParent)MessageSendingObjectParent);
}
