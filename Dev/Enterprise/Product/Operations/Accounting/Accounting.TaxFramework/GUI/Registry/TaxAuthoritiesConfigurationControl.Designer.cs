namespace Enterprise.Accounting.TaxFramework.GUI
{
	public partial class TaxAuthoritiesConfigurationControl
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
			this.TaxAuthoritiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxAuthoritiesGrid)).BeginInit();
			this.TaxAuthoritiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TaxAuthoritiesConfiguration);
			// 
			// TaxAuthoritiesGrid
			// 
			this.TaxAuthoritiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxAuthoritiesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.TaxAuthoritiesConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.TaxAuthoritiesConfiguration)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.TaxAuthoritiesConfiguration)(null)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.TaxAuthoritiesConfiguration)(null)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.TaxAuthoritiesConfiguration)(null)).TaxAuthorityType)));
			this.TaxAuthoritiesGrid.CaptionVisible = false;

			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			zTextBoxColumnStyleInfo2.ColumnName = "Name";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			this.TaxAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			zCodeFindBoxColumnStyleInfo1.ColumnName = "Country";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxAuthoritiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);

			zDropEditColumnStyleInfo1.ColumnName = "TaxAuthorityType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxAuthoritiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);

			this.TaxAuthoritiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxAuthoritiesGrid.GridId = "2A9F100F-398E-48EF-AF59-BD7256157EDF";
			this.TaxAuthoritiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxAuthoritiesGrid.LayoutKey = "TaxAuthoritiesGrid";
			this.TaxAuthoritiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxAuthoritiesGrid.Name = "TaxAuthoritiesGrid";
			this.TaxAuthoritiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.TaxAuthoritiesGrid.TabIndex = 0;
			// 
			// TaxAuthoritiesConfigurationControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.TaxAuthoritiesGrid);
			this.Name = "TaxAuthoritiesConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxAuthoritiesGrid)).EndInit();
			this.TaxAuthoritiesGrid.ResumeLayout(false);
			this.TaxAuthoritiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.ZGrid TaxAuthoritiesGrid;
	}
}
