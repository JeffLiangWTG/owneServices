namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5DeclarationCountryOfRoutingTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CountryOfRoutingsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryOfRoutingsGrid)).BeginInit();
			this.CountryOfRoutingsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.CountryOfRoutingCollection<EU.NCTS.Business.CountryOfRouting>);
			// 
			// CountryOfRoutingsGrid
			// 
			this.CountryOfRoutingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CountryOfRoutingsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.CountryOfRouting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.CountryOfRouting)(null)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CountryOfRouting)(null)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CountryOfRouting)(null)).Description)));
			this.CountryOfRoutingsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(61);
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(204);
			this.CountryOfRoutingsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CountryOfRoutingsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CountryOfRoutingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CountryOfRoutingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryOfRoutingsGrid.GridId = "6d2242bd-7c32-4cf8-8398-8ba52794063f";
			this.CountryOfRoutingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryOfRoutingsGrid.LayoutKey = "zGrid1";
			this.CountryOfRoutingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CountryOfRoutingsGrid.Name = "CountryOfRoutingsGrid";
			this.CountryOfRoutingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.CountryOfRoutingsGrid.TabIndex = 0;
			// 
			// Phase5DeclarationCountryOfRoutingTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CountryOfRoutingsGrid);
			this.Name = "Phase5DeclarationCountryOfRoutingTabUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryOfRoutingsGrid)).EndInit();
			this.CountryOfRoutingsGrid.ResumeLayout(false);
			this.CountryOfRoutingsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid CountryOfRoutingsGrid;
	}
}
