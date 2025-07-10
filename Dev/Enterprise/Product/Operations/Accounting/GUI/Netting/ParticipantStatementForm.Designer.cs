using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Netting
{
	partial class ParticipantStatementForm
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
		protected ZArchitecture.GUI.ZGuidFindBox NettingPeriod;
		

		#region Windows Form Designer generated code

		protected new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.trialStatementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.finalizeNettingCycleTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.finalStatementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.clearingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NettingPeriod = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tabControl.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.NettingPeriod.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 463, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Netting.NettingStatement);
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.trialStatementsTabPage);
			this.tabControl.Controls.Add(this.finalizeNettingCycleTabPage);
			this.tabControl.Controls.Add(this.finalStatementsTabPage);
			this.tabControl.Controls.Add(this.clearingDocumentsTabPage);
			this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 57, true);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 381, true);
			this.tabControl.TabIndex = 0;
			// 
			// trialStatementsTabPage
			//
			this.trialStatementsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("17ca52cf-1f6c-491a-ac54-2826e9f6eb78", "Print Trial Statements");
			this.trialStatementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.trialStatementsTabPage.Name = "trialStatementsTabPage";
			this.trialStatementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 359, true);
			this.trialStatementsTabPage.TabIndex = 0;
			this.trialStatementsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.trialStatementsTabPage_InitializeTab));
			// 
			// finalizeNettingCycleTabPage
			// 
			this.finalizeNettingCycleTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bc264c66-b02d-4ad1-927f-45883286f0eb", "Finalize Netting Cycle");
			this.finalizeNettingCycleTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.finalizeNettingCycleTabPage.Name = "finalizeNettingCycleTabPage";
			this.finalizeNettingCycleTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 359, true);
			this.finalizeNettingCycleTabPage.TabIndex = 1;
			this.finalizeNettingCycleTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.finalizeNettingCycleTabPage_InitializeTab));
			// 
			// finalStatementsTabPage
			// 
			this.finalStatementsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("269395b7-c92f-490c-99ea-c8922bad5b84", "Print Final Statements");
			this.finalStatementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.finalStatementsTabPage.Name = "finalStatementsTabPage";
			this.finalStatementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 359, true);
			this.finalStatementsTabPage.TabIndex = 0;
			this.finalStatementsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.finalStatementsTabPage_InitializeTab));
			// 
			// clearingDocumentsTabPage
			// 
			this.clearingDocumentsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3467dda7-e5a0-452a-930f-510d614aa8d3", "Print Clearing Documents");
			this.clearingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.clearingDocumentsTabPage.Name = "clearingDocumentsTabPage";
			this.clearingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 359, true);
			this.clearingDocumentsTabPage.TabIndex = 0;
			this.clearingDocumentsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.clearingDocumentsTabPage_InitializeTab));
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.tabControl);
			this.mainPanel.Controls.Add(this.NettingPeriod);
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 451, true);
			this.mainPanel.TabIndex = 100;
			// 
			// NettingPeriod
			// 
			this.NettingPeriod.AllowDrop = true;
			this.NettingPeriod.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.NettingPeriod, "NettingPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Netting.NettingStatement)(null)).NettingPeriod)));
			this.NettingPeriod.IsPrimaryKeyFromCodeRequired = false;
			this.NettingPeriod.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5deba1ca-bbd5-4450-8517-4fb97b23a501", "Netting Cycle");
			this.NettingPeriod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 15, true);
			this.NettingPeriod.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NettingPeriod;
			this.NettingPeriod.Name = "NettingPeriod";
			this.NettingPeriod.ShouldResize = true;
			this.NettingPeriod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.NettingPeriod.TabIndex = 10;
			// 
			// ParticipantStatementForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7bc498ba-6998-492b-a1f9-04451ecf96f7", "Netting Statement");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 487, true);
			this.Controls.Add(this.mainPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Netting.NettingStatement);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 525, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 525, true);
			this.Name = "ParticipantStatementForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tabControl.ResumeLayout(false);
			this.tabControl.PerformLayout();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.NettingPeriod.ResumeLayout(true);
			this.NettingPeriod.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}



		#endregion


		void trialStatementsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.trialStatementGroupBox = new ZGroupBox();
			this.trialParticipantStatementButton = new ZButton();
			this.trialStatementLabel = new ZArchitecture.ZLabel();
			this.trialDetailedStatementGroupBox = new ZGroupBox();
			this.trialDetailedParticipantStatementButton = new ZButton();
			this.trialDetailedStatementLabel = new ZArchitecture.ZLabel();
			this.trialStatementsTabPage.SuspendLayout();
			this.trialStatementGroupBox.SuspendLayout();
			this.trialDetailedStatementGroupBox.SuspendLayout();
			this.trialStatementsTabPage.Controls.Add(this.trialStatementGroupBox);
			this.trialStatementsTabPage.Controls.Add(this.trialDetailedStatementGroupBox);
			// 
			// trialStatementGroupBox
			// 
			this.trialStatementGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("93eb6e8c-1b92-4393-9367-4eedacfe790f", "Trial Participant Statement");
			this.trialStatementGroupBox.Controls.Add(this.trialParticipantStatementButton);
			this.trialStatementGroupBox.Controls.Add(this.trialStatementLabel);
			this.trialStatementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 15, true);
			this.trialStatementGroupBox.Name = "trialStatementGroupBox";
			this.trialStatementGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.trialStatementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 169, true);
			this.trialStatementGroupBox.TabIndex = 100;
			this.trialStatementGroupBox.TabStop = false;
			// 
			// trialParticipantStatementButton
			// 
			this.trialParticipantStatementButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ParticipantStatementForm|2663ea13-963c-4ec1-b2f6-03fb63690699", "Print Participant Statement (Trial)");
			this.trialParticipantStatementButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 119, true);
			this.trialParticipantStatementButton.Name = "trialParticipantStatementButton";
			this.trialParticipantStatementButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.trialParticipantStatementButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 28, true);
			this.trialParticipantStatementButton.TabIndex = 20;
			this.trialParticipantStatementButton.ToolTipCaption = null;
			this.trialParticipantStatementButton.Click += new EventHandler(this.allTransactionParticipantStatementButton_Click);
			// 
			// trialStatementLabel
			// 
			this.trialStatementLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("99ba544b-2f05-41a6-9327-fa54d527d0ba", "This statement shows what the Participant can expect to receive and pay to whom based on the transactions that have been matched at the time of running the statement. The calculations here are based on the prevailing Indicative Rate at the date/time that this Statement is run.\r\nThis gives the Participant the opportunity to:\r\n\t\t• See, at any time, how much any Participant can expect to receive and/or pay through the Netting (depending on the Participant’s Netting Type).\r\n\t\t• Be more proactive in chasing other Participants who have not yet approved their A / P and so are not matched to the A / R");
			this.trialStatementLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.trialStatementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 15, true);
			this.trialStatementLabel.Name = "trialStatementLabel";
			this.trialStatementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 103, true);
			this.trialStatementLabel.TabIndex = 100;
			// 
			// trialDetailedStatementGroupBox
			// 
			this.trialDetailedStatementGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2051B326-AE79-4568-AFC8-134E21173713", "Trial Detailed Participant Statement");
			this.trialDetailedStatementGroupBox.Controls.Add(this.trialDetailedParticipantStatementButton);
			this.trialDetailedStatementGroupBox.Controls.Add(this.trialDetailedStatementLabel);
			this.trialDetailedStatementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 220, true);
			this.trialDetailedStatementGroupBox.Name = "trialDetailedStatementGroupBox";
			this.trialDetailedStatementGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.trialDetailedStatementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 127, true);
			this.trialDetailedStatementGroupBox.TabIndex = 100;
			this.trialDetailedStatementGroupBox.TabStop = false;
			// 
			// trialDetailedParticipantStatementButton
			// 
			this.trialDetailedParticipantStatementButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ParticipantStatementForm|05F3096E-12D5-4E8A-B21A-745444F38A00", "Print Detailed Participant Statement (Trial)");
			this.trialDetailedParticipantStatementButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 83, true);
			this.trialDetailedParticipantStatementButton.Name = "trialDetailedParticipantStatementButton";
			this.trialDetailedParticipantStatementButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.trialDetailedParticipantStatementButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 28, true);
			this.trialDetailedParticipantStatementButton.TabIndex = 20;
			this.trialDetailedParticipantStatementButton.ToolTipCaption = null;
			this.trialDetailedParticipantStatementButton.Click += new EventHandler(this.trialDetailedParticipantStatementButton_Click);
			// 
			// trialDetailedStatementLabel
			// 
			this.trialDetailedStatementLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("020EBC7D-33F0-4519-B6D0-2156DF218887", "This document shows details of the transactions that will be settled for each of the participants after a Netting Cycle is finalized. This might be helpful for tracking a particular invoice to understand how it will be settled.");
			this.trialDetailedStatementLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.trialDetailedStatementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 15, true);
			this.trialDetailedStatementLabel.Name = "trialDetailedStatementLabel";
			this.trialDetailedStatementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 53, true);
			this.trialDetailedStatementLabel.TabIndex = 100;
			this.trialStatementsTabPage.PerformLayout();
			this.trialStatementGroupBox.ResumeLayout(false);
			this.trialStatementGroupBox.PerformLayout();
			this.trialDetailedStatementGroupBox.ResumeLayout(false);
			this.trialDetailedStatementGroupBox.PerformLayout();
			this.trialStatementsTabPage.ResumeLayout(true);
		}

		void finalStatementsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.finalStatementGroupBox = new ZGroupBox();
			this.participantStatementPrintButton = new ZButton();
			this.finalStatementLabel = new ZArchitecture.ZLabel();
			this.detailsStatementGroupBox = new ZGroupBox();
			this.detailsStatementButton = new ZButton();
			this.detailStatementLabel = new ZArchitecture.ZLabel();
			this.finalStatementsTabPage.SuspendLayout();
			this.finalStatementGroupBox.SuspendLayout();
			this.detailsStatementGroupBox.SuspendLayout();
			this.finalStatementsTabPage.Controls.Add(this.finalStatementGroupBox);
			this.finalStatementsTabPage.Controls.Add(this.detailsStatementGroupBox);
			// 
			// finalStatementGroupBox
			// 
			this.finalStatementGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ec6bc661-e497-4ba1-85b2-50a90799a42d", "Final Participant Statement");
			this.finalStatementGroupBox.Controls.Add(this.participantStatementPrintButton);
			this.finalStatementGroupBox.Controls.Add(this.finalStatementLabel);
			this.finalStatementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.finalStatementGroupBox.Name = "finalStatementGroupBox";
			this.finalStatementGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.finalStatementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 109, true);
			this.finalStatementGroupBox.TabIndex = 100;
			this.finalStatementGroupBox.TabStop = false;
			// 
			// participantStatementPrintButton
			// 
			this.participantStatementPrintButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ParticipantStatementForm|2c88dc1c-83a1-4404-90bb-67ef5ae426f6", "Print Participant Statement (Final)");
			this.participantStatementPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 75, true);
			this.participantStatementPrintButton.Name = "participantStatementPrintButton";
			this.participantStatementPrintButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.participantStatementPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 25, true);
			this.participantStatementPrintButton.TabIndex = 40;
			this.participantStatementPrintButton.ToolTipCaption = null;
			this.participantStatementPrintButton.Click += new EventHandler(this.PrintButton_Click);
			// 
			// finalStatementLabel
			// 
			this.finalStatementLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8a426550-65dd-48ec-88fa-68c7d1e80a6e", "This statement shows what the Participant will receive and pay to whom based on the transactions that have been matched at the time Executing the Netting. The calculations here are based on the Execution Exchange Rate for the Cycle, irrespective of what date/time that this Statement is run. The Participant will thus see exactly, how much he/she can expect to receive or pay on Value Date.");
			this.finalStatementLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.finalStatementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 15, true);
			this.finalStatementLabel.Name = "finalStatementLabel";
			this.finalStatementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 58, true);
			this.finalStatementLabel.TabIndex = 100;
			// 
			// detailsStatementGroupBox
			// 
			this.detailsStatementGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8368804e-2504-4098-afbe-b0320704fe2f", "Detail Participant Statement");
			this.detailsStatementGroupBox.Controls.Add(this.detailsStatementButton);
			this.detailsStatementGroupBox.Controls.Add(this.detailStatementLabel);
			this.detailsStatementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 155, true);
			this.detailsStatementGroupBox.Name = "detailsStatementGroupBox";
			this.detailsStatementGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.detailsStatementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 99, true);
			this.detailsStatementGroupBox.TabIndex = 100;
			this.detailsStatementGroupBox.TabStop = false;
			// 
			// detailsStatementButton
			// 
			this.detailsStatementButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("647d2109-dcb0-4af9-ace4-867d13cc3718", "Print Detailed Participant Statement");
			this.detailsStatementButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 63, true);
			this.detailsStatementButton.Name = "detailsStatementButton";
			this.detailsStatementButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.detailsStatementButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 25, true);
			this.detailsStatementButton.TabIndex = 50;
			this.detailsStatementButton.ToolTipCaption = null;
			this.detailsStatementButton.Click += new EventHandler(this.detailsStatementButton_Click);
			// 
			// detailStatementLabel
			// 
			this.detailStatementLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d08e4fa8-7753-4a71-9d37-727eccbced72", "This document shows details of the transactions that were settled for each of the participants. This might be used for audit purposes or to track how a particular invoice was settled in case of a query.");
			this.detailStatementLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.detailStatementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 15, true);
			this.detailStatementLabel.Name = "detailStatementLabel";
			this.detailStatementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 47, true);
			this.detailStatementLabel.TabIndex = 100;
			this.finalStatementsTabPage.PerformLayout();
			this.finalStatementGroupBox.ResumeLayout(false);
			this.finalStatementGroupBox.PerformLayout();
			this.detailsStatementGroupBox.ResumeLayout(false);
			this.detailsStatementGroupBox.PerformLayout();
			this.finalStatementsTabPage.ResumeLayout(true);
		}

		void clearingDocumentsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.clearingJournalGroupBox = new ZGroupBox();
			this.clearingJournalButton = new ZButton();
			this.clearingJournalLabel = new ZArchitecture.ZLabel();
			this.clearingBankPaymentGroupBox = new ZGroupBox();
			this.nettingCyclePrintButton = new ZButton();
			this.clearingBankPaymentLabel = new ZArchitecture.ZLabel();
			this.clearingDocumentsTabPage.SuspendLayout();
			this.clearingJournalGroupBox.SuspendLayout();
			this.clearingBankPaymentGroupBox.SuspendLayout();
			this.clearingDocumentsTabPage.Controls.Add(this.clearingJournalGroupBox);
			this.clearingDocumentsTabPage.Controls.Add(this.clearingBankPaymentGroupBox);
			// 
			// clearingJournalGroupBox
			// 
			this.clearingJournalGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1cf8bcbb-5ba2-4aef-88c4-586d30882e1a", "Clearing Journals");
			this.clearingJournalGroupBox.Controls.Add(this.clearingJournalButton);
			this.clearingJournalGroupBox.Controls.Add(this.clearingJournalLabel);
			this.clearingJournalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 11, true);
			this.clearingJournalGroupBox.Name = "clearingJournalGroupBox";
			this.clearingJournalGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.clearingJournalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 192, true);
			this.clearingJournalGroupBox.TabIndex = 101;
			this.clearingJournalGroupBox.TabStop = false;
			// 
			// clearingJournalButton
			// 
			this.clearingJournalButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4ea23681-55cb-42b8-912b-cce3a871e7b7", "Print Clearing Journals");
			this.clearingJournalButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 157, true);
			this.clearingJournalButton.Name = "clearingJournalButton";
			this.clearingJournalButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.clearingJournalButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 25, true);
			this.clearingJournalButton.TabIndex = 60;
			this.clearingJournalButton.ToolTipCaption = null;
			this.clearingJournalButton.Click += new EventHandler(this.zButton1_Click);
			// 
			// clearingJournalLabel
			// 
			this.clearingJournalLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7195b198-30e7-40ec-9ae8-a072033259c2", "The Netting Clearing Journal contains all of the necessary information to enable the automated clearance, from the Accounts Receivable and Accounts Payable ledgers, of all invoices that have been settled in this Netting Cycle.\r\n\r\nFor Participants who are users of this application, the posting of the Journals is an automated function and so no action need be taken by them.\r\nParticipants who are not users of this application can use this document to manually create the journals and clear the Accounts Receivable and Accounts Payable invoices that have been settled through the Netting.\r\nThe details are shown in summary on the Netting Statement and at an individual invoice level on the Detailed Netting Statement.");
			this.clearingJournalLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.clearingJournalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 15, true);
			this.clearingJournalLabel.Name = "clearingJournalLabel";
			this.clearingJournalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 128, true);
			this.clearingJournalLabel.TabIndex = 100;
			// 
			// clearingBankPaymentGroupBox
			// 
			this.clearingBankPaymentGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c5bc4429-9c13-4a0f-a559-3c616d31859a", "Clearing Bank Payment Document");
			this.clearingBankPaymentGroupBox.Controls.Add(this.nettingCyclePrintButton);
			this.clearingBankPaymentGroupBox.Controls.Add(this.clearingBankPaymentLabel);
			this.clearingBankPaymentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 216, true);
			this.clearingBankPaymentGroupBox.Name = "clearingBankPaymentGroupBox";
			this.clearingBankPaymentGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.clearingBankPaymentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 121, true);
			this.clearingBankPaymentGroupBox.TabIndex = 100;
			this.clearingBankPaymentGroupBox.TabStop = false;
			// 
			// nettingCyclePrintButton
			// 
			this.nettingCyclePrintButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ParticipantStatementForm|148bb347-1c19-4813-bce2-98e221336ac4", "Print Clearing Bank Payment Document");
			this.nettingCyclePrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 83, true);
			this.nettingCyclePrintButton.Name = "nettingCyclePrintButton";
			this.nettingCyclePrintButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.nettingCyclePrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 25, true);
			this.nettingCyclePrintButton.TabIndex = 70;
			this.nettingCyclePrintButton.ToolTipCaption = null;
			this.nettingCyclePrintButton.Click += new EventHandler(this.nettingCyclePrintButton_Click);
			// 
			// clearingBankPaymentLabel
			// 
			this.clearingBankPaymentLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1e980894-9e42-4821-961e-890070d52a5d", "This document can be used, if desired, to be sent to the NC’s Bank at which all the Netting Accounts are maintained. It allows the bank to know exactly what monies it has to pay out and can expect to receive in. In practice, with on-line banking, this is rarely requested by the bank and so will be used by the NCA, in conjunction with the Payments In/Out Report to set up and transmit the payments out and to check that the payments in are received in full and on time.");
			this.clearingBankPaymentLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.clearingBankPaymentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 15, true);
			this.clearingBankPaymentLabel.Name = "clearingBankPaymentLabel";
			this.clearingBankPaymentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 66, true);
			this.clearingBankPaymentLabel.TabIndex = 100;
			this.clearingDocumentsTabPage.PerformLayout();
			this.clearingJournalGroupBox.ResumeLayout(false);
			this.clearingJournalGroupBox.PerformLayout();
			this.clearingBankPaymentGroupBox.ResumeLayout(false);
			this.clearingBankPaymentGroupBox.PerformLayout();
			this.clearingDocumentsTabPage.ResumeLayout(true);
		}

		void finalizeNettingCycleTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.finalizeNettingGroupBox = new ZGroupBox();
			this.finalizePeriodButton = new ZButton();
			this.finalizeNettingLabel = new ZArchitecture.ZLabel();
			this.finalizeNettingCycleTabPage.SuspendLayout();
			this.finalizeNettingGroupBox.SuspendLayout();
			this.finalizeNettingCycleTabPage.Controls.Add(this.finalizeNettingGroupBox);
			// 
			// finalizeNettingGroupBox
			// 
			this.finalizeNettingGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("96490fdb-db87-499d-92ee-816f2a0a3fcc", "Finalize Netting Cycle");
			this.finalizeNettingGroupBox.Controls.Add(this.finalizePeriodButton);
			this.finalizeNettingGroupBox.Controls.Add(this.finalizeNettingLabel);
			this.finalizeNettingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 24, true);
			this.finalizeNettingGroupBox.Name = "finalizeNettingGroupBox";
			this.finalizeNettingGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.finalizeNettingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 121, true);
			this.finalizeNettingGroupBox.TabIndex = 100;
			this.finalizeNettingGroupBox.TabStop = false;
			// 
			// finalizePeriodButton
			// 
			this.finalizePeriodButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ParticipantStatementForm|0c5f8758-8ba4-4f12-ab20-e0406de4779c", "Finalize Netting Cycle");
			this.finalizePeriodButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 81, true);
			this.finalizePeriodButton.Name = "finalizePeriodButton";
			this.finalizePeriodButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.finalizePeriodButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 25, true);
			this.finalizePeriodButton.TabIndex = 30;
			this.finalizePeriodButton.ToolTipCaption = null;
			this.finalizePeriodButton.Click += new EventHandler(this.finalizePeriodButton_Click);
			// 
			// finalizeNettingLabel
			// 
			this.finalizeNettingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("36123bb3-3203-4f77-a6d5-02eac2d18f8c", "Selecting Finalize Netting Cycle causes the following things to happen:\r\n\t\t• All invoices that have not been matched, are transferred to the next cycle;\r\n\t\t• All transactions that have been matched, and so will be settled in this Netting Cycle, are locked so that they can only be viewed and searched for but not altered in any way;\r\n\t\t• The Cycle is marked as completed.");
			this.finalizeNettingLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.finalizeNettingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 15, true);
			this.finalizeNettingLabel.Name = "finalizeNettingLabel";
			this.finalizeNettingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 65, true);
			this.finalizeNettingLabel.TabIndex = 100;
			this.finalizeNettingCycleTabPage.PerformLayout();
			this.finalizeNettingGroupBox.ResumeLayout(false);
			this.finalizeNettingGroupBox.PerformLayout();
			this.finalizeNettingCycleTabPage.ResumeLayout(true);
		}

		ZTabControl tabControl;
		ZTabPage trialStatementsTabPage;
		ZTabPage finalizeNettingCycleTabPage;
		ZTabPage finalStatementsTabPage;
		ZTabPage clearingDocumentsTabPage;
		ZPanel mainPanel;
		ZArchitecture.ZLabel trialStatementLabel;
		ZArchitecture.ZLabel trialDetailedStatementLabel;
		ZArchitecture.ZLabel finalizeNettingLabel;
		ZArchitecture.ZLabel finalStatementLabel;
		ZArchitecture.ZLabel clearingBankPaymentLabel;
		ZGroupBox clearingBankPaymentGroupBox;
		ZGroupBox finalStatementGroupBox;
		ZGroupBox finalizeNettingGroupBox;
		ZGroupBox trialStatementGroupBox;
		ZGroupBox detailsStatementGroupBox;
		ZGroupBox trialDetailedStatementGroupBox;
		ZButton detailsStatementButton;
		ZArchitecture.ZLabel detailStatementLabel;
		private ZGroupBox clearingJournalGroupBox;
		private ZArchitecture.ZLabel clearingJournalLabel;
		protected ZButton finalizePeriodButton;
		protected ZButton nettingCyclePrintButton;
		protected ZButton participantStatementPrintButton;
		protected ZButton trialParticipantStatementButton;
		protected ZButton trialDetailedParticipantStatementButton;
		protected ZButton clearingJournalButton;
	}
}
