using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.GUI.Aggregator;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class PeriodManagementForm : ZUserControl
	{
		public PeriodManagementForm()
		{
			InitializeComponent();

			Factory = new BusinessObjectFactory();
			PeriodManager = new PeriodManager(Factory);
			PeriodManager.OnFinancialYearChange += new EventHandler(BusinessObject_OnNoPeriods);
			PeriodManager.OnCloseSubLedgerError += new EventHandler(PeriodManager_OnCloseSubLedgerError);
			SetDataBinding(PeriodManager, "");

			if (LastPeriod == null)
			{
				SetUpNextYearButton.Text = Res.GetString("PeriodManagementForm|SetUpAccountingYear", "Set up accounting year");
			}

			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				ReaggregateAllCompaniesButton.Visible = true;
				ReaggregateThisCompanyButton.Visible = true;
				SimulateButton.Visible = true;

				ReportOnlyButton.Visible = true;
				RecoverPartOfGldPeriodsButton.Visible = true;
				zLabel2.Visible = true;
				PeriodExcludeCalcEdit.Visible = true;
				PurgeGLDButton.Visible = true;
				ReverseToPeriodEdit.Visible = true;

				ReSetPeriodsButton.Visible = true;
				ReAggregateSinglePeriodSingleCompany.Visible = true;
				ExpectedWipAcrAggRes.Visible = true;
				ExpectedDebtorCreditorAggRes.Visible = true;
			}

			ContextMenu menu = PeriodsGrid.ContextMenu.GetContextMenu();
			menu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("16d95b92-0896-4a8d-bc5e-abef1617183d", "Reopen Period"), new EventHandler(ReopenPeriod)));
		}

		public void ReopenPeriod(object sender, EventArgs e)
		{
			var showReopenForm = false;
			AccPeriodManagement period = null;
			if (PeriodsGrid.SelectedElements == null || PeriodsGrid.SelectedElements.Length != 1)
			{
				Globals.Message.Show(Res.GetString("312AE132-99FA-4359-8E31-827FCED55376", "Please select a period to reopen"));
			}
			else
			{
				period = (AccPeriodManagement)PeriodsGrid.SelectedElements[0];
				if (!period.AM_IsSubLedgerClosed && !period.AM_IsGeneralLedgerClosed && !period.AM_IsSubledgerClosedForAdjustments)
				{
					Globals.Message.Show(Res.GetString("49c26019-4ff6-4241-88c4-6550a3b07146", "Nothing to reopen!"));
				}
				else
				{
					var securityCheckpoint = GetMinimumReopenAuthorisationRequirement(period);
					if (securityCheckpoint == Env.Security.None)
					{
						Globals.Message.Show(Res.GetString("A80E5DBD-1DD9-400c-BC55-AF0D6D58D4FC",@"You cannot reopen this period.
You do not have sufficient Reopen Period Security Level rights."));
					}
					else if (IsUserInAuthorizedList())
					{
						showReopenForm = true;
					}
					else if (AccountingConfigurationRegistry.Instance.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.Value && AccountingConfigurationRegistry.Instance.EnableSelfAdministrationToReopenClosedPeriods.Value)
					{
						var reopenPeriodsSecurityOverriderProvider = new ReopenPeriodsSecurityOverriderProvider(GetAuthorizedUsers());
						var securityCertificate = reopenPeriodsSecurityOverriderProvider.PromptForTemporaryAccessCore(securityCheckpoint);
						if (securityCertificate.IsAllowed)
						{
							showReopenForm = true;
						}
					}
					else
					{
						var bo = new ReopenPeriodKeyBusinessObject(GetMessageForPeriodReopenForm(), period);
						ZFormModaliser.ShowDialogAndDispose(new ReopenPeriodKeyForm(bo));
					}
				}
			}
			if (showReopenForm && period != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new ReopenPeriodForm(period));
			}
		}

		List<GlbStaff> GetAuthorizedUsers()
		{
			var factory = new BusinessObjectFactory();
			var authorizedUsersPKs = AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.Value
				.Cast<UsersAuthorizedToReopenClosedPeriods>().Select(u => u.StaffPK);
			return factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, authorizedUsersPKs)).ToList();
		}

		ZString GetMessageForPeriodReopenForm()
		{
			var authorizedUsers = GetAuthorizedUsers();
			var authorizedNames = string.Join(", ", authorizedUsers.Select(u => u.GS_FullName));
			return Res.GetString("2b96712a-8906-4efe-8c33-2eab21533f23",
				@"You are not authorized to reopen periods. Please contact {0} and request a Reopen Period Key so you can reopen this Period. Once you receive the key, you can load it below to reopen the Period.
CargoWise has authorized the following people to reopen periods:
{1}",
				Core.Constants.ProductSupportName, authorizedNames);
		}

		bool IsUserInAuthorizedList()
		{
			return GetAuthorizedUsers().Any(u => u.PK == GlbStaff.CurrentUser.PK);
		}

		SecurityCheckpoint GetMinimumReopenAuthorisationRequirement(AccPeriodManagement period)
		{
			var daysAfterPeriodsEndDate = (int)Math.Truncate((ZDateTime.Now - period.AM_EndDate).TotalDays);
			var periodReopenLevelsCollection = AccountingConfigurationRegistry.Instance.PeriodReopenLevels.Value;
			foreach (var level in periodReopenLevelsCollection
				.Cast<PeriodReopenLevels>()
				.Where(l => IsEligibleForReopening(l, daysAfterPeriodsEndDate))
				.OrderBy(l => l.AuthorisationRequirement.ToString() switch
				{
					PeriodReopenLevels.AuthorisationRequirementCodes.FirstApprovalRequired => 1,
					PeriodReopenLevels.AuthorisationRequirementCodes.SecondApprovalRequired => 2,
					PeriodReopenLevels.AuthorisationRequirementCodes.ThirdApprovalRequired => 3,
					_ => int.MaxValue
				}))
			{
				var required = level.AuthorisationRequirement;
				switch (required)
				{
					case PeriodReopenLevels.AuthorisationRequirementCodes.FirstApprovalRequired:
						if (Env.Security.PeriodManagementReOpenPeriodLevel1.IsAllowed ||
							Env.Security.PeriodManagementReOpenPeriodLevel2.IsAllowed ||
							Env.Security.PeriodManagementReOpenPeriodLevel3.IsAllowed)
						{
							return Env.Security.PeriodManagementReOpenPeriodLevel1;
						}
						break;
					case PeriodReopenLevels.AuthorisationRequirementCodes.SecondApprovalRequired:
						if (Env.Security.PeriodManagementReOpenPeriodLevel2.IsAllowed ||
							Env.Security.PeriodManagementReOpenPeriodLevel3.IsAllowed)
						{
							return Env.Security.PeriodManagementReOpenPeriodLevel2;
						}
						break;
					case PeriodReopenLevels.AuthorisationRequirementCodes.ThirdApprovalRequired:
						if (Env.Security.PeriodManagementReOpenPeriodLevel3.IsAllowed)
						{
							return Env.Security.PeriodManagementReOpenPeriodLevel3;
						}
						break;
				}
			}

			return Env.Security.None;
		}

		bool IsEligibleForReopening(PeriodReopenLevels periodReopenLevel, int daysAfterPeriodsEndDate)
		{
			return (periodReopenLevel.Days >= daysAfterPeriodsEndDate && periodReopenLevel.Range == PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod)
				|| (periodReopenLevel.Days == 0 && periodReopenLevel.Range == PeriodReopenLevels.RangeCodes.Unlimited);
		}

		void PeriodsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (PeriodsGrid.SelectedElements != null && PeriodsGrid.SelectedElements.Length == 1)
			{
				AccPeriodManagement period = (AccPeriodManagement)PeriodsGrid.SelectedElements[0];

				GlPeriodForm form = new GlPeriodForm(period);
				ZFormModaliser.Show(form, ParentForm as ZForm);
			}
		}

		public void PromptForNewPeriodsSetup(bool isResetingPeriods = false)
		{
			if (!Env.Security.PeriodManagementSetupAccountingPeriod.IsAllowed)
			{
				Env.Security.PeriodManagementSetupAccountingPeriod.ShowError();
				return;
			}

			NewYearPeriodSettings newYearSettings;

			if (LastPeriod != null)
			{
				var isAccYearBasedOnStartDate = LastPeriod.AM_Year < LastPeriod.AM_EndDate.Year;
				newYearSettings = new NewYearPeriodSettings(isResetingPeriods, isAccYearBasedOnStartDate);
				newYearSettings.PeriodManagementFormEvent(true);
				newYearSettings.StartDate = LastPeriod.AM_EndDate.AddDays(1).Date;
				newYearSettings.IsPeriodsSetBefore = true;
			}
			else
			{
				newYearSettings = new NewYearPeriodSettings(isResetingPeriods);
				newYearSettings.IsPeriodsSetBefore = false;
			}

			if (ZFormModaliser.ShowDialogAndDispose(new PeriodSetUpForm(newYearSettings)) == DialogResult.OK
				&& newYearSettings.StartDate.IsValid && newYearSettings.EndDate.IsValid)
			{
				OpenPeriodEndDateEditFormNewYearSetting(newYearSettings);
			}
		}

		public void PromptForGLReAggregate()
		{
			PromptForGLReAggregate(MessageForAllCompanyReAggregation, CaptionForMultiCompany, () => ObjectFactory.Get<IReAggregator>().ReAggregate());
		}

		public virtual void PromptForGLReAggregate(ZString prompt, ZString caption, Action reAggregate)
		{
			var buttons = MessageBoxButtons.YesNo;
			var question = MessageBoxIcon.Question;

			if (Globals.Message.Show(prompt, caption, buttons, question) != DialogResult.Yes)
			{
				return;
			}

			var start = Env.Time.CurrentLocalDateTime;
			reAggregate();
			var time = Env.Time.CurrentLocalDateTime - start;
			Globals.Message.Show(Res.GetString("3d68c2b7-a574-449d-8aeb-28e5f8659835", "Re-aggregation finished. It took {0} minutes.", time.TotalMinutes));
			RaiseReAggregationFinishedEvent();
		}

		public PeriodManager PeriodManagerBizO
		{
			get { return PeriodManager; }
		}

		void ReAggregateButton_Click(object sender, EventArgs e)
		{
			PromptForGLReAggregate();
		}

		void ReaggregateThisCompanyButton_Click(object sender, EventArgs e)
		{
			PromptForGLReAggregate(
				MessageForSingleCompanyReAggregation,
				CaptionForSingleCompany,
				() => ObjectFactory.New<ISingleCompanyReAggregator>(GlbCompany.CurrentCompany.PK).ReAggregate());
		}

		void SimulateButton_Click(object sender, EventArgs e)
		{
			if (SimulateAggregate != null)
			{
				Cursor.Current = Cursors.WaitCursor;
				SimulateAggregate(this, EventArgs.Empty);
				Cursor.Current = Cursors.Default;
			}
		}

		void ReaggregateAndCorrectButton_Click(object sender, EventArgs e)
		{
			if (AggregateAndCorrect != null)
			{
				AggrgeateEventArgs arg = new AggrgeateEventArgs(Int32.Parse(PeriodExcludeCalcEdit.Text), Int32.Parse(ReverseToPeriodEdit.Text));
				Cursor.Current = Cursors.WaitCursor;
				AggregateAndCorrect(this, arg);
				Cursor.Current = Cursors.Default;
			}
		}

		void PurgeGLDButton_Click(object sender, EventArgs e)
		{
			var caption = Res.GetString("4cbef0e3-9fd4-4907-ba9c-a8d5192f8dba", "Delete General Ledger Data");

			if (!ConfirmInvalidateFinalisedComplianceReport(caption))
			{
				return;
			}

			var promptMessage = Res.GetString("e83ada3e-5f1d-43a0-afda-c4ca44db0c64", "Are you sure you want to delete ALL General Ledger Data and Settings from Company {0}?", GlbCompany.CurrentCompany.CompanyName);

			if (Globals.Message.Show(promptMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			GeneralLedgerDataScriptHelper.PurgeGeneralLedgerData();
			Globals.Message.Show(Res.GetString("27ea5438-9dd2-41cf-9815-47fcd1c87421", @"All existing General Ledger Data (GLD) records have been deleted and all related GLD configurations have been cleared.

However, new GLD records may be inserted as a result of queued records processed by GLD Service Tasks before or during the delete operation.

Thus, please check that no new GLD records can be found in Manage > General Ledger > Accounting Journals module.

If there are, please run the ""Clear GLD configurations and Data"" function again."));
		}

		bool ConfirmInvalidateFinalisedComplianceReport(string caption)
		{
			var gldComplianceReportCodeList = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value
											.Cast<ComplianceReportConfiguration>()
											.Where(x => x.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData)
											.Select(x => x.ReportCode);
			if (gldComplianceReportCodeList.Any())
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(AccComplianceReportSchema.ACR_ReportType, gldComplianceReportCodeList);
				query.AddToFilter(AccComplianceReportSchema.ACR_Status, AccComplianceReport.Status.ReportFinalised);
				query.AddToFilter(AccComplianceReportSchema.ACR_GC_Company, GlbCompany.CurrentCompany.PK);

				return !Factory.Exists(typeof(AccComplianceReport), query) || AccountingMessageHelper.ConfirmInvalidateFinalisedComplianceReport(caption);
			}
			return true;
		}

		void RecoverPartOfGldPeriodsButton_Click(object sender, EventArgs e)
		{
			using (var form = new RecoverPartOfGldPeriodsDateRangeForm(new RecoverPartOfGldPeriodsDateRangeSetting()))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void ReportOnlyButton_Click(object sender, EventArgs e)
		{
			if (AggregateAndReportAsXML != null)
			{
				EventArgs arg = new AggrgeateEventArgs(0, 0);
				Cursor.Current = Cursors.WaitCursor;
				AggregateAndReportAsXML(this, arg);
				Cursor.Current = Cursors.Default;
			}
		}

		void EditPeriodEndDateButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.PeriodManagementEndDateEdit.IsAllowed)
			{
				Env.Security.PeriodManagementEndDateEdit.ShowError();
			}
			else
			{
				if (LastPeriod == null)
				{
					Globals.Message.ShowInformation(Res.GetString("ec915b6a-ea40-4d0b-9ad3-fdac2a6e887a", "Please setup a new accounting year first"), Res.GetString("13fee2cd-7a3c-49e5-94d9-492ae76c2c0d", "Edit Period End Date"));
				}
				else
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					PeriodManager newPeriodManager = new PeriodManager(newFactory);
					newPeriodManager.FinancialYear = PeriodManager.FinancialYear;

					if (ZFormModaliser.ShowDialogAndDispose(new PeriodEndDateEditForm(newPeriodManager)) == DialogResult.OK)
					{
						PeriodManager.FinancialYear = PeriodManager.FinancialYear;
					}
				}
			}
		}

		void ReSetPeriodsButton_Click(object sender, EventArgs e)
		{
			using (ReSetPeriodsForm form = new ReSetPeriodsForm(this))
			{
				form.ShowDialog(this);
			}
		}

		protected void RaiseReAggregationFinishedEvent()
		{
			if (ReAggregationFinished != null)
			{
				ReAggregationFinished(this, EventArgs.Empty);
			}
		}

		void ReAggregateSinglePeriodSingleCompany_Click(object sender, EventArgs e)
		{
			BusinessObjectFactory reAggFactory = new BusinessObjectFactory();
			ZFormModaliser.ShowDialogAndDispose(new ReAggregateSinglePeriodForm(new SinglePeriodReaggregator(reAggFactory)));
		}

		void ExpectedWipAcrAggRes_Click(object sender, EventArgs e)
		{
			BusinessObjectFactory reAggFactory = new BusinessObjectFactory();
			ZFormModaliser.ShowDialogAndDispose(new ExpectedAggregationResultsForm(new AggregationDiscrepanciesCalculator(reAggFactory, "WIPACR")));
		}

		void ExpectedDebtorCreditorAggRes_Click(object sender, EventArgs e)
		{
			BusinessObjectFactory reAggFactory = new BusinessObjectFactory();
			ZFormModaliser.ShowDialogAndDispose(new ExpectedAggregationResultsForm(new AggregationDiscrepanciesCalculator(reAggFactory, "DebtorCreditor")));
		}

		public event EventHandler ReAggregationFinished;
		public event EventHandler SimulateAggregate;
		public event EventHandler AggregateAndCorrect;
		public event EventHandler AggregateAndReportAsXML;

		static string MessageForAllCompanyReAggregation { get { return Res.GetString("Accounting|PeriodManagementForm|DoYouWantToReaggregateTransactionsForAllCompanies", @"Are you sure you want to re-aggregate transactions for all companies?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this."); } }
		static string MessageForSingleCompanyReAggregation { get { return Res.GetString("Accounting|PeriodManagementForm|DoYouWantToReaggregateTransactionsForThisCompany", @"Are you sure you want to re-aggregate transactions for this company?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this."); } }
		static string CaptionForMultiCompany { get { return Res.GetString("Accounting|PeriodManagementForm|MultiCompanyReAggregator", "Multi Company Re-Aggregator"); } }
		static string CaptionForSingleCompany { get { return Res.GetString("Accounting|PeriodManagementForm|SingleCompanyReAggregator", "Single Company Re-Aggregator"); } }

		#region Implementation

		protected BusinessObjectFactory Factory;
		ZButton ReAggregateSinglePeriodSingleCompany;
		ZButton ExpectedWipAcrAggRes;
		ZButton ExpectedDebtorCreditorAggRes;
		ZButton CloseGLAdjustmentsButton;
		ZButton RecoverPartOfGldPeriodsButton;
		protected PeriodManager PeriodManager;

		#region Show/Close

		#endregion

		#region System stuff

		ZArchitecture.ZCalcEdit FinancialYearCalcEdit;
		ZGroupBox DescriptionGroupBox;
		ZButton SetUpNextYearButton;
		ZButton CloseGLButton;
		ZButton CloseSubledgerButton;
		ZArchitecture.ZGrid PeriodsGrid;
		ZButton ReaggregateAllCompaniesButton;
		ZButton SimulateButton;
		ZButton PurgeGLDButton;
		ZButton ReportOnlyButton;
		ZArchitecture.ZCalcEdit PeriodExcludeCalcEdit;
		ZArchitecture.ZLabel zLabel2;
		ZArchitecture.ZCalcEdit ReverseToPeriodEdit;
		ZButton EditPeriodEndDateButton;
		ZButton ReSetPeriodsButton;
		ZButton ReaggregateThisCompanyButton;
		ZArchitecture.ZLabel PeriodInfoLabel;

		readonly System.ComponentModel.Container components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		#endregion

		void BusinessObject_OnNoPeriods(object message, EventArgs e)
		{
			PeriodInfoLabel.Text = message.ToString();
		}

		void CloseSubledgerButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.PeriodManagementCloseARAPPeriod.IsAllowed)
			{
				Env.Security.PeriodManagementCloseARAPPeriod.ShowError();
				return;
			}
			else if (PeriodManager.NextUnClosedSubLedgerPeriod != null && Globals.Message.Show(Res.GetString("3de77fb0-f830-44c8-bf57-0ed463d529f6", @"Are you sure you want to close Sub Ledger period {0}?
Please note that once a period is closed, it cannot be reopened.", PeriodManager.NextUnClosedSubLedgerPeriod.AM_Period), Res.GetString("8f91a7b2-a179-4e2d-9df1-e642d83cc288", "Close Sub Ledger Period"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				AccPeriodManagement period = PeriodManager.CloseSubLedgerPeriod();
				if (period != null)
				{
					Globals.Message.ShowInformation(Res.GetString("c499435d-e61d-4c49-9ae8-0e9e60ed02b5", "Sub-ledger period {0} closed.", period.AM_Period));
				}
			}
		}

		void CloseGLButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.PeriodManagementCloseGLPeriod.IsAllowed)
			{
				Env.Security.PeriodManagementCloseGLPeriod.ShowError();
				return;
			}
			else if (PeriodManager.NextUnClosedGLPeriod != null && Globals.Message.Show(Res.GetString("214bd06b-8be5-4ae0-b3d1-9fa194af19b9", @"Are you sure you want to close General Ledger period {0}?
Please note that once a period is closed, it cannot be reopened.", PeriodManager.NextUnClosedGLPeriod.AM_Period), Res.GetString("afa7c735-b6ee-4d1c-a813-5d95fde5eac6", "Close Ledger Period"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				AccPeriodManagement period = PeriodManager.CloseGLPeriod();
				if (period != null)
				{
					Globals.Message.ShowInformation(Res.GetString("e5f69db3-f41a-4602-b2c7-0ef4871efa2f", "General Ledger period {0} closed.", period.AM_Period));
				}
			}
		}

		void CloseGLAdjustmentsButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.PeriodManagementCloseGLPeriodForAdjustments.IsAllowed)
			{
				Env.Security.PeriodManagementCloseGLPeriodForAdjustments.ShowError();
				return;
			}
			else if (PeriodManager.NextUnClosedForAdjustmentsSubLedgerPeriod != null &&
				Globals.Message.Show(Res.GetString("7bd3bfe6-9914-4405-8eca-08882bcdfd1b", "Are you sure you want to close General Ledger period {0} for adjustments?"
				, PeriodManager.NextUnClosedForAdjustmentsSubLedgerPeriod.AM_Period)
				, Res.GetString("40ff299b-09b1-46f5-86f6-d75ccf218e68", "Close Ledger Period for Adjustments"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				AccPeriodManagement period = PeriodManager.CloseGLPeriodForAdjustments();
				if (period != null)
				{
					Globals.Message.ShowInformation(Res.GetString("0ef4e8aa-9322-48ba-8c0c-6bf73a1a0dde", "General Ledger period {0} closed for adjustments.", period.AM_Period));
				}
			}
		}

		void SetUpNextYearButton_Click(object sender, EventArgs e)
		{
			PromptForNewPeriodsSetup();
		}

		void OpenPeriodEndDateEditFormNewYearSetting(NewYearPeriodSettings newYearSettings)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			PeriodManager newPeriodManager = new PeriodManager(newFactory);

			GenerateNewPeriod(newYearSettings, newFactory, newPeriodManager);

			if (ZFormModaliser.ShowDialogAndDispose(new PeriodEndDateEditForm(newPeriodManager)) == DialogResult.OK)
			{
				if (LastPeriod != null)
				{
					PeriodManager.FinancialYear = LastPeriod.AM_Year;
				}
			}
			PeriodManager.FinancialYearInfo.RefreshBinding();
		}

		protected void GenerateNewPeriod(NewYearPeriodSettings newYearSettings, BusinessObjectFactory newFactory, PeriodManager newPeriodManager)
		{
			if (newYearSettings.StartDate.IsValid && newYearSettings.EndDate.IsValid)
			{
				if (LastPeriod == null)
				{
					ZShort newYear = (ZShort)newYearSettings.EndDate.Year;
					if (newYearSettings.AccountingYearBasedType == NewYearPeriodSettings.AccountingYearBaseTypes.StartDateCalendarYear)
					{
						newYear = (ZShort)newYearSettings.StartDate.Year;
					}
					NewYearPeriodSettings priorYearSettings = new NewYearPeriodSettings();
					priorYearSettings.StartDate = newYearSettings.StartDate.AddYears(-1);
					priorYearSettings.EndDate = newYearSettings.StartDate.AddDays(-1);
					priorYearSettings.PeriodFormat = newYearSettings.PeriodFormat;
					priorYearSettings.AccountingYearBasedType = newYearSettings.AccountingYearBasedType;
					newPeriodManager.CreatePeriodData(priorYearSettings, newFactory, newYear - 1);
					newPeriodManager.CreatePeriodData(newYearSettings, newFactory, newYear);
					newPeriodManager.FinancialYear = newYear;
				}
				else
				{
					ZShort newYear = LastPeriod.AM_Year + 1;
					newPeriodManager.CreatePeriodData(newYearSettings, newFactory, newYear);
					newPeriodManager.FinancialYear = newYear;
				}
				newPeriodManager.FinancialYearInfo.RefreshBinding();
				newPeriodManager.RunPreSaveValidation();
				SetUpNextYearButton.Text = Res.GetString("PeriodManagementForm|SetUpNextAccountingYear", "Set up next accounting year");
			}
		}

		public AccPeriodManagement LastPeriod
		{
			get
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
				filter.ReLoadExistingRows = true;
				filter.OrderBy = AccPeriodManagementSchema.Constants.AM_EndDate + " DESC";
				return newFactory.LoadTop1<AccPeriodManagement>(filter);
			}
		}

		void PeriodManager_OnCloseSubLedgerError(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning(sender.ToString());
		}

		protected override bool CheckForBindingErrors()
		{
			return false;
		}

		#endregion
	}
}

