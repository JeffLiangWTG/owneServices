using System.Linq;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class DepartureMessageSendingForm : BaseMovementMessageSendingForm
{
	public DepartureMessageSendingForm(NctsHeaderDepartureMessageSendingObjectParent sendingObjectParent)
		: base(sendingObjectParent)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		MessageSendingObjectsGrid.RunAfterBind((s, e) => MessageSendingObjectsGrid.ListManager.CurrentItemChanged += ListManager_CurrentItemChanged);
	}

	public new NctsHeaderDepartureMessageSendingObjectParent MessageSendingObjectParent => base.MessageSendingObjectParent as NctsHeaderDepartureMessageSendingObjectParent;

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		var parent = MessageSendingObjectParent;
		if (parent != null)
		{
			foreach (NctsHeaderDepartureMessageSendingObject sendingObject in parent.SendingObjectsCollection)
			{
				sendingObject.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
			}
		}

		base.SetDataBinding(dataSource, dataMember);

		if (dataSource != null)
		{
			foreach (NctsHeaderDepartureMessageSendingObject sendingObject in MessageSendingObjectParent.SendingObjectsCollection)
			{
				sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}

			ChangeVisibilityOnMessageTypeChanged();
		}
	}

	NctsHeaderDepartureMessageSendingObject CurrentMessageSendingObject => MessageSendingObjectParent?.SelectedSendingObjects?.FirstOrDefault() as NctsHeaderDepartureMessageSendingObject;

	void ListManager_CurrentItemChanged(object sender, System.EventArgs e)
	{
		ChangeVisibilityOnMessageTypeChanged();
	}

	void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
	{
		ChangeVisibilityOnMessageTypeChanged();
	}

	protected override bool IsSupervisorApproved()
	{
		return !MessageSendingObjectParent.ShowValidationErrors || base.IsSupervisorApproved();
	}

	void ChangeVisibilityOnMessageTypeChanged()
	{
		NT141DetailsUserControl.Visible = CurrentMessageSendingObject?.IsNT141 ?? false;
		NC123DetailsUserControl.Visible = CurrentMessageSendingObject?.IsNC123 ?? false;
		ReasonTextGroupBox.Visible = !NC123DetailsUserControl.Visible;

		var showWarnings = MessageSendingObjectParent?.ShowValidationErrors ?? false;
		WarningSplitContainer.Visible = showWarnings;

		ChangeSendWithAdditionalWarningCheckBoxAvailability();
		ChangeSendWithValidationErrorsCheckBoxAvailability();
		ChangeSendButtonAvailability();
	}

	protected override void ChangeSendButtonAvailability()
	{
		base.ChangeSendButtonAvailability();
		if (MessageSendingObjectParent != null && MessageSendingObjectParent.SendingObjectsCollection[0].MessageType == PassarMessageTypeList.Codes.NC123)
		{
			var effectiveSendButton = GetEffectiveSendButton();
			if (effectiveSendButton.Enabled)
			{
				MessageSendingObjectParent.SendingObjectsCollection[0].Validation.ValidateAll();
				effectiveSendButton.Enabled = !MessageSendingObjectParent.HasErrors;
			}
		}
	}
}

