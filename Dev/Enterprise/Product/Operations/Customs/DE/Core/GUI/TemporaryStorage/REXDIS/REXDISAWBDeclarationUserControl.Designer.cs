namespace Enterprise.Customs.DE.GUI
{
	partial class REXDISAWBDeclarationUserControl
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
			this.IdentificationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustodianBranchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustodianEoriTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustodianUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.OwnerRefNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerRefTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IdentificationDetailsGroupBox.SuspendLayout();
			this.CustodianBranchDropEdit.SuspendLayout();
			this.CustodianUserControl.SuspendLayout();
			this.OwnerRefTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec);
			// 
			// IdentificationDetailsGroupBox
			// 
			this.IdentificationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("47667774-9442-4b1e-b0c0-3866c2192d53", "SumA Identification");
			this.IdentificationDetailsGroupBox.Controls.Add(this.CustodianBranchDropEdit);
			this.IdentificationDetailsGroupBox.Controls.Add(this.CustodianEoriTextBox);
			this.IdentificationDetailsGroupBox.Controls.Add(this.CustodianUserControl);
			this.IdentificationDetailsGroupBox.Controls.Add(this.OwnerRefNumberTextBox);
			this.IdentificationDetailsGroupBox.Controls.Add(this.OwnerRefTypeDropEdit);
			this.IdentificationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IdentificationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IdentificationDetailsGroupBox.Name = "IdentificationDetailsGroupBox";
			this.IdentificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 138, true);
			this.IdentificationDetailsGroupBox.TabIndex = 1;
			this.IdentificationDetailsGroupBox.TabStop = false;
			// 
			// CustodianBranchDropEdit
			// 
			this.CustodianBranchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianBranchDropEdit, "CusTempStorageLines.SumALine.TSL_CustodianIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.TSL_CustodianIdentifierBranchNo)));
			this.CustodianBranchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 111, true);
			this.CustodianBranchDropEdit.Name = "CustodianBranchDropEdit";
			this.CustodianBranchDropEdit.PreBoundMaxLength = 4;
			this.CustodianBranchDropEdit.ShouldResizeByMaxLength = true;
			this.CustodianBranchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianBranchDropEdit.TabIndex = 5;
			// 
			// CustodianEoriTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustodianEoriTextBox, "CusTempStorageLines.SumALine.TSL_CustodianIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.TSL_CustodianIdentifier)));
			this.CustodianEoriTextBox.CaptionResourceString = null;
			this.CustodianEoriTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 88, true);
			this.CustodianEoriTextBox.Name = "CustodianEoriTextBox";
			this.CustodianEoriTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianEoriTextBox.TabIndex = 4;
			// 
			// CustodianUserControl
			// 
			this.CustodianUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianUserControl, "CusTempStorageLines.SumALine.TSL_OA_Custodian");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.TSL_OA_Custodian)));
			this.CustodianUserControl.BindToOrgList = "Lookups.OrganizationsFindBoxList";
			this.CustodianUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 65, true);
			this.CustodianUserControl.Name = "CustodianUserControl";
			this.CustodianUserControl.PopupCaption = "";
			this.CustodianUserControl.ReadOnly = false;
			this.CustodianUserControl.ShowAddress = false;
			this.CustodianUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianUserControl.TabIndex = 3;
			// 
			// OwnerRefNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerRefNumberTextBox, "CusTempStorageLines.SumALine.TSL_OwnerReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.TSL_OwnerReferenceNumber)));
			this.OwnerRefNumberTextBox.CaptionResourceString = null;
			this.OwnerRefNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OwnerRefNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 42, true);
			this.OwnerRefNumberTextBox.Name = "OwnerRefNumberTextBox";
			this.OwnerRefNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwnerRefNumberTextBox.TabIndex = 2;
			// 
			// OwnerRefTypeDropEdit
			// 
			this.OwnerRefTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerRefTypeDropEdit, "CusTempStorageLines.SumALine.TSL_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.TSL_OwnerReferenceType)));
			this.OwnerRefTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 19, true);
			this.OwnerRefTypeDropEdit.Name = "OwnerRefTypeDropEdit";
			this.OwnerRefTypeDropEdit.PreBoundMaxLength = 3;
			this.OwnerRefTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OwnerRefTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwnerRefTypeDropEdit.TabIndex = 1;
			// 
			// REXDISAWBDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IdentificationDetailsGroupBox);
			this.Name = "REXDISAWBDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 138, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IdentificationDetailsGroupBox.ResumeLayout(false);
			this.IdentificationDetailsGroupBox.PerformLayout();
			this.CustodianBranchDropEdit.ResumeLayout(true);
			this.CustodianBranchDropEdit.PerformLayout();
			this.CustodianUserControl.ResumeLayout(true);
			this.CustodianUserControl.PerformLayout();
			this.OwnerRefTypeDropEdit.ResumeLayout(true);
			this.OwnerRefTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox IdentificationDetailsGroupBox;
		private ZArchitecture.ZTextBox CustodianEoriTextBox;
		private ZArchitecture.GUI.ZAddressControl CustodianUserControl;
		private ZArchitecture.ZTextBox OwnerRefNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit OwnerRefTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit CustodianBranchDropEdit;
	}
}
