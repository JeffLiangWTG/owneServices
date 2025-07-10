using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EConversationTextForm : ZChildForm, IConversationView
	{
		public EConversationTextForm(SupportIncident supportIncident) : base(supportIncident)
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			conversationControl = new EConversationMessageListUserControl(false);
			conversationControl.messagesLayoutPanel.SuspendLayout();
			conversationControl.SetDataBinding(BusinessEntity, "EConversation");
			conversationControl.Dock = DockStyle.Fill;
			this.Controls.Add(conversationControl);

			BusinessEntity.EConversation.MessageCountChanged += RefreshEConversationControls;
			conversationControl.messagesLayoutPanel.ResumeLayout(false);
			conversationControl.messagesLayoutPanel.PerformLayout();
		}

		EConversationMessageListUserControl conversationControl;

		public JobConversation Conversation => DataSource.EConversation.ExistingConversation;

		public TextBoxBase MessageTextBox => null;

		public ZButton SendButton => null;

		public ZButton AddInternalCommentButton => null;

		public ZButton BroadcastButton => null;

		void RefreshEConversationControls(object sender, EventArgs e)
		{
			conversationControl.RefreshMessages();
		}

		public new SupportIncident BusinessEntity
		{
			get { return (SupportIncident)base.BusinessEntity; }
		}

		new SupportIncident DataSource
		{
			get { return (SupportIncident)base.DataSource; }
		}
	}
}
