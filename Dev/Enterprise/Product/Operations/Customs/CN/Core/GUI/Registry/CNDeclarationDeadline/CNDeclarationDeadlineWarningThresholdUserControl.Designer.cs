namespace Enterprise.Customs.CN.GUI
{
	partial class CNDeclarationDeadlineWarningThresholdUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo colorSelectorColumnStyleInfo1 = new Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo colorSelectorColumnStyleInfo2 = new Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo colorSelectorColumnStyleInfo3 = new Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo();
			Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo colorSelectorColumnStyleInfo4 = new Enterprise.Registry.GUI.ColorSelectorColumnStyleInfo();
			this.MainGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).BeginInit();
			this.MainGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThresholdCollection);
			// 
			// MainGrid
			// 
			this.MainGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MainGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThreshold)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThreshold)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThreshold)(null)).FirstLevelThreshold)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThreshold)(null)).FirstLevelWarningColor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThreshold)(null)).SecondLevelThreshold)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThreshold)(null)).SecondLevelWarningColor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThreshold)(null)).ThirdLevelThreshold)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNDeclarationDeadlineWarningThreshold)(null)).ThirdLevelWarningColor)));
			this.MainGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("b49e6cdd-00b6-449e-b3cd-1a218d5ea21d", "Transport Mode");
			zDropEditColumnStyleInfo1.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("675c9205-2016-4868-bd83-b45a8a03fdcb", "1st Level Threshold");
			zCalcEditColumnStyleInfo1.ColumnName = "FirstLevelThreshold";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			colorSelectorColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("e235dcb2-45f4-465f-84c9-0977679cc4a0", "1st Level Warning Color");
			colorSelectorColumnStyleInfo1.ColumnName = "FirstLevelWarningColor";
			colorSelectorColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			colorSelectorColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("ace7129d-c655-4788-b60e-92bae3fd8a92", "2nd Level Threshold");
			zCalcEditColumnStyleInfo2.ColumnName = "SecondLevelThreshold";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			colorSelectorColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("2d911bf7-3006-4809-8093-33b502c6a28e", "2nd Level Warning Color");
			colorSelectorColumnStyleInfo2.ColumnName = "SecondLevelWarningColor";
			colorSelectorColumnStyleInfo2.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			colorSelectorColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("effdb763-2303-464b-a369-159c67af26cf", "3rd Level Threshold");
			zCalcEditColumnStyleInfo3.ColumnName = "ThirdLevelThreshold";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			colorSelectorColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("a8236ae1-564a-454b-8314-98a799c38942", "3rd Level Warning Color");
			colorSelectorColumnStyleInfo3.ColumnName = "ThirdLevelWarningColor";
			colorSelectorColumnStyleInfo3.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			colorSelectorColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			colorSelectorColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("9B59E12C-A23E-4EEC-9418-6D59098F9377", "Delayed Warning Color");
			colorSelectorColumnStyleInfo4.ColumnName = "DelayedWarningColor";
			colorSelectorColumnStyleInfo4.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			colorSelectorColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			this.MainGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(colorSelectorColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MainGrid.ColumnStyles.Add(colorSelectorColumnStyleInfo2);
			this.MainGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.MainGrid.ColumnStyles.Add(colorSelectorColumnStyleInfo3);
			this.MainGrid.ColumnStyles.Add(colorSelectorColumnStyleInfo4);
			this.MainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGrid.GridId = "f2156ab9-6ddb-4593-ab15-8a10bc8e3067";
			this.MainGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MainGrid.LayoutKey = "MainGrid";
			this.MainGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGrid.Name = "MainGrid";
			this.MainGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 241, true);
			this.MainGrid.TabIndex = 0;
			// 
			// CNDeclarationDeadlineWarningThresholdUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGrid);
			this.Name = "CNDeclarationDeadlineWarningThresholdUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 241, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).EndInit();
			this.MainGrid.ResumeLayout(false);
			this.MainGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid MainGrid;
	}
}
