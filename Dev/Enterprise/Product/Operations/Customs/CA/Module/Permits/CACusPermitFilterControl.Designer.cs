namespace Enterprise.Customs.CA.Module
{
	public partial class CACusPermitFilterControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("27b15b47-349b-4fc4-b3a2-5c12d2931aa0", "DIF URN");
			zTextBoxColumnStyleInfo1.ColumnName = "DIFURNs";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("0d34cb6b-f40a-457f-bfa5-63db04310ab8", "DIF Message Status");
			zTextBoxColumnStyleInfo2.ColumnName = "DIFMessageStatus";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CusPermitHeader);
			// 
			// CACusPermitFilterControl
			// 
			this.Name = "CACusPermitFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
