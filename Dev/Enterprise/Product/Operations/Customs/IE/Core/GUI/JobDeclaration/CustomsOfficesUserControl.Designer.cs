namespace Enterprise.Customs.IE.GUI
{
	public partial class CustomsOfficesUserControl
	{
		void InitializeComponent()
		{
			this.OfficesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).BeginInit();
			this.CustomsOfficesGrid.SuspendLayout();
			this.CustomsOfficeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OfficesGroupBox
			// 
			this.OfficesGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("276ddee6-15a5-4fd2-b506-2b18a3cd5596", "Customs Offices");
			this.OfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 130, true);
			// 
			// CustomsOfficesGrid
			// 
			this.CustomsOfficesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CustomsOfficesGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CustomsOfficesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 42, true);
			this.CustomsOfficesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 86, true);
			this.CustomsOfficesGrid.TabIndex = 1;
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo_PurposeDescription = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo_PurposeDescription.ColumnName = "CY_CodeDescription";
			zTextBoxColumnStyleInfo_PurposeDescription.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("97A1A87A-D712-4CCB-9CAE-C2FBB518272B", "Purpose Desc.");
			zTextBoxColumnStyleInfo_PurposeDescription.IsReadOnly = true;
			zTextBoxColumnStyleInfo_PurposeDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CustomsOfficesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo_PurposeDescription);
			// 
			// CustomsOfficeFindBox
			// 
			this.CustomsOfficeFindBox.CaptionResourceString = null;
			this.CustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 19, true);
			this.CustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 18, true);
			this.CustomsOfficeFindBox.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.Declaration.JobDeclaration);
			// 
			// CustomsOfficesUserControl
			// 
			this.Name = "CustomsOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 130, true);
			this.OfficesGroupBox.ResumeLayout(false);
			this.OfficesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).EndInit();
			this.CustomsOfficesGrid.ResumeLayout(false);
			this.CustomsOfficesGrid.PerformLayout();
			this.CustomsOfficeFindBox.ResumeLayout(true);
			this.CustomsOfficeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
