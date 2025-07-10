namespace Enterprise.Customs.AU.GUI
{
	partial class AUCOLSUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
			this.AUCOLSTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DiscardedMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AttachmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsUserControl = new AUCOLSDetailsUserControl();
			this.MessagesUserControl = new Enterprise.Customs.GUI.BaseMessagesTabUserControl();
			this.DiscardedMessagesUserControl = new Enterprise.Customs.GUI.BaseMessagesTabUserControl();
			this.AttachmentsUserControl = new AUCOLSAttachmentsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AUCOLSTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.DetailsUserControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.DiscardedMessagesTabPage.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			this.DiscardedMessagesUserControl.SuspendLayout();
			this.AttachmentsTabPage.SuspendLayout();
			this.AttachmentsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader);
			// 
			// AUCOLSTabControl
			// 
			this.AUCOLSTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AUCOLSTabControl.Controls.Add(this.DetailsTabPage);
			this.AUCOLSTabControl.Controls.Add(this.AttachmentsTabPage);
			this.AUCOLSTabControl.Controls.Add(this.MessagesTabPage);
			this.AUCOLSTabControl.Controls.Add(this.DiscardedMessagesTabPage);
			this.AUCOLSTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AUCOLSTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AUCOLSTabControl.Name = "AUCOLSTabControl";
			this.AUCOLSTabControl.SelectedIndex = 0;
			this.AUCOLSTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("FA6B6601-D39D-4F8D-9402-0981ACA232EA", "Details");
			this.DetailsTabPage.Controls.Add(this.DetailsUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.TabIndex = 0;
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsUserControl, ".");
			this.DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsUserControl.Name = "DetailsUserControl";
			this.DetailsUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("2684BD49-B44F-4920-B4A8-3CDFE7870C5A", "Messages");
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.TabIndex = 2;
			// 
			// MessagesUserControl
			// 
			this.BindingSource.SetBindingMember(this.MessagesUserControl, "NonDiscardedMessages");
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesUserControl.Name = "NonMessagesUserControl";
			this.MessagesUserControl.TabIndex = 2;
			// 
			// DiscardedMessagesTabPage
			// 
			this.DiscardedMessagesTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("817C681D-E7B1-4594-A657-990F43FE999F", "Discarded Messages");
			this.DiscardedMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DiscardedMessagesTabPage.Controls.Add(this.DiscardedMessagesUserControl);
			this.DiscardedMessagesTabPage.Name = "DiscardedMessagesTabPage";
			this.DiscardedMessagesTabPage.TabIndex = 3;
			// 
			// DiscardedMessagesUserControl
			//
			this.BindingSource.SetBindingMember(this.DiscardedMessagesUserControl, "DiscardedMessages");
			this.DiscardedMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiscardedMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DiscardedMessagesUserControl.Name = "DiscardedMessagesUserControl";
			this.DiscardedMessagesUserControl.TabIndex = 0;
			// 
			// AttachmentsTabPage
			// 
			this.AttachmentsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AB90A192-1BDA-4D68-8F83-D00686D93383", "Attachments");
			this.AttachmentsTabPage.Controls.Add(this.AttachmentsUserControl);
			this.AttachmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AttachmentsTabPage.Name = "AttachmentsTabPage";
			this.AttachmentsTabPage.TabIndex = 1;
			// 
			// AttachmentsUserControl
			// 
			this.AttachmentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttachmentsUserControl, ".");
			this.AttachmentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttachmentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttachmentsUserControl.Name = "AttachmentsUserControl";
			this.AttachmentsUserControl.TabIndex = 0;
			// 
			// AUCOLSUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AUCOLSTabControl);
			this.Name = "AUCOLSUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1148, 656, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AUCOLSTabControl.ResumeLayout(false);
			this.AUCOLSTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.DetailsUserControl.ResumeLayout(true);
			this.DetailsUserControl.PerformLayout();
			this.AttachmentsTabPage.ResumeLayout(false);
			this.AttachmentsTabPage.PerformLayout();
			this.AttachmentsUserControl.ResumeLayout(true);
			this.AttachmentsUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.DiscardedMessagesTabPage.ResumeLayout(false);
			this.DiscardedMessagesTabPage.PerformLayout();
			this.DiscardedMessagesUserControl.ResumeLayout(true);
			this.DiscardedMessagesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZTabControl AUCOLSTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage AttachmentsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage DiscardedMessagesTabPage;
		AUCOLSDetailsUserControl DetailsUserControl;
		Enterprise.Customs.GUI.BaseMessagesTabUserControl MessagesUserControl;
		Enterprise.Customs.GUI.BaseMessagesTabUserControl DiscardedMessagesUserControl;
		internal AUCOLSAttachmentsUserControl AttachmentsUserControl;
	}
}
