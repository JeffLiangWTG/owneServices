using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Shared.GUI;

public partial class EditableMessageSendingForm<T> : ZChildForm where T : IMessageVisualObjectParentProvider
{
	public EditableMessageSendingForm()
	{
	}

	public EditableMessageSendingForm(T sendingObjectParent)
	: base((IBusiness)sendingObjectParent)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		SetupButtons();
	}

	public override string FormHeading => Res.GetString("E2E5C5F4-EC84-4E95-A930-96F7323EB649", "Preview");

	public new T BusinessEntity => (T)base.BusinessEntity;

	#region Events

	void SetupButtons()
	{
		MessageVisualObjectUserControl.ConfirmButton.Click -= ConfirmButton_Click;
		MessageVisualObjectUserControl.ConfirmButton.Click += ConfirmButton_Click;

		MessageVisualObjectUserControl.CancelButton.Click -= CancelPreviewButton_Click;
		MessageVisualObjectUserControl.CancelButton.Click += CancelPreviewButton_Click;
	}

	void ConfirmButton_Click(object sender, EventArgs e)
	{
		BusinessEntity.UseVisualData = CheckCanSend();

		if (BusinessEntity.UseVisualData)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	bool CheckCanSend()
	{
		var result = true;

		var visualObjectParent = (BusinessEntity as IMessageVisualObjectParentProvider)?.VisualObjectParent;

		if (visualObjectParent != null)
		{
			var messageSendingValidation = MessageSendingValidation.New(visualObjectParent, null);
			var notifications = messageSendingValidation.CheckBusinessObjectLevelValidation();

			if (notifications.ErrorCount > 0)
			{
				result = false;
				Globals.Message.ShowError(notifications.ErrorNotificationsAsString());
			}
			else if (notifications.WarningCount > 0)
			{
				var messageErrors = notifications.WarningNotificationsAsString();
				var caption = Res.GetString("3F6A4086-6C8D-459A-8C03-65B3722CE431", "Message Error");
				var messageErrorCheckResult = Globals.Message.Show(messageErrors, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes);
				if (messageErrorCheckResult == ZDialogResult.No)
				{
					result = false;
				}
			}
		}

		return result;
	}

	void CancelPreviewButton_Click(object sender, EventArgs e)
	{
		BusinessEntity.UseVisualData = false;
		DialogResult = DialogResult.Cancel;

		Close();
	}

	#endregion
}
