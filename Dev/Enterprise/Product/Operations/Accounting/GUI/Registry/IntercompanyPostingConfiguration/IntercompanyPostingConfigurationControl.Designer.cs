namespace Enterprise.Accounting.Registry.GUI
{
	public partial class IntercompanyPostingConfigurationControl
	{

		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.IntercompanyPostingConfigurationGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IntercompanyPostingConfigurationGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.IntercompanyPostingConfigurationCollection);
			// 
			// IntercompanyPostingConfigurationGrid
			// 
			this.IntercompanyPostingConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IntercompanyPostingConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.IntercompanyPostingConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.IntercompanyPostingConfiguration)(null)).Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.IntercompanyPostingConfiguration)(null)).MaxCostVarianceApprovalLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.IntercompanyPostingConfiguration)(null)).MaxCostVarianceApprovalLevelList)));
			this.IntercompanyPostingConfigurationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("IntercompanyPostingConfigurationControl|262b4ef7-0e19-439c-8d48-659fdb726311", "Company");
			zTextBoxColumnStyleInfo1.ColumnName = "Company";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("IntercompanyPostingConfigurationControl|29590319-0de2-422b-9db3-4d8cc55d2551", "Max Cost Variance Approval Level");
			zDropEditColumnStyleInfo1.ColumnName = "MaxCostVarianceApprovalLevel";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.IntercompanyPostingConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IntercompanyPostingConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.IntercompanyPostingConfigurationGrid.GridId = "9a5b0c17-e74f-4f29-b601-84698c05f767";
			this.IntercompanyPostingConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IntercompanyPostingConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IntercompanyPostingConfigurationGrid.LayoutKey = "IntercompanyPostingConfigurationGrid";
			this.IntercompanyPostingConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IntercompanyPostingConfigurationGrid.Name = "IntercompanyPostingConfigurationGrid";
			this.IntercompanyPostingConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.IntercompanyPostingConfigurationGrid.TabIndex = 0;
			// 
			// IntercompanyPostingConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IntercompanyPostingConfigurationGrid);
			this.Name = "IntercompanyPostingConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IntercompanyPostingConfigurationGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.ZGrid IntercompanyPostingConfigurationGrid;
	}
}
