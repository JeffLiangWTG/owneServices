namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ExchangeRateToleranceControl
	{

		#region Designer generated code

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ExchangeRateToleranceGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExchangeRateToleranceGrid)).BeginInit();
			this.ExchangeRateToleranceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ExchangeRateToleranceConfiguration);
			// 
			// ExchangeRateToleranceGrid
			// 
			this.ExchangeRateToleranceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExchangeRateToleranceGrid, "ExchangeRateToleranceCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ExchangeRateToleranceConfiguration)(null)).ExchangeRateToleranceCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ExchangeRateTolerance)(((System.Collections.IList)(((Business.ExchangeRateToleranceConfiguration)(null)).ExchangeRateToleranceCollection)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ExchangeRateTolerance)(((System.Collections.IList)(((Business.ExchangeRateToleranceConfiguration)(null)).ExchangeRateToleranceCollection)).SyncRoot)).ExchangeRateTolerancePercentage)));
			this.ExchangeRateToleranceGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExchangeRateToleranceControl|CBAE52AE-B500-4967-83F9-D02602F34B08", "Currency");
			zDropEditColumnStyleInfo1.ColumnName = "Currency";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExchangeRateToleranceControl|05CE223C-8915-4ED2-B59A-9BD69D8A8153", "Exchange Rate Tolerance(%)");
			zCalcEditColumnStyleInfo2.ColumnName = "ExchangeRateTolerancePercentage";
			zCalcEditColumnStyleInfo2.IsCustomColumn = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ExchangeRateToleranceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ExchangeRateToleranceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ExchangeRateToleranceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExchangeRateToleranceGrid.GridId = "08356532-BB1C-4838-95E8-9333568DCF35";
			this.ExchangeRateToleranceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExchangeRateToleranceGrid.LayoutKey = "ExchangeRateToleranceGrid";
			this.ExchangeRateToleranceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.ExchangeRateToleranceGrid.Name = "ExchangeRateToleranceGrid";
			this.ExchangeRateToleranceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 274, true);
			this.ExchangeRateToleranceGrid.TabIndex = 6;
			// 
			// ExchangeRateToleranceControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExchangeRateToleranceGrid);
			this.Name = "ExchangeRateToleranceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExchangeRateToleranceGrid)).EndInit();
			this.ExchangeRateToleranceGrid.ResumeLayout(false);
			this.ExchangeRateToleranceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.ZGrid ExchangeRateToleranceGrid;
	}
}
