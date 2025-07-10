using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CADCorrectionMessageSendingForm : ZChildForm
	{
		public CADCorrectionMessageSendingForm(CADCorrectionMessageSendingActionWrapper wrapper)
			: base(wrapper)
		{
			if (wrapper.ForceSend)
			{
				this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("12380965-59CE-45FC-8AC9-E4BB05CB2929", "Send Cor/Adj Message");
				this.notificationLabel.Visible = false;
				this.sendOption.Visible = false;
				this.discardOption.Visible = false;
				this.SendingActionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
				this.SendingActionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 250, true);
			}
		}

		CADCorrectionMessageSendingActionWrapper Wrapper
		{
			get { return (CADCorrectionMessageSendingActionWrapper)base.DataSource; }
		}

		protected override void InitialiseForm()
		{
			InitializeComponent();
			base.InitialiseForm();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			var wrapper = Wrapper;
			wrapper.RunPreSaveValidation();
			if (!wrapper.HasErrors)
			{
				wrapper.OKClicked = true;
				Close();
			}
			else
			{
				Globals.Message.ShowError(new ZNotificationCollector(wrapper, true, false, ZNotificationCollector.PropertyDescriptionType.None).ToMessageListString());
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Wrapper.CancelClicked = true;
			Close();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);

			if (!Wrapper.OKClicked || Wrapper.SaveWithoutSendMessage)
			{
				Wrapper.ClearSendingActions();
			}
		}
	}
}
