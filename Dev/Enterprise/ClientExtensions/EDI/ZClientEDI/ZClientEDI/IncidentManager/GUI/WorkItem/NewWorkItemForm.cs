using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class NewWorkItemForm : EDIWorkItemForm
	{
		public NewWorkItemForm(NewWorkItem workItem)
			: base(workItem, new EDIWorkflowTabPage())
		{
			InitializeComponent();

			MainTabPage.RunWhenBindingOrFirstShown(delegate
			{
				TopLevelTabControl.SelectedIndexChanged += new EventHandler(TopLevelTabControl_SelectedIndexChanged);
			});

			if (!workItem.IsInDatabase && !workItem.HasChanges)
			{
				workItem.HasChanges = true;
			}
		}

		public NewWorkItemForm()
		{
			InitializeComponent();
		}

		protected override void SetUpEConversationPlugIn()
		{
			base.SetUpEConversationPlugIn();
			var eConversation = PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);

			var userControl = ((EConversationFullControl)eConversation.UserControl);
			userControl.ToggleButtonsAvailability
				(EDISecurityCheckpoints.WorkItemEConversationSendMessages.IsAllowed, EDISecurityCheckpoints.WorkItemEConversationAddInternalComment.IsAllowed, EDISecurityCheckpoints.WorkItemEConversationBroadcast.IsAllowed);
		}

		protected override void InitializeRelatedItemsTabPage(ZTabPage relatedItemsTabPage)
		{
			var control = new EDIWorkTaskRelatedItemUserControl(IsViewOrDeleteMode);
			control.Dock = DockStyle.Fill;
			relatedItemsTabPage.Controls.Add(control);
		}

		#region Email

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			if (result == ContinueWithSave.Yes)
			{
				foreach (BusinessObject relatedItem in DataSource.RelatedItems.ToArray())
				{
					SupportIncident incident = relatedItem as SupportIncident;
					if (incident != null)
					{
						incident.CustomerNotifier.SendQueuedEmails();
					}
				}
			}
			return result;
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				MainTabPage.RunWhenBindingOrFirstShown(delegate
				{
					SetupFormAppearance(false);
				});
			}
		}

		public new NewWorkItem DataSource
		{
			get { return (NewWorkItem)base.DataSource; }
		}

		#endregion

		#region Form Setup

		void TopLevelTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupFormAppearance(true);
		}

		void SetupFormAppearance(bool refreshProperties)
		{
			if (refreshProperties)
			{
				DataSource.RefreshTaskProperties();
			}
		}

		#endregion

		#region For Testing
#if DEBUG

		public ZTabControl TopLevelTabControl_Exposed => TopLevelTabControl;

#endif
		#endregion

	}
}
