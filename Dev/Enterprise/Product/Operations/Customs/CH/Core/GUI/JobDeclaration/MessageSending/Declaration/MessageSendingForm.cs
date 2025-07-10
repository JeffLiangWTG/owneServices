using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using CusEntryHeader = Enterprise.Customs.CH.Business.CusEntryHeader;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Customs.CH.GUI;

public partial class MessageSendingForm : MessageSendingFormWithValidationDetails
{
	public MessageSendingForm(BaseMessageSendingObjectParent sendingObjectParent)
		: base(sendingObjectParent)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		MessageSendingObjectsGrid.RunAfterBind((s, e) =>
		{
			MessageSendingObjectsGrid.ListManager.CurrentItemChanged += ListManager_CurrentItemChanged;
			ChangeVisibilityOnMessageTypeChanged();
		});
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		var isExportDeclarationMessageSendingObjectParent = MessageSendingObjectParent is ExportDeclarationMessageSendingObjectParent;
		if (isExportDeclarationMessageSendingObjectParent)
		{
			foreach (ExportDeclarationMessageSendingObject sendingObject in MessageSendingObjectParent.SendingObjectsCollection)
			{
				sendingObject.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
			}
		}

		base.SetDataBinding(dataSource, dataMember);

		if (dataSource != null && isExportDeclarationMessageSendingObjectParent)
		{
			foreach (ExportDeclarationMessageSendingObject sendingObject in MessageSendingObjectParent.SendingObjectsCollection)
			{
				sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}
		}

		ChangeVisibilityOnMessageTypeChanged();
	}

	void ListManager_CurrentItemChanged(object sender, System.EventArgs e)
	{
		ChangeVisibilityOnMessageTypeChanged();
	}

	void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
	{
		ChangeVisibilityOnMessageTypeChanged();
	}

	void ChangeVisibilityOnMessageTypeChanged()
	{
		NC123DetailsUserControl.Visible = IsShowNC123Details;
		ReasonTextGroupBox.Visible = ExportDeclarationMessageSendingObjectParent != null && !IsShowNC123Details;
		WarningSplitContainer.Visible = WarningSplitContainerVisible;
		ChangeSendWithAdditionalWarningCheckBoxAvailability();
		ChangeSendWithValidationErrorsCheckBoxAvailability();
		ChangeSendButtonAvailability();
	}

	bool WarningSplitContainerVisible => !IsShowNC123Details && !IsCancellationOrDataRequest;

	protected override bool SendWithValidationErrorsCheckBoxVisible => !IsShowNC123Details && !IsCancellationOrDataRequest && base.SendWithValidationErrorsCheckBoxVisible;

	protected override bool CheckIsOKToSend()
	{
		return ConfirmWhenAlreadySent() && base.CheckIsOKToSend();
	}

	JobDeclaration Declaration => BusinessEntity.TopLevelBusinessObject as JobDeclaration;

	bool ConfirmWhenAlreadySent()
	{
		var isConfirmed = true;
		if (MessageSendingObjectParent.SendingObjectsCollection.Cast<DeclarationMessageSendingObject>().Any(mso => mso.ShouldSend && ((CusEntryHeader)mso.Header).IsMessageStatusSent))
		{
			using (var form = new ResendReasonSendForm())
			{
				if (isConfirmed = ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
				{
					Declaration.Logs.AddNew(AutoEvents.Authorised, "Submitting when Message status is sent", new KeyValuePair<string, string>(EventReferenceParameters.Reason, form.Reason));
				}
			}
		}
		return isConfirmed;
	}

	DeclarationMessageSendingObject DeclarationMessageSendingObject => MessageSendingObjectsGrid.GetCurrent() as DeclarationMessageSendingObject;
	ExportDeclarationMessageSendingObjectParent ExportDeclarationMessageSendingObjectParent => MessageSendingObjectParent as ExportDeclarationMessageSendingObjectParent;
	ExportDeclarationMessageSendingObject ExportDeclarationMessageSendingObject => DeclarationMessageSendingObject as ExportDeclarationMessageSendingObject;

	bool IsShowNC123Details => (ExportDeclarationMessageSendingObject?.IsNC123 ?? false) && (!ExportDeclarationMessageSendingObjectParent?.ParentDeclaration.IsExportDeclarationActivation ?? false);
	bool IsCancellationOrDataRequest => DeclarationMessageSendingObject?.IsCancellationOrDataRequest ?? false;
}
