namespace Enterprise.Client.EDI.Mail.Module
{
	partial class EDIMailItemFilterControl
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("EDIMailItemFilterControl|118f3d35-e058-41aa-a827-d528c9e1db07", "Last Status Changed By");
			zTextBoxColumnStyleInfo1.ColumnName = "LastStatusChangedBy";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			// 
			// EDIMailItemFilterControl
			// 
			this.Name = "EDIMailItemFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
