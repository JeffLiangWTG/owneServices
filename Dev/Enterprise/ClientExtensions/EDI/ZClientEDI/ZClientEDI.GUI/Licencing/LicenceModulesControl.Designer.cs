namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class LicenceModulesControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.OwnerBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.hideUnlicencedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.DiscrepancyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DiscrepancyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LastDiscrepancyChangeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.LicenceKeyInSyncLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AMSModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LastLicenceCheckDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ModulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productLabel = new Enterprise.ZArchitecture.ZLabel();
			this.productBox = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OwnerBox.SuspendLayout();
			this.DiscrepancyGroupBox.SuspendLayout();
			this.LastDiscrepancyChangeDateEdit.SuspendLayout();
			this.AMSModeDropEdit.SuspendLayout();
			this.LastLicenceCheckDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ModulesGrid)).BeginInit();
			this.ModulesGrid.SuspendLayout();
			this.EditionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader);
			// 
			// OwnerBox
			// 
			this.OwnerBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerBox, "OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).OH_Owner)));
			this.OwnerBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 85, true);
			this.OwnerBox.Name = "OwnerBox";
			this.OwnerBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 17, true);
			this.OwnerBox.TabIndex = 5;
			// 
			// hideUnlicencedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.hideUnlicencedCheckBox, "HideUnlicenced");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).HideUnlicenced)));
			this.hideUnlicencedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.hideUnlicencedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 0, true);
			this.hideUnlicencedCheckBox.Name = "hideUnlicencedCheckBox";
			this.hideUnlicencedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 17, true);
			this.hideUnlicencedCheckBox.TabIndex = 2;
			this.hideUnlicencedCheckBox.Text = "Hide Unlicenced";
			this.hideUnlicencedCheckBox.UseVisualStyleBackColor = true;
			// 
			// zLabel7
			// 
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 68, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 13, true);
			this.zLabel7.TabIndex = 4;
			this.zLabel7.Text = "Owner:";
			// 
			// DiscrepancyGroupBox
			// 
			this.DiscrepancyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.DiscrepancyGroupBox.Controls.Add(this.DiscrepancyTextBox);
			this.DiscrepancyGroupBox.Controls.Add(this.LastDiscrepancyChangeDateEdit);
			this.DiscrepancyGroupBox.Controls.Add(this.zLabel1);
			this.DiscrepancyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 227, true);
			this.DiscrepancyGroupBox.Name = "DiscrepancyGroupBox";
			this.DiscrepancyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 332, true);
			this.DiscrepancyGroupBox.TabIndex = 9;
			this.DiscrepancyGroupBox.TabStop = false;
			this.DiscrepancyGroupBox.Text = "Discrepancy Details";
			// 
			// DiscrepancyTextBox
			// 
			this.DiscrepancyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DiscrepancyTextBox, "DiscrepancyText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).DiscrepancyText)));
			this.DiscrepancyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DiscrepancyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 58, true);
			this.DiscrepancyTextBox.Multiline = true;
			this.DiscrepancyTextBox.Name = "DiscrepancyTextBox";
			this.DiscrepancyTextBox.ReadOnly = true;
			this.DiscrepancyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.DiscrepancyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 267, true);
			this.DiscrepancyTextBox.TabIndex = 2;
			// 
			// LastDiscrepancyChangeDateEdit
			// 
			this.LastDiscrepancyChangeDateEdit.AllowDrop = true;
			this.LastDiscrepancyChangeDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastDiscrepancyChangeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastDiscrepancyChangeDateEdit, "LA_LastDiscrepancyChange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).LA_LastDiscrepancyChange)));
			this.LastDiscrepancyChangeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 32, true);
			this.LastDiscrepancyChangeDateEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 0, true);
			this.LastDiscrepancyChangeDateEdit.Name = "LastDiscrepancyChangeDateEdit";
			this.LastDiscrepancyChangeDateEdit.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 12, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Last Change:";
			// 
			// LicenceKeyInSyncLabel
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LicenceKeyInSyncLabel, false);
			this.LicenceKeyInSyncLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 148, true);
			this.LicenceKeyInSyncLabel.Name = "LicenceKeyInSyncLabel";
			this.LicenceKeyInSyncLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 23, true);
			this.LicenceKeyInSyncLabel.TabIndex = 7;
			this.LicenceKeyInSyncLabel.Text = "<LICENCE KEY IN SYNC STATUS>";
			// 
			// AMSModeDropEdit
			// 
			this.AMSModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AMSModeDropEdit, "LA_AMS_USMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).LA_AMS_USMode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AMSModeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.AMSModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 122, true);
			this.AMSModeDropEdit.Name = "AMSModeDropEdit";
			this.AMSModeDropEdit.PreBoundMaxLength = 3;
			this.AMSModeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.AMSModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 17, true);
			this.AMSModeDropEdit.TabIndex = 6;
			// 
			// LastLicenceCheckDateEdit
			// 
			this.LastLicenceCheckDateEdit.AllowDrop = true;
			this.LastLicenceCheckDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastLicenceCheckDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastLicenceCheckDateEdit, "LA_LastLicenceSyncCheck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).LA_LastLicenceSyncCheck)));
			this.LastLicenceCheckDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8c425b60-f953-46e3-a5e4-1dec4e4de9d5", "Last License Check");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.LastLicenceCheckDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LastLicenceCheckDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 195, true);
			this.LastLicenceCheckDateEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 0, true);
			this.LastLicenceCheckDateEdit.Name = "LastLicenceCheckDateEdit";
			this.LastLicenceCheckDateEdit.TabIndex = 8;
			// 
			// ModulesGrid
			// 
			this.ModulesGrid.AllowNavigation = false;
			this.ModulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ModulesGrid, "FilteredModules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).FilteredModules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModules)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).FilteredModules)).SyncRoot)).LM_GroupModuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModules)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).FilteredModules)).SyncRoot)).LM_Calc_IndentedGroupModuleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModules)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).FilteredModules)).SyncRoot)).LM_Calc_IsEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModules)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).FilteredModules)).SyncRoot)).LM_UserCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModules)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).FilteredModules)).SyncRoot)).LM_LicenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModules)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).FilteredModules)).SyncRoot)).LM_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModules)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).FilteredModules)).SyncRoot)).LM_RenewalUserCount)));
			this.ModulesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "LM_GroupModuleCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo2.Caption = "Module Name";
			zTextBoxColumnStyleInfo2.ColumnName = "LM_Calc_IndentedGroupModuleDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			zCheckBoxColumnStyleInfo1.Caption = " ";
			zCheckBoxColumnStyleInfo1.ColumnName = "LM_Calc_IsEnabled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "LM_UserCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "LM_LicenceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "LM_ExpiryDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Users Last Renewal";
			zCalcEditColumnStyleInfo2.ColumnName = "LM_RenewalUserCount";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ModulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ModulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ModulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ModulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ModulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ModulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ModulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ModulesGrid.CopySelectedRowsAllowed = true;
			this.ModulesGrid.GridId = "d8fd462f-ac37-4cd7-92dd-bc486bedd093";
			this.ModulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ModulesGrid.LayoutKey = "ModulesGrid";
			this.ModulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 19, true);
			this.ModulesGrid.Name = "ModulesGrid";
			this.ModulesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ModulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 538, true);
			this.ModulesGrid.TabIndex = 11;
			// 
			// EditionDropEdit
			// 
			this.EditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EditionDropEdit, "Edition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Edition)));
			this.EditionDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c1476d11-50ab-460f-a795-669a84af8da2", "Edition");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.EditionDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.EditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 39, true);
			this.EditionDropEdit.Name = "EditionDropEdit";
			this.EditionDropEdit.PreBoundMaxLength = 3;
			this.EditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 17, true);
			this.EditionDropEdit.TabIndex = 3;
			// 
			// productLabel
			// 
			this.productLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.productLabel.Name = "productLabel";
			this.productLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.productLabel.TabIndex = 0;
			this.productLabel.Text = "Product:";
			// 
			// productBox
			// 
			this.productBox.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.productBox, false);
			this.productBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 0, true);
			this.productBox.Name = "productBox";
			this.productBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 13, true);
			this.productBox.TabIndex = 1;
			this.productBox.Text = "<Product>";
			// 
			// LicenceModulesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.productBox);
			this.Controls.Add(this.productLabel);
			this.Controls.Add(this.OwnerBox);
			this.Controls.Add(this.hideUnlicencedCheckBox);
			this.Controls.Add(this.zLabel7);
			this.Controls.Add(this.DiscrepancyGroupBox);
			this.Controls.Add(this.LicenceKeyInSyncLabel);
			this.Controls.Add(this.AMSModeDropEdit);
			this.Controls.Add(this.LastLicenceCheckDateEdit);
			this.Controls.Add(this.ModulesGrid);
			this.Controls.Add(this.EditionDropEdit);
			this.Name = "LicenceModulesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 557, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OwnerBox.ResumeLayout(true);
			this.OwnerBox.PerformLayout();
			this.DiscrepancyGroupBox.ResumeLayout(false);
			this.DiscrepancyGroupBox.PerformLayout();
			this.LastDiscrepancyChangeDateEdit.ResumeLayout(true);
			this.LastDiscrepancyChangeDateEdit.PerformLayout();
			this.AMSModeDropEdit.ResumeLayout(true);
			this.AMSModeDropEdit.PerformLayout();
			this.LastLicenceCheckDateEdit.ResumeLayout(true);
			this.LastLicenceCheckDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ModulesGrid)).EndInit();
			this.ModulesGrid.ResumeLayout(false);
			this.ModulesGrid.PerformLayout();
			this.EditionDropEdit.ResumeLayout(true);
			this.EditionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox OwnerBox;
		private ZArchitecture.GUI.ZCheckBox hideUnlicencedCheckBox;
		private ZArchitecture.ZLabel zLabel7;
		private ZArchitecture.GUI.ZGroupBox DiscrepancyGroupBox;
		private ZArchitecture.ZTextBox DiscrepancyTextBox;
		private ZArchitecture.GUI.ZDateEdit LastDiscrepancyChangeDateEdit;
		private ZArchitecture.ZLabel zLabel1;
		public ZArchitecture.ZLabel LicenceKeyInSyncLabel;
		private ZArchitecture.GUI.ZDropEdit AMSModeDropEdit;
		private ZArchitecture.GUI.ZDateEdit LastLicenceCheckDateEdit;
		private ZArchitecture.ZGrid ModulesGrid;
		private ZArchitecture.GUI.ZDropEdit EditionDropEdit;
		private ZArchitecture.ZLabel productLabel;
		public ZArchitecture.ZLabel productBox;
	}
}
