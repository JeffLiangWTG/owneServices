namespace Enterprise.Customs.FR.Module
{
	partial class EntryHeaderFilterUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("34C8DBE7-7672-4ED0-B7A3-D7E3319EDEBF", "Delta Agreement (Profile Number)");
			zTextBoxColumnStyleInfo1.ColumnName = Schema.CustomsProfile;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("5EB35334-545E-4DDC-AE94-69B22CA6F61F", "Delta Mode");
			zTextBoxColumnStyleInfo2.ColumnName = Schema.DeltaMode;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("19E537F6-57B1-4B91-9EA2-DCAF839182FE", "Is Delta G2 Step One Sent");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsDeltaDStepOneSentOK";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("DD185347-2E24-486C-88BC-90EA4ACC6805", "Is Delta G2 Step Two Sent ");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsDeltaDStepTwoSentOK";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("38015635-A0CE-481A-AA0C-1DB5E62C7673", "Is Delta G2 Step Two Sent but 0 Liquidation");
			zCheckBoxColumnStyleInfo5.ColumnName = "IsDeltaDStepTwoSentOKButZeroLiquidation";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("86DC9158-B36A-4E49-AAD3-236D9DDE40B4", "Delta G Fallback Number");
			zTextBoxColumnStyleInfo6.ColumnName = "FRCustomsFallbackNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("30C0D8D0-36C4-4B68-AC11-44650FDB2414", "Delta G Fallback Status");
			zTextBoxColumnStyleInfo7.ColumnName = "DeltaGFallbackStatus";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("C277F6A8-5886-414D-BC88-CD93ACDB52FA", "Assessment Date");
			zDateEditColumnStyleInfo8.ColumnName = "AssessmentDate";
			zDateEditColumnStyleInfo8.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("4659BB52-864A-4B83-8A32-364F272C5FA5", "ECS Status", "Export Control Status");
			zTextBoxColumnStyleInfo9.ColumnName = "CH_ExitedStatus";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("84B3AA23-B2C1-48EC-AAE1-A3FA421D0B58", "VAA Trig. Point");
			zTextBoxColumnStyleInfo10.ColumnName = "CH_TriggeringPointForValidation";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("44759F34-D7FE-4C86-BEF5-596E0237DF34", "Export Exit Type");
			zTextBoxColumnStyleInfo11.ColumnName = Schema.ExportExitType;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("4cf497fe-fc3a-4016-b20e-06a83efb5f8b", "LRN/Correlation ID");
			zTextBoxColumnStyleInfo12.ColumnName = "CorrelationID";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			// 
			// EntryHeaderFilterUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "EntryHeaderFilterUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 471, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
