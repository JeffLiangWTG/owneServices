namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoicePostingExRateOptionControl
	{
		#region Component Designer Generated Code

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.InvoicePostingExRateOptionGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoicePostingExRateOptionGrid)).BeginInit();
			this.InvoicePostingExRateOptionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.InvoicePostingExRateOptionCollection);
			// 
			// InvoicePostingExRateOptionGrid
			// 
			this.InvoicePostingExRateOptionGrid.AllowCopyToNewRowMenuItem = false;
			this.InvoicePostingExRateOptionGrid.AllowNavigation = false;
			this.InvoicePostingExRateOptionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoicePostingExRateOptionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.InvoicePostingExRateOption)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoicePostingExRateOption)(null)).InvoiceCurrencyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoicePostingExRateOption)(null)).ExRateOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.InvoicePostingExRateOption)(null)).OffSet)));
			this.InvoicePostingExRateOptionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "InvoiceCurrencyType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.ColumnName = "ExRateOption";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OffSet";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.InvoicePostingExRateOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InvoicePostingExRateOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.InvoicePostingExRateOptionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoicePostingExRateOptionGrid.CopySelectedRowsAllowed = false;
			this.InvoicePostingExRateOptionGrid.GridId = "f9442480-1c0a-45c5-98a7-cbde024f5f9c";
			this.InvoicePostingExRateOptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoicePostingExRateOptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoicePostingExRateOptionGrid.LayoutKey = "InvoicePostingExRateOptionGrid";
			this.InvoicePostingExRateOptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoicePostingExRateOptionGrid.Name = "InvoicePostingExRateOptionGrid";
			this.InvoicePostingExRateOptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 236, true);
			this.InvoicePostingExRateOptionGrid.TabIndex = 0;
			// 
			// InvoicePostingExRateOptionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoicePostingExRateOptionGrid);
			this.Name = "InvoicePostingExRateOptionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoicePostingExRateOptionGrid)).EndInit();
			this.InvoicePostingExRateOptionGrid.ResumeLayout(false);
			this.InvoicePostingExRateOptionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZGrid InvoicePostingExRateOptionGrid;
	}
}
