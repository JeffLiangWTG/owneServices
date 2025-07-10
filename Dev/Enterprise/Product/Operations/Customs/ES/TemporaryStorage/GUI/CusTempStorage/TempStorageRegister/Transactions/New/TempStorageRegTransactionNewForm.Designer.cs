using System;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

partial class TempStorageRegTransactionNewForm
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
		this.MainDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
		this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
		this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.MainPanel.SuspendLayout();
		this.SuspendLayout();
		// 
		// MainStatusBar
		// 
		this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 337, true);
		this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 24, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable);
		// 
		// MainDynamicLayoutPanel
		// 
		this.MainDynamicLayoutPanel.AllowDrop = true;
		this.MainDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.MainDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.MainDynamicLayoutPanel.Name = "MainDynamicLayoutPanel";
		this.MainDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 285, true);
		this.MainDynamicLayoutPanel.TabIndex = 0;
		// 
		// MainPanel
		// 
		this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.MainPanel.Controls.Add(this.MainDynamicLayoutPanel);
		this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
		this.MainPanel.Name = "MainPanel";
		this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 285, true);
		this.MainPanel.TabIndex = 1;
		// 
		// CloseButton
		// 
		this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.CloseButton.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("C3FA3E35-D244-4C6B-8A3A-7F8938563308", "Close");
		this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 308, true);
		this.CloseButton.Name = "CloseButton";
		this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
		this.CloseButton.TabIndex = 3;
		this.CloseButton.ToolTipCaption = null;
		this.CloseButton.Click += new EventHandler(this.OnCloseButton_Click);
		// 
		// SaveButton
		// 
		this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.SaveButton.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("8FC7BEBC-4FFA-41F4-B7F2-0831BCC6CAEA", "Save");
		this.SaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 308, true);
		this.SaveButton.Name = "SaveButton";
		this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
		this.SaveButton.TabIndex = 2;
		this.SaveButton.ToolTipCaption = null;
		this.SaveButton.Click += new EventHandler(this.OnSaveButton_Click);
		// 
		// TempStorageRegTransactionNewForm
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 361, true);
		this.Controls.Add(this.MainPanel);
		this.Controls.Add(this.CloseButton);
		this.Controls.Add(this.SaveButton);
		this.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable);
		this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 400, true);
		this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 361, true);
		this.Name = "TempStorageRegTransactionNewForm";
		this.Controls.SetChildIndex(this.SaveButton, 0);
		this.Controls.SetChildIndex(this.CloseButton, 0);
		this.Controls.SetChildIndex(this.MainPanel, 0);
		this.Controls.SetChildIndex(this.MainStatusBar, 0);
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.MainPanel.ResumeLayout(false);
		this.MainPanel.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
	internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel MainDynamicLayoutPanel;
	internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
	internal Enterprise.ZArchitecture.GUI.ZButton SaveButton;
}
