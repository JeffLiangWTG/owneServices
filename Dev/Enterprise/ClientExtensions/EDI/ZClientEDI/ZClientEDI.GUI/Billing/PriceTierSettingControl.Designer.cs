namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class PriceTierSettingControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.priceItemCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.enableUnitsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.categoryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.applyDiscountsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.priceItemCodeDropEdit.SuspendLayout();
			this.categoryCodeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting);
			// 
			// priceItemCodeDropEdit
			// 
			this.priceItemCodeDropEdit.AllowDrop = true;
			this.priceItemCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.priceItemCodeDropEdit, "PriceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting)(null)).PriceCode)));
			this.priceItemCodeDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d965f99c-11bb-4317-983a-687e03459991", "Feature");
			this.priceItemCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 31, true);
			this.priceItemCodeDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.priceItemCodeDropEdit.MaxItemsToShowInDropDown = 40;
			this.priceItemCodeDropEdit.Name = "priceItemCodeDropEdit";
			this.priceItemCodeDropEdit.PreBoundMaxLength = 3;
			this.priceItemCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.priceItemCodeDropEdit.TabIndex = 1;
			// 
			// enableUnitsCheckBox
			// 
			this.enableUnitsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enableUnitsCheckBox, "EnableUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting)(null)).EnableUnits)));
			this.enableUnitsCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d40b5b9b-771f-4592-8fec-d0691b757548", "Enable License Units Override", "Unticked means use the value on the price list. Tick to set a specific value for " +
        "this customer. Tick and set to zero to ensure no licence units apply for this cu" +
        "stomer.");
			this.enableUnitsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 83, true);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting)(null)).PriceCategory)));
			this.categoryCodeDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("48e5e713-e8c5-493a-a17f-25c5c90fb413", "Category");
			this.categoryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.categoryCodeDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.categoryCodeDropEdit.MaxItemsToShowInDropDown = 40;
			this.categoryCodeDropEdit.Name = "categoryCodeDropEdit";
			this.categoryCodeDropEdit.PreBoundMaxLength = 3;
			this.categoryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.categoryCodeDropEdit.TabIndex = 0;
			// 
			// applyDiscountsCheckBox
			// 
			this.applyDiscountsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.applyDiscountsCheckBox, "LS9_ApplyDiscounts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting)(null)).LS9_ApplyDiscounts)));
			this.applyDiscountsCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("6a2b1576-cb42-42ec-91c9-02a962737501", "Apply Discounts", "Unticked means no discounts apply to this item. The given Price will be final pri" +
        "ce. ");
			this.applyDiscountsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 62, true);
			this.applyDiscountsCheckBox.Name = "applyDiscountsCheckBox";
			this.applyDiscountsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.applyDiscountsCheckBox.TabIndex = 3;
			this.applyDiscountsCheckBox.UseVisualStyleBackColor = true;
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.grid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSettingLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting)(null)).Lines)).SyncRoot)).UnitBreak)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSettingLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting)(null)).Lines)).SyncRoot)).Price)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSettingLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.PriceTierLicenceSetting)(null)).Lines)).SyncRoot)).Units)));
			this.grid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "UnitBreak";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Price";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Units";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.GridId = "07e92315-bc5c-449f-9fdb-5cc980e743b0";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "grid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 106, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 285, true);
			this.grid.TabIndex = 5;
			// 
			// PriceTierSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("bfbfc77d-8b16-41f3-9e61-f695c5a28e89", "Apply Discounts");
			this.Controls.Add(this.grid);
			this.Controls.Add(this.applyDiscountsCheckBox);
			this.Controls.Add(this.categoryCodeDropEdit);
			this.Controls.Add(this.enableUnitsCheckBox);
			this.Controls.Add(this.priceItemCodeDropEdit);
			this.Name = "PriceTierSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 405, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.priceItemCodeDropEdit.ResumeLayout(true);
			this.priceItemCodeDropEdit.PerformLayout();
			this.categoryCodeDropEdit.ResumeLayout(true);
			this.categoryCodeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZDropEdit priceItemCodeDropEdit;
		private ZArchitecture.GUI.ZCheckBox enableUnitsCheckBox;
		private ZArchitecture.GUI.ZDropEdit categoryCodeDropEdit;
		private ZArchitecture.GUI.ZCheckBox applyDiscountsCheckBox;
		private ZArchitecture.ZGrid grid;
	}
}
