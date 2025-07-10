using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRBlanketVesselChangeSendingActionForm : ZChildForm
	{
		public JPAFRBlanketVesselChangeSendingActionForm()
		{
		}

		public JPAFRBlanketVesselChangeSendingActionForm(BlanketVesselChange blanketVesselChange)
			: base(blanketVesselChange)
		{
			if (blanketVesselChange == null)
			{
				throw new ArgumentNullException(nameof(blanketVesselChange));
			}
			UpdateControlLayout(blanketVesselChange.IsShippingLineEntry);
			SetBlanketChange(blanketVesselChange.JPM_BlanketChange);
			JPH_VesselNameFindBox.PopupSelected += JPH_VesselNameFindBox_PopupSelected;
		}

		void JPH_VesselNameFindBox_PopupSelected(object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length == 1)
			{
				var vessel = (RefVessel)e.SelectedBusinessObjects[0];
				BusinessEntity.VesselCombination.OnVesselSelected(vessel);
			}
		}

		void UpdateControlLayout(bool isShippingLineEntry)
		{
			this.NewOperatorVoyageTextBox.Visible = isShippingLineEntry;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Event Handlers

		void SendButton_Click(object sender, EventArgs e)
		{
			if (!BusinessEntity.JPM_BlanketChange && NoBillWasFlaggedToSend)
			{
				Globals.Message.ShowError(NoBillWasFlaggedToSendMessage, AFRReportingCaption);
				return;
			}
			if (BusinessEntity.AreVesselDetailsDifferentFromHeader || Globals.Message.Show(NoVesselDetailsHaveChanged, BlanketVesselChangeCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
			{
				var validation = JPAFRMessageSendingValidation.New(BusinessEntity, null);
				var notifications = validation.CheckBusinessObjectLevelValidation();

				if (notifications.ContainsError())
				{
					Globals.Message.ShowError(notifications.ErrorNotificationsAsString(), AFRReportingCaption);
				}
				else if (!notifications.ContainsWarning() || Globals.Message.Show(notifications.NotificationsAsString(), AFRReportingCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					BusinessEntity.Factory.Save();
					this.DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		bool NoBillWasFlaggedToSend => !BusinessEntity.BlanketVesselChangeBills.Cast<BlanketVesselChangeBill>().Any(x => x.JPM_Send);

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		void BlanketChangeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			var blanketChange = BlanketChangeCheckBox.Checked;
			SetBlanketChange(blanketChange);
		}

		void SetBlanketChange(bool isChecked)
		{
			HasCheckedAllBillsCheckBox.Visible = !isChecked;
			BillsGroupBox.Visible = !isChecked;
			VesselInfoGroupBox.Dock = isChecked ? DockStyle.Fill : DockStyle.Top;
		}

		#endregion

		#region ResourceStrings

		static string NoBillWasFlaggedToSendMessage => ResString.GetMultilingualString("JPAFRMessageSendingActionForm|D4B4A1D3-2F25-4D23-BDD1-C8C9631D4B7F", "No bills have been selected for AFR reporting");

		static string AFRReportingCaption => ResString.GetMultilingualString("JPAFRMessageSendingActionForm |23DCADB0-D5AB-4925-824E-FE2D7D4F6B78", "AFR Reporting");

		static string NoVesselDetailsHaveChanged => ResString.GetMultilingualString(
			"JPAFRMessageSendingActionForm|16A1E4EA-C7B3-4E19-88D5-8D8E624B403B",
			"No Vessel Details have changed from the Original AFR Manifest.");

		static string BlanketVesselChangeCaption => Res.GetString("JPAFRMessageSendingActionForm|AB18A842-9D00-4672-AD6C-39EEB795D4D1", "Blanket Vessel Change");
		#endregion

		public override string FormVerb => Res.GetString("b2ecbf72-7456-4e79-8662-f088f4f46562", "Send");

		public override string FormCaption => Res.GetString("491634A8-C0D6-4B69-9ACB-31CD71EEE218", "Blanket Vessel Change");

		public new BlanketVesselChange BusinessEntity => (BlanketVesselChange)base.BusinessEntity;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			JPH_VesselNameFindBox.PopupSelected -= JPH_VesselNameFindBox_PopupSelected;
		}
	}
}
