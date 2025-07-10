using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Module
{
	public partial class PrintJobFilterControl : ZFilterStripControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|8417e2e9-30cb-41e6-98d2-1e8e8984b125", "Document Name");
			zTextBoxColumnStyleInfo1.ColumnName = "SP_DocumentName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(235);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "SP_EmailSubjectLine";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(235);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|7884d011-0d73-4873-bd3d-569c8edc2f06", "Run At (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "SP_RunDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(94);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|373d6f6c-81d4-48a1-9740-4e94fbfab0a2", "Run At (Local)");
			zDateEditColumnStyleInfo2.ColumnName = "SP_RunDateTimeLocal";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(94);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|5bfd0bdf-5e96-4e89-921b-dc1e4295bea4", "Job Type");
			zTextBoxColumnStyleInfo3.ColumnName = "SP_JobTypeDisplayName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|eaef4c44-95f4-4bbd-bf47-f2d1ca446cfb", "Destination");
			zTextBoxColumnStyleInfo4.ColumnName = "SP_Destination";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153);
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|ad80bf8e-a06c-48cf-81d0-bafbbb750e9a", "Printed By");
			zTextBoxColumnStyleInfo5.ColumnName = "SP_UserLoginName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|7075797b-946f-4eb3-8bc7-8074a97aa663", "Retries");
			zCalcEditColumnStyleInfo1.ColumnName = "SP_RetryAttempts";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(41);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|050a72b3-db16-449c-8363-bef0c3b156c3", "Email Attachment Format");
			zTextBoxColumnStyleInfo6.ColumnName = "SP_EmailAttachmentFormat";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|fd6a5a73-925d-4a68-a3ee-bb5f756e9826", "User Full Name");
			zTextBoxColumnStyleInfo7.ColumnName = "SP_UserFullName";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = null;
			zCheckBoxColumnStyleInfo1.ColumnName = "SP_IsLocalCulture";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = null;
			zTextBoxColumnStyleInfo8.ColumnName = "SQ_ServerName";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|B4EC5F2D-89D8-406E-A657-9454E23FD815", "Status");
			zTextBoxColumnStyleInfo9.ColumnName = "SP_StatusDisplayName";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);

			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|9B0B30C3-21A0-40B5-8237-1E70DB0D212B", "Sign By");
			zTextBoxColumnStyleInfo10.ColumnName = "SP_SignBy";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);

			zTextBoxColumnStyleInfo11.Caption = null;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintJobFilterControl|AD5FB1C3-42DF-4CC7-B965-4840B3FBB3EC", "Fail Reason");
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "SP_FailureReason";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(235);

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 344, true);
			this.grid.TabIndex = 8;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.StmPrintJob);
			// 
			// PrintJobFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "PrintJobFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 456, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#region Dispose

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
