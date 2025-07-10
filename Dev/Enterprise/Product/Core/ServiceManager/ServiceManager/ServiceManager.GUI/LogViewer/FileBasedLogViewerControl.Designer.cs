namespace Enterprise.ServiceManager.GUI
{
	partial class FileBasedLogViewerControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
            this.LogFileGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
            this.LogFileLabel = new Enterprise.ZArchitecture.ZLabel();
            this.kPanel1 = new CargoWise.Windows.UI.KPanel();
            this.taskTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TaskTypeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
            this.eventGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
            this.EventLabel = new Enterprise.ZArchitecture.ZLabel();
            this.MessageTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogFileGrid)).BeginInit();
            this.LogFileGrid.SuspendLayout();
            this.kPanel1.SuspendLayout();
            this.taskTypeDropEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventGrid)).BeginInit();
            this.eventGrid.SuspendLayout();
            this.MessageTextBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.FileBasedLogViewer);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.LogFileGrid);
            this.splitContainer1.Panel1.Controls.Add(this.LogFileLabel);
            this.splitContainer1.Panel1.Controls.Add(this.kPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 500, true);
            this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
            this.splitContainer1.SplitterWidth = 17;
            this.splitContainer1.TabIndex = 0;
            // 
            // LogFileGrid
            // 
            this.LogFileGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.LogFileGrid, "LogFileList");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).Host)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).Name)));
            this.LogFileGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|531A7CF6-5C09-43DF-B368-12FE416CF6C5", "Hostname");
            zTextBoxColumnStyleInfo1.ColumnName = "Host";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsMandatory = true;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|AAF5C4C3-22F3-4553-835E-3B2F84C77BA2", "Log file");
            zTextBoxColumnStyleInfo2.ColumnName = "Name";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsMandatory = true;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            this.LogFileGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.LogFileGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.LogFileGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogFileGrid.GridId = "f1eb90e4-0b3b-48c7-808c-b48e4fe139b4";
            this.LogFileGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LogFileGrid.LayoutKey = "LogFileGrid";
            this.LogFileGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
            this.LogFileGrid.Name = "LogFileGrid";
            this.LogFileGrid.ReadOnly = true;
            this.LogFileGrid.ShouldSetErrorsOnTabPage = false;
            this.LogFileGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 424, true);
            this.LogFileGrid.TabIndex = 1;
            // 
            // LogFileLabel
            // 
            this.LogFileLabel.AutoSize = true;
            this.LogFileLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|4c49a3c7-9007-4b2e-9ad1-760abd94319d", "Log Files");
            this.LogFileLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.LogFileLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.LogFileLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
            this.LogFileLabel.Name = "LogFileLabel";
            this.LogFileLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
            this.LogFileLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 25, true);
            this.LogFileLabel.TabIndex = 0;
            this.LogFileLabel.UseMnemonic = false;
            // 
            // kPanel1
            // 
            this.kPanel1.Controls.Add(this.taskTypeDropEdit);
            this.kPanel1.Controls.Add(this.TaskTypeLabel);
            this.kPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.kPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.kPanel1.Name = "kPanel1";
            this.kPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 51, true);
            this.kPanel1.TabIndex = 0;
            // 
            // taskTypeDropEdit
            // 
            this.taskTypeDropEdit.AllowDrop = true;
            this.taskTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.taskTypeDropEdit, "TaskType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).TaskType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).TaskTypes)));
            this.taskTypeDropEdit.BindToList = "TaskTypes";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.taskTypeDropEdit, false);
            this.taskTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 23, true);
            this.taskTypeDropEdit.Name = "taskTypeDropEdit";
            this.taskTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
            this.taskTypeDropEdit.TabIndex = 3;
            // 
            // TaskTypeLabel
            // 
            this.TaskTypeLabel.AutoSize = true;
            this.TaskTypeLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|569ef1d0-4c77-4eb2-b0a1-e2dd92bfe792", "Service Task");
            this.TaskTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TaskTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
            this.TaskTypeLabel.Name = "TaskTypeLabel";
            this.TaskTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
            this.TaskTypeLabel.TabIndex = 2;
            this.TaskTypeLabel.UseMnemonic = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.eventGrid);
            this.splitContainer2.Panel1.Controls.Add(this.EventLabel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.MessageTextBox);
            this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 500, true);
            this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(320);
            this.splitContainer2.SplitterWidth = 17;
            this.splitContainer2.TabIndex = 0;
            // 
            // eventGrid
            // 
            this.eventGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.eventGrid, "LogFileList.EventList");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).EventList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.EventRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).EventList)).SyncRoot)).SequenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ServiceManager.Business.EventRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).EventList)).SyncRoot)).DateTime)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ServiceManager.Business.EventRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).EventList)).SyncRoot)).DateTimeLocal)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.EventRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).EventList)).SyncRoot)).Type)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.EventRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).EventList)).SyncRoot)).ProcessId)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.EventRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).EventList)).SyncRoot)).Message)));
            this.eventGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("1f194e97-5214-4f2f-b2d1-b5b613240385", "Sequence Number");
            zCalcEditColumnStyleInfo1.ColumnName = "SequenceNumber";
            zCalcEditColumnStyleInfo1.Decimals = 0;
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|afdf65d2-d52b-409c-8bfd-c4db084a8bbd", "Date / Time (UTC)");
            zDateEditColumnStyleInfo1.ColumnName = "DateTime";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.IsMandatory = true;
            zDateEditColumnStyleInfo1.IsReadOnly = true;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|e6b773d8-e3f7-4d60-8385-09e4bda29723", "Date / Time (Local)");
            zDateEditColumnStyleInfo2.ColumnName = "DateTimeLocal";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.IsReadOnly = true;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|8c4c2719-2e0d-4f39-8409-011ebd6961ad", "Type");
            zTextBoxColumnStyleInfo3.ColumnName = "Type";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("7878EDF1-A8B1-4EF4-AC4E-FF85FCF32E47", "Process ID");
            zCalcEditColumnStyleInfo2.ColumnName = "ProcessId";
            zCalcEditColumnStyleInfo2.Decimals = 0;
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|54082bfe-f379-4221-b270-d8248207d4a5", "Message");
            zTextBoxColumnStyleInfo4.ColumnName = "Message";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsReadOnly = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
            this.eventGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.eventGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.eventGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.eventGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.eventGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.eventGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.eventGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eventGrid.ForceShowExportToExcelMenuItem = true;
            this.eventGrid.GridId = "cd610f8e-5b20-4fdb-973f-40b2b65cc232";
            this.eventGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.eventGrid.IsWholeRowSelectedOnClick = true;
            this.eventGrid.LayoutKey = "zGrid1";
            this.eventGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
            this.eventGrid.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
            this.eventGrid.Name = "eventGrid";
            this.eventGrid.ReadOnly = true;
            this.eventGrid.ShouldSetErrorsOnTabPage = false;
            this.eventGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 295, true);
            this.eventGrid.TabIndex = 1;
            this.eventGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.eventGrid_ColourDeciding);
            // 
            // EventLabel
            // 
            this.EventLabel.AutoSize = true;
            this.EventLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|9c840232-68ec-4f5c-95d6-4eae2361f16c", "Events");
            this.EventLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.EventLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.EventLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.EventLabel.Name = "EventLabel";
            this.EventLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
            this.EventLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 25, true);
            this.EventLabel.TabIndex = 0;
            this.EventLabel.UseMnemonic = false;
            // 
            // MessageTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageTextBox, "LogFileList.EventList.FormattedMessageAsBlob");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.ServiceManager.Business.EventRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.LogFileRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(null)).LogFileList)).SyncRoot)).EventList)).SyncRoot)).FormattedMessageAsBlob)));
            this.MessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageTextBox.IsToolBarVisible = false;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextBox, false);
            this.MessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MessageTextBox.MaxLength = 10000000;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ParentZForm = null;
            this.MessageTextBox.PopupFormCaption = null;
            this.MessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 163, true);
            this.MessageTextBox.TabIndex = 0;
            // 
            // FileBasedLogViewerControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.splitContainer1);
            this.Name = "FileBasedLogViewerControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 500, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogFileGrid)).EndInit();
            this.LogFileGrid.ResumeLayout(false);
            this.LogFileGrid.PerformLayout();
            this.kPanel1.ResumeLayout(false);
            this.kPanel1.PerformLayout();
            this.taskTypeDropEdit.ResumeLayout(true);
            this.taskTypeDropEdit.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventGrid)).EndInit();
            this.eventGrid.ResumeLayout(false);
            this.eventGrid.PerformLayout();
            this.MessageTextBox.ResumeLayout(true);
            this.MessageTextBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private Enterprise.ZArchitecture.GUI.ZRichTextBox MessageTextBox;
		private Enterprise.ZArchitecture.ZLabel EventLabel;
		private Enterprise.ZArchitecture.GUI.ZDisplayGrid eventGrid;
		private CargoWise.Windows.UI.KPanel kPanel1;
		private Enterprise.ZArchitecture.ZLabel TaskTypeLabel;
		private Enterprise.ZArchitecture.ZLabel LogFileLabel;
		private Enterprise.ZArchitecture.GUI.ZDisplayGrid LogFileGrid;
		private ZArchitecture.GUI.ZDropEdit taskTypeDropEdit;
	}
}
