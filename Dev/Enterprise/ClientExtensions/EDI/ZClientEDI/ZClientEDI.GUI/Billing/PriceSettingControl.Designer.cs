namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class PriceSettingControl
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
			this.priceEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.priceItemCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.licenceUnitsBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.enableUnitsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.categoryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.applyDiscountsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.priceItemCodeDropEdit.SuspendLayout();
			this.categoryCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.PriceLicenceSetting);
			// 
			// priceEdit
			// 
			this.BindingSource.SetBindingMember(this.priceEdit, "LS9_Price");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.PriceLicenceSetting)(null)).LS9_Price)));
			this.priceEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b89b61ed-caaf-4e4e-91da-19db9a3bf262", "Price");
			this.priceEdit.DecimalPlaces = 2;
			this.priceEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 59, true);
			this.priceEdit.Name = "priceEdit";
			this.priceEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.priceEdit.TabIndex = 2;
			this.priceEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// priceItemCodeDropEdit
			// 
			this.priceItemCodeDropEdit.AllowDrop = true;
			this.priceItemCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.priceItemCodeDropEdit, "PriceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.PriceLicenceSetting)(null)).PriceCode)));
			this.priceItemCodeDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d965f99c-11bb-4317-983a-687e03459991", "Feature");
			this.priceItemCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 31, true);
			this.priceItemCodeDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.priceItemCodeDropEdit.MaxItemsToShowInDropDown = 40;
			this.priceItemCodeDropEdit.Name = "priceItemCodeDropEdit";
			this.priceItemCodeDropEdit.PreBoundMaxLength = 3;
			this.priceItemCodeDropEdit.ShouldResizeByMaxLength = true;
			this.priceItemCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 20, true);
			this.priceItemCodeDropEdit.TabIndex = 1;
			// 
			// licenceUnitsBox
			// 
			this.BindingSource.SetBindingMember(this.licenceUnitsBox, "UnitsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.PriceLicenceSetting)(null)).UnitsForBinding)));
			this.licenceUnitsBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d39c6ea3-dc48-4946-8171-c7509bb679d9", "License Units");
			this.licenceUnitsBox.DecimalPlaces = 4;
			this.licenceUnitsBox.Decimals = 4;
			this.licenceUnitsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 130, true);
			this.licenceUnitsBox.Name = "licenceUnitsBox";
			this.licenceUnitsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.licenceUnitsBox.TabIndex = 5;
			this.licenceUnitsBox.Text = "0.0000";
			this.licenceUnitsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// enableUnitsCheckBox
			// 
			this.enableUnitsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enableUnitsCheckBox, "EnableUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.PriceLicenceSetting)(null)).EnableUnits)));
			this.enableUnitsCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d40b5b9b-771f-4592-8fec-d0691b757548", "Enable License Units Override", "Unticked means use the value on the price list. Tick to set a specific value for this customer. Tick and set to zero to ensure no licence units apply for this customer.");
			this.enableUnitsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enableUnitsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 110, true);
			this.enableUnitsCheckBox.Name = "enableUnitsCheckBox";
			this.enableUnitsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 17, true);
			this.enableUnitsCheckBox.TabIndex = 4;
			this.enableUnitsCheckBox.UseVisualStyleBackColor = true;
			// 
			// categoryCodeDropEdit
			// 
			this.categoryCodeDropEdit.AllowDrop = true;
			this.categoryCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.categoryCodeDropEdit, "PriceCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.PriceLicenceSetting)(null)).PriceCategory)));
			this.categoryCodeDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("48e5e713-e8c5-493a-a17f-25c5c90fb413", "Category");
			this.categoryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.categoryCodeDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.categoryCodeDropEdit.MaxItemsToShowInDropDown = 40;
			this.categoryCodeDropEdit.Name = "categoryCodeDropEdit";
			this.categoryCodeDropEdit.PreBoundMaxLength = 3;
			this.categoryCodeDropEdit.ShouldResizeByMaxLength = true;
			this.categoryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 20, true);
			this.categoryCodeDropEdit.TabIndex = 0;
			// 
			// applyDiscountsCheckBox
			// 
			this.applyDiscountsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.applyDiscountsCheckBox, "LS9_ApplyDiscounts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.PriceLicenceSetting)(null)).LS9_ApplyDiscounts)));
			this.applyDiscountsCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("6a2b1576-cb42-42ec-91c9-02a962737501", "Apply Discounts", "Unticked means no discounts apply to this item. The given Price will be final price. ");
			this.applyDiscountsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.applyDiscountsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 89, true);
			this.applyDiscountsCheckBox.Name = "applyDiscountsCheckBox";
			this.applyDiscountsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.applyDiscountsCheckBox.TabIndex = 3;
			this.applyDiscountsCheckBox.UseVisualStyleBackColor = true;
			// 
			// PriceSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("bfbfc77d-8b16-41f3-9e61-f695c5a28e89", "Apply Discounts");
			this.Controls.Add(this.applyDiscountsCheckBox);
			this.Controls.Add(this.categoryCodeDropEdit);
			this.Controls.Add(this.enableUnitsCheckBox);
			this.Controls.Add(this.licenceUnitsBox);
			this.Controls.Add(this.priceItemCodeDropEdit);
			this.Controls.Add(this.priceEdit);
			this.Name = "PriceSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.priceItemCodeDropEdit.ResumeLayout(true);
			this.priceItemCodeDropEdit.PerformLayout();
			this.categoryCodeDropEdit.ResumeLayout(true);
			this.categoryCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit priceEdit;
		private ZArchitecture.GUI.ZDropEdit priceItemCodeDropEdit;
		private ZArchitecture.ZCalcEdit licenceUnitsBox;
		private ZArchitecture.GUI.ZCheckBox enableUnitsCheckBox;
		private ZArchitecture.GUI.ZDropEdit categoryCodeDropEdit;
		private ZArchitecture.GUI.ZCheckBox applyDiscountsCheckBox;
	}
}
