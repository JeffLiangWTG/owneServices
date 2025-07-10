namespace Enterprise.Customs.DE.GUI
{
	partial class AWBLineToConsolidateUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LineDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustodianIdentifierBranchNoZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OwnerReferenceTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustodianIdentifierZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustodianZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.OwnerReferenceNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageQtyZIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LineDetailsGroupBox.SuspendLayout();
			this.CustodianIdentifierBranchNoZDropEdit.SuspendLayout();
			this.OwnerReferenceTypeZDropEdit.SuspendLayout();
			this.CustodianZAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec);
			// 
			// LineDetailsGroupBox
			// 
			this.LineDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("b0104c21-b787-4b15-93aa-975a490d62af", "Line Details");
			this.LineDetailsGroupBox.Controls.Add(this.CustodianIdentifierBranchNoZDropEdit);
			this.LineDetailsGroupBox.Controls.Add(this.OwnerReferenceTypeZDropEdit);
			this.LineDetailsGroupBox.Controls.Add(this.CustodianIdentifierZTextBox);
			this.LineDetailsGroupBox.Controls.Add(this.CustodianZAddressControl);
			this.LineDetailsGroupBox.Controls.Add(this.OwnerReferenceNumberZTextBox);
			this.LineDetailsGroupBox.Controls.Add(this.PackageQtyZIntEdit);
			this.LineDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LineDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 95, true);
			this.LineDetailsGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 114, true);
			this.LineDetailsGroupBox.Name = "LineDetailsGroupBox";
			this.LineDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 94, true);
			this.LineDetailsGroupBox.TabIndex = 100;
			this.LineDetailsGroupBox.TabStop = false;
			// 
			// CustodianIdentifierBranchNoZDropEdit
			// 
			this.CustodianIdentifierBranchNoZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianIdentifierBranchNoZDropEdit, "CusTempStorageLines.TSL_CustodianIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifierBranchNo)));
			this.CustodianIdentifierBranchNoZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 63, true);
			this.CustodianIdentifierBranchNoZDropEdit.Name = "CustodianIdentifierBranchNoZDropEdit";
			this.CustodianIdentifierBranchNoZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustodianIdentifierBranchNoZDropEdit.TabIndex = 5;
			// 
			// OwnerReferenceTypeZDropEdit
			// 
			this.OwnerReferenceTypeZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerReferenceTypeZDropEdit, "CusTempStorageLines.TSL_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			this.OwnerReferenceTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 19, true);
			this.OwnerReferenceTypeZDropEdit.Name = "OwnerReferenceTypeZDropEdit";
			this.OwnerReferenceTypeZDropEdit.PreBoundMaxLength = 3;
			this.OwnerReferenceTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.OwnerReferenceTypeZDropEdit.TabIndex = 0;
			// 
			// CustodianIdentifierZTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustodianIdentifierZTextBox, "CusTempStorageLines.TSL_CustodianIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifier)));
			this.CustodianIdentifierZTextBox.CaptionResourceString = null;
			this.CustodianIdentifierZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 41, true);
			this.CustodianIdentifierZTextBox.Name = "CustodianIdentifierZTextBox";
			this.CustodianIdentifierZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustodianIdentifierZTextBox.TabIndex = 4;
			// 
			// CustodianZAddressControl
			// 
			this.CustodianZAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianZAddressControl, "CusTempStorageLines.TSL_OA_Custodian");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian)));
			this.CustodianZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 17, true);
			this.CustodianZAddressControl.Name = "CustodianZAddressControl";
			this.CustodianZAddressControl.PopupCaption = "";
			this.CustodianZAddressControl.ReadOnly = false;
			this.CustodianZAddressControl.ShowAddress = false;
			this.CustodianZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianZAddressControl.TabIndex = 3;
			// 
			// OwnerReferenceNumberZTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerReferenceNumberZTextBox, "CusTempStorageLines.TSL_OwnerReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
			this.OwnerReferenceNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 41, true);
			this.OwnerReferenceNumberZTextBox.Name = "OwnerReferenceNumberZTextBox";
			this.OwnerReferenceNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.OwnerReferenceNumberZTextBox.TabIndex = 1;
			// 
			// PackageQtyZIntEdit
			// 
			this.PackageQtyZIntEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageQtyZIntEdit, "CusTempStorageLines.TSL_PackageQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_PackageQty)));
			this.PackageQtyZIntEdit.CaptionResourceString = null;
			this.PackageQtyZIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 63, true);
			this.PackageQtyZIntEdit.Name = "PackageQtyZIntEdit";
			this.PackageQtyZIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PackageQtyZIntEdit.TabIndex = 2;
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LinesGrid, "CusTempStorageLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).CustodianOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).Lookups.OrganizationsFindBoxList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifierBranchNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_PackageQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageLineToConsolidate)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
			this.LinesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "TSL_OwnerReferenceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "TSL_OwnerReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(187);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups.OrganizationsFindBoxList";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8b6b5c48-3749-4da2-8058-da94a24abc52", "Custodian");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CustodianOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zGuidDropEditColumnStyleInfo1.BindToList = "TSL_OA_Custodian_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "TSL_OA_Custodian";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(166);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "TSL_CustodianIdentifier";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "TSL_CustodianIdentifierBranchNo";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TSL_PackageQty";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zTextBoxColumnStyleInfo3.ColumnName = "TSL_CustomsStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGrid.GridId = "65f1cbfd-aaf0-4c42-9799-3a85507997fd";
			this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinesGrid.LayoutKey = "LinesGrid";
			this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGrid.Name = "LinesGrid";
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 95, true);
			this.LinesGrid.TabIndex = 101;
			// 
			// AWBLineToConsolidateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LinesGrid);
			this.Controls.Add(this.LineDetailsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 189, true);
			this.Name = "AWBLineToConsolidateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 189, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LineDetailsGroupBox.ResumeLayout(false);
			this.LineDetailsGroupBox.PerformLayout();
			this.CustodianIdentifierBranchNoZDropEdit.ResumeLayout(true);
			this.CustodianIdentifierBranchNoZDropEdit.PerformLayout();
			this.OwnerReferenceTypeZDropEdit.ResumeLayout(true);
			this.OwnerReferenceTypeZDropEdit.PerformLayout();
			this.CustodianZAddressControl.ResumeLayout(true);
			this.CustodianZAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox LineDetailsGroupBox;
		private ZArchitecture.GUI.ZIntEdit PackageQtyZIntEdit;
		private ZArchitecture.ZGrid LinesGrid;
		private ZArchitecture.GUI.ZDropEdit CustodianIdentifierBranchNoZDropEdit;
		private ZArchitecture.GUI.ZDropEdit OwnerReferenceTypeZDropEdit;
		private ZArchitecture.ZTextBox CustodianIdentifierZTextBox;
		private ZArchitecture.GUI.ZAddressControl CustodianZAddressControl;
		private ZArchitecture.ZTextBox OwnerReferenceNumberZTextBox;
	}
}
