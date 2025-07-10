using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Module
{
	public partial class PrintQueueFilterControl : ZFilterStripControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "SQ_DisplayName";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|e2881c9a-be75-4fa0-98c3-2591262c571d", "Queue Name");
			zTextBoxColumnStyleInfo2.ColumnName = "SQ_QueueName";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|ca3ac0f7-1a34-48f1-b7af-a7ee1643a0bc", "Server Name");
			zTextBoxColumnStyleInfo3.ColumnName = "SQ_ServerName";
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = null;
			zCheckBoxColumnStyleInfo1.ColumnName = "SQ_AllowPrinting";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|6f8e4baf-c135-4c2d-9acf-2391ede53d84", "Scale");
			zCalcEditColumnStyleInfo1.ColumnName = "SQ_Scale";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|6854cf0d-9f6b-42d1-ad91-c69144486ef5", "Left Margin");
			zCalcEditColumnStyleInfo2.ColumnName = "SQ_LeftMargin";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|315879b6-966a-45e6-83f9-9a4732cca72d", "Margins");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|31a50add-8812-45fa-85f0-0e08aec05d26", "Top Margin");
			zCalcEditColumnStyleInfo3.ColumnName = "SQ_TopMargin";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|315879b6-966a-45e6-83f9-9a4732cca72d", "Margins");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|61FB18F7-41B5-40B6-BBFF-FB90D81FFE6E", "Horizontal Scale");
			zCalcEditColumnStyleInfo4.ColumnName = "SQ_ColumnScale";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|bed203aa-04c2-4389-a64f-e76bc335a187", "Columns");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|DB4DF429-5C74-4646-B8AF-035D528D5888", "Vertical Scale");
			zCalcEditColumnStyleInfo5.ColumnName = "SQ_RowScale";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|bed203aa-04c2-4389-a64f-e76bc335a187", "Columns");
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|2b4ba11d-3bcd-4ba8-9f17-5018cd286173", "Offline Since");
			zDateEditColumnStyleInfo1.ColumnName = "SQ_QueueDeleted";
			zDateEditColumnStyleInfo1.ToolTip = "The time that this Print Queue was last accessible. If blank, the queue is still " +
	"active.";
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|bcb24018-599b-420b-b6f9-710e86ba14da", "Last Used Date (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "SQ_LastUsedDateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|c0dc7c7f-883d-43c8-bd8e-37f7278d27a6", "Last Used Date (Local)");
			zDateEditColumnStyleInfo3.ColumnName = "SQ_LastUsedDateTimeLocal";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo2.Caption = null;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|11b5c30d-0053-44ad-94c1-135faea0ea5d", "Suppress Letterhead");
			zCheckBoxColumnStyleInfo2.ColumnName = "SQ_SupressLetterhead";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|71f3bd8b-0d6b-4623-bb3d-602240efe6d1", "Paper Type");
			zTextBoxColumnStyleInfo4.ColumnName = "SQ_PaperName";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|634733c6-cb6c-4dbf-b6e6-705b8134cbb5", "Paper Height");
			zCalcEditColumnStyleInfo6.ColumnName = "SQ_PaperHeight";
			zCalcEditColumnStyleInfo6.Decimals = 0;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.Caption = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("PrintQueueFilterControl|bb07f23d-3be5-42c3-be68-3cf22b62a1a8", "Paper Width");
			zCalcEditColumnStyleInfo7.ColumnName = "SQ_PaperWidth";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 46, true);
			this.grid.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue);
			// 
			// PrintQueueFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "PrintQueueFilterControl";
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
