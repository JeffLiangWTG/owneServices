namespace Enterprise.Client.EDI.IssueManager.GUI
{
	partial class OccurrencesControl
	{
		Enterprise.ZArchitecture.ZLabel OccurrencesLabel;
		CargoWise.Windows.UI.KSplitter BottomSplitter;
		CargoWise.Windows.UI.KPanel OccurrencesPanel;
		CargoWise.Windows.UI.KPanel RawDetailsPanel;
		protected Enterprise.ZArchitecture.ZGrid OccurrencesGrid;
		CargoWise.Windows.UI.KPanel operatePanel;
		internal Enterprise.ZArchitecture.GUI.ZButton moreDetailsButton;
		internal Enterprise.ZArchitecture.GUI.ZButton viewLocallyButton;
		protected internal Enterprise.ZArchitecture.GUI.ZRichTextBox richTextBox;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OccurrencesControl));
			this.OccurrencesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OccurrencesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.moreDetailsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.viewLocallyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.richTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.operatePanel = new CargoWise.Windows.UI.KPanel();
			this.RawDetailsPanel = new CargoWise.Windows.UI.KPanel();
			this.OccurrencesPanel = new CargoWise.Windows.UI.KPanel();
			this.BottomSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OccurrencesGrid)).BeginInit();
			this.operatePanel.SuspendLayout();
			this.RawDetailsPanel.SuspendLayout();
			this.OccurrencesPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog);
			// 
			// OccurrencesLabel
			// 
			this.OccurrencesLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OccurrencesLabel, "OccurrencesText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).OccurrencesText)));
			this.OccurrencesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.OccurrencesLabel.Name = "OccurrencesLabel";
			this.OccurrencesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 13, true);
			this.OccurrencesLabel.TabIndex = 0;
			this.OccurrencesLabel.Text = "Occurrences";
			// 
			// OccurrencesGrid
			// 
			this.OccurrencesGrid.AllowBeginDrag = false;
			this.OccurrencesGrid.AllowDragDropWithChanges = false;
			this.OccurrencesGrid.AllowNavigation = false;
			this.OccurrencesGrid.AllowReadOnlyToModifyTabStop = true;
			this.OccurrencesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OccurrencesGrid, "Occurrences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).HO_ExceptionDateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).HO_ExceptionID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).HO_Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).HO_ServerName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).DatabaseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).ReleaseBuild.ReleaseDisplayText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).HO_EXEDateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).HO_VersionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).LicenceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).FinalKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).SessionIdAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).SequenceAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.HelpErrorLogOccurrence)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Occurrences)).SyncRoot)).TestRigOrigin)));
			this.OccurrencesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.Caption = "Exception Date";
			zDateEditColumnStyleInfo1.ColumnName = "HO_ExceptionDateTimeLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.Caption = "Exception ID";
			zTextBoxColumnStyleInfo1.ColumnName = "HO_ExceptionID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Company";
			zTextBoxColumnStyleInfo2.ColumnName = "HO_Company";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo3.Caption = "Server";
			zTextBoxColumnStyleInfo3.ColumnName = "HO_ServerName";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "Database";
			zTextBoxColumnStyleInfo4.ColumnName = "DatabaseCode";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.Caption = "Release Information";
			zTextBoxColumnStyleInfo5.ColumnName = "ReleaseBuild+ReleaseDisplayText";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo2.Caption = "Exe Date";
			zDateEditColumnStyleInfo2.ColumnName = "HO_EXEDateTimeLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.Caption = "Version No.";
			zTextBoxColumnStyleInfo6.ColumnName = "HO_VersionNumber";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.Caption = "Licence";
			zTextBoxColumnStyleInfo7.ColumnName = "LicenceCode";
			zMultiLineTextBoxColumnInfo1.Caption = "Final Key";
			zMultiLineTextBoxColumnInfo1.ColumnName = "FinalKey";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.Caption = "Session ID";
			zTextBoxColumnStyleInfo8.ColumnName = "SessionIdAsText";
			zTextBoxColumnStyleInfo9.Caption = "Seq#";
			zTextBoxColumnStyleInfo9.ColumnName = "SequenceAsText";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo10.Caption = "Test Rig Origin";
			zTextBoxColumnStyleInfo10.ColumnName = "TestRigOrigin";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			this.OccurrencesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OccurrencesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.OccurrencesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.OccurrencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.OccurrencesGrid.CopySelectedRowsAllowed = true;
			this.OccurrencesGrid.GridId = "6f46d90b-ed13-49e8-a1cf-3a3fd234851b";
			this.OccurrencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OccurrencesGrid.LayoutKey = "LogsGrid";
			this.OccurrencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.OccurrencesGrid.Name = "OccurrencesGrid";
			this.OccurrencesGrid.ReadOnly = true;
			this.OccurrencesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OccurrencesGrid.ShouldSetErrorsOnTabPage = false;
			this.OccurrencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 98, true);
			this.OccurrencesGrid.TabIndex = 1;
			this.OccurrencesGrid.TabStop = false;
			this.OccurrencesGrid.DoubleClick += new System.EventHandler(this.OccurrencesGrid_DoubleClick);
			// 
			// moreDetailsButton
			// 
			this.moreDetailsButton.TabIndex = 2;
			this.moreDetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.moreDetailsButton.Name = "MoreDetailsButton";
			this.moreDetailsButton.Text = "More Details";
			this.moreDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 30, true);
			this.moreDetailsButton.Click += MoreDetailsButton_Click;
			//
			// viewLocallyButton
			// 
			this.viewLocallyButton.TabIndex = 3;
			this.viewLocallyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 0, true);
			this.viewLocallyButton.Name = "ViewLocallyButton";
			this.viewLocallyButton.Text = "View Locally";
			this.viewLocallyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 30, true);
			this.viewLocallyButton.Click += ViewLocallyButton_Click;
			// 
			// OperatePanel
			// 
			this.operatePanel.Name = "OperatePanel";
			this.operatePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.operatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 30, true);
			this.operatePanel.Controls.Add(this.moreDetailsButton);
			this.operatePanel.Controls.Add(this.viewLocallyButton);
			// 
			// richTextBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.richTextBox, false);
			this.richTextBox.IsToolBarVisible = false;
			this.richTextBox.ReadOnly = true;
			this.richTextBox.ForcedBackColor = System.Drawing.Color.White;
			this.richTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
			this.richTextBox.WordWrap = false;
			this.richTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 33, true);
			this.richTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 167, true);
			this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// RawDetailsPanel
			// 
			this.RawDetailsPanel.Controls.Add(this.richTextBox);
			this.RawDetailsPanel.Controls.Add(this.operatePanel);
			this.RawDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RawDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 124, true);
			this.RawDetailsPanel.Name = "RawDetailsPanel";
			this.RawDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 200, true);
			this.RawDetailsPanel.TabIndex = 4;
			this.RawDetailsPanel.TabStop = false;
			// 
			// OccurrencesPanel
			// 
			this.OccurrencesPanel.Controls.Add(this.OccurrencesGrid);
			this.OccurrencesPanel.Controls.Add(this.OccurrencesLabel);
			this.OccurrencesPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OccurrencesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OccurrencesPanel.Name = "OccurrencesPanel";
			this.OccurrencesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 120, true);
			this.OccurrencesPanel.TabIndex = 0;
			// 
			// BottomSplitter
			// 
			this.BottomSplitter.BackColor = System.Drawing.SystemColors.ControlDark;
			this.BottomSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.BottomSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 120, true);
			this.BottomSplitter.Name = "BottomSplitter";
			this.BottomSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 4, true);
			this.BottomSplitter.TabIndex = 1;
			this.BottomSplitter.TabStop = false;
			// 
			// OccurrencesControl
			// 
			this.Controls.Add(this.RawDetailsPanel);
			this.Controls.Add(this.BottomSplitter);
			this.Controls.Add(this.OccurrencesPanel);
			this.Name = "OccurrencesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 324, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OccurrencesGrid)).EndInit();
			this.operatePanel.ResumeLayout(false);
			this.RawDetailsPanel.ResumeLayout(false);
			this.OccurrencesPanel.ResumeLayout(false);
			this.OccurrencesPanel.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
