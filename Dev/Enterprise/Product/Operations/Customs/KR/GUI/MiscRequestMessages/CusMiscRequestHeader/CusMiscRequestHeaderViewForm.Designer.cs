namespace Enterprise.Customs.KR.GUI
{
	partial class CusMiscRequestHeaderViewForm
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
			this.ExtendedHoursRequestViewUserControl = new Enterprise.Customs.KR.GUI.CusMiscRequestHeaderViewUserControl();
			this.MiscRequestMessagesUserControl = new Enterprise.Customs.KR.GUI.MiscRequestMessagesUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExtendedHoursRequestViewUserControl.SuspendLayout();
			this.MiscRequestMessagesUserControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 466, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a637beb1-80b3-4ae9-bc8b-2b1dab290284", "Misc.Request");
			this.MainTabPage.Controls.Add(this.ExtendedHoursRequestViewUserControl);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 445, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 445, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 445, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 466, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusMiscRequestHeader);
			// 
			// ExtendedHoursRequestViewUserControl
			// 
			this.ExtendedHoursRequestViewUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExtendedHoursRequestViewUserControl, ".");
			this.ExtendedHoursRequestViewUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendedHoursRequestViewUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendedHoursRequestViewUserControl.Name = "ExtendedHoursRequestViewUserControl";
			this.ExtendedHoursRequestViewUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 445, true);
			this.ExtendedHoursRequestViewUserControl.TabIndex = 0;
			// 
			// MiscRequestMessagesUserControl
			// 
			this.MiscRequestMessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MiscRequestMessagesUserControl, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).Messages)));
			this.MiscRequestMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiscRequestMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiscRequestMessagesUserControl.Name = "MiscRequestMessagesUserControl";
			this.MiscRequestMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 445, true);
			this.MiscRequestMessagesUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("e0f64a59-26d5-48ac-8492-38fbeddfc71f", "Message");
			this.MessagesTabPage.Controls.Add(this.MiscRequestMessagesUserControl);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessagesTabPage, false);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 445, true);
			this.MessagesTabPage.TabIndex = 3;
			// 
			// ExtendedHoursRequestViewForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 522, true);
			this.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusMiscRequestHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 559, true);
			this.Name = "ExtendedHoursRequestViewForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "CusMiscRequestForm";
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
			this.ExtendedHoursRequestViewUserControl.ResumeLayout(true);
			this.ExtendedHoursRequestViewUserControl.PerformLayout();
			this.MiscRequestMessagesUserControl.ResumeLayout(true);
			this.MiscRequestMessagesUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		CusMiscRequestHeaderViewUserControl ExtendedHoursRequestViewUserControl;
		#endregion

		private MiscRequestMessagesUserControl MiscRequestMessagesUserControl;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
	}
}
