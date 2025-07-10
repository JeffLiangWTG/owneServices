namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class UnloadingDifferencesDetailsUserControl
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
			this.DeclaredValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnloadedValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalGrossMassDeclaredValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EffectiveGrossWeightUnloadedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPackagesDeclaredValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPackagesUnloadedValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RecalculateTotalsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InlandTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SeparatorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InlandTransportModeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader);
			// 
			// DeclaredValueLabel
			// 
			this.DeclaredValueLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b6dc1620-b39d-4551-90ef-1cda8f12c21d", "Declared Value");
			this.DeclaredValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DeclaredValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 14, true);
			this.DeclaredValueLabel.Name = "DeclaredValueLabel";
			this.DeclaredValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.DeclaredValueLabel.TabIndex = 0;
			// 
			// UnloadedValueLabel
			// 
			this.UnloadedValueLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("8d5e3a6e-7c12-4d11-89ea-2b2a5901c8e8", "Unloaded Value");
			this.UnloadedValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnloadedValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 14, true);
			this.UnloadedValueLabel.Name = "UnloadedValueLabel";
			this.UnloadedValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.UnloadedValueLabel.TabIndex = 1;
			// 
			// TotalGrossMassDeclaredValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossMassDeclaredValueCalcEdit, "TotalGrossMassInKilograms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).TotalGrossMassInKilograms)));
			this.TotalGrossMassDeclaredValueCalcEdit.CaptionResourceString = null;
			this.TotalGrossMassDeclaredValueCalcEdit.DecimalPlaces = 6;
			this.TotalGrossMassDeclaredValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 40, true);
			this.TotalGrossMassDeclaredValueCalcEdit.Name = "TotalGrossMassDeclaredValueCalcEdit";
			this.TotalGrossMassDeclaredValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.TotalGrossMassDeclaredValueCalcEdit.TabIndex = 2;
			this.TotalGrossMassDeclaredValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EffectiveGrossWeightUnloadedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EffectiveGrossWeightUnloadedCalcEdit, "EffectiveGrossWeightUnloaded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).EffectiveGrossWeightUnloaded)));
			this.EffectiveGrossWeightUnloadedCalcEdit.CaptionResourceString = null;
			this.EffectiveGrossWeightUnloadedCalcEdit.DecimalPlaces = 6;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EffectiveGrossWeightUnloadedCalcEdit, false);
			this.EffectiveGrossWeightUnloadedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 40, true);
			this.EffectiveGrossWeightUnloadedCalcEdit.Name = "EffectiveGrossWeightUnloadedCalcEdit";
			this.EffectiveGrossWeightUnloadedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.EffectiveGrossWeightUnloadedCalcEdit.TabIndex = 3;
			this.EffectiveGrossWeightUnloadedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPackagesDeclaredValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPackagesDeclaredValueCalcEdit, "TotalNumberOfPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).TotalNumberOfPackages)));
			this.TotalPackagesDeclaredValueCalcEdit.CaptionResourceString = null;
			this.TotalPackagesDeclaredValueCalcEdit.DecimalPlaces = 2;
			this.TotalPackagesDeclaredValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 66, true);
			this.TotalPackagesDeclaredValueCalcEdit.Name = "TotalPackagesDeclaredValueCalcEdit";
			this.TotalPackagesDeclaredValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.TotalPackagesDeclaredValueCalcEdit.TabIndex = 4;
			this.TotalPackagesDeclaredValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPackagesUnloadedValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPackagesUnloadedValueCalcEdit, "TotalUnloadedNumberOfPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).TotalUnloadedNumberOfPackages)));
			this.TotalPackagesUnloadedValueCalcEdit.CaptionResourceString = null;
			this.TotalPackagesUnloadedValueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalPackagesUnloadedValueCalcEdit, false);
			this.TotalPackagesUnloadedValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 66, true);
			this.TotalPackagesUnloadedValueCalcEdit.Name = "TotalPackagesUnloadedValueCalcEdit";
			this.TotalPackagesUnloadedValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.TotalPackagesUnloadedValueCalcEdit.TabIndex = 5;
			this.TotalPackagesUnloadedValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RecalculateTotalsButton
			// 
			this.RecalculateTotalsButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("457b3164-9dfd-45e1-b654-23b928cb80b7", "Recalculate Totals");
			this.RecalculateTotalsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 40, true);
			this.RecalculateTotalsButton.Name = "RecalculateTotalsButton";
			this.RecalculateTotalsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 36, true);
			this.RecalculateTotalsButton.TabIndex = 6;
			this.RecalculateTotalsButton.ToolTipCaption = null;
			this.RecalculateTotalsButton.Click += new System.EventHandler(this.RecalculateTotalsButton_Click);
			// 
			// InlandTransportModeDropEdit
			// 
			this.InlandTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandTransportModeDropEdit, "BM_InlandTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).BM_InlandTransportMode)));
			this.InlandTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 92, true);
			this.InlandTransportModeDropEdit.Name = "InlandTransportModeDropEdit";
			this.InlandTransportModeDropEdit.PreBoundMaxLength = 1;
			this.InlandTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.InlandTransportModeDropEdit.TabIndex = 7;
			// 
			// SeparatorLabel
			// 
			this.SeparatorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SeparatorLabel, false);
			this.SeparatorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 89, true);
			this.SeparatorLabel.Name = "SeparatorLabel";
			this.SeparatorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 23, true);
			this.SeparatorLabel.TabIndex = 27;
			// 
			// UnloadingDifferencesDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeclaredValueLabel);
			this.Controls.Add(this.UnloadedValueLabel);
			this.Controls.Add(this.TotalGrossMassDeclaredValueCalcEdit);
			this.Controls.Add(this.EffectiveGrossWeightUnloadedCalcEdit);
			this.Controls.Add(this.TotalPackagesDeclaredValueCalcEdit);
			this.Controls.Add(this.TotalPackagesUnloadedValueCalcEdit);
			this.Controls.Add(this.RecalculateTotalsButton);
			this.Controls.Add(this.InlandTransportModeDropEdit);
			this.Controls.Add(this.SeparatorLabel);
			this.Name = "UnloadingDifferencesDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 389, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InlandTransportModeDropEdit.ResumeLayout(true);
			this.InlandTransportModeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZLabel DeclaredValueLabel;
		internal ZArchitecture.ZLabel UnloadedValueLabel;
		internal ZArchitecture.ZCalcEdit TotalGrossMassDeclaredValueCalcEdit;
		internal ZArchitecture.ZCalcEdit EffectiveGrossWeightUnloadedCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalPackagesDeclaredValueCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalPackagesUnloadedValueCalcEdit;
		internal ZArchitecture.GUI.ZButton RecalculateTotalsButton;
		internal ZArchitecture.GUI.ZDropEdit InlandTransportModeDropEdit;
		internal ZArchitecture.ZLabel SeparatorLabel;
	}
}
