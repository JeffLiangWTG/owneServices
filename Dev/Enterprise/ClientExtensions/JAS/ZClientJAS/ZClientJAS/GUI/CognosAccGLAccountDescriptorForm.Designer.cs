
namespace Enterprise.Client.JAS.GUI.Cognos
{
	partial class CognosAccGLAccountDescriptorForm
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (!SubClassificationTabPage.IsDisposed)
				{
					SubClassificationTabPage.Dispose();
				}

				if (!CognosTabPage.IsDisposed)
				{
					CognosTabPage.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SubClassificationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SubClassAccountGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CognosAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SubClassifyByGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AccountAgeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DebtorModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.CreditorModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.IsSubClassifiedByAgeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.IsSubClassifiedByCreditorRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.IsSubClassifiedByDebtorRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.CognosTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ExportOptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReconciliationTotalTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsPublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReconciliateTotalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GroupingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntercompanyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GeographicCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BusinessCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BranchCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ModeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SubClassificationTabPage.SuspendLayout();
			this.SubClassAccountGroupBox.SuspendLayout();
			this.SubClassifyByGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DebtorModuleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CreditorModuleButtonGrid.InnerGrid)).BeginInit();
			this.CognosTabPage.SuspendLayout();
			this.ExportOptionGroupBox.SuspendLayout();
			this.GroupingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.CognosTabPage);
			this.MainTabControl.Controls.Add(this.SubClassificationTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 317, true);
			this.MainTabControl.Controls.SetChildIndex(this.SubClassificationTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CognosTabPage, 0);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 360, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor);
			// 
			// SubClassificationTabPage
			// 
			this.SubClassificationTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.SubClassificationTabPage.Controls.Add(this.SubClassAccountGroupBox);
			this.SubClassificationTabPage.Controls.Add(this.SubClassifyByGroupBox);
			this.SubClassificationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SubClassificationTabPage.Name = "SubClassificationTabPage";
			this.SubClassificationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SubClassificationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 290, true);
			this.SubClassificationTabPage.TabIndex = 4;
			this.SubClassificationTabPage.Text = "Cognos Sub-Classification";
			// 
			// SubClassAccountGroupBox
			// 
			this.SubClassAccountGroupBox.Controls.Add(this.CognosAccountGuidFindBox);
			this.SubClassAccountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.SubClassAccountGroupBox.Name = "SubClassAccountGroupBox";
			this.SubClassAccountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 49, true);
			this.SubClassAccountGroupBox.TabIndex = 3;
			this.SubClassAccountGroupBox.TabStop = false;
			this.SubClassAccountGroupBox.Text = "Account to be Sub-Classified";
			// 
			// CognosAccountGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.CognosAccountGuidFindBox, "ExtraInfoForBinding.T9_AJ_AccountToBeSubClassified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).T9_AJ_AccountToBeSubClassified)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).Lookups.CognosAccounts)));
			this.CognosAccountGuidFindBox.BindToList = "ExtraInfoForBinding.Lookups+CognosAccounts";
			this.CognosAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.CognosAccountGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccGLAccountDescriptor;
			this.CognosAccountGuidFindBox.Name = "CognosAccountGuidFindBox";
			this.CognosAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 20, true);
			this.CognosAccountGuidFindBox.TabIndex = 2;
			// 
			// SubClassifyByGroupBox
			// 
			this.SubClassifyByGroupBox.Controls.Add(this.AccountAgeDropEdit);
			this.SubClassifyByGroupBox.Controls.Add(this.DebtorModuleButtonGrid);
			this.SubClassifyByGroupBox.Controls.Add(this.CreditorModuleButtonGrid);
			this.SubClassifyByGroupBox.Controls.Add(this.IsSubClassifiedByAgeRadioButton);
			this.SubClassifyByGroupBox.Controls.Add(this.IsSubClassifiedByCreditorRadioButton);
			this.SubClassifyByGroupBox.Controls.Add(this.IsSubClassifiedByDebtorRadioButton);
			this.SubClassifyByGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 61, true);
			this.SubClassifyByGroupBox.Name = "SubClassifyByGroupBox";
			this.SubClassifyByGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 225, true);
			this.SubClassifyByGroupBox.TabIndex = 3;
			this.SubClassifyByGroupBox.TabStop = false;
			this.SubClassifyByGroupBox.Text = "Sub Classify by:";
			// 
			// AccountAgeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.AccountAgeDropEdit, "ExtraInfoForBinding.T9_AccountAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).T9_AccountAge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).Lookups.AccountAgeList)));
			this.AccountAgeDropEdit.BindToList = "ExtraInfoForBinding.Lookups+AccountAgeList";
			this.AccountAgeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 43, true);
			this.AccountAgeDropEdit.Name = "AccountAgeDropEdit";
			this.AccountAgeDropEdit.PreBoundMaxLength = 2;
			this.AccountAgeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.AccountAgeDropEdit.TabIndex = 2;
			// 
			// DebtorModuleButtonGrid
			// 
			this.DebtorModuleButtonGrid.AttachButtonText = CargoWiseOne.ResourceStrings.Res.GetData("726713D5-8D3A-42E0-BB7E-ABFBBE984319", "Add");
			this.BindingSource.SetBindingMember(this.DebtorModuleButtonGrid, "ExtraInfoForBinding.MappedDebtorGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).MappedDebtorGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).Lookups.DebtorGroups)));
			this.DebtorModuleButtonGrid.BindToFindBoxList = "ExtraInfoForBinding.Lookups+DebtorGroups";
			zTextBoxColumnStyleInfo1.ColumnName = "OJ_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "OJ_Desc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.DebtorModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DebtorModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DebtorModuleButtonGrid.GridId = "d0e64452-469b-45cc-b280-ba4e721dd32f";
			this.DebtorModuleButtonGrid.DetachButtonText = CargoWiseOne.ResourceStrings.Res.GetData("5B5774F1-A785-4C84-AA7E-8ED33B251E76", "Remove");
			this.DebtorModuleButtonGrid.DetachMessage = CargoWiseOne.ResourceStrings.Res.GetData("29D6ACB3-21E9-41ED-B6E8-CA2F38AA3CE5", "Are you sure you want to remove the selected record?");
			// 
			// 
			// 
			this.DebtorModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.DebtorModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DebtorModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DebtorModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DebtorModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.DebtorModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.DebtorModuleButtonGrid.InnerGrid.Name = "Grid";
			this.DebtorModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.DebtorModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 140, true);
			this.DebtorModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DebtorModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 41, true);
			this.DebtorModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgDebtorGroup;
			this.DebtorModuleButtonGrid.Name = "DebtorModuleButtonGrid";
			this.DebtorModuleButtonGrid.NameOfAGridElement = CargoWiseOne.ResourceStrings.Res.GetData("AB5F61EA-DF46-434F-96DF-52590263AEF7", "Debtor Group Mapping");
			this.DebtorModuleButtonGrid.ReadOnly = true;
			this.DebtorModuleButtonGrid.ShowEditButton = false;
			this.DebtorModuleButtonGrid.ShowNewButton = false;
			this.DebtorModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 178, true);
			this.DebtorModuleButtonGrid.TabIndex = 1;
			// 
			// CreditorModuleButtonGrid
			// 
			this.CreditorModuleButtonGrid.AttachButtonText = CargoWiseOne.ResourceStrings.Res.GetData("726713D5-8D3A-42E0-BB7E-ABFBBE984319", "Add");
			this.BindingSource.SetBindingMember(this.CreditorModuleButtonGrid, "ExtraInfoForBinding.MappedCreditorGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).MappedCreditorGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).Lookups.CreditorGroups)));
			this.CreditorModuleButtonGrid.BindToFindBoxList = "ExtraInfoForBinding.Lookups+CreditorGroups";
			zTextBoxColumnStyleInfo3.Caption = "Code";
			zTextBoxColumnStyleInfo3.ColumnName = "OG_Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = "Description";
			zTextBoxColumnStyleInfo4.ColumnName = "OG_Desc";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.CreditorModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CreditorModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CreditorModuleButtonGrid.GridId = "dc67562b-60a7-47e5-9b76-8d19d5b12009";
			this.CreditorModuleButtonGrid.DetachButtonText = CargoWiseOne.ResourceStrings.Res.GetData("5B5774F1-A785-4C84-AA7E-8ED33B251E76", "Remove");
			this.CreditorModuleButtonGrid.DetachMessage = CargoWiseOne.ResourceStrings.Res.GetData("29D6ACB3-21E9-41ED-B6E8-CA2F38AA3CE5", "Are you sure you want to remove the selected record?");
			// 
			// 
			// 
			this.CreditorModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.CreditorModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CreditorModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.CreditorModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CreditorModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.CreditorModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.CreditorModuleButtonGrid.InnerGrid.Name = "Grid";
			this.CreditorModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.CreditorModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 140, true);
			this.CreditorModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.CreditorModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 41, true);
			this.CreditorModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgCreditorGroup;
			this.CreditorModuleButtonGrid.Name = "CreditorModuleButtonGrid";
			this.CreditorModuleButtonGrid.NameOfAGridElement = CargoWiseOne.ResourceStrings.Res.GetData("E8010C61-83EF-4713-8741-CC23392CCDA4", "Creditor Group Mapping");
			this.CreditorModuleButtonGrid.ReadOnly = true;
			this.CreditorModuleButtonGrid.ShowEditButton = false;
			this.CreditorModuleButtonGrid.ShowNewButton = false;
			this.CreditorModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 178, true);
			this.CreditorModuleButtonGrid.TabIndex = 1;
			// 
			// IsSubClassifiedByAgeRadioButton
			// 
			this.IsSubClassifiedByAgeRadioButton.AutoCheck = false;
			this.IsSubClassifiedByAgeRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSubClassifiedByAgeRadioButton, "ExtraInfoForBinding.IsSubClassifiedByAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).IsSubClassifiedByAge)));
			this.IsSubClassifiedByAgeRadioButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("CognosAccGLAccountDescriptorForm|3e26d61a-c557-4e32-b318-9f65bd0638e7", "Age");
			this.IsSubClassifiedByAgeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSubClassifiedByAgeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 18, true);
			this.IsSubClassifiedByAgeRadioButton.Name = "IsSubClassifiedByAgeRadioButton";
			this.IsSubClassifiedByAgeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 17, true);
			this.IsSubClassifiedByAgeRadioButton.TabIndex = 0;
			this.IsSubClassifiedByAgeRadioButton.TabStop = true;
			this.IsSubClassifiedByAgeRadioButton.UseVisualStyleBackColor = true;
			// 
			// IsSubClassifiedByCreditorRadioButton
			// 
			this.IsSubClassifiedByCreditorRadioButton.AutoCheck = false;
			this.IsSubClassifiedByCreditorRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSubClassifiedByCreditorRadioButton, "ExtraInfoForBinding.IsSubClassifiedByCreditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).IsSubClassifiedByCreditor)));
			this.IsSubClassifiedByCreditorRadioButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("CognosAccGLAccountDescriptorForm|61adbba5-d081-4b0a-985c-d70f395f12bb", "Creditor");
			this.IsSubClassifiedByCreditorRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSubClassifiedByCreditorRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 19, true);
			this.IsSubClassifiedByCreditorRadioButton.Name = "IsSubClassifiedByCreditorRadioButton";
			this.IsSubClassifiedByCreditorRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 17, true);
			this.IsSubClassifiedByCreditorRadioButton.TabIndex = 0;
			this.IsSubClassifiedByCreditorRadioButton.TabStop = true;
			this.IsSubClassifiedByCreditorRadioButton.UseVisualStyleBackColor = true;
			// 
			// IsSubClassifiedByDebtorRadioButton
			// 
			this.IsSubClassifiedByDebtorRadioButton.AutoCheck = false;
			this.IsSubClassifiedByDebtorRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSubClassifiedByDebtorRadioButton, "ExtraInfoForBinding.IsSubClassifiedByDebtor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).IsSubClassifiedByDebtor)));
			this.IsSubClassifiedByDebtorRadioButton.Checked = true;
			this.IsSubClassifiedByDebtorRadioButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("CognosAccGLAccountDescriptorForm|a04fa4a5-096c-432d-8a0f-c7b9e20cf360", "Debtor");
			this.IsSubClassifiedByDebtorRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSubClassifiedByDebtorRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 19, true);
			this.IsSubClassifiedByDebtorRadioButton.Name = "IsSubClassifiedByDebtorRadioButton";
			this.IsSubClassifiedByDebtorRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 17, true);
			this.IsSubClassifiedByDebtorRadioButton.TabIndex = 0;
			this.IsSubClassifiedByDebtorRadioButton.TabStop = true;
			this.IsSubClassifiedByDebtorRadioButton.UseVisualStyleBackColor = true;
			// 
			// CognosTabPage
			// 
			this.CognosTabPage.Controls.Add(this.ExportOptionGroupBox);
			this.CognosTabPage.Controls.Add(this.GroupingsGroupBox);
			this.CognosTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CognosTabPage.Name = "CognosTabPage";
			this.CognosTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 290, true);
			this.CognosTabPage.TabIndex = 5;
			this.CognosTabPage.Text = "Cognos Settings";
			// 
			// ExportOptionGroupBox
			// 
			this.ExportOptionGroupBox.Controls.Add(this.ReconciliationTotalTextBox);
			this.ExportOptionGroupBox.Controls.Add(this.IsPublishedCheckBox);
			this.ExportOptionGroupBox.Controls.Add(this.ReconciliateTotalCheckBox);
			this.ExportOptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 3, true);
			this.ExportOptionGroupBox.Name = "ExportOptionGroupBox";
			this.ExportOptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 89, true);
			this.ExportOptionGroupBox.TabIndex = 1;
			this.ExportOptionGroupBox.TabStop = false;
			this.ExportOptionGroupBox.Text = "Export Option";
			// 
			// ReconciliationTotalTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReconciliationTotalTextBox, "ExtraInfoForBinding.T9_ReconciliationTotalAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).T9_ReconciliationTotalAccount)));
			this.ReconciliationTotalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 51, true);
			this.ReconciliationTotalTextBox.Name = "ReconciliationTotalTextBox";
			this.ReconciliationTotalTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ReconciliationTotalTextBox.TabIndex = 1;
			// 
			// IsPublishedCheckBox
			// 
			this.IsPublishedCheckBox.AutoSize = true;
			this.IsPublishedCheckBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.IsPublishedCheckBox, "ExtraInfoForBinding.T9_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).T9_IsPublished)));
			this.IsPublishedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 22, true);
			this.IsPublishedCheckBox.Name = "IsPublishedCheckBox";
			this.IsPublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsPublishedCheckBox.TabIndex = 0;
			this.IsPublishedCheckBox.UseVisualStyleBackColor = false;
			// 
			// ReconciliateTotalCheckBox
			// 
			this.ReconciliateTotalCheckBox.AutoSize = true;
			this.ReconciliateTotalCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ReconciliateTotalCheckBox, "ExtraInfoForBinding.ShouldReconciliateTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptorExtraInfo)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).ExtraInfoForBinding)).SyncRoot)).ShouldReconciliateTotal)));
			this.ReconciliateTotalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReconciliateTotalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 53, true);
			this.ReconciliateTotalCheckBox.Name = "ReconciliateTotalCheckBox";
			this.ReconciliateTotalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ReconciliateTotalCheckBox.TabIndex = 0;
			this.ReconciliateTotalCheckBox.UseVisualStyleBackColor = false;
			// 
			// GroupingsGroupBox
			// 
			this.GroupingsGroupBox.Controls.Add(this.IntercompanyDropEdit);
			this.GroupingsGroupBox.Controls.Add(this.GeographicCalcEdit);
			this.GroupingsGroupBox.Controls.Add(this.BusinessCalcEdit);
			this.GroupingsGroupBox.Controls.Add(this.BranchCalcEdit);
			this.GroupingsGroupBox.Controls.Add(this.ModeCalcEdit);
			this.GroupingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 98, true);
			this.GroupingsGroupBox.Name = "GroupingsGroupBox";
			this.GroupingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 181, true);
			this.GroupingsGroupBox.TabIndex = 1;
			this.GroupingsGroupBox.TabStop = false;
			this.GroupingsGroupBox.Text = "Groupings";
			// 
			// IntercompanyDropEdit
			// 
			this.IntercompanyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.IntercompanyDropEdit, "GroupingFlagsForBinding.T4_Company");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.JAS.Business.Cognos.CognosGroupingFlags)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).GroupingFlagsForBinding)).SyncRoot)).T4_Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosGroupingFlags)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).GroupingFlagsForBinding)).SyncRoot)).Lookups.IntercompanyCodeList)));
			this.IntercompanyDropEdit.BindToList = "GroupingFlagsForBinding.Lookups+IntercompanyCodeList";
			this.IntercompanyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 149, true);
			this.IntercompanyDropEdit.Name = "IntercompanyDropEdit";
			this.IntercompanyDropEdit.PreBoundMaxLength = 3;
			this.IntercompanyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 20, true);
			this.IntercompanyDropEdit.TabIndex = 2;
			// 
			// GeographicCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GeographicCalcEdit, "GroupingFlagsForBinding.T4_Geographical");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.JAS.Business.Cognos.CognosGroupingFlags)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).GroupingFlagsForBinding)).SyncRoot)).T4_Geographical)));
			this.GeographicCalcEdit.Decimals = 0;
			this.GeographicCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 116, true);
			this.GeographicCalcEdit.Name = "GeographicCalcEdit";
			this.GeographicCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.GeographicCalcEdit.TabIndex = 1;
			this.GeographicCalcEdit.Text = "0";
			this.GeographicCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BusinessCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BusinessCalcEdit, "GroupingFlagsForBinding.T4_BusinessType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.JAS.Business.Cognos.CognosGroupingFlags)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).GroupingFlagsForBinding)).SyncRoot)).T4_BusinessType)));
			this.BusinessCalcEdit.Decimals = 0;
			this.BusinessCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 83, true);
			this.BusinessCalcEdit.Name = "BusinessCalcEdit";
			this.BusinessCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.BusinessCalcEdit.TabIndex = 1;
			this.BusinessCalcEdit.Text = "0";
			this.BusinessCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BranchCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BranchCalcEdit, "GroupingFlagsForBinding.T4_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.JAS.Business.Cognos.CognosGroupingFlags)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).GroupingFlagsForBinding)).SyncRoot)).T4_Branch)));
			this.BranchCalcEdit.Decimals = 0;
			this.BranchCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 50, true);
			this.BranchCalcEdit.Name = "BranchCalcEdit";
			this.BranchCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.BranchCalcEdit.TabIndex = 1;
			this.BranchCalcEdit.Text = "0";
			this.BranchCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ModeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ModeCalcEdit, "GroupingFlagsForBinding.T4_Mode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.JAS.Business.Cognos.CognosGroupingFlags)(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor)(null)).GroupingFlagsForBinding)).SyncRoot)).T4_Mode)));
			this.ModeCalcEdit.Decimals = 0;
			this.ModeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 17, true);
			this.ModeCalcEdit.Name = "ModeCalcEdit";
			this.ModeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ModeCalcEdit.TabIndex = 1;
			this.ModeCalcEdit.Text = "0";
			this.ModeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CognosAccGLAccountDescriptorForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 384, true);
			this.DataSourceType = typeof(Enterprise.Client.JAS.Business.Cognos.CognosAccGLAccountDescriptor);
			this.Name = "CognosAccGLAccountDescriptorForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SubClassificationTabPage.ResumeLayout(false);
			this.SubClassAccountGroupBox.ResumeLayout(false);
			this.SubClassifyByGroupBox.ResumeLayout(false);
			this.SubClassifyByGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DebtorModuleButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CreditorModuleButtonGrid.InnerGrid)).EndInit();
			this.CognosTabPage.ResumeLayout(false);
			this.ExportOptionGroupBox.ResumeLayout(false);
			this.ExportOptionGroupBox.PerformLayout();
			this.GroupingsGroupBox.ResumeLayout(false);
			this.GroupingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion


		private Enterprise.ZArchitecture.GUI.ZTabPage SubClassificationTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SubClassifyByGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton IsSubClassifiedByDebtorRadioButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SubClassAccountGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CognosAccountGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton IsSubClassifiedByCreditorRadioButton;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid CreditorModuleButtonGrid;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid DebtorModuleButtonGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage CognosTabPage;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ReconciliateTotalCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox GroupingsGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit BusinessCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit BranchCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ModeCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit IntercompanyDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit GeographicCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ExportOptionGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsPublishedCheckBox;
		private Enterprise.ZArchitecture.ZTextBox ReconciliationTotalTextBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton IsSubClassifiedByAgeRadioButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AccountAgeDropEdit;
	}
}
