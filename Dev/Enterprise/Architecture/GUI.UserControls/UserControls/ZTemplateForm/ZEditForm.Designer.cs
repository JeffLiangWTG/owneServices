namespace Enterprise.ZArchitecture.GUI
{
	using System;
	using System.Windows.Forms;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Windows.UI.Testing;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Enterprise.ZArchitecture.Modules;

	public partial class ZEditForm
	{
		#region Windows Form Designer generated code

		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PreviousNextControlForDesigner = new ZPreviousNextControl();
			this.MainPanel = new ZPanel();
			this.StatusBarPanel = new ZPanel();
			this.BottomPanel = new ZPanel();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel = new ZPanel();
			this.SaveButtonUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusBarPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 24, true);
			this.MainStatusBar.TabIndex = 10;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(409);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(410);
			//
			// PreviousNextControlForDesigner
			//
			this.PreviousNextControlForDesigner.AllowDrop = true;
			this.PreviousNextControlForDesigner.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 5, true);
			this.PreviousNextControlForDesigner.Name = "PreviousNextControlForDesigner";
			this.PreviousNextControlForDesigner.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 26, true);
			this.PreviousNextControlForDesigner.TabIndex = 12;
			this.PreviousNextControlForDesigner.Visible = false;
			//
			// MainPanel
			//
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 519, true);
			this.MainPanel.TabIndex = 0;
			//
			// StatusBarPanel
			//
			this.StatusBarPanel.Controls.Add(this.MainStatusBar);
			this.StatusBarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.StatusBarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.StatusBarPanel.Name = "StatusBarPanel";
			this.StatusBarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 24, true);
			this.StatusBarPanel.TabIndex = 13;
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel);
			this.BottomPanel.Controls.Add(this.SaveButtonUserControl);
			this.BottomPanel.Controls.Add(this.PreviousNextControlForDesigner);
			this.BottomPanel.Controls.Add(this.StatusBarPanel);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 519, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 56, true);
			this.BottomPanel.TabIndex = 0;
			//
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			//
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 1, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Name = "PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel";
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 30, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 14;
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = false;
			//
			// SaveButtonUserControl
			//
			this.SaveButtonUserControl.AllowDrop = true;
			this.SaveButtonUserControl.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 6, true);
			this.SaveButtonUserControl.Name = "SaveButtonUserControl";
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.SaveButtonUserControl.TabIndex = 11;
			//
			// ZEditForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 575, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.Name = "ZEditForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusBarPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
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
		private ZPanel BottomPanel;
		protected ZPanel MainPanel;
		private ZPanel StatusBarPanel;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl SaveButtonUserControl;
		protected ZPanel PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel;
		private ZPreviousNextControl PreviousNextControlForDesigner;

		#endregion
	}
}
