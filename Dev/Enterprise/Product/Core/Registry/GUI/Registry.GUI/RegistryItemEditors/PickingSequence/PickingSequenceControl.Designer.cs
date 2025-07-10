using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class PickingSequenceControl
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
			this.PickAlgorithmSequenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HighPriorityLocationsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExpiryDateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PickFacesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ConsolidatedPalletsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FifoFallbackCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FullPalletsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PalletOverflowCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BrokenPalletsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IsPickfaceEmptyPreventPickingFromBulkCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LocationSortOrderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LevelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RowCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ColumnCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickAlgorithmSequenceGroupBox.SuspendLayout();
			this.LocationSortOrderGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.PickingSequence);
			// 
			// PickAlgorithmSequenceGroupBox
			// 
			this.PickAlgorithmSequenceGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|87466f09-c17a-4e4d-b912-9b9c5a9762f9", "Algorithm Sequence");
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.HighPriorityLocationsCalcEdit);
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.ExpiryDateCalcEdit);
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.PickFacesCalcEdit);
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.ConsolidatedPalletsCalcEdit);
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.FifoFallbackCalcEdit);
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.FullPalletsCalcEdit);
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.PalletOverflowCalcEdit);
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.BrokenPalletsCalcEdit);
			this.PickAlgorithmSequenceGroupBox.Controls.Add(this.IsPickfaceEmptyPreventPickingFromBulkCheckBox);
			this.PickAlgorithmSequenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PickAlgorithmSequenceGroupBox.Name = "PickAlgorithmSequenceGroupBox";
			this.PickAlgorithmSequenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 224, true);
			this.PickAlgorithmSequenceGroupBox.TabIndex = 0;
			this.PickAlgorithmSequenceGroupBox.TabStop = false;
			// 
			// HighPriorityLocationsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.HighPriorityLocationsCalcEdit, "HighPriorityLocations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).HighPriorityLocations)));
			this.HighPriorityLocationsCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|HP", "High Priority Locations");
			this.HighPriorityLocationsCalcEdit.DecimalPlaces = 0;
			this.HighPriorityLocationsCalcEdit.Decimals = 0;
			this.HighPriorityLocationsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 15, true);
			this.HighPriorityLocationsCalcEdit.Name = "HighPriorityLocationsCalcEdit";
			this.HighPriorityLocationsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.HighPriorityLocationsCalcEdit.TabIndex = 0;
			this.HighPriorityLocationsCalcEdit.Text = "0";
			this.HighPriorityLocationsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExpiryDateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExpiryDateCalcEdit, "ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).ExpiryDate)));
			this.ExpiryDateCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|82f7b915-c754-4d32-97a1-d01e0408efff", "Expiry Date");
			this.ExpiryDateCalcEdit.DecimalPlaces = 0;
			this.ExpiryDateCalcEdit.Decimals = 0;
			this.ExpiryDateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 119, true);
			this.ExpiryDateCalcEdit.Name = "ExpiryDateCalcEdit";
			this.ExpiryDateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ExpiryDateCalcEdit.TabIndex = 4;
			this.ExpiryDateCalcEdit.Text = "0";
			this.ExpiryDateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PickFacesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PickFacesCalcEdit, "PickFaces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).PickFaces)));
			this.PickFacesCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|431bdc81-a0b6-40d7-8fd8-246f35f29bbf", "Pick Faces");
			this.PickFacesCalcEdit.DecimalPlaces = 0;
			this.PickFacesCalcEdit.Decimals = 0;
			this.PickFacesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 145, true);
			this.PickFacesCalcEdit.Name = "PickFacesCalcEdit";
			this.PickFacesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.PickFacesCalcEdit.TabIndex = 5;
			this.PickFacesCalcEdit.Text = "0";
			this.PickFacesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ConsolidatedPalletsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ConsolidatedPalletsCalcEdit, "ConsolidatedPallets");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).ConsolidatedPallets)));
			this.ConsolidatedPalletsCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|dc5ef03a-c4d2-4d70-8ef9-9f02a3f73ef5", "Consolidated Pallets");
			this.ConsolidatedPalletsCalcEdit.DecimalPlaces = 0;
			this.ConsolidatedPalletsCalcEdit.Decimals = 0;
			this.ConsolidatedPalletsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 93, true);
			this.ConsolidatedPalletsCalcEdit.Name = "ConsolidatedPalletsCalcEdit";
			this.ConsolidatedPalletsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ConsolidatedPalletsCalcEdit.TabIndex = 3;
			this.ConsolidatedPalletsCalcEdit.Text = "0";
			this.ConsolidatedPalletsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FifoFallbackCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FifoFallbackCalcEdit, "FifoOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).FifoOption)));
			this.FifoFallbackCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|51d87a62-2cc1-4964-962a-fe0894310c33", "FIFO Fallback");
			this.FifoFallbackCalcEdit.DecimalPlaces = 0;
			this.FifoFallbackCalcEdit.Decimals = 0;
			this.FifoFallbackCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 41, true);
			this.FifoFallbackCalcEdit.Name = "FifoFallbackCalcEdit";
			this.FifoFallbackCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.FifoFallbackCalcEdit.TabIndex = 1;
			this.FifoFallbackCalcEdit.Text = "0";
			this.FifoFallbackCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FullPalletsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FullPalletsCalcEdit, "FullPallets");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).FullPallets)));
			this.FullPalletsCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|21ee2e11-5895-4fa8-93f6-80b0ca0beb22", "Full Pallets");
			this.FullPalletsCalcEdit.DecimalPlaces = 0;
			this.FullPalletsCalcEdit.Decimals = 0;
			this.FullPalletsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 67, true);
			this.FullPalletsCalcEdit.Name = "FullPalletsCalcEdit";
			this.FullPalletsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.FullPalletsCalcEdit.TabIndex = 2;
			this.FullPalletsCalcEdit.Text = "0";
			this.FullPalletsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PalletOverflowCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PalletOverflowCalcEdit, "PalletOverflow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).PalletOverflow)));
			this.PalletOverflowCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|4abae921-ebca-46e7-a96c-22414a73c91c", "Pallet Overflow");
			this.PalletOverflowCalcEdit.DecimalPlaces = 0;
			this.PalletOverflowCalcEdit.Decimals = 0;
			this.PalletOverflowCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 171, true);
			this.PalletOverflowCalcEdit.Name = "PalletOverflowCalcEdit";
			this.PalletOverflowCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.PalletOverflowCalcEdit.TabIndex = 7;
			this.PalletOverflowCalcEdit.Text = "0";
			this.PalletOverflowCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BrokenPalletsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BrokenPalletsCalcEdit, "BrokenPallets");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).BrokenPallets)));
			this.BrokenPalletsCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|a6e53229-8cef-4e66-bad0-d8e53a216db1", "Broken Pallets");
			this.BrokenPalletsCalcEdit.DecimalPlaces = 0;
			this.BrokenPalletsCalcEdit.Decimals = 0;
			this.BrokenPalletsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 197, true);
			this.BrokenPalletsCalcEdit.Name = "BrokenPalletsCalcEdit";
			this.BrokenPalletsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.BrokenPalletsCalcEdit.TabIndex = 8;
			this.BrokenPalletsCalcEdit.Text = "0";
			this.BrokenPalletsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IsPickfaceEmptyPreventPickingFromBulkCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsPickfaceEmptyPreventPickingFromBulkCheckBox, "IsPickfaceEmptyPreventPickingFromBulk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).IsPickfaceEmptyPreventPickingFromBulk)));
			this.IsPickfaceEmptyPreventPickingFromBulkCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4a5de7a4-f613-492b-b286-eb639774de35", "If Fixed Pickface empty prevent picking from bulk");
			this.IsPickfaceEmptyPreventPickingFromBulkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPickfaceEmptyPreventPickingFromBulkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 129, true);
			this.IsPickfaceEmptyPreventPickingFromBulkCheckBox.Name = "IsPickfaceEmptyPreventPickingFromBulkCheckBox";
			this.IsPickfaceEmptyPreventPickingFromBulkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 59, true);
			this.IsPickfaceEmptyPreventPickingFromBulkCheckBox.TabIndex = 6;
			this.IsPickfaceEmptyPreventPickingFromBulkCheckBox.UseVisualStyleBackColor = true;
			// 
			// LocationSortOrderGroupBox
			// 
			this.LocationSortOrderGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|4c3f635f-798d-43d9-abd9-25625c670f8a", "Location Sort");
			this.LocationSortOrderGroupBox.Controls.Add(this.LevelCalcEdit);
			this.LocationSortOrderGroupBox.Controls.Add(this.RowCalcEdit);
			this.LocationSortOrderGroupBox.Controls.Add(this.ColumnCalcEdit);
			this.LocationSortOrderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 231, true);
			this.LocationSortOrderGroupBox.Name = "LocationSortOrderGroupBox";
			this.LocationSortOrderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 96, true);
			this.LocationSortOrderGroupBox.TabIndex = 1;
			this.LocationSortOrderGroupBox.TabStop = false;
			// 
			// LevelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LevelCalcEdit, "Level");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).Level)));
			this.LevelCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|776d2694-ee45-4a45-b709-39222f3ae5a1", "Level");
			this.LevelCalcEdit.DecimalPlaces = 0;
			this.LevelCalcEdit.Decimals = 0;
			this.LevelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 70, true);
			this.LevelCalcEdit.Name = "LevelCalcEdit";
			this.LevelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.LevelCalcEdit.TabIndex = 3;
			this.LevelCalcEdit.Text = "0";
			this.LevelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RowCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RowCalcEdit, "Row");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).Row)));
			this.RowCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|f082d48c-e498-4b33-8472-85fd4767a2a0", "Row");
			this.RowCalcEdit.DecimalPlaces = 0;
			this.RowCalcEdit.Decimals = 0;
			this.RowCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 18, true);
			this.RowCalcEdit.Name = "RowCalcEdit";
			this.RowCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.RowCalcEdit.TabIndex = 1;
			this.RowCalcEdit.Text = "0";
			this.RowCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ColumnCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ColumnCalcEdit, "Column");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PickingSequence)(null)).Column)));
			this.ColumnCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PickingSequenceControl|05263bf2-923d-4fdc-917b-2caee1cc3c82", "Column");
			this.ColumnCalcEdit.DecimalPlaces = 0;
			this.ColumnCalcEdit.Decimals = 0;
			this.ColumnCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 44, true);
			this.ColumnCalcEdit.Name = "ColumnCalcEdit";
			this.ColumnCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ColumnCalcEdit.TabIndex = 2;
			this.ColumnCalcEdit.Text = "0";
			this.ColumnCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PickingSequenceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationSortOrderGroupBox);
			this.Controls.Add(this.PickAlgorithmSequenceGroupBox);
			this.Name = "PickingSequenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 339, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickAlgorithmSequenceGroupBox.ResumeLayout(false);
			this.PickAlgorithmSequenceGroupBox.PerformLayout();
			this.LocationSortOrderGroupBox.ResumeLayout(false);
			this.LocationSortOrderGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox PickAlgorithmSequenceGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit PickFacesCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ConsolidatedPalletsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit FullPalletsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit FifoFallbackCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PalletOverflowCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit BrokenPalletsCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LocationSortOrderGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit LevelCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RowCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ColumnCalcEdit;
		private ZArchitecture.GUI.ZCheckBox IsPickfaceEmptyPreventPickingFromBulkCheckBox;
		private ZArchitecture.ZCalcEdit ExpiryDateCalcEdit;
		private ZArchitecture.ZCalcEdit HighPriorityLocationsCalcEdit;
	}
}
