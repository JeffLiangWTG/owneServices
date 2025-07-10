using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class MiscDeclarationForm
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
      this.EntriesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.zWorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
      this.ValuationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
      this.valuationDeclarationUserControl = new Enterprise.Customs.KR.GUI.ValuationDeclarationUserControl();
      this.MainTabControl.SuspendLayout();
      this.NotesTabPage.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.SaveButtonUserControl.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.zWorkflowTabPage.SuspendLayout();
      this.ValuationTabPage.SuspendLayout();
      this.valuationDeclarationUserControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainTabControl
      // 
      this.MainTabControl.Controls.Add(this.ValuationTabPage);
      this.MainTabControl.Controls.Add(this.EntriesTabPage);
      this.MainTabControl.Controls.Add(this.zWorkflowTabPage);
      this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1320, 608, true);
      this.MainTabControl.Controls.SetChildIndex(this.zWorkflowTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.EntriesTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.ValuationTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
      this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
      // 
      // MainTabPage
      // 
      this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 565, true);
      // 
      // NotesTabPage
      // 
      this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 565, true);
      // 
      // LogsTabPage
      // 
      this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 565, true);
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
      // EntriesTabPage
      // 
      this.EntriesTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0A015302-A910-4FE2-B65A-7E6013A65A95", "Entries");
      this.EntriesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.EntriesTabPage.Name = "EntriesTabPage";
      this.EntriesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 565, true);
      this.EntriesTabPage.TabIndex = 3;
      // 
      // zWorkflowTabPage
      // 
      this.zWorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.zWorkflowTabPage.Name = "zWorkflowTabPage";
      this.zWorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 550, true);
      this.zWorkflowTabPage.TabIndex = 4;
      // 
      // ValuationTabPage
      // 
      this.ValuationTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("27780f69-6aed-4799-a7bc-1ec1e301a02f", "Valuation Lines");
      this.ValuationTabPage.Controls.Add(this.valuationDeclarationUserControl);
      this.ValuationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
      this.ValuationTabPage.Name = "ValuationTabPage";
      this.ValuationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 585, true);
      this.ValuationTabPage.TabIndex = 2;
      // 
      // valuationDeclarationUserControl
      // 
      this.valuationDeclarationUserControl.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.valuationDeclarationUserControl, ".");
			this.valuationDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.valuationDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.valuationDeclarationUserControl.Name = "valuationDeclarationUserControl";
      this.valuationDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1312, 585, true);
      this.valuationDeclarationUserControl.TabIndex = 0;
      // 
      // MiscDeclarationForm
      // 
      this.CaptionRenderingEnabled = true;
      this.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("e0972d82-960b-4ef4-9ac4-41d771a5fbef", "Misc.Declaration Form");
      this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1320, 664, true);
      this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1320, 686, true);
      this.Name = "MiscDeclarationForm";
      this.ShouldSerializeTabPageMethods = false;
      this.Text = "";
      this.MainTabControl.ResumeLayout(false);
      this.MainTabControl.PerformLayout();
      this.NotesTabPage.ResumeLayout(false);
      this.NotesTabPage.PerformLayout();
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.SaveButtonUserControl.ResumeLayout(true);
      this.SaveButtonUserControl.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.zWorkflowTabPage.ResumeLayout(false);
      this.zWorkflowTabPage.PerformLayout();
      this.ValuationTabPage.ResumeLayout(false);
      this.ValuationTabPage.PerformLayout();
      this.valuationDeclarationUserControl.ResumeLayout(true);
      this.valuationDeclarationUserControl.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}
		#endregion

		ZTabPage EntriesTabPage;
		MasterFiles.GUI.ZWorkflowTabPage zWorkflowTabPage;
		ZTabPage ValuationTabPage;
		ValuationDeclarationUserControl valuationDeclarationUserControl;

		protected override bool SupportsEDocs => false;
	}
}
