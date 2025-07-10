namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ReportOrderControl
	{


		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ReportOrderGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportOrderGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ReportOrderCollection);
			// 
			// ReportOrderGrid
			// 
			this.ReportOrderGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportOrderGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ReportOrder)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ReportOrder)(null)).Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ReportOrder)(null)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ReportOrder)(null)).AccountsOrderBeginsWith)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ReportOrder)(null)).GLAccountFirstReportStartsFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ReportOrder)(null)).AccountsOrderEndsWith)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ReportOrder)(null)).GLAccountSecondReportStartsFrom)));
			this.ReportOrderGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReportOrderControl|312477d3-0535-46fe-94c2-1374ffe5aa36", "Language");
			zDropEditColumnStyleInfo1.ColumnName = "Language";
			zDropEditColumnStyleInfo1.ToolTip = "Please select a language code for which you have configured the mapped GL Account" +
	" Number.";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7e1177b1-249d-49ee-8b0d-e4d110094943", "Country/Region");
			zDropEditColumnStyleInfo2.ColumnName = "CountryCode";
			zDropEditColumnStyleInfo2.ToolTip = "Please select a country code for which you have configured the mapped GL Account " +
	"Number.";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.Caption = null;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReportOrderControl|3d851067-f830-4243-bdf8-c0d75be3878b", "First Report");
			zDropEditColumnStyleInfo3.ColumnName = "AccountsOrderBeginsWith";
			zDropEditColumnStyleInfo3.ToolTip = "Please select the GL Account order of which you have configured the mapped GL Acc" +
	"ount Number.";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReportOrderControl|040955c5-c1cf-48c5-999d-2ccd24b3c801", "First Report Start Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GLAccountFirstReportStartsFrom";
			zGuidFindBoxColumnStyleInfo1.ToolTip = "First mapped GL Account Number of which the first report will begin with. Note: I" +
	"t is the first  \'HDR\' account.";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDropEditColumnStyleInfo4.Caption = null;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReportOrderControl|11d6f352-1fe6-40ad-aca4-8b09c6867ae5", "Second Report");
			zDropEditColumnStyleInfo4.ColumnName = "AccountsOrderEndsWith";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo2.Caption = null;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReportOrderControl|2f7b09b5-171a-4523-81bc-55ffeb45a6a0", "Second Report Start Account");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "GLAccountSecondReportStartsFrom";
			zGuidFindBoxColumnStyleInfo2.ToolTip = "Please select the first mapped GL Account Number of which the second report will " +
	"begin with. Note: This must be a \'HDR\' account type.";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.ReportOrderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReportOrderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ReportOrderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ReportOrderGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ReportOrderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ReportOrderGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ReportOrderGrid.CopySelectedRowsAllowed = true;
			this.ReportOrderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportOrderGrid.GridId = "756abeed-6094-4813-b217-45936e4cca09";
			this.ReportOrderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportOrderGrid.LayoutKey = "ReportOrderGrid";
			this.ReportOrderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportOrderGrid.Name = "ReportOrderGrid";
			this.ReportOrderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.ReportOrderGrid.TabIndex = 0;
			// 
			// ReportOrderControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportOrderGrid);
			this.Name = "ReportOrderControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportOrderGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		internal ZArchitecture.ZGrid ReportOrderGrid;
	}
}
