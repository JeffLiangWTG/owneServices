namespace Enterprise.ZArchitecture
{
#if DEBUG
	public partial class TestForm
	{
		ZGrid zGrid1;

		private void InitializeComponent()
		{
			var zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			var zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Collection";
			zGuidFindBoxColumnStyleInfo1.Caption = "GuidFindBox";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Z0_Guid";
			zCodeFindBoxColumnStyleInfo1.BindToList = "Collection";
			zCodeFindBoxColumnStyleInfo1.Caption = "Code";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Z0_Code";
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 8, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 352, true);
			this.zGrid1.TabIndex = 0;
			// 
			// TestForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 374, true);
			this.Controls.Add(this.zGrid1);
			this.Name = "TestForm";
			this.Text = "TestForm";
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}
	}
#endif
}
