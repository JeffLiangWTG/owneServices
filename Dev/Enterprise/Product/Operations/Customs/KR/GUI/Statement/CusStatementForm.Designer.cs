
namespace Enterprise.Customs.KR.GUI
{
	partial class Statements
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
		private new void InitializeComponent()
		{
            this.HeaderDetailsUserControl = new Enterprise.Customs.KR.GUI.HeaderDetailsUserControl();
            this.StatementMessagesUserControl = new Enterprise.Customs.KR.GUI.StatementMessagesUserControl();
            this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.HeaderDetailsUserControl.SuspendLayout();
            this.StatementMessagesUserControl.SuspendLayout();
            this.MessagesTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 493, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.HeaderDetailsUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 447, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 447, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 466, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 493, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusStatementHeader);
			// 
			// HeaderDetailsUserControl
			// 
			this.HeaderDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderDetailsUserControl, ".");
			this.HeaderDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderDetailsUserControl.Name = "HeaderDetailsUserControl";
			this.HeaderDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 447, true);
			this.HeaderDetailsUserControl.TabIndex = 0;
			// 
			// StatementMessagesUserControl
			// 
			this.StatementMessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatementMessagesUserControl, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).Messages)));
			this.StatementMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatementMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StatementMessagesUserControl.Name = "StatementMessagesUserControl";
			this.StatementMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 441, true);
			this.StatementMessagesUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("238463e9-dd6b-481c-9a0e-06738f0b33ac", "Messages");
            this.MessagesTabPage.Controls.Add(this.StatementMessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 447, true);
			this.MessagesTabPage.TabIndex = 3;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// Statements
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 549, true);
			this.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusStatementHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 588, true);
			this.Name = "Statements";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "CusStatementForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderDetailsUserControl.ResumeLayout(true);
			this.HeaderDetailsUserControl.PerformLayout();
			this.StatementMessagesUserControl.ResumeLayout(true);
			this.StatementMessagesUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		HeaderDetailsUserControl HeaderDetailsUserControl;
		StatementMessagesUserControl StatementMessagesUserControl;
		#endregion

		protected ZArchitecture.GUI.ZTabPage MessagesTabPage;
	}
}
