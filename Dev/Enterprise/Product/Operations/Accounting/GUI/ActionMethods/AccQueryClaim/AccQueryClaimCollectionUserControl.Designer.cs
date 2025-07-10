using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AccQueryClaimCollectionUserControl
	{
		#region Component Designer generated code

		private ZArchitecture.ZGrid Grid;
		private AccQueryClaimUserControl AccQueryClaimUserControl;
		private ZPanel TopPanel;
		private readonly System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			this.Grid = new ZArchitecture.ZGrid();
			this.AccQueryClaimUserControl = new AccQueryClaimUserControl();
			this.TopPanel = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MasterFiles.Business.AccQueryClaimCollection);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.AccQueryClaim)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.AccQueryClaim)(null)).AY_QueryClaimReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.AccQueryClaim)(null)).AY_QueryClaimType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.AccQueryClaim)(null)).AY_QueryClaimReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.AccQueryClaim)(null)).AY_QueryClaimStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.AccQueryClaim)(null)).AY_ShortDescriptionOfClaim)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((MasterFiles.Business.AccQueryClaim)(null)).AY_QueryClaimAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((MasterFiles.Business.AccQueryClaim)(null)).AY_QueryClaimNextFollowUp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((MasterFiles.Business.AccQueryClaim)(null)).AY_GB)));
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "AY_QueryClaimReference";
			zDropEditColumnStyleInfo1.ColumnName = "AY_QueryClaimType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo2.ColumnName = "AY_QueryClaimReasonCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo3.ColumnName = "AY_QueryClaimStatus";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.ColumnName = "AY_ShortDescriptionOfClaim";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AY_QueryClaimAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "AY_QueryClaimNextFollowUp";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AY_GB";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Grid.GridId = "dec8d54c-e593-40ed-935b-f8013d554920";
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "zGrid1";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 5, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 104, true);
			this.Grid.TabIndex = 0;
			// 
			// AccQueryClaimUserControl
			// 
			this.BindingSource.SetBindingMember(this.AccQueryClaimUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.AccQueryClaim)(((MasterFiles.Business.AccQueryClaim)(null)))));
			this.AccQueryClaimUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AccQueryClaimUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 114, true);
			this.AccQueryClaimUserControl.Name = "AccQueryClaimUserControl";
			this.AccQueryClaimUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 406, true);
			this.AccQueryClaimUserControl.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.Grid);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 5, 5, 5, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 114, true);
			this.TopPanel.TabIndex = 2;
			// 
			// AccQueryClaimCollectionUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.AccQueryClaimUserControl);
			this.Name = "AccQueryClaimCollectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
