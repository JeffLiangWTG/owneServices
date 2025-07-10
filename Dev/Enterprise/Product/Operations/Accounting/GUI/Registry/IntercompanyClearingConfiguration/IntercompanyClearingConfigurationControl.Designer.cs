namespace Enterprise.Accounting.Registry.GUI
{
	public partial class IntercompanyClearingConfigurationControl
	{

		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.IntercompanyClearingConfigurationGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IntercompanyClearingConfigurationGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.IntercompanyClearingConfigurationCollection);
			// 
			// IntercompanyClearingConfigurationGrid
			// 
			this.IntercompanyClearingConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IntercompanyClearingConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.IntercompanyClearingConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.IntercompanyClearingConfiguration)(null)).Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.IntercompanyClearingConfiguration)(null)).ClearingGLAccount)));
			this.IntercompanyClearingConfigurationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("IntercompanyClearingConfigurationControl|a022cc11-38d9-4502-9f07-67f977006d42", "Company");
			zTextBoxColumnStyleInfo1.ColumnName = "Company";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("IntercompanyClearingConfigurationControl|d68ad7b5-ed2d-442f-86d5-4644e4e50b99", "Clearing GL Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ClearingGLAccount";
			this.IntercompanyClearingConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IntercompanyClearingConfigurationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.IntercompanyClearingConfigurationGrid.GridId = "e27ca705-6acc-4b54-90b4-29e9564d8e73";
			this.IntercompanyClearingConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IntercompanyClearingConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IntercompanyClearingConfigurationGrid.LayoutKey = "IntercompanyClearingConfigurationGrid";
			this.IntercompanyClearingConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IntercompanyClearingConfigurationGrid.Name = "IntercompanyClearingConfigurationGrid";
			this.IntercompanyClearingConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.IntercompanyClearingConfigurationGrid.TabIndex = 0;
			// 
			// IntercompanyClearingConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IntercompanyClearingConfigurationGrid);
			this.Name = "IntercompanyClearingConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IntercompanyClearingConfigurationGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.ZGrid IntercompanyClearingConfigurationGrid;
	}
}
