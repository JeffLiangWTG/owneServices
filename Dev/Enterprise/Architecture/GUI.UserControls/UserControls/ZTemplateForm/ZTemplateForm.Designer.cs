namespace Enterprise.ZArchitecture.GUI
{
	using System;
	using System.Globalization;
	using System.Threading;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Windows.UI;
	using CargoWise.Windows.UI.Testing;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Enterprise.ZArchitecture.Modules;

	public partial class ZTemplateForm
	{
		#region Windows Form Designer generated code

		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new ZTemplateTabControl();
			this.MainTabPage = new ZTabPage();
			this.NotesTabPage = new ZStmNoteTabPage();
			this.LogsTabPage = new ZLogsTabPage();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			//
			// MainPanel
			//
			this.MainPanel.Controls.Add(this.MainTabControl);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 519, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 24, true);
			//
			// MainTabControl
			//
			this.MainTabControl.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.NotesTabPage);
			this.MainTabControl.Controls.Add(this.LogsTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 519, true);
			this.MainTabControl.TabIndex = 2;
			//
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZTemplateForm|a5157a5a-3ad2-4eef-8825-91b5da6a7f30", "Details");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 492, true);
			this.MainTabPage.TabIndex = 2;
			//
			// NotesTabPage
			//
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 492, true);
			this.NotesTabPage.TabIndex = 0;
			//
			// LogsTabPage
			//
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 492, true);
			this.LogsTabPage.TabIndex = 0;
			//
			// ZTemplateForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 575, true);
			this.Name = "ZTemplateForm";
			this.Text = "ZTemplateForm";
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		private System.ComponentModel.IContainer components;
		protected ZStmNoteTabPage NotesTabPage;
		protected ZLogsTabPage LogsTabPage;

		#endregion
	}
}
