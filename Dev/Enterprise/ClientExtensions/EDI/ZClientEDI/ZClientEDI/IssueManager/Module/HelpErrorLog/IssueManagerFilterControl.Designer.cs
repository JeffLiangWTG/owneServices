namespace Enterprise.Client.EDI.IssueManager.Module
{
	partial class IssueManagerFilterControl
	{
		public CargoWise.Windows.UI.KTextBox PreviewTextBox;
		CargoWise.Windows.UI.KSplitter Splitter;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PreviewTextBox = new CargoWise.Windows.UI.KTextBox();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Issue Number";
			zTextBoxColumnStyleInfo1.ColumnName = "HE_IssueNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "First Processed";
			zDateEditColumnStyleInfo1.ColumnName = "HE_FirstProcessedLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "First Reported";
			zDateEditColumnStyleInfo2.ColumnName = "HE_FirstReportedLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.Caption = "Last Reported";
			zDateEditColumnStyleInfo3.ColumnName = "HE_LastReportedLocal";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.Caption = "Latest EXE";
			zDateEditColumnStyleInfo4.ColumnName = "HE_LastEXEVersionDateLocal";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.Caption = "Fixed";
			zDateEditColumnStyleInfo5.ColumnName = "HE_FixedDateLocal";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Exception Type";
			zTextBoxColumnStyleInfo2.ColumnName = "HE_ExceptionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.Caption = "Exception Source";
			zTextBoxColumnStyleInfo3.ColumnName = "HE_ExceptionSource";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.Caption = "Exception Message";
			zTextBoxColumnStyleInfo4.ColumnName = "HE_ExceptionMessageWithoutLineBreaks";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Fail Count";
			zCalcEditColumnStyleInfo1.ColumnName = "HE_FailCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.Caption = "Client Visible";
			zCheckBoxColumnStyleInfo1.ColumnName = "HE_IsClientVisible";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.Caption = "Work Item";
			zCheckBoxColumnStyleInfo2.ColumnName = "HasWorkItems";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "First Key (Randomly Loaded)";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo5.ColumnName = "FirstKeyString";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "First Key Hash";
			zCalcEditColumnStyleInfo2.ColumnName = "FirstKeyHashCode";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Key Count";
			zCalcEditColumnStyleInfo3.ColumnName = "KeyCount";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Fixed Count";
			zCalcEditColumnStyleInfo4.ColumnName = "HE_FixedCount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "First EXE";
			zTextBoxColumnStyleInfo6.ColumnName = "HE_FirstEXEVersionDateLocal";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.Caption = "First Version Number";
			zTextBoxColumnStyleInfo7.ColumnName = "HE_FirstVersionNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.Caption = "Last Version Number";
			zTextBoxColumnStyleInfo8.ColumnName = "HE_LastVersionNumber";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 385, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog);
			// 
			// PreviewTextBox
			// 
			this.PreviewTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PreviewTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 432, true);
			this.PreviewTextBox.Multiline = true;
			this.PreviewTextBox.Name = "PreviewTextBox";
			this.PreviewTextBox.ReadOnly = true;
			this.PreviewTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.PreviewTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 64, true);
			this.PreviewTextBox.TabIndex = 5;
			// 
			// Splitter
			// 
			this.Splitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 422, true);
			this.Splitter.MinExtra = 0;
			this.Splitter.MinSize = 0;
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 10, true);
			this.Splitter.TabIndex = 6;
			this.Splitter.TabStop = false;
			this.Splitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.Splitter_SplitterMoved);
			// 
			// IssueManagerFilterControl
			// 
			this.Controls.Add(this.Splitter);
			this.Controls.Add(this.PreviewTextBox);
			this.Name = "IssueManagerFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 496, true);
			this.Controls.SetChildIndex(this.PreviewTextBox, 0);
			this.Controls.SetChildIndex(this.Splitter, 0);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.grid, 0);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
