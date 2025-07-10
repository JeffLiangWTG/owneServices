using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ResourceStrings.Module
{
	partial class LocalLanguagesForm
	{
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

		protected Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		protected Enterprise.ZArchitecture.ZTextBox ISOTextBox;
		protected Enterprise.ZArchitecture.ZTranslatableTextControl DescriptionTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox RA_RN_NKCountryCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit RA_RA_ParentLanguageDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		protected Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		protected Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		ZTextBox fullLanguageCodeTextBox;
		ZCheckBox isActiveCheckbox;
		ZCheckBox isSystemCheckbox;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.isActiveCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isSystemCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.fullLanguageCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ISOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RA_RN_NKCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RA_RA_ParentLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.RA_RN_NKCountryCodeFindBox.SuspendLayout();
			this.RA_RA_ParentLanguageDropEdit.SuspendLayout();
			this.DescriptionTextBox.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 387, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 20, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(433);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.RefLocalLanguage);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 358, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 7, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(643, 346, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("LocalLanguagesForm|1f2f7d9f-93fc-45b8-8258-59d38fcd7c6f", "Local Language");
			this.MainTabPage.Controls.Add(this.isActiveCheckbox);
			this.MainTabPage.Controls.Add(this.isSystemCheckbox);
			this.MainTabPage.Controls.Add(this.fullLanguageCodeTextBox);
			this.MainTabPage.Controls.Add(this.ISOTextBox);
			this.MainTabPage.Controls.Add(this.RA_RN_NKCountryCodeFindBox);
			this.MainTabPage.Controls.Add(this.RA_RA_ParentLanguageDropEdit);
			this.MainTabPage.Controls.Add(this.DescriptionTextBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 324, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// ISOTextBox
			// 
			this.ISOTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ISOTextBox, "RA_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.RefLocalLanguage)(null)).RA_Code)));
			this.ISOTextBox.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("3e5e8770-5cfb-48d4-a1bd-2f03a4d69060", "ISO-639 Language Code");
			this.ISOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 15, true);
			this.ISOTextBox.Name = "ISOTextBox";
			this.ISOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.ISOTextBox.TabIndex = 0;
			// 
			// RA_RN_NKCountryCodeFindBox
			// 
			this.RA_RN_NKCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RA_RN_NKCountryCodeFindBox, "RA_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.RefLocalLanguage)(null)).RA_RN_NKCountryCode)));
			this.RA_RN_NKCountryCodeFindBox.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("LocalLanguagesForm|a5d01b65-e323-41d3-b7f9-d833914d69a2", "Country/Region");
			this.RA_RN_NKCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 43, true);
			this.RA_RN_NKCountryCodeFindBox.Name = "RA_RN_NKCountryCodeFindBox";
			this.RA_RN_NKCountryCodeFindBox.ShouldResize = true;
			this.RA_RN_NKCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 17, true);
			this.RA_RN_NKCountryCodeFindBox.TabIndex = 1;
			// 
			// fullLanguageCodeTextBox
			// 
			this.fullLanguageCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.fullLanguageCodeTextBox, "FullLanguageCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.RefLocalLanguage)(null)).FullLanguageCode)));
			this.fullLanguageCodeTextBox.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("7e573846-9bdd-4ea4-adce-74865030c228", "Full Language Code");
			this.fullLanguageCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 71, true);
			this.fullLanguageCodeTextBox.Name = "fullLanguageCodeTextBox";
			this.fullLanguageCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.fullLanguageCodeTextBox.TabIndex = 2;
			// 
			// RA_RA_ParentLanguageGuidFindBox
			// 
			this.RA_RA_ParentLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RA_RA_ParentLanguageDropEdit, "ParentLanguageCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ResourceStrings.Business.RefLocalLanguage)(null)).RA_RA_ParentLanguage)));
			this.RA_RA_ParentLanguageDropEdit.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("LocalLanguagesForm|2a96a5f6-0e6f-4170-b9f4-64d741c8c542", "Parent Language");
			this.RA_RA_ParentLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 101, true);
			this.RA_RA_ParentLanguageDropEdit.Name = "RA_RA_ParentLanguageGuidFindBox";
			this.RA_RA_ParentLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 17, true);
			this.RA_RA_ParentLanguageDropEdit.TabIndex = 3;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.AcceptsReturn = false;
			this.DescriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "RA_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.RefLocalLanguage)(null)).RA_Description)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("LocalLanguagesForm|d441e849-be98-404c-b4b0-faB81810cc76", "Language Name");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.GridCurrent = null;
			this.DescriptionTextBox.GridMember = null;
			this.DescriptionTextBox.IsLanguageEditingEnabled = true;
			this.DescriptionTextBox.IsMultiLine = false;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 133, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ReadOnly = false;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 20, true);
			this.DescriptionTextBox.TabIndex = 4;
			// 
			// isActiveCheckbox
			// 
			this.BindingSource.SetBindingMember(this.isActiveCheckbox, "RA_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.RefLocalLanguage)(null)).RA_IsActive)));
			this.isActiveCheckbox.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("LocalLanguagesForm|d66ede8b-5a06-49c3-b782-4fe71d9fef9e", "Is Active");
			this.isActiveCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isActiveCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 13, true);
			this.isActiveCheckbox.Name = "isActiveCheckbox";
			this.isActiveCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.isActiveCheckbox.TabIndex = 5;
			// 
			// isSystemCheckbox
			// 
			this.BindingSource.SetBindingMember(this.isSystemCheckbox, "RA_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ResourceStrings.Business.RefLocalLanguage)(null)).RA_IsSystem)));
			this.isSystemCheckbox.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("LocalLanguagesForm|b3aac09a-1033-4198-9dca-f902c77b6a7f", "Is System");
			this.isSystemCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isSystemCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 37, true);
			this.isSystemCheckbox.Name = "isSystemCheckbox";
			this.isSystemCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.isSystemCheckbox.TabIndex = 6;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 324, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 324, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// LocalLanguagesForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("LocalLanguagesForm|ef70ed1b-fe6b-4b1d-8913-074b4ac59026", "Local Language");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 407, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.ResourceStrings.Business.RefLocalLanguage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 468, true);
			this.Name = "LocalLanguagesForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.RA_RN_NKCountryCodeFindBox.ResumeLayout(true);
			this.RA_RN_NKCountryCodeFindBox.PerformLayout();
			this.RA_RA_ParentLanguageDropEdit.ResumeLayout(true);
			this.RA_RA_ParentLanguageDropEdit.PerformLayout();
			this.DescriptionTextBox.ResumeLayout(true);
			this.DescriptionTextBox.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected IContainer components;
	}
}