using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentManagementGroupForm : ZTemplateForm
	{
		public IncidentManagementGroupForm(IncidentManagementGroup incidentManagementGroup) : base(incidentManagementGroup)
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
			ControllerID = Modules.ClientControllerRegistration.IncidentManagementGroup;
			WorkflowTabPage.Initialize(incidentManagementGroup);
			InitializeEConversationTabPage();
		}

		internal IncidentManagementGroup IncidentManagementGroup => BusinessEntity as IncidentManagementGroup;

		protected override bool SupportsEDocs => true;

		protected override bool ShowNotesTab => false;

		#region GUI Setup

		public override string FormCaption
		{
			get
			{
				var managementGroup = IncidentManagementGroup;
				if (managementGroup != null && managementGroup.ING_IncidentGroupNumber != string.Empty)
				{
					var caption = new StringBuilder();
					caption.Append(managementGroup.ING_IncidentGroupNumber);
					caption.Append(" - ");
					caption.Append(managementGroup.ING_Description);
					return caption.ToString();
				}
				else
				{
					return "Incident Management Group";
				}
			}
		}

		protected virtual void InitializeRelatedItemsTabPage(ZTabPage relatedItemsTabPage)
		{
			RelatedItemsTabPageControl = new EDIWorkTaskRelatedItemUserControl(IsViewOrDeleteMode);
			RelatedItemsTabPageControl.Dock = DockStyle.Fill;
			relatedItemsTabPage.Controls.Add(RelatedItemsTabPageControl);
		}
		EDIWorkTaskRelatedItemUserControl RelatedItemsTabPageControl { get; set; }

		void InitializeEConversationTabPage()
		{
			PlugIns.AddPlugInAtTabPageIndex(ClientControllerRegistration.IncidentManagementEConversationPlugin, 3);
			ConversationTabControl = PlugIns.GetPlugIn(ClientControllerRegistration.IncidentManagementEConversationPlugin).UserControl as IncidentManagementEConversationTabControl;
			ConversationTabControl.ShouldShowAddInternalCommentButton = true;
			if (ConversationTabControl.Controls.Find("AddInternalCommentButton", true).Single() is ZButton addInternalCommentButton)
			{
				addInternalCommentButton.CaptionResourceString = Res.GetData("aec10158-1f36-41d5-944b-a353938acb47", "Add Internal Log");
			}
		}
		IncidentManagementEConversationTabControl ConversationTabControl { get; set; }

		void MainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPage == RelatedItemsTabPage && RelatedItemsTabPage.Controls.Count == 0)
			{
				InitializeRelatedItemsTabPage(RelatedItemsTabPage);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			var currentStatus = IncidentManagementGroup.ING_Status;
			if (!currentStatus.IsEmpty && IncidentManagementGroup.LatestStages.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code.EqualsIgnoringCase(currentStatus)) == null)
			{
				var form = new InvalidStagePopupForm(IncidentManagementGroup);
				if (ShowPopupForm(form) != DialogResult.OK)
				{
					this.Close();
				}
				else
				{
					if ((this.FindForm() as ZForm).FireSaveButton() == ContinueWithSave.Yes)
					{
						this.IncidentManagementGroup.Reload();
					}
					else
					{
						var message = Res.GetString("0dbb070b-f42f-49a9-938d-123784cddb81", "Stage change aborted as conflicting changes require attention. Please review the form and try again.");
						Globals.Message.ShowWarning(message);
					}
				}
			}
		}

		#endregion

		DialogResult ShowPopupForm(InvalidStagePopupForm form)
		{
			return ZFormModaliser.ShowDialogAndDispose(form, this);
		}

		#region For Testing
#if DEBUG

		public ZTabControl TopLevelTabControl_Exposed => TopLevelTabControl;

#endif
		#endregion
	}
}
