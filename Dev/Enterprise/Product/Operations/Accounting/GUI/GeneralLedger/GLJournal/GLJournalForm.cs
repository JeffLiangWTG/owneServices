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
	//
	// Every new editable field for GL Journal screen have to be added in GLJournalApprovalRequestDetails.IsJournalsEqual and in GLJournalDataAdapter
	//
	public partial class GLJournalForm : ZForm, IButtonDeleteTextOverride
	{
		ZTemplateTabControl TabControl;
		ZStmNoteTabPage zStmNoteTabPage1;
		ZLogsTabPage zEventTabPage1;
		ZPanel BottomPanel;
		ZPostingButtonsUserControl PostingButtonsUserControl;
		ZTabPage LinesTabPage;
		GLJournalUserControl glJournalUserControl;
		ZTabPage ApprovalsTab;
		ZArchitecture.ZGrid ApprovalsGrid;
		System.ComponentModel.IContainer components;

		public GLJournalForm(GLJournal journal)
			: base(journal)
		{
			this.Journal = journal;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			if (IsApprovalRequestPosting)
			{
				AutoAddPreviousNextButtons = false;
			}
			if (IsApprovalRequestEditing)
			{
				zStmNoteTabPage1.TabVisible = false;
				zEventTabPage1.TabVisible = false;
				AutoAddPreviousNextButtons = false;
			}
			if (!IsApprovalRequestPosting && !IsApprovalRequestEditing)  // disable EDocs tab as it will conflict with the relink request edocs logic
			{
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
			}
			DisplayModeChanged += GLJournalForm_DisplayModeChanged;

			var dataExportBatchSource = BusinessEntity as IDataExportBatchSource;
			if (dataExportBatchSource != null && dataExportBatchSource.IsDataExportBatchSupported)
			{
				PlugIns.Add(ControllerIDs.DataExportBatchPlugin);
			}

			PlugIns.Add(ControllerIDs.Audit);
		}

		void GLJournalForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected GLJournal Journal;
		protected GLJournal OriginalJournal;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				if (IsApprovalRequestEditing || IsApprovalRequestPosting)
				{
					if (IsApprovalRequestPosting)
					{
						if (DisplayMode == ODisplayMode.Browse || DisplayMode == ODisplayMode.New || DisplayMode == ODisplayMode.NewSaved)
						{
							DisplayMode = ODisplayMode.Edit;
						}
					}

					if (IsApprovalRequestEditing)
					{
						if (DisplayMode == ODisplayMode.Browse || DisplayMode == ODisplayMode.New || DisplayMode == ODisplayMode.Edit)
						{
							DisplayMode = ODisplayMode.NewSaved;
						}
					}

					DisableNewAction();
				}
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			AllowOnlyOneUserEditThisForm();

			SubAccountHelper.SetSubClassParentTableCode(Journal, DisplayMode);
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				GLJournal journal = BusinessEntity as GLJournal;
				if (journal != null && journal.IsJournalCanBeReversed)
				{
					return;
				}
			}

			base.SetReadOnlyIncludingChildren();
		}

		#region ShowPreDeleteDialogs

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			var reverseTransaction = BusinessEntity as GLJournal;
			if (reverseTransaction != null && reverseTransaction.IsJournalCanBeReversed)
			{
				reverseTransaction.RunPreSaveValidation();
				if (reverseTransaction.HasErrors())
				{
					ShowErrorsDialog();
					return ContinueWithDelete.No;
				}

				var continueProcessing = new GLJournalLevelAuthorizationWithApprovalRequest(ApprovalGUIProvider, Journal, false).PerformLevelAuthorization();
				return continueProcessing ? ContinueWithDelete.Yes : ContinueWithDelete.No;
			}

			return base.ShowPreDeleteDialogs();
		}

		#endregion

		void AllowOnlyOneUserEditThisForm()
		{
			mutex = new ZGlobalMutex(MutexIDs.GLJournalForm, GetMutexKey(Journal.PK));
			if (mutex.IsLocked)
			{
				var message = Journal.GetLockMessage(mutex, (NoResString)"edit");
				Globals.Message.ShowInformation(message, Res.GetString("GLJournalForm|UserActionCaption", "Another session is editing the same information"));

				DisplayMode = ODisplayMode.ReadOnly;
				SetReadOnlyIncludingChildren();
			}
			else
			{
				mutex.Lock();
			}
		}

		ZGlobalMutex mutex;

		static ZString GetMutexKey(ZGuid pk)
		{
			return pk + "_" + GlbCompany.CurrentCompany.GC_Code;
		}

		public override void ShowOtherUsersCurrentlyAccessingThisEntity()
		{
		}

		protected override void ZForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (DisplayMode != ODisplayMode.NewSaved && (IsApprovalRequestEditing || Journal.HasNotPostedApprovalRequest))
			{
				base.ZForm_Closing(sender, e);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (mutex != null)
				{
					((IDisposable)mutex).Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (Journal.IsInDatabase)
			{
				BusinessObjectFactory originalFactory = new BusinessObjectFactory();
				OriginalJournal = originalFactory.Load(typeof(GLJournal), Journal.PK) as GLJournal;

				if (OriginalJournal == null)
				{
					return ContinueWithSave.No;
				}

				OriginalJournal.GLJournalLines.Load();
				Journal.Factory.RefreshEnabled = true;
			}
			else
			{
				OriginalJournal = Journal;
			}

			var result = base.ValidateAndSave();
			Journal.RefreshRequestFields();

			return result;
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			return DialogResult.Yes;
		}

		protected override void Delete()
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				OriginalJournal = Journal;
				SaveInternal();
				Close();
			}
			else
			{
				base.Delete();
			}
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("Accounting|GLJournalForm|ReverseButton", "&Reverse"); }
		}

		public override string FormVerb
		{
			get
			{
				var result = base.FormVerb;
				if (DisplayMode == ODisplayMode.Delete)
				{
					return Res.GetString("Accounting|GLJournalForm|FromVerbReverse", "Reverse");
				}

				if (IsApprovalRequestPosting)
				{
					result = Res.GetString("732f0c4f-1731-4c84-88f6-01386c71fa8f", "Post");
				}

				return result;
			}
		}

		public override string FormCaption
		{
			get { return FormCaptionPrefix + UnprefixedFormCaption; }
		}

		protected string FormCaptionPrefix
		{
			get
			{
				if ((Journal.HasNotPostedApprovalRequest || IsApprovalRequestEditing) && !IsApprovalRequestPosting)
				{
					return string.Format("{0} ", Res.GetString("782087D9-A921-4A9C-AB6C-D3E3CDD8E50D", "Waiting Approval"));
				}
				else
				{
					return string.Empty;
				}
			}
		}

		protected string UnprefixedFormCaption
		{
			get { return Journal.AH_ReceiptType == ReceiptTypes.ForeignCurrencyBalance ? Res.GetString("2fd57d04-1597-4eee-8724-df6c2c0c2ab9", "Foreign Currency Balance Adjustment Journal") : Res.GetString("f8d5b750-0d91-4bb1-bea1-bf87d2056172", "GL Journal"); }
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			var factoryList = new List<ITransactionParticipant>(new[] { ApprovalGUIProvider.FactoryForApprovalRequests }); //request should be saved before journal, so new journal on saving can set its number to the request
			if (Journal.GLJournalLineGLDDeleter.HasDeletedGLJournalLine)
			{
				factoryList.Add(Journal.GLJournalLineGLDDeleter);
			}

			if (!ApprovalGUIProvider.IsPostingCanceled)
			{
				factoryList.AddRange(factories);
				Journal.LinkOriginalJournalToApprovalRequest(Journal.Factory);
				var aggregator = new AggregateWrapper(Journal, OriginalJournal);
				factoryList.Add(aggregator);

				var pairReverseTransaction = Journal.OriginalTransaction?.PairTransaction?.ReverseTransaction as GLJournal;
				if (pairReverseTransaction != null)
				{
					var pairReverseAggregator = new AggregateWrapper(pairReverseTransaction, pairReverseTransaction);
					factoryList.Add(pairReverseAggregator);
				}
			}
			else
			{
				Journal.LinkOriginalJournalToApprovalRequest(approvalGUIProvider.FactoryForApprovalRequests);
			}

			if (!Journal.IsInDatabase && Journal.IsPeriodExists)
			{
				var addPeriodQueueAction = new PeriodManager(Journal.Factory).GetSavePeriodQueueInTransactionAction(Journal.PeriodPK, Journal.Factory);
				factoryList.Add(addPeriodQueueAction);
			}

			if (Journal.IsInDatabase && Journal.GetComplianceReportTypesByTablePrefixAndStatus(ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData, new[] { AccComplianceReport.Status.ReportGenerated, AccComplianceReport.Status.ReportFinalised }).Any())
			{
				factoryList.Add(Journal.GLJournalGLDComplianceReportAction);
			}

			if (Journal.IsInDatabase && Journal.GetComplianceReportTypesByTablePrefixAndStatus(ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.AllTransactions,
				new[] { AccComplianceReport.Status.ReportDataQueued, AccComplianceReport.Status.ReportGenerated, AccComplianceReport.Status.ReportFinalised }).Any())
			{
				factoryList.Add(Journal.GLJournalAllTransactionComplianceReportAction);
			}

			base.Save(factoryList.ToArray());

			SaveEDocsAndReLinkToApprovalRequestForNewJournal(ApprovalGUIProvider.IsPostingCanceled);
		}

		void SaveEDocsAndReLinkToApprovalRequestForNewJournal(bool edocsNotSavedYet)
		{
			if (edocsNotSavedYet)
			{
				if (PlugIns.Instances.Length > 0)
				{
					eDocsPlugIn plugIn = (eDocsPlugIn)PlugIns.Instances.FirstOrDefault(p => p is eDocsPlugIn);
					if (plugIn != null)
					{
						if (plugIn.MasterFactory.ChildFactories.Contains(Journal.Factory))
						{
							plugIn.MasterFactory.ChildFactories.Remove(Journal.Factory);  // only save eDocs, not Journal
						}
						plugIn.MasterFactory.Save();  // After MasterFactory Save(), all the eDocs are saved in the DB now.

						if (!Journal.IsInDatabase && !Journal.LatestLinkedApprovalRequestPK.IsEmpty)    // if create a new Journal and a request
						{
							DocumentFactory documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
							StorageMain storageMainForJournal = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, Journal.PK)); // relink the eDocs from the journal to the current request

							if (storageMainForJournal != null)
							{
								LinkEDocsToLatestApprovalRequest(storageMainForJournal);
							}
							else if (!Journal.PreviousLinkedApprovalRequestPK.IsEmpty)
							{
								StorageMain storageMainForPreviousRequest = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, Journal.PreviousLinkedApprovalRequestPK)); // relink the eDocs From previous request to the current request
								if (storageMainForPreviousRequest != null)
								{
									LinkEDocsToLatestApprovalRequest(storageMainForPreviousRequest);
								}
							}
						}
					}
				}
			}
		}

		void LinkEDocsToLatestApprovalRequest(StorageMain storageMain)
		{
			storageMain.SM_ParentFK = Journal.LatestLinkedApprovalRequestPK;
			storageMain.SM_Type = Core.Constants.DocManagerCodes.GLJournalApprovalRequest;
			storageMain.Factory.Save();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = ContinueWithSave.Yes;
			ApprovalGUIProvider.InitializeNewPosting();

			if (Journal.GLJournalLines.Count == 0)
			{
				string error = Res.GetString("25cfd94c-cdaa-4bde-884a-78513de623a4", "You cannot post a Journal with no lines. Please add at least two lines before saving");
				Globals.Message.ShowError(error, FormCaption);
				result = ContinueWithSave.No;
			}

			if (result == ContinueWithSave.Yes && !Journal.IsBalanced && !Journal.IsNoteJournal)
			{
				string autoBalanceMessage = Res.GetString("a7e2cdea-9281-411c-bee6-2fd21ed55f75", "Journal does not balance. Would you like the balance to be posted to the '{0}'?", Journal.BalancingAccount.Caption);
				DialogResult decision = Globals.Message.Show(autoBalanceMessage, FormCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (decision != DialogResult.Yes)
				{
					result = ContinueWithSave.No;
				}
				else
				{
					if ((Guid)Journal.BalancingAccount.Value == Guid.Empty)
					{
						var balancingAccountNotSetMessage = Res.GetString("218dae04-6ce7-427c-8dd1-77bfbcf42833", "Please set up a correct value for the registry item: {0}/{1}", Journal.BalancingAccount.Category, Journal.BalancingAccount.Caption);
						Globals.Message.ShowError(balancingAccountNotSetMessage, Journal.BalancingAccount.Caption);
						result = ContinueWithSave.No;
					}
					else
					{
						Journal.Balance();
					}
				}
			}

			if (result == ContinueWithSave.Yes && Journal.DoesHeaderAndLinesHavePersistentChanges)
			{
				var provider = SecurityOverrideProviderSource.Get(Journal).Provider;
				try
				{
					var alwaysCreateApprovalRequest = IsApprovalRequestEditing;

#if DEBUG
					if (Globals.IsTest && AlwaysCreateApprovalRequest_ForTestOnly)
					{
						alwaysCreateApprovalRequest = true;
					}
#endif
					ApprovalGUIProvider.AlwaysCreateApprovalRequest = alwaysCreateApprovalRequest;

					var continueProcessing = new GLJournalLevelAuthorizationWithApprovalRequest(ApprovalGUIProvider, Journal, alwaysCreateApprovalRequest).PerformLevelAuthorization();
					result = continueProcessing ? ContinueWithSave.Yes : ContinueWithSave.No;
				}
				finally
				{
					SecurityOverrideProviderSource.Get(Journal).Provider = provider;
				}
			}

			return result;
		}

		GLJournalFormApprovalGUIProvider ApprovalGUIProvider
		{
			get { return approvalGUIProvider ?? (approvalGUIProvider = new GLJournalFormApprovalGUIProvider()); }
		}
		GLJournalFormApprovalGUIProvider approvalGUIProvider;

#if DEBUG
		internal bool AlwaysCreateApprovalRequest_ForTestOnly { get; set; }
#endif

		protected override void HandleSaveException(Exception ex)
		{
			if (BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation))
			{
				this.DisplayMode = ODisplayMode.ReadOnly;
				SetReadOnlyIncludingChildren();
			}

			base.HandleSaveException(ex);
		}

		protected bool IsApprovalRequestPosting
		{
			get { return Journal.HasContext(GLJournalApprovalRequest.Context.Posting); }
		}

		protected bool IsApprovalRequestEditing
		{
			get { return Journal.HasContext(GLJournalApprovalRequest.Context.Editing); }
		}

		void LinesTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.glJournalUserControl = new GLJournalUserControl();
			this.LinesTabPage.SuspendLayout();
			this.glJournalUserControl.SuspendLayout();
			this.LinesTabPage.Controls.Add(this.glJournalUserControl);
			//
			// glJournalUserControl
			//
			this.glJournalUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.glJournalUserControl, ".");
			this.glJournalUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glJournalUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.glJournalUserControl.Name = "glJournalUserControl";
			this.glJournalUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 361, true);
			this.glJournalUserControl.TabIndex = 0;
			this.LinesTabPage.PerformLayout();
			this.glJournalUserControl.ResumeLayout(true);
			this.glJournalUserControl.PerformLayout();
			this.LinesTabPage.ResumeLayout(true);
		}

		void ApprovalsTab_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ApprovalsGrid = new ZArchitecture.ZGrid();
			this.ApprovalsTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApprovalsGrid)).BeginInit();
			this.ApprovalsGrid.SuspendLayout();
			this.ApprovalsTab.Controls.Add(this.ApprovalsGrid);
			//
			// ApprovalsGrid
			//
			this.ApprovalsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ApprovalsGrid, "Approvals");
			this.ApprovalsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "XP_RequestID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4599bce9-dae0-4ac5-865d-24ca8e2767fa", "Created By");
			zTextBoxColumnStyleInfo2.ColumnName = "XP_SystemCreateUser";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2adbec8d-9d40-4e01-a0ba-f98d2ed90017", "Creating User");
			zTextBoxColumnStyleInfo3.ColumnName = "CreatedUser_FullName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2e60799b-faae-49ab-99d9-faa14e13e237", "Created Time");
			zDateEditColumnStyleInfo1.ColumnName = "CreatedTimeLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "XP_ApprovalStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fa382490-044c-4f30-bb49-17a28f655bce", "Approved By");
			zTextBoxColumnStyleInfo5.ColumnName = "XP_GS_NKApprovingUser1";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("438bb119-c3dd-4863-aa67-d6515f0ce130", "Approving User");
			zTextBoxColumnStyleInfo6.ColumnName = "ApprovedUser_FullName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "XP_ApprovalDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "XP_ReasonDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ApprovalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ApprovalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ApprovalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ApprovalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ApprovalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ApprovalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ApprovalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ApprovalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ApprovalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ApprovalsGrid.CopySelectedRowsAllowed = true;
			this.ApprovalsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApprovalsGrid.GridId = "7b6e69f2-9053-43ce-8428-8b8cd5e4a007";
			this.ApprovalsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApprovalsGrid.LayoutKey = "zGrid1";
			this.ApprovalsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApprovalsGrid.Name = "ApprovalsGrid";
			this.ApprovalsGrid.ReadOnly = true;
			this.ApprovalsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 361, true);
			this.ApprovalsGrid.TabIndex = 2;
			this.ApprovalsTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApprovalsGrid)).EndInit();
			this.ApprovalsGrid.ResumeLayout(false);
			this.ApprovalsGrid.PerformLayout();
			this.ApprovalsTab.ResumeLayout(true);
		}

		void zStmNoteTabPage1_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.zStmNoteTabPage1.SuspendLayout();
			this.zStmNoteTabPage1.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(true);
		}
	}
}

