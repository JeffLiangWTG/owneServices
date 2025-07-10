using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class GLJournalForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "CW1042", Justification = "Generated code")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017:DpiAwareDevelopmentRule", Justification = "Generated code")]
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TabControl = new ZTemplateTabControl();
			this.LinesTabPage = new ZTabPage();
			this.ApprovalsTab = new ZTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.BottomPanel = new ZPanel();
			this.PostingButtonsUserControl = new ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 425, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 26, true);
			this.MainStatusBar.TabIndex = 2;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = 419;
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = 419;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(GLJournal);
			//
			// TabControl
			//
			this.TabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.LinesTabPage);
			this.TabControl.Controls.Add(this.ApprovalsTab);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 388, true);
			this.TabControl.TabIndex = 0;
			//
			// LinesTabPage
			//
			this.LinesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalForm|af3d5ff6-58ff-4e6b-bf50-1fc4bfca2081", "Journal Details");
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 361, true);
			this.LinesTabPage.TabIndex = 0;
			this.LinesTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.LinesTabPage_InitializeTab));
			//
			// ApprovalsTab
			//
			this.ApprovalsTab.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a04bd79a-e9ca-4cb4-9687-f00c308c06fe", "Approval Audit");
			this.ApprovalsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ApprovalsTab.Name = "ApprovalsTab";
			this.ApprovalsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 361, true);
			this.ApprovalsTab.TabIndex = 3;
			this.ApprovalsTab.RunWhenBindingOrFirstShown(new EventHandler(this.ApprovalsTab_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLJournal)(null)).Approvals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).XP_RequestID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).XP_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).CreatedUser_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).CreatedTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).XP_ApprovalStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).XP_GS_NKApprovingUser1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).ApprovedUser_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).XP_ApprovalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((GLJournalApprovalRequest)(((System.Collections.IList)(((GLJournal)(null)).Approvals)).SyncRoot)).XP_ReasonDescription)));
			//
			// zStmNoteTabPage1
			//
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 361, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new EventHandler(this.zStmNoteTabPage1_InitializeTab));
			//
			// zEventTabPage1
			//
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 361, true);
			this.zEventTabPage1.TabIndex = 2;
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 388, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 37, true);
			this.BottomPanel.TabIndex = 1;
			//
			// PostingButtonsUserControl
			//
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 6, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			//
			// GLJournalForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLJournalForm|f30c5aec-7e1a-4841-b036-d9dfd6b57b55", "GL Journal");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 451, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(GLJournal);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.GeneralLedger.GLJournal.GLJournal";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 550, true);
			this.Name = "GLJournalForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}