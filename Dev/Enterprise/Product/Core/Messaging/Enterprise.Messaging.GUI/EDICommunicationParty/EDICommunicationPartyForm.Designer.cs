namespace Enterprise.Messaging.GUI
{
	partial class EDICommunicationPartyForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.MainTabControl.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.SaveButtonUserControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainTabPage
            // 
            this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
            // 
            // EDICommunicationPartyForm
            // 
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 629, true);
            this.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 650, true);
            this.Name = "EDICommunicationPartyForm";
            this.ShouldSerializeTabPageMethods = true;
            this.Text = "EDICommunicationPartyForm";
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.SaveButtonUserControl.ResumeLayout(true);
            this.SaveButtonUserControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private EDICommunicationPartyUserControl zUserControl;
	}
}
