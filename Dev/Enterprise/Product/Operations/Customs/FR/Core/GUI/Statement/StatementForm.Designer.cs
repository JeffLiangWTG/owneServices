using System;

namespace Enterprise.Customs.FR.GUI
{
	partial class StatementForm
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

		private new void InitializeComponent()
		{
            this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MainTabControl.SuspendLayout();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.MessagesTabPage);
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 609, true);
            this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
            // 
            // MainTabPage
            // 
            this.MainTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("d0431978-576c-4036-ad96-e6e13a0d88b5", "Liquidation");
            this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
            this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 667, true);
            this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 667, true);
            this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 571, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 609, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader);
            // 
            // MessagesTabPage
            // 
            this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("c782288c-1e99-4e6b-9f40-7df87eace6e9", "Messages");
            this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
            this.MessagesTabPage.Name = "MessagesTabPage";
            this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 667, true);
            this.MessagesTabPage.TabIndex = 3;
            this.MessagesTabPage.UseVisualStyleBackColor = true;
            this.MessagesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MessagesTabPage_InitializeTab));
            // 
            // StatementForm
            // 
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("4c8330b4-210c-4bd5-8f68-21b2c18f45b7", "Liquidation Statement");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 665, true);
            this.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 725, true);
            this.Name = "StatementForm";
            this.ShouldSerializeTabPageMethods = true;
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.HeaderDetailsUserControl = new Enterprise.Customs.FR.GUI.StatementHeaderUserControl();
			this.MainTabPage.SuspendLayout();
			this.HeaderDetailsUserControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.HeaderDetailsUserControl);
			// 
			// HeaderDetailsUserControl
			// 
			this.HeaderDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderDetailsUserControl, ".");
			this.HeaderDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HeaderDetailsUserControl.Name = "HeaderDetailsUserControl";
			this.HeaderDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 661, true);
			this.HeaderDetailsUserControl.TabIndex = 1;
			this.MainTabPage.PerformLayout();
			this.HeaderDetailsUserControl.ResumeLayout(true);
			this.HeaderDetailsUserControl.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

		}

		private void MessagesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MessagesUserControl = new Enterprise.Customs.FR.GUI.StatementMessagesUserControl();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, ".");
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 661, true);
			this.MessagesUserControl.TabIndex = 2;
			this.MessagesTabPage.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(true);

		}

		private void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}



		private StatementHeaderUserControl HeaderDetailsUserControl;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private StatementMessagesUserControl MessagesUserControl;
	}
}
