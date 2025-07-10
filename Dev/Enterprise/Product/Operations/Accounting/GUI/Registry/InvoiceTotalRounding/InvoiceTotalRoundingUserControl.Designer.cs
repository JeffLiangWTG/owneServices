namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoiceTotalRoundingUserControl
	{

		#region Component Designer generated code

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.InvoiceTotalRoundingGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceTotalRoundingGrid)).BeginInit();
			this.InvoiceTotalRoundingGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.InvoiceTotalRoundingCollection);
			// 
			// InvoiceTotalRoundingGrid
			// 
			this.InvoiceTotalRoundingGrid.AllowNavigation = false;
			this.InvoiceTotalRoundingGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoiceTotalRoundingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.InvoiceTotalRounding)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceTotalRounding)(null)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceTotalRounding)(null)).RoundingOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceTotalRounding)(null)).RoundToCurrencyUnit)));
			this.InvoiceTotalRoundingGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Currency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("480C8D07-F3DE-4054-BAF0-4B9DE69D430D", "Currency");
			zDropEditColumnStyleInfo1.ColumnName = "RoundingOption";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8C2818D9-CB94-4D2F-8283-7D3EA10F8993", "Rounding Option");
			zDropEditColumnStyleInfo2.ColumnName = "RoundToCurrencyUnit";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2739C5E6-D1F9-4E9B-901B-932E66711353", "Round to Currency Unit");
			this.InvoiceTotalRoundingGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoiceTotalRoundingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InvoiceTotalRoundingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.InvoiceTotalRoundingGrid.GridId = "70cf0221-727d-4e76-9f3c-bed3bc74e4ff";
			this.InvoiceTotalRoundingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceTotalRoundingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceTotalRoundingGrid.LayoutKey = "zGrid1";
			this.InvoiceTotalRoundingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InvoiceTotalRoundingGrid.Name = "InvoiceTotalRoundingGrid";
			this.InvoiceTotalRoundingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 107, true);
			this.InvoiceTotalRoundingGrid.TabIndex = 1;
			// 
			// InvoiceTotalRoundingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceTotalRoundingGrid);
			this.Name = "InvoiceTotalRoundingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 113, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceTotalRoundingGrid)).EndInit();
			this.InvoiceTotalRoundingGrid.ResumeLayout(false);
			this.InvoiceTotalRoundingGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZGrid InvoiceTotalRoundingGrid;
	}
}
