using System.Runtime.InteropServices;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemDifferencesDetailsColumnUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Phase5GoodsItemDifferencesDetailsColumnUserControl));
			this.DeclaredDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnloadedDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclaredValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeclaredGrossWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DeclaredNetWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.UnloadedGrossWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.UnloadedNetWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.UnloadedValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeclaredCommodityCodeCodeFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.UnloadedCommodityCodeCodeFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.DeclaredCusCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.UnloadedCusCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclaredGrossWeightDropEdit.SuspendLayout();
			this.DeclaredNetWeightDropEdit.SuspendLayout();
			this.UnloadedGrossWeightDropEdit.SuspendLayout();
			this.UnloadedNetWeightDropEdit.SuspendLayout();
			this.DeclaredCommodityCodeCodeFindBox.SuspendLayout();
			this.UnloadedCommodityCodeCodeFindBox.SuspendLayout();
			this.DeclaredCusCodeCodeFindBox.SuspendLayout();
			this.UnloadedCusCodeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc);
			// 
			// DeclaredDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclaredDescriptionTextBox, "BY_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_Description)));
			this.DeclaredDescriptionTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("DeclaredDescriptionTextBox.CaptionResourceString")));
			this.DeclaredDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeclaredDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 151, true);
			this.DeclaredDescriptionTextBox.Multiline = true;
			this.DeclaredDescriptionTextBox.Name = "DeclaredDescriptionTextBox";
			this.DeclaredDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 91, true);
			this.DeclaredDescriptionTextBox.TabIndex = 3;
			// 
			// UnloadedDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnloadedDescriptionTextBox, "UnloadedGoodsItem.BY_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.BY_Description)));
			this.UnloadedDescriptionTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("UnloadedDescriptionTextBox.CaptionResourceString")));
			this.UnloadedDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnloadedDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 150, true);
			this.UnloadedDescriptionTextBox.Multiline = true;
			this.UnloadedDescriptionTextBox.Name = "UnloadedDescriptionTextBox";
			this.UnloadedDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 91, true);
			this.UnloadedDescriptionTextBox.TabIndex = 8;
			// 
			// DeclaredValueLabel
			// 
			this.BindingSource.SetBindingMember(this.DeclaredValueLabel, "DeclaredNewLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).DeclaredNewLabel)));
			this.DeclaredValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeclaredValueLabel, false);
			this.DeclaredValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 73, true);
			this.DeclaredValueLabel.Name = "DeclaredValueLabel";
			this.DeclaredValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.DeclaredValueLabel.TabIndex = 0;
			// 
			// DeclaredGrossWeightDropEdit
			// 
			this.DeclaredGrossWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclaredGrossWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_GrossWeightUnit)));
			this.DeclaredGrossWeightDropEdit.BindToAmount = "BY_GrossWeight";
			this.DeclaredGrossWeightDropEdit.BindToUnit = "BY_GrossWeightUnit";
			this.DeclaredGrossWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 248, true);
			this.DeclaredGrossWeightDropEdit.Name = "DeclaredGrossWeightDropEdit";
			this.DeclaredGrossWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 31, true);
			this.DeclaredGrossWeightDropEdit.TabIndex = 4;
			this.DeclaredGrossWeightDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// DeclaredNetWeightDropEdit
			// 
			this.DeclaredNetWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclaredNetWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_NetWeightUnit)));
			this.DeclaredNetWeightDropEdit.BindToAmount = "BY_NetWeight";
			this.DeclaredNetWeightDropEdit.BindToUnit = "BY_NetWeightUnit";
			this.DeclaredNetWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 274, true);
			this.DeclaredNetWeightDropEdit.Name = "DeclaredNetWeightDropEdit";
			this.DeclaredNetWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 31, true);
			this.DeclaredNetWeightDropEdit.TabIndex = 5;
			this.DeclaredNetWeightDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// UnloadedGrossWeightDropEdit
			// 
			this.UnloadedGrossWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedGrossWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.BY_GrossWeightUnit)));
			this.UnloadedGrossWeightDropEdit.BindToAmount = "UnloadedGoodsItem.BY_GrossWeight";
			this.UnloadedGrossWeightDropEdit.BindToUnit = "UnloadedGoodsItem.BY_GrossWeightUnit";
			this.UnloadedGrossWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 247, true);
			this.UnloadedGrossWeightDropEdit.Name = "UnloadedGrossWeightDropEdit";
			this.UnloadedGrossWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 31, true);
			this.UnloadedGrossWeightDropEdit.TabIndex = 9;
			this.UnloadedGrossWeightDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// UnloadedNetWeightDropEdit
			// 
			this.UnloadedNetWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedNetWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.BY_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.BY_NetWeightUnit)));
			this.UnloadedNetWeightDropEdit.BindToAmount = "UnloadedGoodsItem.BY_NetWeight";
			this.UnloadedNetWeightDropEdit.BindToUnit = "UnloadedGoodsItem.BY_NetWeightUnit";
			this.UnloadedNetWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 273, true);
			this.UnloadedNetWeightDropEdit.Name = "UnloadedNetWeightDropEdit";
			this.UnloadedNetWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 31, true);
			this.UnloadedNetWeightDropEdit.TabIndex = 10;
			this.UnloadedNetWeightDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// UnloadedValueLabel
			// 
			this.UnloadedValueLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("36dba5eb-c088-489d-851b-26120d607879", "Unloaded Value");
			this.UnloadedValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnloadedValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 72, true);
			this.UnloadedValueLabel.Name = "UnloadedValueLabel";
			this.UnloadedValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.UnloadedValueLabel.TabIndex = 0;
			// 
			// DeclaredCommodityCodeCodeFindBox
			// 
			this.DeclaredCommodityCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclaredCommodityCodeCodeFindBox, "BY_HarmonisedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_HarmonisedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UniversalTariffDescription)));
			this.DeclaredCommodityCodeCodeFindBox.ErrorForUnsupportedCountry = null;
			this.DeclaredCommodityCodeCodeFindBox.GetEffectiveDate = null;
			this.DeclaredCommodityCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 99, true);
			this.DeclaredCommodityCodeCodeFindBox.Name = "DeclaredCommodityCodeCodeFindBox";
			this.DeclaredCommodityCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DeclaredCommodityCodeCodeFindBox.ParentType = null;
			this.DeclaredCommodityCodeCodeFindBox.PreBoundMaxLength = 8;
			this.DeclaredCommodityCodeCodeFindBox.SelectNomenclatureModes = null;
			this.DeclaredCommodityCodeCodeFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.DeclaredCommodityCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 31, true);
			this.DeclaredCommodityCodeCodeFindBox.TabIndex = 1;
			this.DeclaredCommodityCodeCodeFindBox.TariffType = null;
			this.DeclaredCommodityCodeCodeFindBox.BindToForDescription = "UniversalTariffDescription";
			// 
			// UnloadedCommodityCodeCodeFindBox
			// 
			this.UnloadedCommodityCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedCommodityCodeCodeFindBox, "UnloadedGoodsItem.BY_HarmonisedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.BY_HarmonisedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.UniversalTariffDescription)));
			this.UnloadedCommodityCodeCodeFindBox.ErrorForUnsupportedCountry = null;
			this.UnloadedCommodityCodeCodeFindBox.GetEffectiveDate = null;
			this.UnloadedCommodityCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 98, true);
			this.UnloadedCommodityCodeCodeFindBox.Name = "UnloadedCommodityCodeCodeFindBox";
			this.UnloadedCommodityCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.UnloadedCommodityCodeCodeFindBox.ParentType = null;
			this.UnloadedCommodityCodeCodeFindBox.PreBoundMaxLength = 8;
			this.UnloadedCommodityCodeCodeFindBox.SelectNomenclatureModes = null;
			this.UnloadedCommodityCodeCodeFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.UnloadedCommodityCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 31, true);
			this.UnloadedCommodityCodeCodeFindBox.TabIndex = 6;
			this.UnloadedCommodityCodeCodeFindBox.TariffType = null;
			this.UnloadedCommodityCodeCodeFindBox.BindToForDescription = "UnloadedGoodsItem.UniversalTariffDescription";
			// 
			// DeclaredCusCodeCodeFindBox
			// 
			this.DeclaredCusCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclaredCusCodeCodeFindBox, "BY_CusC4Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_CusC4Number)));
			this.DeclaredCusCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 125, true);
			this.DeclaredCusCodeCodeFindBox.Name = "DeclaredCusCodeCodeFindBox";
			this.DeclaredCusCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DeclaredCusCodeCodeFindBox.ParentType = null;
			this.DeclaredCusCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 31, true);
			this.DeclaredCusCodeCodeFindBox.TabIndex = 2;
			// 
			// UnloadedCusCodeCodeFindBox
			// 
			this.UnloadedCusCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedCusCodeCodeFindBox, "UnloadedGoodsItem.BY_CusC4Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.BY_CusC4Number)));
			this.UnloadedCusCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 124, true);
			this.UnloadedCusCodeCodeFindBox.Name = "UnloadedCusCodeCodeFindBox";
			this.UnloadedCusCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.UnloadedCusCodeCodeFindBox.ParentType = null;
			this.UnloadedCusCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 31, true);
			this.UnloadedCusCodeCodeFindBox.TabIndex = 7;
			// 
			// Phase5GoodsItemDifferencesDetailsColumnUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnloadedCusCodeCodeFindBox);
			this.Controls.Add(this.DeclaredCusCodeCodeFindBox);
			this.Controls.Add(this.UnloadedCommodityCodeCodeFindBox);
			this.Controls.Add(this.DeclaredCommodityCodeCodeFindBox);
			this.Controls.Add(this.UnloadedValueLabel);
			this.Controls.Add(this.UnloadedNetWeightDropEdit);
			this.Controls.Add(this.UnloadedGrossWeightDropEdit);
			this.Controls.Add(this.DeclaredNetWeightDropEdit);
			this.Controls.Add(this.DeclaredGrossWeightDropEdit);
			this.Controls.Add(this.DeclaredValueLabel);
			this.Controls.Add(this.DeclaredDescriptionTextBox);
			this.Controls.Add(this.UnloadedDescriptionTextBox);
			this.Name = "Phase5GoodsItemDifferencesDetailsColumnUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 407, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclaredGrossWeightDropEdit.ResumeLayout(true);
			this.DeclaredGrossWeightDropEdit.PerformLayout();
			this.DeclaredNetWeightDropEdit.ResumeLayout(true);
			this.DeclaredNetWeightDropEdit.PerformLayout();
			this.UnloadedGrossWeightDropEdit.ResumeLayout(true);
			this.UnloadedGrossWeightDropEdit.PerformLayout();
			this.UnloadedNetWeightDropEdit.ResumeLayout(true);
			this.UnloadedNetWeightDropEdit.PerformLayout();
			this.DeclaredCommodityCodeCodeFindBox.ResumeLayout(true);
			this.DeclaredCommodityCodeCodeFindBox.PerformLayout();
			this.UnloadedCommodityCodeCodeFindBox.ResumeLayout(true);
			this.UnloadedCommodityCodeCodeFindBox.PerformLayout();
			this.DeclaredCusCodeCodeFindBox.ResumeLayout(true);
			this.DeclaredCusCodeCodeFindBox.PerformLayout();
			this.UnloadedCusCodeCodeFindBox.ResumeLayout(true);
			this.UnloadedCusCodeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox DeclaredDescriptionTextBox;
		internal ZArchitecture.ZTextBox UnloadedDescriptionTextBox;
		internal ZArchitecture.ZLabel DeclaredValueLabel;
		internal ZArchitecture.GUI.ZCalcDropEdit DeclaredGrossWeightDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit DeclaredNetWeightDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit UnloadedGrossWeightDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit UnloadedNetWeightDropEdit;
		internal ZArchitecture.ZLabel UnloadedValueLabel;
		internal Universal.GUI.TariffFindBox UnloadedCommodityCodeCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox DeclaredCusCodeCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox UnloadedCusCodeCodeFindBox;
		internal Universal.GUI.TariffFindBox DeclaredCommodityCodeCodeFindBox;
	}
}
