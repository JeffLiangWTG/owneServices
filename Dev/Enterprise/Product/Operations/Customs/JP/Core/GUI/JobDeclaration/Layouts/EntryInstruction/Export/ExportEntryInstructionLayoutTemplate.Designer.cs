using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.JP.GUI
{
	partial class ExportEntryInstructionLayoutTemplate
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

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ExportControlNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AwbOrBillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreInspectedCargoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LoadingConfirmationIsRequiredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.VanningLocationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VanningLocationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DeclarationCargoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PreInspectedCargoDropEdit.SuspendLayout();
			this.VanningLocationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VanningLocationsGrid)).BeginInit();
			this.VanningLocationsGrid.SuspendLayout();
			this.DeclarationCargoTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			//
			// ExportControlNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.ExportControlNumberTextBox, "ExportControlNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).ExportControlNumber)));
			this.ExportControlNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.ExportControlNumberTextBox.Name = "ExportControlNumberTextBox";
			this.ExportControlNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ExportControlNumberTextBox.TabIndex = 0;
			//
			// AwbOrBillNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.AwbOrBillNumberTextBox, "CEI_BillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_BillNumber)));
			this.AwbOrBillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.AwbOrBillNumberTextBox.Name = "AwbOrBillNumberTextBox";
			this.AwbOrBillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.AwbOrBillNumberTextBox.TabIndex = 0;
			//
			// GoodsDescriptionTextBox
			//
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "CEI_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_GoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 0;
			//
			// PreInspectedCargoDropEdit
			//
			this.PreInspectedCargoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreInspectedCargoDropEdit, "CEI_PreInspectedCargoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_PreInspectedCargoType)));
			this.PreInspectedCargoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.PreInspectedCargoDropEdit.Name = "PreInspectedCargoDropEdit";
			this.PreInspectedCargoDropEdit.PreBoundMaxLength = 1;
			this.PreInspectedCargoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PreInspectedCargoDropEdit.TabIndex = 1;
			//
			// LoadingConfirmationIsRequiredCheckBox
			//
			this.BindingSource.SetBindingMember(this.LoadingConfirmationIsRequiredCheckBox, "CEI_LoadingConfirmationIsRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_LoadingConfirmationIsRequired)));
			this.LoadingConfirmationIsRequiredCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LoadingConfirmationIsRequiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.LoadingConfirmationIsRequiredCheckBox.Name = "LoadingConfirmationIsRequiredCheckBox";
			this.LoadingConfirmationIsRequiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.LoadingConfirmationIsRequiredCheckBox.TabIndex = 3;
			//
			// VanningLocationsGroupBox
			//
			this.VanningLocationsGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("13F9B5BE-0D67-CDBA-793C-0A931DB0EA06", "Vanning Locations");
			this.VanningLocationsGroupBox.Controls.Add(this.VanningLocationsGrid);
			this.VanningLocationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 3, true);
			this.VanningLocationsGroupBox.Name = "VanningLocationsGroupBox";
			this.VanningLocationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 182, true);
			this.VanningLocationsGroupBox.TabIndex = 0;
			this.VanningLocationsGroupBox.TabStop = false;
			//
			// VanningLocationsGrid
			//
			this.VanningLocationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.VanningLocationsGrid, "VanningLocations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_AddressSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_AddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_GovRegNumType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_GovRegNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.VanningAddress)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).VanningLocations)).SyncRoot)).E2_AdditionalAddressInformation)));
			this.VanningLocationsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("6DB44DC9-20B3-695C-6F72-0A5DB3B13EB7", "Sequence");
			zCalcEditColumnStyleInfo1.ColumnName = "E2_AddressSequence";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zAddressDropEditColumnStyleInfo1.ColumnName = "E2_OA_Address";
			zAddressDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("D9451007-8E1F-CF19-DC8C-9D0013757641", "Override");
			zCheckBoxColumnStyleInfo1.ColumnName = "E2_AddressOverride";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "E2_GovRegNumType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "E2_GovRegNum";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.ColumnName = "E2_RN_NKCountryCode";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "E2_CompanyName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = "E2_State";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "E2_City";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "E2_Address1";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "E2_AdditionalAddressInformation";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.VanningLocationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.VanningLocationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.VanningLocationsGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.VanningLocationsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.VanningLocationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.VanningLocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.VanningLocationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.VanningLocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.VanningLocationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.VanningLocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.VanningLocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.VanningLocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.VanningLocationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VanningLocationsGrid.GridId = "D582958F-BCF5-E858-E954-4EA711ADE213";
			this.VanningLocationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VanningLocationsGrid.LayoutKey = "VanningLocationsGrid";
			this.VanningLocationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.VanningLocationsGrid.Name = "VanningLocationsGrid";
			this.VanningLocationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 163, true);
			this.VanningLocationsGrid.TabIndex = 5;
			//
			// DeclarationCargoTypeDropEdit
			//
			this.DeclarationCargoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationCargoTypeDropEdit, "CEI_DeclarationCargoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_DeclarationCargoType)));
			this.DeclarationCargoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.DeclarationCargoTypeDropEdit.Name = "DeclarationCargoTypeDropEdit";
			this.DeclarationCargoTypeDropEdit.PreBoundMaxLength = 1;
			this.DeclarationCargoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DeclarationCargoTypeDropEdit.TabIndex = 7;
			//
			// ExportEntryInstructionLayoutTemplate
			//
			this.Controls.Add(this.DeclarationCargoTypeDropEdit);
			this.Controls.Add(this.ExportControlNumberTextBox);
			this.Controls.Add(this.AwbOrBillNumberTextBox);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.PreInspectedCargoDropEdit);
			this.Controls.Add(this.LoadingConfirmationIsRequiredCheckBox);
			this.Controls.Add(this.VanningLocationsGroupBox);
			this.Name = "ExportEntryInstructionLayoutTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 258, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreInspectedCargoDropEdit.ResumeLayout(true);
			this.PreInspectedCargoDropEdit.PerformLayout();
			this.VanningLocationsGroupBox.ResumeLayout(false);
			this.VanningLocationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.VanningLocationsGrid)).EndInit();
			this.VanningLocationsGrid.ResumeLayout(false);
			this.VanningLocationsGrid.PerformLayout();
			this.DeclarationCargoTypeDropEdit.ResumeLayout(true);
			this.DeclarationCargoTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZTextBox AwbOrBillNumberTextBox;
		ZTextBox ExportControlNumberTextBox;
		ZTextBox GoodsDescriptionTextBox;
		ZDropEdit PreInspectedCargoDropEdit;
		ZCheckBox LoadingConfirmationIsRequiredCheckBox;
		ZGroupBox VanningLocationsGroupBox;
		ZGrid VanningLocationsGrid;
		ZDropEdit DeclarationCargoTypeDropEdit;
	}
}
