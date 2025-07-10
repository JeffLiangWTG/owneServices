using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using System.Windows.Forms;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAForm
	{
		protected override void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			this.buttonsPanel = new ZArchitecture.GUI.ZPanel();
			this.oKButton = new ZArchitecture.GUI.ZButton();
			this.cPQACancelButton = new ZArchitecture.GUI.ZButton();
			this.mainPanel = new ZArchitecture.GUI.ZPanel();
			this.mainDownPanel = new ZArchitecture.GUI.ZPanel();
			this.cPDecQuestionsPanel = new ZArchitecture.GUI.ZPanel();
			this.cPDecQuestionsGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.cpqAsForLineControl1 = new CPQAsForLineControl();
			this.splitter2 = new CargoWise.Windows.UI.KSplitter();
			this.declarationQuestionsGroupBoxPanel = new ZArchitecture.GUI.ZPanel();
			this.declarationQuestionsGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.cpqAsForHeaderControl2 = new CPQAsForHeaderControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.mainUpPanel = new ZArchitecture.GUI.ZPanel();
			this.mainUpEntryPanel = new ZArchitecture.GUI.ZPanel();
			this.entryHeadersGrid = new ZGrid();
			this.mainUpDropEditPanel = new ZArchitecture.GUI.ZPanel();
			this.zDropEdit1 = new ZArchitecture.GUI.ZDropEdit();
			this.displayOptionLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.buttonsPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.mainDownPanel.SuspendLayout();
			this.cPDecQuestionsPanel.SuspendLayout();
			this.cPDecQuestionsGroupBox.SuspendLayout();
			this.declarationQuestionsGroupBoxPanel.SuspendLayout();
			this.declarationQuestionsGroupBox.SuspendLayout();
			this.mainUpPanel.SuspendLayout();
			this.mainUpEntryPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.entryHeadersGrid)).BeginInit();
			this.mainUpDropEditPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 565, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.MinWidth = 0;
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			// 
			// ButtonsPanel
			// 
			this.buttonsPanel.Controls.Add(this.oKButton);
			this.buttonsPanel.Controls.Add(this.cPQACancelButton);
			this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 525, true);
			this.buttonsPanel.Name = "ButtonsPanel";
			this.buttonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 40, true);
			this.buttonsPanel.TabIndex = 1;
			// 
			// OKButton
			// 
			this.oKButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 8, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.oKButton.TabIndex = 0;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// CPQACancelButton
			// 
			this.cPQACancelButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cPQACancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 8, true);
			this.cPQACancelButton.Name = "CPQACancelButton";
			this.cPQACancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.cPQACancelButton.TabIndex = 1;
			this.cPQACancelButton.Text = "Cancel";
			this.cPQACancelButton.Click += new EventHandler(this.CPQACancelButton_Click);
			// 
			// MainPanel
			// 
			this.mainPanel.Controls.Add(this.mainDownPanel);
			this.mainPanel.Controls.Add(this.splitter1);
			this.mainPanel.Controls.Add(this.mainUpPanel);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "MainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 525, true);
			this.mainPanel.TabIndex = 0;
			// 
			// MainDownPanel
			// 
			this.mainDownPanel.Controls.Add(this.cPDecQuestionsPanel);
			this.mainDownPanel.Controls.Add(this.splitter2);
			this.mainDownPanel.Controls.Add(this.declarationQuestionsGroupBoxPanel);
			this.mainDownPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainDownPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 141, true);
			this.mainDownPanel.Name = "MainDownPanel";
			this.mainDownPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 384, true);
			this.mainDownPanel.TabIndex = 1;
			// 
			// CPDecQuestionsPanel
			// 
			this.cPDecQuestionsPanel.Controls.Add(this.cPDecQuestionsGroupBox);
			this.cPDecQuestionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cPDecQuestionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cPDecQuestionsPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 172, true);
			this.cPDecQuestionsPanel.Name = "CPDecQuestionsPanel";
			this.cPDecQuestionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 179, true);
			this.cPDecQuestionsPanel.TabIndex = 0;
			// 
			// CPDecQuestionsGroupBox
			// 
			this.cPDecQuestionsGroupBox.Controls.Add(this.cpqAsForLineControl1);
			this.cPDecQuestionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cPDecQuestionsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cPDecQuestionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cPDecQuestionsGroupBox.Name = "CPDecQuestionsGroupBox";
			this.cPDecQuestionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 179, true);
			this.cPDecQuestionsGroupBox.TabIndex = 0;
			this.cPDecQuestionsGroupBox.TabStop = false;
			this.cPDecQuestionsGroupBox.Text = "CP Dec Questions";
			// 
			// cpqAsForLineControl1
			// 
			this.cpqAsForLineControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cpqAsForLineControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.cpqAsForLineControl1.Name = "cpqAsForLineControl1";
			this.cpqAsForLineControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 160, true);
			this.cpqAsForLineControl1.TabIndex = 0;
			// 
			// splitter2
			// 
			this.splitter2.BackColor = System.Drawing.Color.White;
			this.splitter2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 179, true);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 5, true);
			this.splitter2.TabIndex = 1;
			this.splitter2.TabStop = false;
			// 
			// DeclarationQuestionsGroupBoxPanel
			// 
			this.declarationQuestionsGroupBoxPanel.Controls.Add(this.declarationQuestionsGroupBox);
			this.declarationQuestionsGroupBoxPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.declarationQuestionsGroupBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 184, true);
			this.declarationQuestionsGroupBoxPanel.Name = "DeclarationQuestionsGroupBoxPanel";
			this.declarationQuestionsGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 200, true);
			this.declarationQuestionsGroupBoxPanel.TabIndex = 1;
			// 
			// DeclarationQuestionsGroupBox
			// 
			this.declarationQuestionsGroupBox.Controls.Add(this.cpqAsForHeaderControl2);
			this.declarationQuestionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.declarationQuestionsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.declarationQuestionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.declarationQuestionsGroupBox.Name = "DeclarationQuestionsGroupBox";
			this.declarationQuestionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 200, true);
			this.declarationQuestionsGroupBox.TabIndex = 0;
			this.declarationQuestionsGroupBox.TabStop = false;
			this.declarationQuestionsGroupBox.Text = "Declaration Questions";
			// 
			// cpqAsForHeaderControl2
			// 
			this.cpqAsForHeaderControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cpqAsForHeaderControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.cpqAsForHeaderControl2.Name = "cpqAsForHeaderControl2";
			this.cpqAsForHeaderControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 181, true);
			this.cpqAsForHeaderControl2.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.splitter1.MinSize = 30;
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 5, true);
			this.splitter1.TabIndex = 0;
			this.splitter1.TabStop = false;
			// 
			// MainUpPanel
			// 
			this.mainUpPanel.Controls.Add(this.mainUpEntryPanel);
			this.mainUpPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.mainUpPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainUpPanel.Name = "MainUpPanel";
			this.mainUpPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 136, true);
			this.mainUpPanel.TabIndex = 0;
			// 
			// MainUpEntryPanel
			// 
			this.mainUpEntryPanel.Controls.Add(this.entryHeadersGrid);
			this.mainUpEntryPanel.Controls.Add(this.mainUpDropEditPanel);
			this.mainUpEntryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainUpEntryPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 136, true);
			this.mainUpEntryPanel.Name = "MainUpEntryPanel";
			this.mainUpEntryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 136, true);
			this.mainUpEntryPanel.TabIndex = 0;
			// 
			// EntryHeadersGrid
			// 
			this.entryHeadersGrid.AllowNavigation = false;
			this.entryHeadersGrid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))));
			this.entryHeadersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Reference No";
			zTextBoxColumnStyleInfo1.ColumnName = "CH_BGMReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Caption = "Entry Number";
			zTextBoxColumnStyleInfo2.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Caption = "Message Status Description";
			zTextBoxColumnStyleInfo3.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.entryHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.entryHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.entryHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.entryHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.entryHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.entryHeadersGrid.LayoutKey = "EntryHeadersGrid";
			this.entryHeadersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.entryHeadersGrid.Name = "EntryHeadersGrid";
			this.entryHeadersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 98, true);
			this.entryHeadersGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CH_BGMReferenceInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CH_BGMReference)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).EntryNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).EntryNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).MessageStatusDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).MessageStatusDescription)));
			// 
			// MainUpDropEditPanel
			// 
			this.mainUpDropEditPanel.Controls.Add(this.zDropEdit1);
			this.mainUpDropEditPanel.Controls.Add(this.displayOptionLabel);
			this.mainUpDropEditPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.mainUpDropEditPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 98, true);
			this.mainUpDropEditPanel.Name = "MainUpDropEditPanel";
			this.mainUpDropEditPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 38, true);
			this.mainUpDropEditPanel.TabIndex = 1;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.BindTo = "CPDecQuestionViewType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionViewTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionViewType)));
			this.zDropEdit1.BindToList = "Lookups+CPDecQuestionViewTypeList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((CusEntryHeader)(((object)(((CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Lookups.CPDecQuestionViewTypeList)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 8, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// DisplayOptionLabel
			// 
			this.displayOptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 8, true);
			this.displayOptionLabel.Name = "DisplayOptionLabel";
			this.displayOptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 23, true);
			this.displayOptionLabel.TabIndex = 0;
			this.displayOptionLabel.Text = "Display Option for CP Dec Questions:";
			// 
			// CPQAForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 589, true);
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.buttonsPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCol" +
				"lection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 616, true);
			this.Name = "CPQAForm";
			this.Text = "CPQA";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.buttonsPanel, 0);
			this.Controls.SetChildIndex(this.mainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.buttonsPanel.ResumeLayout(false);
			this.mainPanel.ResumeLayout(false);
			this.mainDownPanel.ResumeLayout(false);
			this.cPDecQuestionsPanel.ResumeLayout(false);
			this.cPDecQuestionsGroupBox.ResumeLayout(false);
			this.declarationQuestionsGroupBoxPanel.ResumeLayout(false);
			this.declarationQuestionsGroupBox.ResumeLayout(false);
			this.mainUpPanel.ResumeLayout(false);
			this.mainUpEntryPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.entryHeadersGrid)).EndInit();
			this.mainUpDropEditPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		ZArchitecture.GUI.ZPanel buttonsPanel;
		internal ZArchitecture.GUI.ZButton oKButton;
		ZArchitecture.GUI.ZButton cPQACancelButton;
		ZArchitecture.GUI.ZPanel mainPanel;
		ZArchitecture.GUI.ZPanel mainUpPanel;
		ZArchitecture.GUI.ZPanel mainDownPanel;
		ZArchitecture.GUI.ZPanel mainUpEntryPanel;
		ZArchitecture.GUI.ZPanel declarationQuestionsGroupBoxPanel;
		internal ZArchitecture.GUI.ZGroupBox declarationQuestionsGroupBox;
		CPQAsForHeaderControl cpqAsForHeaderControl2;
		ZArchitecture.GUI.ZPanel cPDecQuestionsPanel;
		ZArchitecture.GUI.ZGroupBox cPDecQuestionsGroupBox;
		CPQAsForLineControl cpqAsForLineControl1;
		ZGrid entryHeadersGrid;
		CargoWise.Windows.UI.KSplitter splitter1;
		CargoWise.Windows.UI.KSplitter splitter2;
		ZArchitecture.GUI.ZPanel mainUpDropEditPanel;
		ZArchitecture.GUI.ZDropEdit zDropEdit1;
		ZLabel displayOptionLabel;
	}
}
