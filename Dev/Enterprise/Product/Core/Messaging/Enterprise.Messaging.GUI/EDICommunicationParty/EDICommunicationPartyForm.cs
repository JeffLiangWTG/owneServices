using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class EDICommunicationPartyForm : ZTemplateForm
	{
		public EDICommunicationPartyForm()
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return EDICommunicationParty.Name == "" ? (NoResString)"EDI Client" : EDICommunicationParty.Name; }
		}

		public EDICommunicationPartyForm(EDICommunicationParty party)
			: base(party)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;
		protected override bool ShowNotesTab => true;
		protected override bool ShowAuditTab => true;

		void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);
		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.zUserControl = new EDICommunicationPartyUserControl();
			this.MainTabPage.SuspendLayout();
			this.zUserControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.zUserControl);
			//
			// zUserControl
			//
			this.zUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zUserControl, ".");
			this.zUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zUserControl.Name = "zUserControl";
			this.zUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 551, true);
			this.zUserControl.TabIndex = 0;
			this.MainTabPage.PerformLayout();
			this.zUserControl.ResumeLayout(true);
			this.zUserControl.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		protected EDICommunicationParty EDICommunicationParty
		{
			get { return (EDICommunicationParty)DataSource; }
		}
	}
}
