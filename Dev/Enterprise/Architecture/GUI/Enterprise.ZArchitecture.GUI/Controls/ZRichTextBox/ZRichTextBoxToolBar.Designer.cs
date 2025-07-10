using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Interop.DataObjects;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZRichTextBoxToolBar
	{
		#region Component Designer generated code

		protected ZPanel PanelFont;
		protected ZPanel PanelButtons;
		protected internal ZToolBar ToolBar;
		protected ZToolBarButton Separator4;
		protected ZToolBarButton BoldButton;
		protected ZToolBarButton ItalicButton;
		protected ZToolBarButton UnderlineButton;
		protected ZToolBarButton StrikeoutButton;
		protected ZToolBarButton BulletsButton;
		protected ZToolBarButton NumberedListButton;
		protected ZToolBarButton Separator1;
		public ZToolBarButton AttachButton;
		public ZToolBarButton InsertImageButton;
		protected ZToolBarButton LeftIndentButton;
		protected ZToolBarButton RightIndentButton;
		public ZToolBarButton FormatPainterButton;
		internal Enterprise.ZArchitecture.GUI.ZButton PopupButton;
		protected ZRichTextBoxToolBar.FontDetailsComboBox FontFamilyComboBox;
		protected ZRichTextBoxToolBar.FontDetailsComboBox FontSizeComboBox;
		private System.Windows.Forms.ImageList ToolBarImageList;
		private ZToolBarButton ColorButton;
		private System.ComponentModel.IContainer components;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
		void InitializeComponent()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
		{
			this.components = new System.ComponentModel.Container();
			this.ToolBar = new Enterprise.ZArchitecture.GUI.ZToolBar();
			this.Separator4 = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.BoldButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.ItalicButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.UnderlineButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.StrikeoutButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.BulletsButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.NumberedListButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.Separator1 = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.InsertImageButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.LeftIndentButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.RightIndentButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.ColorButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.FormatPainterButton = new Enterprise.ZArchitecture.GUI.ZToolBarButton();
			this.ToolBarImageList = new System.Windows.Forms.ImageList(this.components);
			this.PopupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FontFamilyComboBox = new Enterprise.ZArchitecture.GUI.ZRichTextBoxToolBar.FontDetailsComboBox();
			this.FontSizeComboBox = new Enterprise.ZArchitecture.GUI.ZRichTextBoxToolBar.FontDetailsComboBox();
			this.PanelFont = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PanelButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PanelFont.SuspendLayout();
			this.PanelButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// ToolBar
			// 
			this.ToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
			this.Separator4,
			this.BoldButton,
			this.ItalicButton,
			this.UnderlineButton,
			this.StrikeoutButton,
			this.BulletsButton,
			this.NumberedListButton,
			this.Separator1,
			this.AttachButton,
			this.InsertImageButton,
			this.LeftIndentButton,
			this.RightIndentButton,
			this.ColorButton,
			this.FormatPainterButton
			});
			this.ToolBar.Divider = false;
			this.ToolBar.DropDownArrows = true;
			this.ToolBar.Font = new System.Drawing.Font(OFont.NormalFontName, 8F);
			this.ToolBar.ImageList = this.ToolBarImageList;
			this.ToolBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolBar.Name = "ToolBar";
			this.ToolBar.ShowToolTips = true;
			this.ToolBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 17, true);
			this.ToolBar.TabIndex = 0;
			this.ToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.ToolBar_ButtonClick);
			// 
			// Separator4
			// 
			this.Separator4.Enabled = false;
			this.Separator4.Name = "Separator4";
			this.Separator4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// BoldButton
			// 
			this.BoldButton.ImageIndex = 0;
			this.BoldButton.Name = "BoldButton";
			this.BoldButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// ItalicButton
			// 
			this.ItalicButton.ImageIndex = 1;
			this.ItalicButton.Name = "ItalicButton";
			this.ItalicButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// UnderlineButton
			// 
			this.UnderlineButton.ImageIndex = 2;
			this.UnderlineButton.Name = "UnderlineButton";
			this.UnderlineButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// StrikeoutButton
			// 
			this.StrikeoutButton.ImageIndex = 14;
			this.StrikeoutButton.Name = "StrikeoutButton";
			this.StrikeoutButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// BulletsButton
			// 
			this.BulletsButton.ImageIndex = 6;
			this.BulletsButton.Name = "BulletsButton";
			this.BulletsButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// NumberedListButton
			// 
			this.NumberedListButton.ImageIndex = 13;
			this.NumberedListButton.Name = "NumberedListButton";
			this.NumberedListButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// Separator1
			// 
			this.Separator1.Name = "Separator1";
			this.Separator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// AttachButton
			// 
			this.AttachButton.ImageIndex = 7;
			this.AttachButton.Name = "AttachButton";
			// 
			// InsertImageButton
			// 
			this.InsertImageButton.ImageIndex = 8;
			this.InsertImageButton.Name = "InsertImageButton";
			// 
			// LeftIndentButton
			// 
			this.LeftIndentButton.ImageIndex = 9;
			this.LeftIndentButton.Name = "LeftIndentButton";
			// 
			// RightIndentButton
			// 
			this.RightIndentButton.ImageIndex = 10;
			this.RightIndentButton.Name = "RightIndentButton";
			// 
			// ColorButton
			// 
			this.ColorButton.ImageIndex = 12;
			this.ColorButton.Name = "ColorButton";
			// 
			// FormatPainterButton
			// 
			this.FormatPainterButton.ImageIndex = 15;
			this.FormatPainterButton.Name = "FormatPainterButton";
			// 
			// PopupButton
			// 
			this.PopupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PopupButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZRichTextBoxToolBar|Popup", "Popup");
			this.PopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 0, true);
			this.PopupButton.Name = "PopupButton";
			this.PopupButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.PopupButton.TabIndex = 1;
			this.PopupButton.ToolTipCaption = null;
			this.PopupButton.Click += new System.EventHandler(this.PopupButton_Click);
			// 
			// FontFamilyComboBox
			// 
			this.FontFamilyComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.FontFamilyComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.FontFamilyComboBox.DropDownWidth = 116;
			this.FontFamilyComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.FontFamilyComboBox.Name = "FontFamilyComboBox";
			this.FontFamilyComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 19, true);
			this.FontFamilyComboBox.TabIndex = 2;
			this.FontFamilyComboBox.Text = "FontFamilyComboBox";
			// 
			// FontSizeComboBox
			// 
			this.FontSizeComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.FontSizeComboBox.DropDownWidth = 48;
			this.FontSizeComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 0, true);
			this.FontSizeComboBox.Name = "FontSizeComboBox";
			this.FontSizeComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 19, true);
			this.FontSizeComboBox.TabIndex = 3;
			this.FontSizeComboBox.Text = "FontSizeComboBox";
			// 
			// PanelFont
			// 
			this.PanelFont.Controls.Add(this.FontFamilyComboBox);
			this.PanelFont.Controls.Add(this.FontSizeComboBox);
			this.PanelFont.Dock = System.Windows.Forms.DockStyle.Left;
			this.PanelFont.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PanelFont.Name = "PanelFont";
			this.PanelFont.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 28, true);
			this.PanelFont.TabIndex = 4;
			// 
			// PanelButtons
			// 
			this.PanelButtons.Controls.Add(this.ToolBar);
			this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PanelButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true);
			this.PanelButtons.Name = "PanelButtons";
			this.PanelButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 28, true);
			this.PanelButtons.TabIndex = 5;
			// 
			// ZRichTextBoxToolBar
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PopupButton);
			this.Controls.Add(this.PanelButtons);
			this.Controls.Add(this.PanelFont);
			this.Name = "ZRichTextBoxToolBar";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 28, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 28, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PanelFont.ResumeLayout(false);
			this.PanelFont.PerformLayout();
			this.PanelButtons.ResumeLayout(false);
			this.PanelButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
