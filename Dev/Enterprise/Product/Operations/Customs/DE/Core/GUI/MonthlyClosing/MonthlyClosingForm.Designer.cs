namespace Enterprise.Customs.DE.GUI
{
	partial class MonthlyClosingForm
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
			this.HeaderDetailsUserControl = new Enterprise.Customs.DE.GUI.HeaderDetailsUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabUserControl = new Enterprise.Customs.EU.GUI.MessagesTabUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderDetailsUserControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesTabUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1301, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.HeaderDetailsUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 603, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 603, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1301, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1301, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusReconDeclaration);
			// 
			// HeaderDetailsUserControl
			// 
			this.HeaderDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderDetailsUserControl, ".");
			this.HeaderDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderDetailsUserControl.Name = "HeaderDetailsUserControl";
			this.HeaderDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 603, true);
			this.HeaderDetailsUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("E72D781B-81E7-4844-84FC-D8E418333382", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessagesTabUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 603, true);
			this.MessagesTabPage.TabIndex = 1;
			// 
			// MessagesTabUserControl
			// 
			this.MessagesTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesTabUserControl, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).Messages)));
			this.MessagesTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesTabUserControl.Name = "MessagesTabUserControl";
			this.MessagesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 603, true);
			this.MessagesTabUserControl.TabIndex = 0;
			// 
			// MonthlyClosingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1301, 686, true);
			this.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusReconDeclaration);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1317, 725, true);
			this.Name = "MonthlyClosingForm";
			this.ShouldSerializeTabPageMethods = false;
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
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesTabUserControl.ResumeLayout(true);
			this.MessagesTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		HeaderDetailsUserControl HeaderDetailsUserControl;
		Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		Enterprise.Customs.EU.GUI.MessagesTabUserControl MessagesTabUserControl;

		#endregion
	}
}
