using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class ExportStatementSettingControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DocumentsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SeaGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PullToConsolidationMBLCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PullToDirectMBLCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PullToHBLCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AirGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PullToConsolidationMawbCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PullToDirectIATAMawbCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PullToHawbCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportStatementSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CountryExportStatementSettingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExportStatementSettingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocumentsPanel.SuspendLayout();
			this.SeaGroupBox.SuspendLayout();
			this.AirGroupBox.SuspendLayout();
			this.ExportStatementSplitContainer.Panel1.SuspendLayout();
			this.ExportStatementSplitContainer.Panel2.SuspendLayout();
			this.ExportStatementSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryExportStatementSettingGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportStatementSettingGrid)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CountryExportStatementSetting);
			//
			// DocumentsPanel
			//
			this.DocumentsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DocumentsPanel.Controls.Add(this.SeaGroupBox);
			this.DocumentsPanel.Controls.Add(this.AirGroupBox);
			this.DocumentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 271, true);
			this.DocumentsPanel.Name = "DocumentsPanel";
			this.DocumentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 175, true);
			this.DocumentsPanel.TabIndex = 4;
			//
			// SeaGroupBox
			//
			this.SeaGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|863a52ff-5fa9-443d-905b-c669d1f6bf58", "Ocean");
			this.SeaGroupBox.Controls.Add(this.PullToConsolidationMBLCheckBox);
			this.SeaGroupBox.Controls.Add(this.PullToDirectMBLCheckBox);
			this.SeaGroupBox.Controls.Add(this.PullToHBLCheckBox);
			this.SeaGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SeaGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 86, true);
			this.SeaGroupBox.Name = "SeaGroupBox";
			this.SeaGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 86, true);
			this.SeaGroupBox.TabIndex = 3;
			this.SeaGroupBox.TabStop = false;
			//
			// PullToConsolidationMBLCheckBox
			//
			this.PullToConsolidationMBLCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PullToConsolidationMBLCheckBox, "Statements.UseOnConsolidationMasterBillOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).UseOnConsolidationMasterBillOfLading)));
			this.PullToConsolidationMBLCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|cead1dac-cbd8-4539-b280-077d181541a5", "Pull To Back/Back and Consolidation MBL");
			this.PullToConsolidationMBLCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PullToConsolidationMBLCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 65, true);
			this.PullToConsolidationMBLCheckBox.Name = "PullToConsolidationMBLCheckBox";
			this.PullToConsolidationMBLCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
			this.PullToConsolidationMBLCheckBox.TabIndex = 5;
			this.PullToConsolidationMBLCheckBox.UseVisualStyleBackColor = true;
			//
			// PullToDirectMBLCheckBox
			//
			this.PullToDirectMBLCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PullToDirectMBLCheckBox, "Statements.UseOnDirectMasterBillOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).UseOnDirectMasterBillOfLading)));
			this.PullToDirectMBLCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|69b2877d-aa30-48ba-abe5-63aa2f3d8cf8", "Pull To Direct Master B/L");
			this.PullToDirectMBLCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PullToDirectMBLCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.PullToDirectMBLCheckBox.Name = "PullToDirectMBLCheckBox";
			this.PullToDirectMBLCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 17, true);
			this.PullToDirectMBLCheckBox.TabIndex = 4;
			this.PullToDirectMBLCheckBox.UseVisualStyleBackColor = true;
			//
			// PullToHBLCheckBox
			//
			this.PullToHBLCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PullToHBLCheckBox, "Statements.UseOnHouseBillOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).UseOnHouseBillOfLading)));
			this.PullToHBLCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|8796d1c4-98d2-4d64-af9f-e8de612a6c08", "Pull To House B/L");
			this.PullToHBLCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PullToHBLCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.PullToHBLCheckBox.Name = "PullToHBLCheckBox";
			this.PullToHBLCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.PullToHBLCheckBox.TabIndex = 3;
			this.PullToHBLCheckBox.UseVisualStyleBackColor = true;
			//
			// AirGroupBox
			//
			this.AirGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|db29f669-c9b0-4e0f-9058-b7cc5996c332", "Air");
			this.AirGroupBox.Controls.Add(this.PullToConsolidationMawbCheckBox);
			this.AirGroupBox.Controls.Add(this.PullToDirectIATAMawbCheckBox);
			this.AirGroupBox.Controls.Add(this.PullToHawbCheckBox);
			this.AirGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.AirGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AirGroupBox.Name = "AirGroupBox";
			this.AirGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 86, true);
			this.AirGroupBox.TabIndex = 1;
			this.AirGroupBox.TabStop = false;
			//
			// PullToConsolidationMawbCheckBox
			//
			this.PullToConsolidationMawbCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PullToConsolidationMawbCheckBox, "Statements.UseOnConsolidationMawb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).UseOnConsolidationMawb)));
			this.PullToConsolidationMawbCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|860d1aa9-55c8-4da8-b228-ee8998fe9813", "Pull To Back/Back and Consolidation MAWB");
			this.PullToConsolidationMawbCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PullToConsolidationMawbCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 65, true);
			this.PullToConsolidationMawbCheckBox.Name = "PullToConsolidationMawbCheckBox";
			this.PullToConsolidationMawbCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 17, true);
			this.PullToConsolidationMawbCheckBox.TabIndex = 2;
			this.PullToConsolidationMawbCheckBox.UseVisualStyleBackColor = true;
			//
			// PullToDirectIATAMawbCheckBox
			//
			this.PullToDirectIATAMawbCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PullToDirectIATAMawbCheckBox, "Statements.UseOnDirectIATAMawb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).UseOnDirectIATAMawb)));
			this.PullToDirectIATAMawbCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|e7313be6-bb83-4319-9d61-0d590dc040b6", "Pull To Direct IATA MAWB");
			this.PullToDirectIATAMawbCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PullToDirectIATAMawbCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.PullToDirectIATAMawbCheckBox.Name = "PullToDirectIATAMawbCheckBox";
			this.PullToDirectIATAMawbCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
			this.PullToDirectIATAMawbCheckBox.TabIndex = 1;
			this.PullToDirectIATAMawbCheckBox.UseVisualStyleBackColor = true;
			//
			// PullToHawbCheckBox
			//
			this.PullToHawbCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PullToHawbCheckBox, "Statements.UseOnHawb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).UseOnHawb)));
			this.PullToHawbCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|6ccf3c09-ad9d-4235-a28a-d6a271c59fe6", "Pull To HAWB");
			this.PullToHawbCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PullToHawbCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.PullToHawbCheckBox.Name = "PullToHawbCheckBox";
			this.PullToHawbCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.PullToHawbCheckBox.TabIndex = 0;
			this.PullToHawbCheckBox.UseVisualStyleBackColor = true;
			//
			// ExportStatementSplitContainer
			//
			this.ExportStatementSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ExportStatementSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.ExportStatementSplitContainer.Name = "ExportStatementSplitContainer";
			this.ExportStatementSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// ExportStatementSplitContainer.Panel1
			//
			this.ExportStatementSplitContainer.Panel1.Controls.Add(this.CountryExportStatementSettingGrid);
			//
			// ExportStatementSplitContainer.Panel2
			//
			this.ExportStatementSplitContainer.Panel2.Controls.Add(this.ExportStatementSettingGrid);
			this.ExportStatementSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 260, true);
			this.ExportStatementSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ExportStatementSplitContainer.TabIndex = 6;
			//
			// CountryExportStatementSettingGrid
			//
			this.CountryExportStatementSettingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CountryExportStatementSettingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).CountryCodes)));
			this.CountryExportStatementSettingGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "CountryCodes";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|c1626a2b-cf81-47dd-8e7e-b100e0daeb0f", "Country/Region Code");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CountryExportStatementSettingGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CountryExportStatementSettingGrid.GridId = "9fa16d86-0873-490c-8e5b-e7ac1f32a66e";
			this.CountryExportStatementSettingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryExportStatementSettingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryExportStatementSettingGrid.LayoutKey = "CountryExportStatementSettingGrid";
			this.CountryExportStatementSettingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CountryExportStatementSettingGrid.Name = "CountryExportStatementSettingGrid";
			this.CountryExportStatementSettingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 160, true);
			this.CountryExportStatementSettingGrid.TabIndex = 2;
			//
			// ExportStatementSettingGrid
			//
			this.ExportStatementSettingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExportStatementSettingGrid, "Statements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).Statement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).StatementDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).Visibility)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).VisibilityTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).Field1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).StatementFieldTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).Field2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ExportStatementSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.CountryExportStatementSetting)(null)).Statements)).SyncRoot)).StatementFieldTypeList)));
			this.ExportStatementSettingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|17BAE203-BF29-4259-8B43-B22B416F5B7D", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|2338fdcd-dcec-4326-8d33-7ebe17bee844", "Statement");
			zMultiLineTextBoxColumnInfo1.ColumnName = "Statement";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|b58ae418-03bd-4a88-84d9-2893d5eac4d9", "Description");
			zMultiLineTextBoxColumnInfo2.ColumnName = "StatementDescription";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zDropEditColumnStyleInfo1.BindToList = "VisibilityTypeList";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|d2b399a4-aeab-4791-a11c-1610774f1697", "Visibility");
			zDropEditColumnStyleInfo1.ColumnName = "Visibility";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zDropEditColumnStyleInfo2.BindToList = "StatementFieldTypeList";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|b9d34d81-c897-43cd-8f65-18e93a8fc5d9", "Field 1");
			zDropEditColumnStyleInfo2.ColumnName = "Field1";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zDropEditColumnStyleInfo3.BindToList = "StatementFieldTypeList";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ExportStatementSettingControl|f4458a6f-1d85-4e65-989f-e889e2b5bf79", "Field 2");
			zDropEditColumnStyleInfo3.ColumnName = "Field2";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			this.ExportStatementSettingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ExportStatementSettingGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ExportStatementSettingGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.ExportStatementSettingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ExportStatementSettingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ExportStatementSettingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ExportStatementSettingGrid.GridId = "aff2ab5c-a8b8-40f0-a05c-e08e36018edf";
			this.ExportStatementSettingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportStatementSettingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExportStatementSettingGrid.LayoutKey = "ExportStatementSettingGrid";
			this.ExportStatementSettingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportStatementSettingGrid.Name = "ExportStatementSettingGrid";
			this.ExportStatementSettingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 96, true);
			this.ExportStatementSettingGrid.TabIndex = 2;
			//
			// ExportStatementSettingControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExportStatementSplitContainer);
			this.Controls.Add(this.DocumentsPanel);
			this.Name = "ExportStatementSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 449, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocumentsPanel.ResumeLayout(false);
			this.SeaGroupBox.ResumeLayout(false);
			this.SeaGroupBox.PerformLayout();
			this.AirGroupBox.ResumeLayout(false);
			this.AirGroupBox.PerformLayout();
			this.ExportStatementSplitContainer.Panel1.ResumeLayout(false);
			this.ExportStatementSplitContainer.Panel2.ResumeLayout(false);
			this.ExportStatementSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CountryExportStatementSettingGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportStatementSettingGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel DocumentsPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AirGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PullToConsolidationMawbCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PullToDirectIATAMawbCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PullToHawbCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SeaGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PullToConsolidationMBLCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PullToDirectMBLCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PullToHBLCheckBox;
		private CargoWise.Windows.UI.KSplitContainer ExportStatementSplitContainer;
		private Enterprise.ZArchitecture.ZGrid CountryExportStatementSettingGrid;
		private Enterprise.ZArchitecture.ZGrid ExportStatementSettingGrid;

		protected internal ZGrid CountryExportStatementSettingGridInternal => CountryExportStatementSettingGrid;
		protected internal ZGrid ExportStatementSettingGridInternal => ExportStatementSettingGrid;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox AirGroupBoxInternal => AirGroupBox;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox SeaGroupBoxInternal => SeaGroupBox;
	}
}
