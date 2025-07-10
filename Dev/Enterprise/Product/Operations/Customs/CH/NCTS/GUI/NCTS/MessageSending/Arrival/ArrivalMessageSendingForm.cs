using Enterprise.Customs.CH.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class ArrivalMessageSendingForm : BaseMovementMessageSendingForm
{
	public ArrivalMessageSendingForm(NctsHeaderArrivalMessageSendingObjectParent sendingObjectParent)
		: base(sendingObjectParent)
	{
		base.SplitContainer.Panel1.Controls.Remove(messageSendingObjectsGroupBox);
		TopSplitContainer.Panel1.Controls.Add(messageSendingObjectsGroupBox);
	}

	new NctsHeaderArrivalMessageSendingObjectParent MessageSendingObjectParent => (NctsHeaderArrivalMessageSendingObjectParent)base.MessageSendingObjectParent;

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		TopSplitContainer.Panel2.Visible = MessageSendingObjectParent.IsMultiMrnView;
		TopSplitContainer.Panel2Collapsed = !MessageSendingObjectParent.IsMultiMrnView;
	}
}
