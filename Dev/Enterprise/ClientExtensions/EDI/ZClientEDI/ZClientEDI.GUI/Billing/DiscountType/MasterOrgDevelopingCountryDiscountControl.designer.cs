namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class MasterOrgDevelopingCountryDiscountControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CountryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryGrid)).BeginInit();
			this.CountryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.MasterOrgDevelopingCountryDiscount);
			// 
			// CountryGrid
			// 
			this.CountryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CountryGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.MasterOrgDevelopingCountryDiscount)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.MasterOrgDevelopingCountryDiscountLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.MasterOrgDevelopingCountryDiscount)(null)).Lines)).SyncRoot)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.MasterOrgDevelopingCountryDiscountLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.MasterOrgDevelopingCountryDiscount)(null)).Lines)).SyncRoot)).Percent)));
			this.CountryGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Country";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Percent";
			zCalcEditColumnStyleInfo1.MaxValue = 100;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.CountryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CountryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CountryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryGrid.GridId = "bb84792f-bd99-418d-aa8c-f5a7e47a4aa3";
			this.CountryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryGrid.LayoutKey = "CountryGrid";
			this.CountryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CountryGrid.Name = "CountryGrid";
			this.CountryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 216, true);
			this.CountryGrid.TabIndex = 2;
			// 
			// MasterOrgDevelopingCountryDiscountControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CountryGrid);
			this.Name = "MasterOrgDevelopingCountryDiscountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 216, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryGrid)).EndInit();
			this.CountryGrid.ResumeLayout(false);
			this.CountryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid CountryGrid;
	}
}
