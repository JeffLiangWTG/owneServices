namespace Enterprise.Accounting.TaxFramework.GUI
{
	public partial class TaxSystemsConfigurationControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.TaxSystemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxSystemsGrid)).BeginInit();
			this.TaxSystemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration);
			// 
			// TaxSystemsGrid
			// 
			this.TaxSystemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxSystemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).TaxAuthorityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).TaxSuperType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).RegistrationLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).IncludeInInvoceTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).AdjustmentSign)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).TaxBaseCalculationMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).TaxAmountCalculationMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).ThresholdRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxSystemsConfiguration)(null)).TaxRateSource)));
			this.TaxSystemsGrid.CaptionVisible = false;

			zTextBoxColumnStyleInfo1.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.Code);
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxSystemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			zTextBoxColumnStyleInfo2.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.Name);
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			zCodeFindBoxColumnStyleInfo1.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.Country);
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxSystemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);

			zDropEditColumnStyleInfo1.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.TaxAuthorityType);
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);

			zDropEditColumnStyleInfo2.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.TaxSuperType);
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);

			zDropEditColumnStyleInfo3.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.RegistrationLevel);
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);

			zCheckBoxColumnStyleInfo1.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.IncludeInInvoceTotal);
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxSystemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);

			zDropEditColumnStyleInfo4.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.AdjustmentSign);
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);

			zDropEditColumnStyleInfo6.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.TaxBaseCalculationMethod);
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);

			zDropEditColumnStyleInfo7.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.TaxAmountCalculationMethod);
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);

			zDropEditColumnStyleInfo8.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.ThresholdRule);
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);

			zDropEditColumnStyleInfo5.ColumnName = nameof(Enterprise.MasterFiles.Business.TaxSystemsConfiguration.TaxRateSource);
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSystemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);

			this.TaxSystemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxSystemsGrid.GridId = "5BF88CEA-D83A-49C1-9AFF-4751A9BDA32B";
			this.TaxSystemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxSystemsGrid.LayoutKey = nameof(TaxSystemsGrid);
			this.TaxSystemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxSystemsGrid.Name = nameof(TaxSystemsGrid);
			this.TaxSystemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.TaxSystemsGrid.TabIndex = 0;
			// 
			// TaxSystemsConfigurationControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.TaxSystemsGrid);
			this.Name = nameof(TaxSystemsConfigurationControl);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxSystemsGrid)).EndInit();
			this.TaxSystemsGrid.ResumeLayout(false);
			this.TaxSystemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.ZGrid TaxSystemsGrid;
	}
}
