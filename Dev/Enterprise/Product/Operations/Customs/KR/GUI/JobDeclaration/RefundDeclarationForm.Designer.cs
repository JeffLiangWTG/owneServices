using System.Windows.Forms;

namespace Enterprise.Customs.KR.GUI
{
	partial class RefundDeclarationForm
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
		protected override void InitializeComponent()
		{
      this.ImportEntriesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.RefundDeclarationImportEntriesUserControl = new Enterprise.Customs.KR.GUI.RefundDeclarationImportEntriesUserControl();
      this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.MessagesUserControl = new Enterprise.Customs.KR.GUI.MessagesTabUserControl();
      this.RefundDeclarationUserControl = new Enterprise.Customs.KR.GUI.RefundDeclarationUserControl();
      this.MainTabControl.SuspendLayout();
      this.MainTabPage.SuspendLayout();
      this.NotesTabPage.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.SaveButtonUserControl.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.ImportEntriesTabPage.SuspendLayout();
      this.RefundDeclarationImportEntriesUserControl.SuspendLayout();
      this.MessagesTabPage.SuspendLayout();
      this.MessagesUserControl.SuspendLayout();
      this.RefundDeclarationUserControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainTabControl
      // 
      this.MainTabControl.Controls.Add(this.ImportEntriesTabPage);
      this.MainTabControl.Controls.Add(this.MessagesTabPage);
      this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1320, 608, true);
      this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.ImportEntriesTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
      // 
      // MainTabPage
      // 
      this.MainTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("FE08EFA6-C5AF-45A8-90FB-1C81450A4E7D", "Declaration");
      this.MainTabPage.Controls.Add(this.RefundDeclarationUserControl);
      this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 585, true);
      // 
      // NotesTabPage
      // 
      this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 585, true);
      // 
      // LogsTabPage
      // 
      this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 585, true);
      // 
      // MainPanel
      // 
      this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1320, 608, true);
      // 
      // SaveButtonUserControl
      // 
      this.SaveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
      this.SaveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Right;
      this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1020, 0, true);
      this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 32, true);
      // 
      // MainStatusBar
      // 
      this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1320, 24, true);
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusReconDeclaration);
      // 
      // ImportEntriesTabPage
      // 
      this.ImportEntriesTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("6A2AE71C-4EB2-45DF-ACAE-675C8514AB78", "Import Entries");
      this.ImportEntriesTabPage.Controls.Add(this.RefundDeclarationImportEntriesUserControl);
      this.ImportEntriesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.ImportEntriesTabPage.Name = "ImportEntriesTabPage";
      this.ImportEntriesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
      this.ImportEntriesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 585, true);
      this.ImportEntriesTabPage.TabIndex = 3;
      this.ImportEntriesTabPage.UseVisualStyleBackColor = true;
      // 
      // RefundDeclarationImportEntriesUserControl
      // 
      this.RefundDeclarationImportEntriesUserControl.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.RefundDeclarationImportEntriesUserControl, ".");
      this.RefundDeclarationImportEntriesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.RefundDeclarationImportEntriesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
      this.RefundDeclarationImportEntriesUserControl.Name = "RefundDeclarationImportEntriesUserControl";
      this.RefundDeclarationImportEntriesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1306, 579, true);
      this.RefundDeclarationImportEntriesUserControl.TabIndex = 0;
      // 
      // MessagesTabPage
      // 
      this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("F461D3F7-44B1-4297-9E47-C57DBE464E56", "Messages");
      this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
      this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.MessagesTabPage.Name = "MessagesTabPage";
      this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
      this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 585, true);
      this.MessagesTabPage.TabIndex = 4;
      this.MessagesTabPage.UseVisualStyleBackColor = true;
      // 
      // MessagesUserControl
      // 
      this.MessagesUserControl.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.MessagesUserControl, "Messages");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.KR.Business.EDIMessageCollection)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).Messages)));
      this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
      this.MessagesUserControl.Name = "MessagesUserControl";
      this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1306, 579, true);
      this.MessagesUserControl.TabIndex = 0;
      // 
      // RefundDeclarationUserControl
      // 
      this.RefundDeclarationUserControl.AllowDrop = true;
      this.RefundDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.RefundDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.RefundDeclarationUserControl.Name = "RefundDeclarationUserControl";
      this.RefundDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 585, true);
      this.RefundDeclarationUserControl.TabIndex = 0;
      // 
      // RefundDeclarationForm
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0C3260FA-98FA-41BE-890A-BBDF197B8A88", "Refund Declaration Form");
      this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1320, 664, true);
      this.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusReconDeclaration);
      this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1320, 686, true);
      this.Name = "RefundDeclarationForm";
      this.ShouldSerializeTabPageMethods = false;
      this.Text = "";
      this.MainTabControl.ResumeLayout(false);
      this.MainTabControl.PerformLayout();
      this.MainTabPage.ResumeLayout(false);
      this.MainTabPage.PerformLayout();
      this.NotesTabPage.ResumeLayout(false);
      this.NotesTabPage.PerformLayout();
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.SaveButtonUserControl.ResumeLayout(true);
      this.SaveButtonUserControl.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.ImportEntriesTabPage.ResumeLayout(false);
      this.ImportEntriesTabPage.PerformLayout();
      this.RefundDeclarationImportEntriesUserControl.ResumeLayout(true);
      this.RefundDeclarationImportEntriesUserControl.PerformLayout();
      this.MessagesTabPage.ResumeLayout(false);
      this.MessagesTabPage.PerformLayout();
      this.MessagesUserControl.ResumeLayout(true);
      this.MessagesUserControl.PerformLayout();
      this.RefundDeclarationUserControl.ResumeLayout(true);
      this.RefundDeclarationUserControl.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage ImportEntriesTabPage;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private MessagesTabUserControl MessagesUserControl;
		public RefundDeclarationUserControl RefundDeclarationUserControl;
		private RefundDeclarationImportEntriesUserControl RefundDeclarationImportEntriesUserControl;
	}
}
