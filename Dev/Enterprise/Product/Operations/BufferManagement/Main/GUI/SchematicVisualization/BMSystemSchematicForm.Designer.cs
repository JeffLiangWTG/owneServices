namespace Enterprise.BufferManagement.GUI
{
	partial class BMSystemSchematicForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		protected new void InitializeComponent()
		{
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.schematicPanel = new CargoWise.Windows.UI.KPanel();
			this.schematicPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.labelInvalidSchematicPictureBoxImage = new Enterprise.ZArchitecture.ZLabel();
			this.legendPanel = new CargoWise.Windows.UI.KPanel();
			this.LegendLabel = new Enterprise.ZArchitecture.ZLabel();
			this.legendPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.IncludeNonPrimaryPathCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LegendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.schematicPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.schematicPictureBox)).BeginInit();
			this.legendPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.legendPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 538, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.IncludeNonPrimaryPathCheckBox);
			this.splitContainer1.Panel2.Controls.Add(this.LegendButton);
			this.splitContainer1.Panel2.Controls.Add(this.CloseButton);
			this.splitContainer1.Panel2.Controls.Add(this.RefreshButton);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 562, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(514);
			this.splitContainer1.TabIndex = 1;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer2.IsSplitterFixed = true;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.TitleLabel);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.MainSplitContainer);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 514, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(51);
			this.splitContainer2.TabIndex = 0;
			// 
			// TitleLabel
			// 
			this.TitleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 44, true);
			this.TitleLabel.TabIndex = 0;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.MainSplitContainer.IsSplitterFixed = true;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.schematicPanel);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.legendPanel);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 459, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(594);
			this.MainSplitContainer.TabIndex = 1;
			// 
			// schematicPanel
			// 
			this.schematicPanel.AutoScroll = true;
			this.schematicPanel.Controls.Add(this.schematicPictureBox);
			this.schematicPanel.Controls.Add(this.labelInvalidSchematicPictureBoxImage);
			this.schematicPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.schematicPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.schematicPanel.Name = "schematicPanel";
			this.schematicPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 459, true);
			this.schematicPanel.TabIndex = 0;
			// 
			// schematicPictureBox
			// 
			this.schematicPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.schematicPictureBox.Name = "schematicPictureBox";
			this.schematicPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 260, true);
			this.schematicPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.schematicPictureBox.TabIndex = 2;
			this.schematicPictureBox.TabStop = false;
			// 
			// labelInvalidSchematicPictureBoxImage
			// 
			this.labelInvalidSchematicPictureBoxImage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("6b4daf33-770b-403f-8b6d-ed5635fb91a0", "There are too many components for the specified buffer management system to generate an image. Please view the Text Schematic, available in the Edit/View Buffer Management System window.");
			this.labelInvalidSchematicPictureBoxImage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelInvalidSchematicPictureBoxImage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.labelInvalidSchematicPictureBoxImage.Name = "labelInvalidSchematicPictureBoxImage";
			this.labelInvalidSchematicPictureBoxImage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 460, true);
			this.labelInvalidSchematicPictureBoxImage.TabIndex = 4;
			this.labelInvalidSchematicPictureBoxImage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// legendPanel
			// 
			this.legendPanel.AutoScroll = true;
			this.legendPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.legendPanel.Controls.Add(this.LegendLabel);
			this.legendPanel.Controls.Add(this.legendPictureBox);
			this.legendPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.legendPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.legendPanel.Name = "legendPanel";
			this.legendPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 459, true);
			this.legendPanel.TabIndex = 0;
			// 
			// LegendLabel
			// 
			this.LegendLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c6e7a7aa-849e-4a67-9d4f-2edb335bbfd6", "Legend");
			this.LegendLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 3, true);
			this.LegendLabel.Name = "LegendLabel";
			this.LegendLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 40, true);
			this.LegendLabel.TabIndex = 0;
			this.LegendLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// legendPictureBox
			// 
			this.legendPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 46, true);
			this.legendPictureBox.Name = "legendPictureBox";
			this.legendPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 387, true);
			this.legendPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.legendPictureBox.TabIndex = 3;
			this.legendPictureBox.TabStop = false;
			// 
			// IncludeNonPrimaryPathCheckBox
			// 
			this.IncludeNonPrimaryPathCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.IncludeNonPrimaryPathCheckBox.AutoSize = true;
			this.IncludeNonPrimaryPathCheckBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("9b85f4bd-b5b8-4df2-8f24-64ca1bf02f4c", "Include Non-Primary Paths");
			this.IncludeNonPrimaryPathCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeNonPrimaryPathCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 14, true);
			this.IncludeNonPrimaryPathCheckBox.Name = "IncludeNonPrimaryPathCheckBox";
			this.IncludeNonPrimaryPathCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 17, true);
			this.IncludeNonPrimaryPathCheckBox.TabIndex = 3;
			this.IncludeNonPrimaryPathCheckBox.UseVisualStyleBackColor = true;
			this.IncludeNonPrimaryPathCheckBox.CheckedChanged += new System.EventHandler(this.IncludeNonPrimaryPathCheckBox_CheckedChanged);
			// 
			// LegendButton
			// 
			this.LegendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LegendButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4b686b69-0a1a-45c7-8174-b689c125edc9", "Show/Hide Legend");
			this.LegendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 8, true);
			this.LegendButton.Name = "LegendButton";
			this.LegendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LegendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 28, true);
			this.LegendButton.TabIndex = 1;
			this.LegendButton.ToolTipCaption = null;
			this.LegendButton.UseVisualStyleBackColor = true;
			this.LegendButton.Click += new System.EventHandler(this.LegendButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CloseButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d07426c8-0446-44f1-873d-549ce585b0cf", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 8, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 28, true);
			this.CloseButton.TabIndex = 0;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// RefreshButton
			// 
			this.RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RefreshButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("27c60dac-a8a3-41b8-8cf7-cde6afc69c50", "Refresh Schematic");
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 8, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 28, true);
			this.RefreshButton.TabIndex = 2;
			this.RefreshButton.ToolTipCaption = null;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// BMSystemSchematicForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("080a1f4c-8254-4f64-b07f-65753dd87d39", "Buffer Management System Schematic");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 562, true);
			this.Controls.Add(this.splitContainer1);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.Name = "BMSystemSchematicForm";
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.schematicPanel.ResumeLayout(false);
			this.schematicPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.schematicPictureBox)).EndInit();
			this.legendPanel.ResumeLayout(false);
			this.legendPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.legendPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		CargoWise.Windows.UI.KSplitContainer splitContainer2;
		ZArchitecture.GUI.ZButton CloseButton;
		ZArchitecture.GUI.ZButton RefreshButton;
		CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		CargoWise.Windows.UI.KPanel schematicPanel;
		Enterprise.ZArchitecture.GUI.ZPictureBox schematicPictureBox;
		CargoWise.Windows.UI.KPanel legendPanel;
		Enterprise.ZArchitecture.GUI.ZPictureBox legendPictureBox;
		ZArchitecture.ZLabel TitleLabel;
		ZArchitecture.ZLabel LegendLabel;
		ZArchitecture.GUI.ZButton LegendButton;
		private ZArchitecture.GUI.ZCheckBox IncludeNonPrimaryPathCheckBox;
		private ZArchitecture.ZLabel labelInvalidSchematicPictureBoxImage;
	}
}