using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	partial class VisualiserForm
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
			this.components = new System.ComponentModel.Container();
			this.saveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.discardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.toolsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.resetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.visualiserTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.toolsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 335, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(232);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233);
			// 
			// SaveButton
			// 
			this.saveButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.saveButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("33c36d94-4ff0-4e6b-9d35-ca9c81777526", "Save && Close");
			this.saveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 0, true);
			this.saveButton.Name = "SaveButton";
			this.saveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 25, true);
			this.saveButton.TabIndex = 1;
			this.saveButton.UseVisualStyleBackColor = true;
			// 
			// DiscardButton
			// 
			this.discardButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.discardButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("7690aa43-7056-401b-a524-9b51aa0e8cf8", "Close");
			this.discardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 0, true);
			this.discardButton.Name = "DiscardButton";
			this.discardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 25, true);
			this.discardButton.TabIndex = 2;
			this.discardButton.UseVisualStyleBackColor = true;
			// 
			// ToolsPanel
			// 
			this.toolsPanel.BackColor = System.Drawing.SystemColors.Control;
			this.toolsPanel.Controls.Add(this.resetButton);
			this.toolsPanel.Controls.Add(this.saveButton);
			this.toolsPanel.Controls.Add(this.discardButton);
			this.toolsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 309, true);
			this.toolsPanel.Name = "ToolsPanel";
			this.toolsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 26, true);
			this.toolsPanel.TabIndex = 1;
			// 
			// ResetButton
			// 
			this.resetButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.resetButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("2c8baf25-ceef-4f7f-a235-0be9478c60ac", "Reset && Close");
			this.resetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 0, true);
			this.resetButton.Name = "ResetButton";
			this.resetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 25, true);
			this.resetButton.TabIndex = 0;
			this.resetButton.UseVisualStyleBackColor = true;
			// 
			// VisualiserTabControl
			// 
			this.visualiserTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.visualiserTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.visualiserTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.visualiserTabControl.Name = "VisualiserTabControl";
			this.visualiserTabControl.SelectedIndex = 0;
			this.visualiserTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 309, true);
			this.visualiserTabControl.TabIndex = 2;
			// 
			// VisualiserForm
			// 
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("71627724-75a1-4998-8bdc-bcfc01d29d88", "Modify");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 359, true);
			this.Controls.Add(this.visualiserTabControl);
			this.Controls.Add(this.toolsPanel);
			this.Name = "VisualiserForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.toolsPanel, 0);
			this.Controls.SetChildIndex(this.visualiserTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.toolsPanel.ResumeLayout(false);
			this.toolsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZButton saveButton;
		private ZButton discardButton;
		private ZPanel toolsPanel;
		private ZTabControl visualiserTabControl;
		private ZButton resetButton;

	}
}
