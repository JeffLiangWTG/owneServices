using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZColumnsCustomise
	{
		#region Windows Form Designer generated code

		private ZButton AddButton;
		private ZButton RemoveButton;
		protected ZButton MoveUpButton;
		protected ZButton MoveDownButton;
		protected ZButton PostButton;
		protected ZButton Cancel_Button;
		protected ZButton ResetButton;
		protected ZListBox AvailableColumnsListBox;
		protected ZListBox CurrentColumnsListBox;
		private ZLabel label1;
		private ZLabel label2;
		private ZLabel label3;
		private ZDropEdit CurrentLayoutNameDropEdit;
		private ZLabel label4;
		private ZLabel label5;
		private ZTextBox columnSearchBox;
		private ZToolStrip ToolStripLayout;
		protected ZToolStripButton ToolStripManageLayoutsButton;
		protected ZToolStripButton ToolStripSaveLayoutButton;
		private ZToolStripMenuItem ToolStripNoLayoutsAddedMenuItem;
		protected ZCheckBox CustomColumnsCheckBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.AvailableColumnsListBox = new ZListBox();
			this.label1 = new ZLabel();
			this.label2 = new ZLabel();
			this.label3 = new ZLabel();
			this.columnSearchBox = new ZTextBox();
			this.CurrentColumnsListBox = new ZListBox();
			this.AddButton = new ZButton();
			this.RemoveButton = new ZButton();
			this.PostButton = new ZButton();
			this.Cancel_Button = new ZButton();
			this.CustomColumnsCheckBox = new ZCheckBox();
			this.MoveUpButton = new ZButton();
			this.MoveDownButton = new ZButton();
			this.ResetButton = new ZButton();
			this.label4 = new ZLabel();
			this.label5 = new ZLabel();
			this.ToolStripLayout = new ZToolStrip();
			this.ToolStripManageLayoutsButton = new ZToolStripButton();
			this.ToolStripSaveLayoutButton = new ZToolStripButton();
			this.ToolStripNoLayoutsAddedMenuItem = new ZToolStripMenuItem();
			this.CurrentLayoutNameDropEdit = new ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToolStripLayout.SuspendLayout();
			this.CurrentLayoutNameDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 121, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ZGridCustomiseBizObj);
			// 
			// AvailableColumnsListBox
			// 
			this.AvailableColumnsListBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left);
			this.AvailableColumnsListBox.ItemHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
			this.AvailableColumnsListBox.Items.AddRange(new object[] {
			"1.Agent commercial (Nom complet)",
			"2. Tu",
			"3. Free",
			"4. Fo",
			"5. Faiv" });
			this.AvailableColumnsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 80, true);
			this.AvailableColumnsListBox.Name = "AvailableColumnsListBox";
			this.AvailableColumnsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.AvailableColumnsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 212, true);
			this.AvailableColumnsListBox.Sorted = true;
			this.AvailableColumnsListBox.TabStop = false;
			this.AvailableColumnsListBox.MouseDown += new MouseEventHandler(this.AvailableColumns_MouseDown);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|e0452341-959e-4ffa-8d12-97b17921f838", "Available Columns");
			this.label1.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 62, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 14, true);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|2de17d6e-2dc5-4f3e-88cb-a8bde351a5fa", "Show Columns in this order");
			this.label2.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 62, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 14, true);
			//
			// columnSearchBox
			//
			this.columnSearchBox.AutoSize = true;
			this.columnSearchBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|76FFE6F9-C853-4D63-BD63-174C1C8A97CF", "Search For:");
			this.columnSearchBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.columnSearchBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 38, true);
			this.columnSearchBox.Name = "columnSearchBox";
			this.columnSearchBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.columnSearchBox.TabIndex = 0;
			this.columnSearchBox.TextChanged += ColumnSearchBox_TextChanged;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|B77D7308-847B-4E9C-99DE-056C130214F9", "Use Columns from the filter :");
			this.label3.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.label3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 10, true);
			this.label3.Name = "label3";
			this.label3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 14, true);
			// 
			// CurrentColumnsListBox
			// 
			this.CurrentColumnsListBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left);
			this.CurrentColumnsListBox.ItemHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
			this.CurrentColumnsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 80, true);
			this.CurrentColumnsListBox.Name = "CurrentColumnsListBox";
			this.CurrentColumnsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.CurrentColumnsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 212, true);
			this.CurrentColumnsListBox.TabStop = false;
			this.CurrentColumnsListBox.MouseDown += new MouseEventHandler(this.CurrentColumns_MouseDown);
			// 
			// AddButton
			// 
			this.AddButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|09a69674-bf4c-4b9d-aefb-3af79aeae541", "&Add →");
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 80, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.AddButton.TabIndex = 3;
			this.AddButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AddButton.Click += new EventHandler(this.AddButton_Click);
			// 
			// RemoveButton
			// 
			this.RemoveButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|9eabcb7e-d989-4c8a-88e1-c6a5ea57dfc6", "← &Remove");
			this.RemoveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 108, true);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.RemoveButton.TabIndex = 4;
			this.RemoveButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RemoveButton.Click += new EventHandler(this.RemoveButton_Click);
			// 
			// CustomColumnsCheckBox
			//
			this.CustomColumnsCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|E272AE59-2612-4980-BE10-D0D53BFB4BF0", "Include Custom Fields (*)");
			this.CustomColumnsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 38, true);
			this.CustomColumnsCheckBox.Name = "CustomColumnsCheckBox";
			this.CustomColumnsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CustomColumnsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomColumnsCheckBox.TabIndex = 2;
			this.CustomColumnsCheckBox.Checked = true;
			this.CustomColumnsCheckBox.CheckedChanged += new EventHandler(this.CustomColumnsCheckBox_Checked);
			// 
			// PostButton
			// 
			this.PostButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|c2906933-9f0d-4bee-a4cb-1ad3daed2389", "OK");
			this.PostButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 345, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.PostButton.TabIndex = 9;
			this.PostButton.Click += new EventHandler(this.PostButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|2d880b24-ff01-427d-8b32-cf575211dace", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 345, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.Cancel_Button.TabIndex = 10;
			this.Cancel_Button.Click += new EventHandler(this.CloseButton_Click);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 80, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 33, true);
			this.MoveUpButton.TabIndex = 6;
			this.MoveUpButton.Text = "↑";
			this.MoveUpButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.MoveUpButton.Click += new EventHandler(this.MoveUpButton_Click);
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 116, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 33, true);
			this.MoveDownButton.TabIndex = 7;
			this.MoveDownButton.Text = "↓";
			this.MoveDownButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.MoveDownButton.Click += new EventHandler(this.MoveDownButton_Click);
			// 
			// ResetButton
			// 
			this.ResetButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|ca71eff3-f44d-442a-9d34-cd06524929d2", "R&eset");
			this.ResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 218, true);
			this.ResetButton.Name = "ResetButton";
			this.ResetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.ResetButton.TabIndex = 5;
			this.ResetButton.Click += new EventHandler(this.ResetButton_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("A0D65D78-F6EC-4636-B8FD-93DE9F5D0136", "* You can drag and drop selected items between 'Available Columns' and 'Show Columns in this order'");
			this.label4.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.label4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 297, true);
			this.label4.Name = "label4";
			this.label4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 14, true);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("5C7B23BB-8462-4BD0-8050-A0336D8AF786", "* Columns in 'Show Columns in this order' can be rearranged by drag and drop");
			this.label5.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.label5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 317, true);
			this.label5.Name = "label5";
			this.label5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 14, true);
			// 
			// ToolStripLayout
			// 
			this.ToolStripLayout.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.ToolStripLayout.BackColor = System.Drawing.SystemColors.Control;
			this.ToolStripLayout.Dock = System.Windows.Forms.DockStyle.None;
			this.ToolStripLayout.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStripLayout.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ToolStripLayout.Items.AddRange(new ToolStripItem[] {
			this.ToolStripManageLayoutsButton,
			this.ToolStripSaveLayoutButton });
			this.ToolStripLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 347, true);
			this.ToolStripLayout.Name = "ToolStripLayout";
			this.ToolStripLayout.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.ToolStripLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 22, true);
			this.ToolStripLayout.TabIndex = 8;
			// 
			// ToolStripManageLayoutsButton
			// 
			this.ToolStripManageLayoutsButton.BackColor = System.Drawing.Color.Transparent;
			this.ToolStripManageLayoutsButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("c6613efc-9440-48fc-8a1a-9bc40e113baa", "Manage");
			this.ToolStripManageLayoutsButton.Name = "ToolStripManageLayoutsButton";
			this.ToolStripManageLayoutsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.ToolStripManageLayoutsButton.Click += new EventHandler(this.ToolStripManageLayoutsButton_Click);
			// 
			// ToolStripSaveLayoutButton
			// 
			this.ToolStripSaveLayoutButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("9d4847c0-d611-47ce-9612-ad4b888ee83a", "Save");
			this.ToolStripSaveLayoutButton.Name = "ToolStripSaveLayoutButton";
			this.ToolStripSaveLayoutButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, 0, 0, 0, true);
			this.ToolStripSaveLayoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.ToolStripSaveLayoutButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ToolStripSaveLayoutButton.Click += new EventHandler(this.ToolStripSaveLayoutButton_Click);
			// 
			// ToolStripNoLayoutsAddedMenuItem
			// 
			this.ToolStripNoLayoutsAddedMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("40aa31aa-1b6c-41af-ae01-a3118c7e6486", "<no filters have been added>");
			this.ToolStripNoLayoutsAddedMenuItem.Enabled = false;
			this.ToolStripNoLayoutsAddedMenuItem.Name = "ToolStripNoLayoutsAddedMenuItem";
			this.ToolStripNoLayoutsAddedMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 23, true);
			// 
			// CurrentLayoutNameDropEdit
			// 
			this.CurrentLayoutNameDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrentLayoutNameDropEdit, "CurrentLayoutNameDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGridCustomiseBizObj)(null)).CurrentLayoutNameDisplay);
			this.CurrentLayoutNameDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CurrentLayoutNameDropEdit, false);
			this.CurrentLayoutNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 8, true);
			this.CurrentLayoutNameDropEdit.Name = "CurrentLayoutNameDropEdit";
			this.CurrentLayoutNameDropEdit.ShowDescriptionBox = false;
			this.CurrentLayoutNameDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.CurrentLayoutNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.CurrentLayoutNameDropEdit.TabIndex = 1;
			// 
			// ZColumnsCustomise
			// 
			this.AcceptButton = this.PostButton;
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZColumnsCustomise|afd2c3d4-7752-4f24-8278-aed25fdcbdb2", "Customize Columns");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 370, true);
			this.Controls.Add(this.ResetButton);
			this.Controls.Add(this.MoveDownButton);
			this.Controls.Add(this.MoveUpButton);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.CustomColumnsCheckBox);
			this.Controls.Add(this.PostButton);
			this.Controls.Add(this.RemoveButton);
			this.Controls.Add(this.AddButton);
			this.Controls.Add(this.columnSearchBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.CurrentColumnsListBox);
			this.Controls.Add(this.AvailableColumnsListBox);
			this.Controls.Add(this.CurrentLayoutNameDropEdit);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.ToolStripLayout);
			this.DataSourceType = typeof(ZGridCustomiseBizObj);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 1024, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 345, true);
			this.Name = "ZColumnsCustomise";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ToolStripLayout, 0);
			this.Controls.SetChildIndex(this.CurrentLayoutNameDropEdit, 0);
			this.Controls.SetChildIndex(this.AvailableColumnsListBox, 0);
			this.Controls.SetChildIndex(this.CurrentColumnsListBox, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.columnSearchBox, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.label5, 0);
			this.Controls.SetChildIndex(this.AddButton, 0);
			this.Controls.SetChildIndex(this.RemoveButton, 0);
			this.Controls.SetChildIndex(this.CustomColumnsCheckBox, 0);
			this.Controls.SetChildIndex(this.PostButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MoveUpButton, 0);
			this.Controls.SetChildIndex(this.MoveDownButton, 0);
			this.Controls.SetChildIndex(this.ResetButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.SizeChanged += FormSizeChanged;
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToolStripLayout.ResumeLayout(false);
			this.ToolStripLayout.PerformLayout();
			this.CurrentLayoutNameDropEdit.ResumeLayout(true);
			this.CurrentLayoutNameDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
