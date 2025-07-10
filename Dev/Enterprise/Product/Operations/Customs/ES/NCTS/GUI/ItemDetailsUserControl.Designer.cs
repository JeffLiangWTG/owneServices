namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class ItemDetailsUserControl
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
			this.IsVehiclesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ItemDetailsGroupBox.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.OriginCountryDropEdit.SuspendLayout();
			this.CountryOfDispatchDropEdit.SuspendLayout();
			this.ItemConsigneeDocAddressControl.SuspendLayout();
			this.ItemConsignorDocAddressControl.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.CommodityCodeTariffFindBox.SuspendLayout();
			this.FiscalUnitsDropEdit.SuspendLayout();
			this.CustomsValueDropEdit.SuspendLayout();
			this.TaxOrFeeDropEdit.SuspendLayout();
			this.CustomsFirstQtyDropEdit.SuspendLayout();
			this.CustomsThirdQtyDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ItemDetailsGroupBox
			// 
			this.ItemDetailsGroupBox.Controls.Add(this.IsVehiclesCheckBox);
			this.ItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1138, 539, true);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.OriginCountryDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CustomsFirstQtyDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CustomsThirdQtyDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.ItemNumberTextBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDispatchDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.DeclarationTypeDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.DescriptionOfGoodsTextBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.AdditionalSupplementaryCodesEditButton, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.AdditionalSupplementaryCodesTextBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.TaxOrFeeDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CommodityCodeTariffFindBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDestinationDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.ItemConsignorDocAddressControl, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.ItemConsigneeDocAddressControl, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CustomsValueDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.FiscalUnitsDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.IsVehiclesCheckBox, 0);
			// 
			// FiscalUnitsDropEdit
			// 
			this.FiscalUnitsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 299, true);
			// 
			// CustomsValueDropEdit
			// 
			this.CustomsValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 324, true);
			// 
			// TaxOrFeeDropEdit
			// 
			this.TaxOrFeeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 349, true);
			// 
			// FeesGroupBox
			// 
			this.FeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 399, true);
			// 
			// AdditionalSupplementaryCodesTextBox
			// 
			this.AdditionalSupplementaryCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 374, true);
			// 
			// AdditionalSupplementaryCodesEditButton
			// 
			this.AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 374, true);
			// 
			// CustomsThirdQtyDropEdit
			// 
			this.CustomsThirdQtyDropEdit.Visible = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// IsVehiclesCheckBox
			// 
			this.IsVehiclesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsVehiclesCheckBox, "IsVehicles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc)(null)).IsVehicles)));
			this.IsVehiclesCheckBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("DC7AD238-CA75-4AC4-A4AF-D80934AFF5B2", "Vehicles");
			this.IsVehiclesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsVehiclesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 16, true);
			this.IsVehiclesCheckBox.Name = "IsVehiclesCheckBox";
			this.IsVehiclesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.IsVehiclesCheckBox.TabIndex = 2;
			this.IsVehiclesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1138, 579, true);
			this.ItemDetailsGroupBox.ResumeLayout(false);
			this.ItemDetailsGroupBox.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.OriginCountryDropEdit.ResumeLayout(true);
			this.OriginCountryDropEdit.PerformLayout();
			this.CountryOfDispatchDropEdit.ResumeLayout(true);
			this.CountryOfDispatchDropEdit.PerformLayout();
			this.ItemConsigneeDocAddressControl.ResumeLayout(true);
			this.ItemConsigneeDocAddressControl.PerformLayout();
			this.ItemConsignorDocAddressControl.ResumeLayout(true);
			this.ItemConsignorDocAddressControl.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.CommodityCodeTariffFindBox.ResumeLayout(true);
			this.CommodityCodeTariffFindBox.PerformLayout();
			this.FiscalUnitsDropEdit.ResumeLayout(true);
			this.FiscalUnitsDropEdit.PerformLayout();
			this.CustomsValueDropEdit.ResumeLayout(true);
			this.CustomsValueDropEdit.PerformLayout();
			this.TaxOrFeeDropEdit.ResumeLayout(true);
			this.TaxOrFeeDropEdit.PerformLayout();
			this.CustomsFirstQtyDropEdit.ResumeLayout(true);
			this.CustomsFirstQtyDropEdit.PerformLayout();
			this.CustomsThirdQtyDropEdit.ResumeLayout(true);
			this.CustomsThirdQtyDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZCheckBox IsVehiclesCheckBox;
	}
}
