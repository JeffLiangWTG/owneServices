using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class EComplaintMessageSendingForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
{
	public EComplaintMessageSendingForm()
	{
	}

	public EComplaintMessageSendingForm(EComplaintMessageSendingObject cusEntryHeaderWrapper)
		: base(cusEntryHeaderWrapper)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}

	void CancelButton_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	void SendButton_Click(object sender, EventArgs e)
	{
		var sendingValidation = MessageSendingValidation.New(MessageSendingObject, null);
		var notifications = sendingValidation.CheckBusinessObjectLevelValidation();

		if (notifications.ContainsError())
		{
			Globals.Message.ShowError(notifications.ErrorNotificationsAsString());
		}
		else
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	EComplaintMessageSendingObject MessageSendingObject => (EComplaintMessageSendingObject)base.BusinessEntity;

	public override string FormHeading => Res.GetString("88BD6CC3-9300-42D9-B265-B1A6DA77AFBD", "Send eCom - Entry: {0}", (DataSource as EComplaintMessageSendingObject)?.EntryHeader.MovementReferenceNumber);

	bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
	{
		return (control.Name == "CorrectionReasonDropEdit" && previousControl.Name == "CancelButton2") || (control.Name == "CancelButton2" && previousControl.Name == "CorrectionReasonDropEdit");
	}
}
