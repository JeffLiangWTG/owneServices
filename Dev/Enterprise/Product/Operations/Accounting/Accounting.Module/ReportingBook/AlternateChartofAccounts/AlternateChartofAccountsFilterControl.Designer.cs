namespace Enterprise.Accounting.Module
{
	public partial class AlternateChartofAccountsFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AlternateChartFormatsControl = new Enterprise.Accounting.GUI.AlternateChartFormatsControl();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AlternateChartFormatsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_IsGlobal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_IsFixedLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_BalanceSheetStyle)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("7041BD1A-0057-4673-A989-23249B6B8F58", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "AAC_Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("CF7130DE-B498-43F0-90D1-4DA367B903D6", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "AAC_Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("69DE0B55-8ACB-44B9-B568-9E9E013FB3AC", "Is Global");
			zCheckBoxColumnStyleInfo1.ColumnName = "AAC_IsGlobal";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("A7CB156E-DB05-4875-A82A-33AAF4403113", "Fixed Length");
			zCheckBoxColumnStyleInfo2.ColumnName = "AAC_IsFixedLength";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("583d6015-2b9d-4fa3-bd5c-40a7d01198d5", "Balance Sheet Style");
			zTextBoxColumnStyleInfo3.ColumnName = "AAC_BalanceSheetStyle";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 503, true);
			this.grid.TabIndex = 3;
			this.grid.AfterBind += new System.EventHandler(this.FilteredGrid_AfterBind);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccAlternateChart);
			// 
			// GridSplitter
			// 
			this.GridSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.GridSplitter.DoNotSaveSplitterLayout = false;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 320, true);
			this.GridSplitter.MinExtra = 0;
			this.GridSplitter.MinSize = 0;
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 8, true);
			this.GridSplitter.TabIndex = 16;
			this.GridSplitter.TabStop = false;
			this.GridSplitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.Splitter_SplitterMoved);
			// 
			// AlternateChartFormatsControl
			// 
			this.AlternateChartFormatsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AlternateChartFormatsControl, "AlternateChartFormats");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AlternateChartFormats)).SyncRoot)))));
			this.AlternateChartFormatsControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AlternateChartFormatsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.AlternateChartFormatsControl.Name = "AlternateChartFormatsControl";
			this.AlternateChartFormatsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 200, true);
			this.AlternateChartFormatsControl.TabIndex = 18;
			// 
			// AlternateChartofAccountsFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.AlternateChartFormatsControl);
			this.Name = "AlternateChartofAccountsFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 528, true);
			this.Controls.SetChildIndex(this.AlternateChartFormatsControl, 0);
			this.Controls.SetChildIndex(this.CoveringLabel, 0);
			this.Controls.SetChildIndex(this.GridSplitter, 0);
			this.Controls.SetChildIndex(this.ToolStripPermissionsLabel, 0);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.grid, 0);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AlternateChartFormatsControl.ResumeLayout(true);
			this.AlternateChartFormatsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		CargoWise.Windows.UI.KSplitter GridSplitter;
	}
}
