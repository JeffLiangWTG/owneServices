using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using ExternalMailManager = MailManager.Module;

namespace Enterprise.MailManager.Module
{
	/// <summary>
	/// Filter control for MailItem.
	/// </summary>
	public class MailItemFilterControl : ZFilterStripControl<ZFilterStrip>
	{
		public MailItemFilterControl()
		{
			InitializeComponent();
		}

		public MailItemFilterControl(IBusinessObjectCollection gridCollection, MailItemFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "MI_Direction";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.ColumnName = "MI_Status";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "CalcReceivedDateTimeLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemFilterControl|E98EEEA2-D449-43c3-9FCC-F2EF87D0F70B", "Received Time (local)");
			zDateEditColumnStyleInfo2.ColumnName = "CalcSendDateTimeLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemFilterControl|12D6CEA9-E682-4525-AA2C-F217C6A6B046", "Send Time (local)");
			zTextBoxColumnStyleInfo3.ColumnName = "MI_Subject";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			zTextBoxColumnStyleInfo4.ColumnName = "MI_From";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.ColumnName = "AllRecipients";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemFilterControl|D7F9C0D5-9DEA-44b4-8BDE-571E550A3C07", "Recipients");
			zTextBoxColumnStyleInfo6.ColumnName = "BodyFirst2K";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemFilterControl|29AFE3D9-B8E9-4eda-8F21-CBF40B75CF09", "Body");
			zTextBoxColumnStyleInfo7.ColumnName = "MI_Application";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 264, true);
			this.FilteredGrid.TabIndex = 19;
			// 
			// MailItemFilterControl
			// 
			this.DataSourceAssemblyName = "MailManager";
			this.DataSourceTypeName = "Enterprise.MailManager.Business.MailItem";
			this.Name = "MailItemFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 416, true);
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
