namespace Enterprise.Customs.GB.Module.StandAloneFsrEnquiry
{
	partial class StandAloneFsrEnquiryFilterStripControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("ad3bf21b-ac93-4f0a-a22d-07fc9a26f604", "Reference Number");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(166);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("7e3a71f9-bb83-4d2e-8868-4aa840b6fd7c", "Response?");
			zCheckBoxColumnStyleInfo1.ColumnName = "HasLinkedMessage";
			zCheckBoxColumnStyleInfo1.GroupName = null;
			zCheckBoxColumnStyleInfo1.IsVisible = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("8e3a71f9-bb83-4d2e-8868-4aa840b6fd7c", "Response Text");
			zTextBoxColumnStyleInfo2.ColumnName = "ResponseText";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(234);
			zTextBoxColumnStyleInfo2.IsVisible = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("806867d8-28b5-4fa8-a7cb-b0f58669cd6b", "PIMA");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageOwner";
			zTextBoxColumnStyleInfo3.IsVisible = true;
			zTextBoxColumnStyleInfo3.GroupName = null;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("87c9f116-6b8b-4b64-bab1-129f8dfb555e", "Message number");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo4.GroupName = null;
			zTextBoxColumnStyleInfo4.IsVisible = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("6932406f-d37b-4b02-ae22-b2196763e895", "Status");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo5.GroupName = null;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo5.IsVisible = true;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("1596c2f8-0f12-40c8-967f-820c011a0a20", "Created date");
			zDateEditColumnStyleInfo1.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.GroupName = null;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry);
			// 
			// StandAloneFsrEnquiryFilterStripControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "StandAloneFsrEnquiryFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
