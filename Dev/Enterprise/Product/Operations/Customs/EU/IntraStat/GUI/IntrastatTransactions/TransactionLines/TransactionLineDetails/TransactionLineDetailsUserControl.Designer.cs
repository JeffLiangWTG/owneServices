using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	partial class TransactionLineDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.CountryOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceValueDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.StatisticalValueDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.MassDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SupplementaryUnitsCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryOfOriginDropEdit.SuspendLayout();
			this.RegionDropEdit.SuspendLayout();
			this.InvoiceValueDropEdit.SuspendLayout();
			this.StatisticalValueDropEdit.SuspendLayout();
			this.MassDropEdit.SuspendLayout();
			this.SupplementaryUnitsCalcDropEdit.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine);
			// 
			// CountryOfOriginDropEdit
			// 
			this.CountryOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginDropEdit, "CIL_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_RN_NKCountryOfOrigin)));
			this.CountryOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 181, true);
			this.CountryOfOriginDropEdit.Name = "CountryOfOriginDropEdit";
			this.CountryOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CountryOfOriginDropEdit.TabIndex = 7;
			// 
			// RegionDropEdit
			// 
			this.RegionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegionDropEdit, "CIL_Region");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_Region)));
			this.RegionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 207, true);
			this.RegionDropEdit.Name = "RegionDropEdit";
			this.RegionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.RegionDropEdit.TabIndex = 8;
			// 
			// DescriptionOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionOfGoodsTextBox, "CIL_DescriptionOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_DescriptionOfGoods)));
			this.DescriptionOfGoodsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 25, true);
			this.DescriptionOfGoodsTextBox.Name = "DescriptionOfGoodsTextBox";
			this.DescriptionOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.DescriptionOfGoodsTextBox.TabIndex = 1;
			// 
			// InvoiceValueDropEdit
			// 
			this.InvoiceValueDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceValueDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_InvoiceValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_RX_NKCurrency)));
			this.InvoiceValueDropEdit.BindToAmount = "CIL_InvoiceValue";
			this.InvoiceValueDropEdit.BindToUnit = "CIL_RX_NKCurrency";
			this.InvoiceValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 77, true);
			this.InvoiceValueDropEdit.Name = "InvoiceValueDropEdit";
			this.InvoiceValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.InvoiceValueDropEdit.TabIndex = 3;
			this.InvoiceValueDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// StatisticalValueDropEdit
			// 
			this.StatisticalValueDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatisticalValueDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_StatisticalValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_RX_NKCurrency)));
			this.StatisticalValueDropEdit.BindToAmount = "CIL_StatisticalValue";
			this.StatisticalValueDropEdit.BindToUnit = "CIL_RX_NKCurrency";
			this.StatisticalValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 103, true);
			this.StatisticalValueDropEdit.Name = "StatisticalValueDropEdit";
			this.StatisticalValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.StatisticalValueDropEdit.TabIndex = 4;
			this.StatisticalValueDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// MassDropEdit
			// 
			this.MassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MassDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_MassInKilograms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_MassInKilogramsUnit)));
			this.MassDropEdit.BindToAmount = "CIL_MassInKilograms";
			this.MassDropEdit.BindToUnit = "CIL_MassInKilogramsUnit";
			this.MassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 129, true);
			this.MassDropEdit.Name = "MassDropEdit";
			this.MassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.MassDropEdit.TabIndex = 5;
			this.MassDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// SupplementaryUnitsCalcDropEdit
			// 
			this.SupplementaryUnitsCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryUnitsCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_SupplementaryQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_SupplementaryQuantityUnit)));
			this.SupplementaryUnitsCalcDropEdit.BindToAmount = "CIL_SupplementaryQuantity";
			this.SupplementaryUnitsCalcDropEdit.BindToUnit = "CIL_SupplementaryQuantityUnit";
			this.SupplementaryUnitsCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 155, true);
			this.SupplementaryUnitsCalcDropEdit.Name = "SupplementaryUnitsCalcDropEdit";
			this.SupplementaryUnitsCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.SupplementaryUnitsCalcDropEdit.TabIndex = 6;
			this.SupplementaryUnitsCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "CIL_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine)(null)).CIL_Tariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 51, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.PreBoundMaxLength = 12;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.TariffFindBox.TabIndex = 2;
			this.TariffFindBox.TariffType = "EXP";
			// 
			// TransactionLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceValueDropEdit);
			this.Controls.Add(this.StatisticalValueDropEdit);
			this.Controls.Add(this.MassDropEdit);
			this.Controls.Add(this.DescriptionOfGoodsTextBox);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.CountryOfOriginDropEdit);
			this.Controls.Add(this.RegionDropEdit);
			this.Controls.Add(this.SupplementaryUnitsCalcDropEdit);
			this.Name = "TransactionLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 459, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryOfOriginDropEdit.ResumeLayout(true);
			this.CountryOfOriginDropEdit.PerformLayout();
			this.RegionDropEdit.ResumeLayout(true);
			this.RegionDropEdit.PerformLayout();
			this.InvoiceValueDropEdit.ResumeLayout(true);
			this.InvoiceValueDropEdit.PerformLayout();
			this.StatisticalValueDropEdit.ResumeLayout(true);
			this.StatisticalValueDropEdit.PerformLayout();
			this.MassDropEdit.ResumeLayout(true);
			this.MassDropEdit.PerformLayout();
			this.SupplementaryUnitsCalcDropEdit.ResumeLayout(true);
			this.SupplementaryUnitsCalcDropEdit.PerformLayout();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.ZTextBox DescriptionOfGoodsTextBox;
		internal Universal.GUI.TariffFindBox TariffFindBox;
		internal ZArchitecture.GUI.ZDropEdit CountryOfOriginDropEdit;
		internal ZArchitecture.GUI.ZDropEdit RegionDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit InvoiceValueDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit StatisticalValueDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit MassDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit SupplementaryUnitsCalcDropEdit;

	}
}
