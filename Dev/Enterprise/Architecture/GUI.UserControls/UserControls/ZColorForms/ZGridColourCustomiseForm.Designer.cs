using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZGridColourCustomiseForm
	{
		#region Windows Form Designer generated code

		System.ComponentModel.IContainer components;
		protected ZButton AddNewRuleButton;
		protected ZTextBox SchemeNameTextBox;
		protected ZTabControl RulesTabControl;
		protected ZTabPage DefaultTabPage;
		protected ZStmALogTabPage LogsTabPage;
		protected ZButton SaveButton;
		protected ZButton RemoveRuleButton;
		protected ZButton CloseButton;
		protected ZCheckBox PublishCheckBox;
		protected ZButton RemoveSchemeButton;
		protected ZButton RenameRuleButton;
		protected TabPage SelectedTabPage;

		protected new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.AddNewRuleButton = new ZButton();
			this.SchemeNameTextBox = new ZTextBox();
			this.RulesTabControl = new ZTabControl();
			this.DefaultTabPage = new ZTabPage();
			this.LogsTabPage = new ZStmALogTabPage();
			this.SaveButton = new ZButton();
			this.RemoveRuleButton = new ZButton();
			this.PublishCheckBox = new ZCheckBox();
			this.CloseButton = new ZButton();
			this.RemoveSchemeButton = new ZButton();
			this.RenameRuleButton = new ZButton();
			this.checkBoxIsPublishedAcrossAllCompanies = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RulesTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 386, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 24, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GridColourScheme);
			// 
			// AddNewRuleButton
			// 
			this.AddNewRuleButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|734e7366-e020-4651-b658-745de5dc8087", "Add Rule", "Add Rule", "Add Rule", "");
			this.AddNewRuleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 11, true);
			this.AddNewRuleButton.Name = "AddNewRuleButton";
			this.AddNewRuleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 23, true);
			this.AddNewRuleButton.TabIndex = 1;
			this.AddNewRuleButton.UseVisualStyleBackColor = true;
			this.AddNewRuleButton.Click += new EventHandler(this.AddNewRuleButton_Click);
			// 
			// SchemeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.SchemeNameTextBox, "S9_FilterName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GridColourScheme)(null)).S9_FilterName);
			this.SchemeNameTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|e6c217d1-ca93-450e-9abb-6a78547a713a", "Scheme Name", "Scheme Name", "Scheme Name", "");
			this.SchemeNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SchemeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 14, true);
			this.SchemeNameTextBox.Name = "SchemeNameTextBox";
			this.SchemeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.SchemeNameTextBox.TabIndex = 0;
			// 
			// RulesTabControl
			// 
			this.RulesTabControl.Controls.Add(this.DefaultTabPage);
			this.RulesTabControl.Controls.Add(this.LogsTabPage);
			this.RulesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 41, true);
			this.RulesTabControl.Name = "RulesTabControl";
			this.RulesTabControl.SelectedIndex = 0;
			this.RulesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 282, true);
			this.RulesTabControl.TabIndex = 4;
			// 
			// DefaultTabPage
			// 
			this.DefaultTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|72b03ea7-7a62-463a-bb15-6dd5158685e7", "Rule.");
			this.DefaultTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DefaultTabPage.Name = "DefaultTabPage";
			this.DefaultTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DefaultTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 255, true);
			this.DefaultTabPage.TabIndex = 1;
			this.DefaultTabPage.UseVisualStyleBackColor = true;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|592ef8f5-db24-4c3d-aec4-27547110e604", "Change Log");
			this.LogsTabPage.GetStmALogFilterStripBusinessObject = null;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.LogsTabPage.LogsToShow = (Enterprise.ZArchitecture.Business.LogsToShow.ChangeLogs | Enterprise.ZArchitecture.Business.LogsToShow.Operations);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 256, true);
			this.LogsTabPage.TabIndex = 2;
			this.LogsTabPage.UseVisualStyleBackColor = true;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.SaveButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|71bff869-35d5-440b-b7d8-bd7cfec0f382", "Save && Close", "Save && Close", "Save && Close", "");
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 344, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.SaveButton.TabIndex = 8;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new EventHandler(this.SaveButton_Click);
			// 
			// RemoveRuleButton
			// 
			this.RemoveRuleButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|ea013f81-edd8-46f9-90de-c19fa3430dc0", "Remove Rule", "Remove Rule", "Remove Rule", "");
			this.RemoveRuleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 11, true);
			this.RemoveRuleButton.Name = "RemoveRuleButton";
			this.RemoveRuleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 23, true);
			this.RemoveRuleButton.TabIndex = 2;
			this.RemoveRuleButton.UseVisualStyleBackColor = true;
			this.RemoveRuleButton.Click += new EventHandler(this.RemoveRuleButton_Click);
			// 
			// PublishCheckBox
			// 
			this.PublishCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.BindingSource.SetBindingMember(this.PublishCheckBox, "S9_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GridColourScheme)(null)).S9_IsPublished);
			this.PublishCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|187b4b83-66d4-407b-85f4-700c38dd0458", "Published", "Published", "Published", "");
			this.PublishCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PublishCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 341, true);
			this.PublishCheckBox.Name = "PublishCheckBox";
			this.PublishCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 28, true);
			this.PublishCheckBox.TabIndex = 6;
			this.PublishCheckBox.UseVisualStyleBackColor = true;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|acea772d-0503-43c0-8c0d-e5527cac67d9", "Close", "Close", "Close", "");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(765, 344, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// RemoveSchemeButton
			// 
			this.RemoveSchemeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.RemoveSchemeButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|3e6888ec-627c-47b8-b0d8-df737f270ad5", "Remove Scheme", "Remove Scheme", "Remove Scheme", "");
			this.RemoveSchemeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.RemoveSchemeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 344, true);
			this.RemoveSchemeButton.Name = "RemoveSchemeButton";
			this.RemoveSchemeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.RemoveSchemeButton.TabIndex = 5;
			this.RemoveSchemeButton.UseVisualStyleBackColor = true;
			this.RemoveSchemeButton.Click += new EventHandler(this.RemoveSchemeButton_Click);
			// 
			// RenameRuleButton
			// 
			this.RenameRuleButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|8adfcf36-c662-4c81-8c33-9b5107979060", "Rename Rule", "Rename Rule", "Rename Rule", "");
			this.RenameRuleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(670, 11, true);
			this.RenameRuleButton.Name = "RenameRuleButton";
			this.RenameRuleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 23, true);
			this.RenameRuleButton.TabIndex = 3;
			this.RenameRuleButton.UseVisualStyleBackColor = true;
			this.RenameRuleButton.Click += new EventHandler(this.RenameRuleButton_Click);
			// 
			// checkBoxIsPublishedAcrossAllCompanies
			// 
			this.checkBoxIsPublishedAcrossAllCompanies.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.checkBoxIsPublishedAcrossAllCompanies.AutoSize = true;
			this.BindingSource.SetBindingMember(this.checkBoxIsPublishedAcrossAllCompanies, "PublishAcrossAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GridColourScheme)(null)).PublishAcrossAllCompanies);
			this.checkBoxIsPublishedAcrossAllCompanies.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|271c6ec7-5261-4f40-ba43-7dff71a85005", "Publish across all companies", "If selected, published layout will be available in all companies on this database.");
			this.checkBoxIsPublishedAcrossAllCompanies.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxIsPublishedAcrossAllCompanies.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 346, true);
			this.checkBoxIsPublishedAcrossAllCompanies.Name = "checkBoxIsPublishedAcrossAllCompanies";
			this.checkBoxIsPublishedAcrossAllCompanies.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
			this.checkBoxIsPublishedAcrossAllCompanies.TabIndex = 7;
			this.checkBoxIsPublishedAcrossAllCompanies.UseVisualStyleBackColor = true;
			// 
			// ZGridColourCustomiseForm
			// 
			this.AcceptButton = this.SaveButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZGridColourCustomiseForm|e946ca55-0df1-40b2-bf36-cb5a2329fe3a", "Grid Color Scheme.");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 410, true);
			this.Controls.Add(this.RenameRuleButton);
			this.Controls.Add(this.RemoveSchemeButton);
			this.Controls.Add(this.RemoveRuleButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.RulesTabControl);
			this.Controls.Add(this.SchemeNameTextBox);
			this.Controls.Add(this.checkBoxIsPublishedAcrossAllCompanies);
			this.Controls.Add(this.PublishCheckBox);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.AddNewRuleButton);
			this.DataSourceType = typeof(GridColourScheme);
			this.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.IsPostOnly = true;
			this.MinimizeBox = false;
			this.Name = "ZGridColourCustomiseForm";
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.AddNewRuleButton, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.PublishCheckBox, 0);
			this.Controls.SetChildIndex(this.checkBoxIsPublishedAcrossAllCompanies, 0);
			this.Controls.SetChildIndex(this.SchemeNameTextBox, 0);
			this.Controls.SetChildIndex(this.RulesTabControl, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.RemoveRuleButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.RemoveSchemeButton, 0);
			this.Controls.SetChildIndex(this.RenameRuleButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RulesTabControl.ResumeLayout(false);
			this.RulesTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

			this.RulesTabControl.MouseDown += new MouseEventHandler(TabControlMouseDown);
			this.RulesTabControl.DragOver += new DragEventHandler(TabControlDragOver);
			this.RulesTabControl.QueryContinueDrag += new QueryContinueDragEventHandler(TabControlQueryContinueDrag);
			this.RulesTabControl.AllowDrop = true;

			SelectedTabPage = null;
		}
		#endregion
	}
}
