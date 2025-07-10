namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class LiabilityDetailsUserControl
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
			this.CountryOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommodityCodeTariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.SupplementaryUnitsCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsThirdQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsFourthQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.AdditionalSupplementaryCodesUserControl = new Enterprise.Customs.EU.NCTS.GUI.AdditionalSupplementaryCodesUserControl();
			this.FeesUserControl = new Enterprise.Customs.EU.NCTS.GUI.FeesGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryOfOriginDropEdit.SuspendLayout();
			this.CommodityCodeTariffFindBox.SuspendLayout();
			this.SupplementaryUnitsCalcDropEdit.SuspendLayout();
			this.CustomsThirdQuantityDropEdit.SuspendLayout();
			this.CustomsFourthQuantityDropEdit.SuspendLayout();
			this.CustomsValueCalcDropEdit.SuspendLayout();
			this.AdditionalSupplementaryCodesUserControl.SuspendLayout();
			this.FeesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc);
			// CountryOfOriginDropEdit
			// 
			this.CountryOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginDropEdit, "BY_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_RN_NKCountryOfOrigin)));
			this.CountryOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 374, true);
			this.CountryOfOriginDropEdit.Name = "CountryOfOriginDropEdit";
			this.CountryOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CountryOfOriginDropEdit.TabIndex = 13;
			// 
			// CommodityCodeTariffFindBox
			// 
			this.CommodityCodeTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeTariffFindBox, "LiabilityFormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).LiabilityFormattedTariff)));
			this.CommodityCodeTariffFindBox.ErrorForUnsupportedCountry = null;
			this.CommodityCodeTariffFindBox.GetEffectiveDate = null;
			this.CommodityCodeTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 166, true);
			this.CommodityCodeTariffFindBox.Name = "CommodityCodeTariffFindBox";
			this.CommodityCodeTariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CommodityCodeTariffFindBox.ParentType = null;
			this.CommodityCodeTariffFindBox.PreBoundMaxLength = 8;
			this.CommodityCodeTariffFindBox.SelectNomenclatureModes = null;
			this.CommodityCodeTariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.CommodityCodeTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CommodityCodeTariffFindBox.TabIndex = 5;
			this.CommodityCodeTariffFindBox.TariffType = null;
			// 
			// SupplementaryUnitsCalcDropEdit
			// 
			this.SupplementaryUnitsCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryUnitsCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_CustomsSecondUnitQty)));
			this.SupplementaryUnitsCalcDropEdit.BindToAmount = "BY_CustomsSecondQuantity";
			this.SupplementaryUnitsCalcDropEdit.BindToUnit = "BY_CustomsSecondUnitQty";
			this.SupplementaryUnitsCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 139, true);
			this.SupplementaryUnitsCalcDropEdit.Name = "SupplementaryUnitsCalcDropEdit";
			this.SupplementaryUnitsCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.SupplementaryUnitsCalcDropEdit.TabIndex = 4;
			this.SupplementaryUnitsCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CustomsThirdQuantityDropEdit
			// 
			this.CustomsThirdQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsThirdQuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_CustomsThirdUnitQty)));
			this.CustomsThirdQuantityDropEdit.BindToAmount = "BY_CustomsThirdQuantity";
			this.CustomsThirdQuantityDropEdit.BindToUnit = "BY_CustomsThirdUnitQty";
			this.CustomsThirdQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 113, true);
			this.CustomsThirdQuantityDropEdit.Name = "CustomsThirdQuantityDropEdit";
			this.CustomsThirdQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsThirdQuantityDropEdit.TabIndex = 17;
			this.CustomsThirdQuantityDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// CustomsFourthQuantityDropEdit
			// 
			this.CustomsFourthQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsFourthQuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_CustomsFourthQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_CustomsFourthUnitQty)));
			this.CustomsFourthQuantityDropEdit.BindToAmount = "BY_CustomsFourthQuantity";
			this.CustomsFourthQuantityDropEdit.BindToUnit = "BY_CustomsFourthUnitQty";
			this.CustomsFourthQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 113, true);
			this.CustomsFourthQuantityDropEdit.Name = "CustomsFourthQuantityDropEdit";
			this.CustomsFourthQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsFourthQuantityDropEdit.TabIndex = 17;
			this.CustomsFourthQuantityDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// CustomsValueCalcDropEdit
			// 
			this.CustomsValueCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_MonetaryValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_RX_NKCurrency)));
			this.CustomsValueCalcDropEdit.BindToAmount = "BY_MonetaryValue";
			this.CustomsValueCalcDropEdit.BindToUnit = "BY_RX_NKCurrency";
			this.CustomsValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 139, true);
			this.CustomsValueCalcDropEdit.Name = "CustomsValueCalcDropEdit";
			this.CustomsValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsValueCalcDropEdit.TabIndex = 18;
			this.CustomsValueCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// AdditionalSupplementaryCodesUserControl
			// 
			this.AdditionalSupplementaryCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesUserControl, ".");
			this.AdditionalSupplementaryCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 191, true);
			this.AdditionalSupplementaryCodesUserControl.Name = "AdditionalSupplementaryCodesUserControl";
			this.AdditionalSupplementaryCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 20, true);
			this.AdditionalSupplementaryCodesUserControl.TabIndex = 25;
			// 
			// FeesControl
			//
			this.BindingSource.SetBindingMember(this.FeesUserControl, ".");
			this.FeesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(825, 223, true);
			this.FeesUserControl.Name = "FeesUserControl";
			this.FeesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 135, true);
			this.FeesUserControl.TabIndex = 27;
			// 
			// LiabilityDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalSupplementaryCodesUserControl);
			this.Controls.Add(this.FeesUserControl);
			this.Controls.Add(this.CustomsValueCalcDropEdit);
			this.Controls.Add(this.CustomsFourthQuantityDropEdit);
			this.Controls.Add(this.CustomsThirdQuantityDropEdit);
			this.Controls.Add(this.SupplementaryUnitsCalcDropEdit);
			this.Controls.Add(this.CommodityCodeTariffFindBox);
			this.Controls.Add(this.CountryOfOriginDropEdit);
			this.Name = "LiabilityDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 407, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryOfOriginDropEdit.ResumeLayout(true);
			this.CountryOfOriginDropEdit.PerformLayout();
			this.CommodityCodeTariffFindBox.ResumeLayout(true);
			this.CommodityCodeTariffFindBox.PerformLayout();
			this.SupplementaryUnitsCalcDropEdit.ResumeLayout(true);
			this.SupplementaryUnitsCalcDropEdit.PerformLayout();
			this.CustomsThirdQuantityDropEdit.ResumeLayout(true);
			this.CustomsThirdQuantityDropEdit.PerformLayout();
			this.CustomsFourthQuantityDropEdit.ResumeLayout(true);
			this.CustomsFourthQuantityDropEdit.PerformLayout();
			this.CustomsValueCalcDropEdit.ResumeLayout(true);
			this.CustomsValueCalcDropEdit.PerformLayout();
			this.AdditionalSupplementaryCodesUserControl.ResumeLayout(true);
			this.AdditionalSupplementaryCodesUserControl.PerformLayout();
			this.FeesUserControl.ResumeLayout(false);
			this.FeesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit CountryOfOriginDropEdit;
		internal Universal.GUI.TariffFindBox CommodityCodeTariffFindBox;
		internal ZArchitecture.GUI.ZCalcDropEdit SupplementaryUnitsCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsThirdQuantityDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsFourthQuantityDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsValueCalcDropEdit;
		internal AdditionalSupplementaryCodesUserControl AdditionalSupplementaryCodesUserControl;
		internal FeesGridUserControl FeesUserControl;

	}
}
