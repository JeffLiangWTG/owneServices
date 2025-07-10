namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class EventLogsListProvidersAndTheirCapacityControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxProviderCode = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditLevelCode = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxCapability = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();

			this.grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.EventLogsListProvidersAndTheirCapacityCollection);
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.EventLogsListProvidersAndTheirCapacity)(null)).ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.Registry.Business.EventLogsListProvidersAndTheirCapacity)(null)).LevelCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.EventLogsListProvidersAndTheirCapacity)(null)).CapabilityCode)));
			this.grid.CaptionVisible = false;
			zTextBoxProviderCode.ColumnName = "ProviderCode";
			zTextBoxProviderCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zCalcEditLevelCode.ColumnName = "LevelCode";
			zCalcEditLevelCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxCapability.ColumnName = "CapabilityCode";
			zGuidFindBoxCapability.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);

			this.grid.ColumnStyles.Add(zTextBoxProviderCode);
			this.grid.ColumnStyles.Add(zCalcEditLevelCode);
			this.grid.ColumnStyles.Add(zGuidFindBoxCapability);

			this.grid.CopySelectedRowsAllowed = true;
			this.grid.GridId = "c769f6f5-0fed-4003-9e58-50d6385c4b0a";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "gridRates";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 8, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 315, true);
			this.grid.TabIndex = 0;

			this.Controls.Add(this.grid);
			this.Name = "EventLogsListProvidersAndTheirCapacityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 330, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid grid;
	}
}
