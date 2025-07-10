using System;
using System.Linq;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class InvalidStagePopupForm : ZChildForm
	{
		public InvalidStagePopupForm(IncidentManagementGroup incidentManagementGroup)
		: base(incidentManagementGroup)
		{
		}

		IncidentGroupStatusConfiguration currentSelectStage;
		const string MessageTemplate = "The active stage ({0} - {1}) for this Incident Group is no longer available. Please select a valid stage for this Group Type ({2} - {3}).";
		IncidentManagementGroup IncidentManagementGroup
		{
			get { return BusinessEntity as IncidentManagementGroup; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			loadMessageAndCheckbox();
		}

		void loadMessageAndCheckbox()
		{
			var invalidStage = IncidentManagementGroup.AllStages.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault((x) => x.Code.Equals(IncidentManagementGroup.ING_Status));
			var labelText = string.Format(MessageTemplate, IncidentManagementGroup.ING_Status, invalidStage?.DescriptionOnGroup ?? string.Empty, IncidentManagementGroup.ING_Type, IncidentManagementGroup.Lookups.Types.GetDescriptionFromCode(IncidentManagementGroup.ING_Type));
			var rowIndex = stageGrid.CurrentRowIndex;
			var stage = (rowIndex == -1) ? null : IncidentManagementGroup.Stages.Cast<IncidentGroupStatusConfiguration>().ElementAt(rowIndex);
			this.okButton.Enabled = stage != null;
			this.messageLabel.Text = labelText;
			this.broadcastCheckbox.Checked = false;
			if (string.Equals(stage?.Code, "ACI") || string.Equals(stage?.Code, "PSI"))
			{
				var publishedMessages = IncidentManagementGroup
										.IncidentManagementGroupMessages
										.Where(m => m.IGM_IsPublished);

				var isACI = string.Equals(stage.Code, "ACI");

				var publishedMsgExists = isACI ? publishedMessages.Any(x => x.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Opening) :
												 publishedMessages.Any(x => x.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Closing);

				this.broadcastCheckbox.Visible = publishedMsgExists;
				this.warnningLabel.Visible = !publishedMsgExists;
				this.broadcastCheckbox.Text = publishedMsgExists ? (isACI ? "Send Opening Broadcast" : "Send Closing Broadcast") : "";
			}
			else
			{
				this.broadcastCheckbox.Visible = false;
				this.warnningLabel.Visible = false;
			}
			currentSelectStage = stage;
		}

		void stageGrid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			if (stageGrid.SelectedRowCount > 1)
			{
				Globals.Message.ShowWarning(Res.GetString("5043ef2a-3a3f-477b-bdd5-4ae89c441008", "You can only select one stage."));
				stageGrid.UnSelectAll();
			}
			else
			{
				loadMessageAndCheckbox();
			}
		}

		void stageGrid_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
			{
				stageGrid_SelectedRowsChangedInMouseDown(sender, e);
			}
		}

		void okButton_Click(object sender, EventArgs e)
		{
			IncidentManagementGroup.TriggerFromPopupForm = true;
			IncidentManagementGroup.ING_Status = currentSelectStage.Code;
		}
	}
}
