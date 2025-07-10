using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class JPManifestBillSpecificUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FinalDestinationUserControl = new Enterprise.Customs.JP.Manifest.GUI.FinalDestinationUserControl();
			this.GoodsLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SpecialCargoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CargoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.RepresentativeHSCodeFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsNetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FinalDestinationUserControl.SuspendLayout();
			this.GoodsLocationCodeFindBox.SuspendLayout();
			this.SpecialCargoCodeFindBox.SuspendLayout();
			this.CargoTypeDropEdit.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.RepresentativeHSCodeFindBox.SuspendLayout();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.CustomsWeightCalcDropEdit.SuspendLayout();
			this.CustomsNetWeightCalcDropEdit.SuspendLayout();
			this.CustomsVolumeCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaBill);
			// 
			// FinalDestinationUserControl
			// 
			this.FinalDestinationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationUserControl, ".");
			this.FinalDestinationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FinalDestinationUserControl.Name = "FinalDestinationUserControl";
			this.FinalDestinationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.FinalDestinationUserControl.CaptionRenderingEnabled = true;
			this.FinalDestinationUserControl.TabIndex = 0;
			// 
			// GoodsLocationCodeFindBox
			// 
			this.GoodsLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationCodeFindBox, "ABL_GoodsLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_GoodsLocation)));
			this.GoodsLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.GoodsLocationCodeFindBox.Name = "GoodsLocationCodeFindBox";
			this.GoodsLocationCodeFindBox.ParentType = null;
			this.GoodsLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.GoodsLocationCodeFindBox.TabIndex = 0;
			// 
			// SpecialCargoCodeFindBox
			// 
			this.SpecialCargoCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialCargoCodeFindBox, "ABL_SpecialCargoCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_SpecialCargoCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.SpecialCargoCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.SpecialCargoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 52, true);
			this.SpecialCargoCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.SpecialCargoCodeFindBox.Name = "SpecialCargoCodeFindBox";
			this.SpecialCargoCodeFindBox.ParentType = null;
			this.SpecialCargoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.SpecialCargoCodeFindBox.TabIndex = 0;
			// 
			// CargoTypeDropEdit
			// 
			this.CargoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoTypeDropEdit, "ABL_CargoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_CargoType)));
			this.CargoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CargoTypeDropEdit.Name = "CargoTypeDropEdit";
			this.CargoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CargoTypeDropEdit.TabIndex = 1;
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "ABL_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_Tariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetDataGrouping = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.GetTariffType = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 78, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.NeedLoadNomenclatureWhenTariffNotFound = false;
			this.TariffFindBox.NeedLoadParentDataGroup = true;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.PreBoundMaxLength = 10;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.TariffFindBox.TabIndex = 11;
			this.TariffFindBox.TariffType = null;
			// 
			// RepresentativeHSCodeFindBox
			// 
			this.RepresentativeHSCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeHSCodeFindBox, "ABL_RepresentativeHSCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_RepresentativeHSCode)));
			this.RepresentativeHSCodeFindBox.ErrorForUnsupportedCountry = null;
			this.RepresentativeHSCodeFindBox.GetDataGrouping = null;
			this.RepresentativeHSCodeFindBox.GetEffectiveDate = null;
			this.RepresentativeHSCodeFindBox.GetTariffType = null;
			this.RepresentativeHSCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 234, true);
			this.RepresentativeHSCodeFindBox.Name = "RepresentativeHSCodeFindBox";
			this.RepresentativeHSCodeFindBox.NeedLoadNomenclatureWhenTariffNotFound = false;
			this.RepresentativeHSCodeFindBox.NeedLoadParentDataGroup = true;
			this.RepresentativeHSCodeFindBox.ParentType = null;
			this.RepresentativeHSCodeFindBox.PreBoundMaxLength = 10;
			this.RepresentativeHSCodeFindBox.SelectNomenclatureModes = null;
			this.RepresentativeHSCodeFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.RepresentativeHSCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.RepresentativeHSCodeFindBox.TabIndex = 12;
			this.RepresentativeHSCodeFindBox.TariffType = null;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "ABL_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_CountryOfOrigin)));
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 61, true);
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.GoodsOriginCodeFindBox.TabIndex = 3;
			// 
			// CustomsWeightCalcDropEdit
			// 
			this.CustomsWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).CustomsWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).CustomsWeightUQ)));
			this.CustomsWeightCalcDropEdit.BindToAmount = "CustomsWeight";
			this.CustomsWeightCalcDropEdit.BindToUnit = "CustomsWeightUQ";
			this.CustomsWeightCalcDropEdit.Decimals = 3;
			this.CustomsWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 104, true);
			this.CustomsWeightCalcDropEdit.Name = "CustomsWeightCalcDropEdit";
			this.CustomsWeightCalcDropEdit.ShowDescriptionBox = true;
			this.CustomsWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustomsWeightCalcDropEdit.TabIndex = 13;
			this.CustomsWeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CustomsNetWeightCalcDropEdit
			// 
			this.CustomsNetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsNetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).CustomsNetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).CustomsNetWeightUQ)));
			this.CustomsNetWeightCalcDropEdit.BindToAmount = "CustomsNetWeight";
			this.CustomsNetWeightCalcDropEdit.BindToUnit = "CustomsNetWeightUQ";
			this.CustomsNetWeightCalcDropEdit.Decimals = 3;
			this.CustomsNetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 130, true);
			this.CustomsNetWeightCalcDropEdit.Name = "CustomsNetWeightCalcDropEdit";
			this.CustomsNetWeightCalcDropEdit.ShowDescriptionBox = true;
			this.CustomsNetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustomsNetWeightCalcDropEdit.TabIndex = 14;
			this.CustomsNetWeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CustomsVolumeCalcDropEdit
			// 
			this.CustomsVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).CustomsVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).CustomsVolumeUQ)));
			this.CustomsVolumeCalcDropEdit.BindToAmount = "CustomsVolume";
			this.CustomsVolumeCalcDropEdit.BindToUnit = "CustomsVolumeUQ";
			this.CustomsVolumeCalcDropEdit.Decimals = 3;
			this.CustomsVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 156, true);
			this.CustomsVolumeCalcDropEdit.Name = "CustomsVolumeCalcDropEdit";
			this.CustomsVolumeCalcDropEdit.ShowDescriptionBox = true;
			this.CustomsVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustomsVolumeCalcDropEdit.TabIndex = 15;
			this.CustomsVolumeCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JPManifestBillSpecificUserControl
			// 
			this.Controls.Add(this.CustomsVolumeCalcDropEdit);
			this.Controls.Add(this.CustomsNetWeightCalcDropEdit);
			this.Controls.Add(this.CustomsWeightCalcDropEdit);
			this.Controls.Add(this.FinalDestinationUserControl);
			this.Controls.Add(this.RepresentativeHSCodeFindBox);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.GoodsLocationCodeFindBox);
			this.Controls.Add(this.SpecialCargoCodeFindBox);
			this.Controls.Add(this.CargoTypeDropEdit);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Name = "JPManifestBillSpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 354, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FinalDestinationUserControl.ResumeLayout(true);
			this.FinalDestinationUserControl.PerformLayout();
			this.GoodsLocationCodeFindBox.ResumeLayout(true);
			this.GoodsLocationCodeFindBox.PerformLayout();
			this.SpecialCargoCodeFindBox.ResumeLayout(true);
			this.SpecialCargoCodeFindBox.PerformLayout();
			this.CargoTypeDropEdit.ResumeLayout(true);
			this.CargoTypeDropEdit.PerformLayout();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.RepresentativeHSCodeFindBox.ResumeLayout(true);
			this.RepresentativeHSCodeFindBox.PerformLayout();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.CustomsWeightCalcDropEdit.ResumeLayout(true);
			this.CustomsWeightCalcDropEdit.PerformLayout();
			this.CustomsNetWeightCalcDropEdit.ResumeLayout(true);
			this.CustomsNetWeightCalcDropEdit.PerformLayout();
			this.CustomsVolumeCalcDropEdit.ResumeLayout(true);
			this.CustomsVolumeCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		FinalDestinationUserControl FinalDestinationUserControl;
		ZArchitecture.GUI.ZCodeFindBox GoodsLocationCodeFindBox;
		ZArchitecture.GUI.ZCodeFindBox SpecialCargoCodeFindBox;
		ZDropEdit CargoTypeDropEdit;
		ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
		Universal.GUI.TariffFindBox TariffFindBox;
		Universal.GUI.TariffFindBox RepresentativeHSCodeFindBox;
		private ZCalcDropEdit CustomsWeightCalcDropEdit;
		private ZCalcDropEdit CustomsNetWeightCalcDropEdit;
		private ZCalcDropEdit CustomsVolumeCalcDropEdit;
	}
}
