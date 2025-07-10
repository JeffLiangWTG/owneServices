namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class ProductBundleDiscountControl
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
			this.ProductBundleGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductBundleGrid)).BeginInit();
			this.ProductBundleGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.ProductBundleDiscount);
			// 
			// ProductBundleGrid
			// 
			this.ProductBundleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductBundleGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.ProductBundleDiscount)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ProductBundleDiscountLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.ProductBundleDiscount)(null)).Lines)).SyncRoot)).ProductCode)));
			this.ProductBundleGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1c5cda45-9364-4fc0-8aa4-37051be50641", "Product Code");
			zDropEditColumnStyleInfo1.ColumnName = "ProductCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ProductBundleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProductBundleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductBundleGrid.GridId = "3be23e57-ceba-4b60-9869-2c85186f51c3";
			this.ProductBundleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductBundleGrid.LayoutKey = "ProductBundleGrid";
			this.ProductBundleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProductBundleGrid.Name = "ProductBundleGrid";
			this.ProductBundleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 245, true);
			this.ProductBundleGrid.TabIndex = 2;
			// 
			// ProductBundleDiscountControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ProductBundleGrid);
			this.Name = "ProductBundleDiscountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 245, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductBundleGrid)).EndInit();
			this.ProductBundleGrid.ResumeLayout(false);
			this.ProductBundleGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ProductBundleGrid;
	}
}
