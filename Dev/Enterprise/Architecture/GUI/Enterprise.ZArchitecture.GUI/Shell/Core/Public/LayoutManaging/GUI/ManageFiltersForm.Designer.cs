
namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ManageLayoutsForm
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
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageLayoutsForm));
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelChangesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RenameLayoutButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.DeleteLayoutButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.TreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.FiltersTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.SaveColumnsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveGridColourCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveGridColourNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.editLocalLanguageValuesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 495, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 24, true);
			this.MainStatusBar.TabIndex = 3;
			this.MainStatusBar.Visible = false;
			// 
			// SaveCloseButton
			// 
			this.SaveCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveCloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ManageLayoutsForm|3010e512-caa4-41f4-a78f-a98f09a7ed10", "&Save && Close");
			this.SaveCloseButton.Enabled = false;
			this.SaveCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 532, true);
			this.SaveCloseButton.Name = "SaveCloseButton";
			this.SaveCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 23, true);
			this.SaveCloseButton.TabIndex = 4;
			this.SaveCloseButton.Click += new System.EventHandler(this.SaveCloseButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ManageLayoutsForm|50647ac4-0227-4d24-ae97-f926eaba17c5", "&Save");
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(683, 532, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 4;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// CancelChangesButton
			// 
			this.CancelChangesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelChangesButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ManageLayoutsForm|8484e65e-9aee-4652-a9d6-3996e47d0385", "&Cancel");
			this.CancelChangesButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelChangesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(763, 532, true);
			this.CancelChangesButton.Name = "CancelChangesButton";
			this.CancelChangesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelChangesButton.TabIndex = 5;
			this.CancelChangesButton.Click += new System.EventHandler(this.CancelChangesButton_Click);
			// 
			// RenameLayoutButton
			// 
			this.RenameLayoutButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("B2586AEB-5E0A-4CBF-BBE4-F8F12844B98C", "Rename Layout");
			this.RenameLayoutButton.Enabled = false;
			this.RenameLayoutButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RenameLayoutButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.RenameLayoutButton.Name = "RenameLayoutButton";
			this.RenameLayoutButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, 0, 0, 0);
			this.RenameLayoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 19);
			this.RenameLayoutButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RenameLayoutButton.Click += new System.EventHandler(this.RenameLayoutButton_Click);
			// 
			// DeleteLayoutButton
			// 
			this.DeleteLayoutButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("900D182B-3AC1-4DFD-868E-BC3E49CCF0B2", "Delete Layout");
			this.DeleteLayoutButton.Enabled = false;
			this.DeleteLayoutButton.Image = ((System.Drawing.Image)(resources.GetObject("DeleteLayoutButton.Image")));
			this.DeleteLayoutButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DeleteLayoutButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.DeleteLayoutButton.Name = "DeleteLayoutButton";
			this.DeleteLayoutButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, 0, 0, 0);
			this.DeleteLayoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20);
			this.DeleteLayoutButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DeleteLayoutButton.Click += new System.EventHandler(this.DeleteLayoutButton_Click);
			// 
			// ToolStrip
			// 
			this.ToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.RenameLayoutButton,
			this.DeleteLayoutButton});
			this.ToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 23, true);
			this.ToolStrip.TabIndex = 0;
			this.ToolStrip.Text = "toolStrip1";
			// 
			// TreeView
			// 
			this.TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TreeView.Name = "TreeView";
			this.TreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 451, true);
			this.TreeView.TabIndex = 0;
			this.TreeView.TabStop = false;
			// 
			// FiltersTreeView
			// 
			this.FiltersTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiltersTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FiltersTreeView.Name = "FiltersTreeView";
			this.FiltersTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 451, true);
			this.FiltersTreeView.TabIndex = 0;
			this.FiltersTreeView.TabStop = false;
			this.FiltersTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FiltersTreeView_AfterSelect);
			this.FiltersTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.FiltersTreeView_BeforeExpand);
			this.FiltersTreeView.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.FiltersTreeView_BeforeCollapse);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 36, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.FiltersTreeView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.TreeView);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 490, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(231);
			this.splitContainer1.TabIndex = 1;
			// 
			// SaveColumnsCheckBox
			// 
			this.SaveColumnsCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ManageLayoutsForm|bfdbe389-4721-463c-af69-b04311b3a6f0", "Save columns with this layout");
			this.SaveColumnsCheckBox.Enabled = false;
			this.SaveColumnsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveColumnsCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.SaveColumnsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 7, true);
			this.SaveColumnsCheckBox.Name = "SaveColumnsCheckBox";
			this.SaveColumnsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 24, true);
			this.SaveColumnsCheckBox.TabIndex = 2;
			this.SaveColumnsCheckBox.TabStop = false;
			this.SaveColumnsCheckBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.SaveColumnsCheckBox.CheckedChanged += new System.EventHandler(this.SaveColumnsCheckBox_CheckedChanged);
			// 
			// SaveGridColourCheckBox
			// 
			this.SaveGridColourCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("A96C4DE9-ED80-4408-AE74-69F3AA0BAC80", "Save grid color with this layout");
			this.SaveGridColourCheckBox.Enabled = false;
			this.SaveGridColourCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveGridColourCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.SaveGridColourCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 7, true);
			this.SaveGridColourCheckBox.Name = "SaveGridColourCheckBox";
			this.SaveGridColourCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 24, true);
			this.SaveGridColourCheckBox.TabIndex = 3;
			this.SaveGridColourCheckBox.TabStop = false;
			this.SaveGridColourCheckBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.SaveGridColourCheckBox.CheckedChanged += new System.EventHandler(this.SaveGridColourCheckBox_CheckedChanged);
			// 
			// SaveGridColourDropEdit
			// 
			this.SaveGridColourNameTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("5471AFC5-3644-4044-8887-396256183A17", "Save grid color name");
			this.SaveGridColourNameTextBox.Enabled = false;
			this.SaveGridColourNameTextBox.ReadOnly = true;
			this.SaveGridColourNameTextBox.AllowDrop = true;
			this.SaveGridColourNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(636, 7, true);
			this.SaveGridColourNameTextBox.Name = "SaveGridColourNameTextBox";
			this.SaveGridColourNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 24, true);
			this.SaveGridColourNameTextBox.TabIndex = 4;
			this.SaveGridColourNameTextBox.TabStop = false;
			this.SaveGridColourNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SaveGridColourNameTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 0, true);
			// 
			// editLocalLanguageValuesButton
			// 
			this.editLocalLanguageValuesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.editLocalLanguageValuesButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("d0a0f972-4700-4a49-812c-6f7a9f5a3797", "Edit Local Language Values");
			this.editLocalLanguageValuesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 532, true);
			this.editLocalLanguageValuesButton.Name = "editLocalLanguageValuesButton";
			this.editLocalLanguageValuesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.editLocalLanguageValuesButton.TabIndex = 5;
			this.editLocalLanguageValuesButton.UseVisualStyleBackColor = true;
			this.editLocalLanguageValuesButton.Click += new System.EventHandler(this.editLocalLanguageValuesButton_Click);
			// 
			// ManageLayoutsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelChangesButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ManageLayoutsForm|0e6a5ca3-7ede-47d3-bcf8-d51b385d0812", "Manage Layouts");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 560, true);
			this.Controls.Add(this.editLocalLanguageValuesButton);
			this.Controls.Add(this.SaveColumnsCheckBox);
			this.Controls.Add(this.SaveGridColourCheckBox);
			this.Controls.Add(this.SaveGridColourNameTextBox);
			this.Controls.Add(this.ToolStrip);
			this.Controls.Add(this.SaveCloseButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.CancelChangesButton);
			this.Controls.Add(this.splitContainer1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 560, true);
			this.Name = "ManageLayoutsForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.CancelChangesButton, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.ToolStrip, 0);
			this.Controls.SetChildIndex(this.SaveColumnsCheckBox, 0);
			this.Controls.SetChildIndex(this.SaveGridColourCheckBox, 0);
			this.Controls.SetChildIndex(this.SaveGridColourNameTextBox, 0);
			this.Controls.SetChildIndex(this.editLocalLanguageValuesButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZButton SaveCloseButton;
		protected ZButton SaveButton;
		protected ZButton CancelChangesButton;
		protected Enterprise.ZArchitecture.GUI.ZToolStripButton RenameLayoutButton;
		protected Enterprise.ZArchitecture.GUI.ZToolStripButton DeleteLayoutButton;
		private Enterprise.ZArchitecture.GUI.ZTreeView TreeView;
		internal Enterprise.ZArchitecture.GUI.ZTreeView FiltersTreeView;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		public ZCheckBox SaveColumnsCheckBox;
		public ZCheckBox SaveGridColourCheckBox;
		public ZTextBox SaveGridColourNameTextBox;
		private ZToolStrip ToolStrip;
		internal ZButton editLocalLanguageValuesButton;
	}
}
