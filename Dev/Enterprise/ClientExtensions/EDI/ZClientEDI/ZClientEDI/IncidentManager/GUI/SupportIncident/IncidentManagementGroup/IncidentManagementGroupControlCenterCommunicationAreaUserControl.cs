using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentManagementGroupControlCenterCommunicationAreaUserControl : ZUserControl, IConversationView
	{
		public IncidentManagementGroupControlCenterCommunicationAreaUserControl() : base()
		{
			InitializeComponent();
			ConversationController = new IncidentConversationViewControllerForGroup(this);
			ConversationController.Initialize(this);
			sendMessageButton.Click += SendMessageButton_Click;
			eConversationMessageListUserControl1.ForceSilentRefreshMessages = true;
		}

		public IncidentManagementGroupControlCenterCommunicationAreaUserControl(SupportIncident incident) : this()
		{
			SetDataBinding(incident, "");
		}

		IncidentManagementControlCenterUserControl ControlCenter
		{
			get
			{
				if (controlCenter == null)
				{
					controlCenter = ((ZForm)ParentForm).Controls.Find("incidentManagementControlCenterUserControl", true).FirstOrDefault() as IncidentManagementControlCenterUserControl;
				}
				return controlCenter;
			}
		}
		IncidentManagementControlCenterUserControl controlCenter;

		LinkedIncidentsModuleButtonGrid LinkedIncidentsGrid
		{
			get
			{
				if (linkedIncidentsGrid == null)
				{
					linkedIncidentsGrid = ControlCenter?.Controls.Find("LinkedIncidentsGrid", true).FirstOrDefault() as LinkedIncidentsModuleButtonGrid;
				}
				return linkedIncidentsGrid;
			}
		}
		LinkedIncidentsModuleButtonGrid linkedIncidentsGrid;

		public SupportIncident Incident => (SupportIncident)DataSource;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Incident != null)
			{
				Incident.HasChangesChanged -= EnableSendingMessage;
				Incident.EConversation.MessageCountChanged -= EConversation_MessageCountChanged;
			}
			eConversationMessageListUserControl1.BindingContext = new ZBindingContext(); //Do not delete this line, or setting a previously set data source for the control will result in the control's data source being null.
			base.SetDataBinding(dataSource, dataMember);
			if (Incident != null)
			{
				SetTextForLabels();
				Incident.EConversation.MessageCountChanged -= EConversation_MessageCountChanged;
				Incident.EConversation.MessageCountChanged += EConversation_MessageCountChanged;
				Incident.HasChangesChanged -= EnableSendingMessage;
				Incident.HasChangesChanged += EnableSendingMessage;
			}
		}

		void SetTextForLabels()
		{
			this.databaseLabel.Text = Incident.Lookups.DatabaseCodeDescriptionPairList.GetDescriptionFromCode(Incident.DatabaseServerCode);
			var enterprise = Incident.Factory.Load<LicenceEnterprise>(Incident.EnterprisePK);
			this.enterpriseIDTextBox.Text = enterprise == null ? ZString.Empty : enterprise.LE_EnterpriseID;
		}

		public override void Refresh()
		{
			base.Refresh();
			SetTextForLabels();
			eConversationMessageListUserControl1.messagesLayoutPanel.SuspendLayout();
			eConversationMessageListUserControl1.messagesLayoutPanel.Controls.RemoveAndDisposeAll();
			eConversationMessageListUserControl1.messagesLayoutPanel.ResumeLayout(true);
			eConversationMessageListUserControl1.RefreshMessages(true);
			EnableSendingMessage(null, null);
		}

		#region ConversationView

		internal IncidentConversationViewControllerForGroup ConversationController { get; set; }

		JobConversation IConversationView.Conversation => Incident.EConversation.ExistingConversation;

		TextBoxBase IConversationView.MessageTextBox => this.conversationMessageTextBox;

		ZButton IConversationView.SendButton => this.sendMessageButton;

		ZButton IConversationView.AddInternalCommentButton => null;

		ZButton IConversationView.BroadcastButton => null;

		#endregion

		#region Event

		void EConversation_MessageCountChanged(object sender, System.EventArgs e)
		{
			eConversationMessageListUserControl1.RefreshMessages();
			((ZForm)this.FindForm()).BusinessEntity.HasChanges = true;
		}

		void AddInternalLogButton_Click(object sender, System.EventArgs e)
		{
			var popup = new AddIncidentLogPopupForm(new SupportIncidentLogCommentAction(Incident));
			ZFormModaliser.ShowDialogAndDispose(popup, FindForm());
		}

		void SendMessageButton_Click(object sender, System.EventArgs e)
		{
			LinkedIncidentsGrid?.Refresh();
		}

		public void EnableSendingMessage(object sender, System.EventArgs e)
		{
			var enableControls = false;
			if (Incident != null)
			{
				enableControls = Incident.IM_Status != SupportIncidentLookups.Status.Closed;
			}
			conversationMessageTextBox.Enabled = enableControls;
			sendMessageButton.Enabled = enableControls;
		}

		#endregion
	}
}
