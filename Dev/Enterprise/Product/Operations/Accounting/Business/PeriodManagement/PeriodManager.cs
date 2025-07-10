using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class PeriodManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string FinancialYear = "FinancialYear";
		}

		#endregion

		public PeriodManager(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public event EventHandler OnFinancialYearChange;
		public event EventHandler OnCloseSubLedgerError;

		public Period CloseSubLedgerPeriod()
		{
			Period period = NextUnClosedSubLedgerPeriod;

			if (period != null)
			{
				string errorMessage = CanClosePeriod(period, false);

				if (string.IsNullOrEmpty(errorMessage))
				{
					try
					{
						period.AM_IsSubLedgerClosed = true;

						var action = GetSavePeriodQueueInTransactionAction(period.PK, Factory);
						BusinessObjectFactory.SaveTogether(action, Factory);
					}
					catch (ZSaveConcurrencyException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
					if (period.AM_Year == FinancialYear)
					{
						UpdatePeriodsCollection();
					}

					return period;
				}
				else
				{
					if (OnCloseSubLedgerError != null)
					{
						OnCloseSubLedgerError(errorMessage, EventArgs.Empty);
					}
				}
			}

			return null;
		}

		public SaveInTransactionDelegateAction GetSavePeriodQueueInTransactionAction(ZGuid periodPK, BusinessObjectFactory factory)
		{
			var connection = ((IDbConnected)factory).Connection;
			return new SaveInTransactionDelegateAction(connection, () =>
			{
				AddPeriodToCurrencyAdjustmentQueue(periodPK, connection);
				return ChangedTableNames.Empty;
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void AddPeriodToCurrencyAdjustmentQueue(ZGuid periodPK, DbConnection connection)
		{
			if (AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.Value &&
				!CheckCurrencyAdjustmentQueueExisted(periodPK))
			{
				var cmdText = FormattableString.Invariant($@"INSERT INTO dbo.AccCurrencyAdjustmentQueue (ACA_ParentID, ACA_ParentTableCode, ACA_GC, ACA_Date) VALUES (@ParentID, @ParentTableCode, @CompanyPK, @Date)");

				using (var cmd = connection.Command(cmdText))
				{
					cmd.CommandType = System.Data.CommandType.Text;
					cmd.AddParameter("@ParentID", System.Data.SqlDbType.UniqueIdentifier, periodPK.ToGuid());
					cmd.AddParameter("@ParentTableCode", System.Data.SqlDbType.VarChar, AccPeriodManagementSchema.Constants.Prefix);
					cmd.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
					cmd.AddParameter("@Date", System.Data.SqlDbType.DateTime, ZDateTime.Now.ToDateTime());
					cmd.ExecuteNonQuery();
				}
			}
		}

		public Period CloseGLPeriod()
		{
			Period period = NextUnClosedGLPeriod;

			if (period != null)
			{
				string errorMessage = CanClosePeriod(period, true);

				if (string.IsNullOrEmpty(errorMessage))
				{
					if (!period.AM_IsSubLedgerClosed)
					{
						errorMessage = Res.GetString("36d80aa1-3ba7-49b8-9333-8baee97b8fc4", "You cannot close the General Ledger for this period until the Sub Ledger is closed.");
					}
					else if (CheckCurrencyAdjustmentQueueExisted(period.PK))
					{
						errorMessage = Res.GetString("7bccd83c-ad3b-44e0-888a-0b4421a6ca45", @"You cannot close the general ledger for this period as the automated A/R and A/P outstanding balances currency adjustment has not been created.

The automated currency adjustment journal may not be created due to one of the following reasons:
1. The 'CAQ - Currency Adjustment Queue Service Task' is not running.
2. The adjustment journal could not be created due to missing configuration, exchange rate not found or other reasons.

Please check that service task is running and/or rectify the error listed in this accounting period > change logs.");
					}
					else
					{
						try
						{
							period.AM_IsGeneralLedgerClosed = true;
							Factory.Save();
						}
						catch (ZSaveConcurrencyException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
						if (period.AM_Year == FinancialYear)
						{
							UpdatePeriodsCollection();
						}

						return period;
					}
				}

				if (OnCloseSubLedgerError != null)
				{
					OnCloseSubLedgerError(errorMessage, EventArgs.Empty);
				}
			}

			return null;
		}

		bool CheckCurrencyAdjustmentQueueExisted(ZGuid periodPK)
		{
			return Db.Connection.Exists(FormattableString.Invariant($"FROM dbo.AccCurrencyAdjustmentQueue WHERE ACA_ParentID = @PeriodPK"), cmd => cmd.AddParameter("@PeriodPK", SqlDbType.UniqueIdentifier, periodPK.ToGuid()));
		}

		public Period CloseGLPeriodForAdjustments()
		{
			Period period = NextUnClosedForAdjustmentsSubLedgerPeriod;

			ZString errorMessage = ZString.Empty;

			if (period != null)
			{
				if (!period.AM_IsSubLedgerClosed || !period.AM_IsGeneralLedgerClosed)
				{
					errorMessage = Res.GetString("094bbd73-8dda-4dfe-ae2a-57ca4af6339e", "This Period cannot be closed for Adjustments until both the Sub Ledger and General Ledger are closed.");
				}
				else
				{
					try
					{
						period.AM_IsSubledgerClosedForAdjustments = true;
						Factory.Save();
					}
					catch (ZSaveConcurrencyException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
					if (period.AM_Year == FinancialYear)
					{
						UpdatePeriodsCollection();
					}
				}
			}

			if (errorMessage != ZString.Empty && OnCloseSubLedgerError != null)
			{
				OnCloseSubLedgerError(errorMessage, EventArgs.Empty);
				return null;
			}
			return period;
		}

		public Period NextUnClosedSubLedgerPeriod
		{
			get
			{
				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_IsSubLedgerClosed, SQLComparisonOperator.Equal, false);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK.ToGuid());
				filter.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate + " ASC";

				Period period = Factory.LoadTop1<Period>(filter);

				return period;
			}
		}

		public Period NextUnClosedGLPeriod
		{
			get
			{
				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_IsGeneralLedgerClosed, SQLComparisonOperator.Equal, false);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK.ToGuid());
				filter.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate + " ASC";

				Period period = Factory.LoadTop1<Period>(filter);

				return period;
			}
		}

		public Period NextUnClosedForAdjustmentsSubLedgerPeriod
		{
			get
			{
				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_IsSubledgerClosedForAdjustments, SQLComparisonOperator.Equal, false);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK.ToGuid());
				filter.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate + " ASC";

				Period period = Factory.LoadTop1<Period>(filter);

				return period;
			}
		}

		internal protected string CanClosePeriod(Period period, bool checkGLClearingAccount)
		{
			if (PeriodCalculator.IsCurrentPeriod(period.AM_StartDate))
			{
				return Res.GetString("9beb7b5c-9e71-418a-b716-bc788bbf09bd", "Cannot close current period.");
			}

			if (checkGLClearingAccount && !DoesGLClearingAccountHaveZeroBalanceForPeriod(period))
			{
				return Res.GetString("447a1855-8c84-469c-8ae3-bd9d68392df5", "Account Period {0} cannot be closed as the year to date balance for GL Journal Clearing Account Number {1} as at {2} does not equal to zero.\r\n\r\nPlease run the GL Transaction Report for this GL Account up to {3} and make the necessary adjustment entries before closing the period", period.AM_Period, GLClearingAccountNumber, period.AM_Period, period.AM_Period);
			}

			var defaultARInvoiceDateCanClosePeriod = ARDefaultInvoiceAndPostDateCalculator.CanClosePeriod(period);
			if (!string.IsNullOrEmpty(defaultARInvoiceDateCanClosePeriod))
			{
				return defaultARInvoiceDateCanClosePeriod;
			}

			ZQuery unbatchedReceiptFilter = new ZQuery(AccTransactionHeaderSchema.AH_ReceiptBatchNo, SQLComparisonOperator.Equal, "");
			ZQuery transactionTypeFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			transactionTypeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DirectReceipt);
			unbatchedReceiptFilter.AddToFilter(transactionTypeFilter);
			unbatchedReceiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			unbatchedReceiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
			unbatchedReceiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualTo, period.AM_EndDate);

			Base.Transaction.TransactionHeader firstUnbatchedReceipt = Factory.LoadTop1<Base.Transaction.TransactionHeader>(unbatchedReceiptFilter);

			if (firstUnbatchedReceipt != null)
			{
				return Res.GetString("d0b59ed4-823f-4c68-92d0-df9e292ce54d", @"You cannot close the sub-ledger for this period as there are receipts posted in this period that are not part of a deposit batch.
Please create deposit batches for these receipts.");
			}

			return "";
		}

		internal ZBool DoesGLClearingAccountHaveZeroBalanceForPeriod(Period gLPeriod)
		{
			DynamicBusinessObjectCollection readOnlyDynamicBusinessObjectCollection = new DynamicBusinessObjectCollection(Factory);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add(ZSqlParameter.New("@GLClearingAccount", (Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), AccGLHeaderSchema.PK));
			@params.Add(ZSqlParameter.New("@Period", gLPeriod.AM_Period, AccGLAggregateSchema.AA_Period));
			@params.Add(ZSqlParameter.New("@CurrentCompanyPK", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC));

			string sQLString = @"SELECT SUM(AA_Amount) AS AMOUNT FROM " + AccGLAggregateSchema.Constants.SqlSchemaName + "." + AccGLAggregateSchema.Constants.TableName +
				" INNER JOIN " + AccGLHeaderSchema.Constants.SqlSchemaName + "." + AccGLHeaderSchema.Constants.TableName +
				" ON " + AccGLHeaderSchema.PK.Name + " = " + AccGLAggregateSchema.AA_AG.Name +
				" INNER JOIN " + GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName +
				" ON " + GlbBranchSchema.PK.Name + " = " + AccGLAggregateSchema.AA_GB.Name +
				" INNER JOIN " + GlbCompanySchema.Constants.SqlSchemaName + "." + GlbCompanySchema.Constants.TableName +
				" ON " + GlbCompanySchema.PK.Name + " = " + GlbBranchSchema.GB_GC.Name +
				" WHERE " + AccGLHeaderSchema.PK.Name + " = @GLClearingAccount " +
				" AND " + AccGLAggregateSchema.AA_Period.Name + " <= @Period " +
				" AND " + GlbCompanySchema.PK.Name + " = @CurrentCompanyPK";

			readOnlyDynamicBusinessObjectCollection.Load(sQLString, @params);

			return (ZDecimal)readOnlyDynamicBusinessObjectCollection[0]["AMOUNT"] == 0;
		}

		internal string GLClearingAccountNumber
		{
			get
			{
				AccGLHeader gLHeader = Factory.Load<AccGLHeader>((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				string gLJournalClearingAccount = "";
				if (gLHeader != null)
				{
					gLJournalClearingAccount = gLHeader.AG_AccountNum;
				}
				return gLJournalClearingAccount;
			}
		}

		public void CreatePeriodData(NewYearPeriodSettings newYearSettings, BusinessObjectFactory factory)
		{
			PeriodCalculator periodCalculator = new PeriodCalculator(newYearSettings.PeriodFormat, newYearSettings.StartDate.ToDateTime(), newYearSettings.WeekDay);
			CreatePeriodDataCore(newYearSettings, factory, periodCalculator);
		}

		public void CreatePeriodData(NewYearPeriodSettings newYearSettings, BusinessObjectFactory factory, int newPeriodIdentifier)
		{
			PeriodCalculator periodCalculator = new PeriodCalculator(newYearSettings.PeriodFormat, newYearSettings.StartDate.ToDateTime(), newYearSettings.WeekDay, newPeriodIdentifier);
			CreatePeriodDataCore(newYearSettings, factory, periodCalculator);
		}

		void CreatePeriodDataCore(NewYearPeriodSettings newYearSettings, BusinessObjectFactory factory, PeriodCalculator periodCalculator)
		{
			this.Periods.RemoveAll();
			int initialPeriod = periodCalculator.GetPeriodFromDate(newYearSettings.StartDate.AddDays(1).ToDateTime());
			for (int i = 0; i < periodCalculator.PeriodsPerYear; i++)
			{
				int period = periodCalculator.GetNextPeriod(initialPeriod - 1 + i);
				ZDateTime startDate = periodCalculator.GetStartDayOfPeriod(period);
				ZDateTime endDate = periodCalculator.GetEndDayOfPeriod(period);
				Periods.Add(CreateOnePeriod(period, startDate, endDate, factory));
			}

			if (Periods.Count > 0 && newYearSettings.EndDate.IsValid)
			{
				Periods[Periods.Count - 1].AM_EndDate = newYearSettings.EndDate;
			}

			if (Periods.Count > 0)
			{
				foreach (Period period in Periods)
				{
					period.PeriodAddEventDetails = string.Format((NoResString)"Format = {0}, Start Date = {1}, End Date = {2}, End Week Day = {3}, Accounting Year Based On = {4}",
						newYearSettings.PeriodFormat, newYearSettings.StartDate.Date.ToString(), newYearSettings.EndDate.Date.ToString(), newYearSettings.WeekDay, newYearSettings.AccountingYearBasedType);
				}
			}
		}

		public void ExtendLastYearPeriod(ExtendLastFinancialYearSettings extendLastFinancialYearSettings)
		{
			var periodCalculator = new PeriodCalculator(extendLastFinancialYearSettings.PeriodFormat, extendLastFinancialYearSettings.StartDate.ToDateTime(), DayOfWeekCodeList.Codes.Friday);
			var endDate = extendLastFinancialYearSettings.LastPeriodEndDate;
			var period = extendLastFinancialYearSettings.LastPeriod;
			do
			{
				period = periodCalculator.GetNextPeriod(period, true);
				var startDate = endDate.Date.AddDays(1);
				endDate = periodCalculator.GetEndDayOfPeriodForExtend(period, startDate);
				Periods.Add(CreateOnePeriod(period, startDate, endDate, Factory, true));
			} while ((endDate - extendLastFinancialYearSettings.EndDate).Days < 0);

			if (Periods.Count > 0 && extendLastFinancialYearSettings.EndDate.IsValid)
			{
				(Periods[Periods.Count - 1]).AM_EndDate = extendLastFinancialYearSettings.EndDate;
			}
			Factory.Save();
		}

		public Period CreateOnePeriod(int period, ZDateTime startDate, ZDateTime endDate, BusinessObjectFactory factory, bool suspendValidation = false)
		{
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_Period, period);
			filter.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK.ToGuid());
			Period periodManagement = factory.LoadTop1<Period>(filter) ?? factory.New<Period>();
			if (suspendValidation)
			{
				periodManagement.SuspendValidation();
			}
			periodManagement.AM_Year = new ZShort((short)(period / 100));
			periodManagement.AM_Period = period;
			periodManagement.AM_StartDate = startDate;
			periodManagement.AM_EndDate = endDate;
			periodManagement.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			periodManagement.PeriodManager = this;
			return periodManagement;
		}

		public void DeletePeriodsFromStartDateInCurrentCompany(ZDateTime startDate)
		{
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK.ToGuid());
			filter.AddToFilter(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
			foreach (Period period in Factory.Load<Period>(filter))
			{
				period.Delete();
				Factory.Saving += deleteQueuePeriod => AccountingUtils.DeleteQueuePeriod(Factory, period.PK);
			}
			Factory.Save();
		}

		public void DeleteAllPeriodsInCurrentCompany()
		{
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK.ToGuid());
			foreach (Period period in Factory.Load<Period>(filter))
			{
				period.Delete();
				Factory.Saving += deleteQueuePeriod => AccountingUtils.DeleteQueuePeriod(Factory, period.PK);
			}
			Factory.Save();
		}

		public void PeriodEndDateFormEvent(bool isOpening)
		{
			financialYear_ReadOnly = isOpening;
		}

		#region FinancialYear

		public ZShort FinancialYear
		{
			get
			{
				if (!financialYear.HasValue)
				{
					ZShort value = (ZShort)(PeriodCalculator.GetPeriodFromDate(Env.Time.CurrentLocalDate) / 100);
					if (value == 0)
					{
						value = (ZShort)Env.Time.CurrentLocalDate.Year;
					}

					FinancialYear = value;
				}

				return financialYear.Value;
			}
			set
			{
				ZShort? oldValue = financialYear;
				financialYear = value;
				UpdatePeriodsCollection();

				string message = "";
				if (Periods.Count == 0)
				{
					message = Res.GetString("Accounting|PeriodManager|NoPeriodsMessage", "There are no periods set up for {0} financial year.", value);
				}

				if (oldValue != financialYear)
				{
					if (OnFinancialYearChange != null)
					{
						// putting the message as the sender is a horrible hack.
						OnFinancialYearChange(message, EventArgs.Empty);
					}
				}
				FinancialYearInfo.RefreshBinding();
			}
		}

		ZShort? financialYear;

		public ZPropertyInfo FinancialYearInfo
		{
			get { return GetZPropertyInfo(Schema.FinancialYear); }
		}

		protected bool FinancialYear_ReadOnly
		{
			get { return financialYear_ReadOnly; }
		}

		bool financialYear_ReadOnly;

		#endregion

		#region Collection

		protected ZQuery PeriodFilter = new ZQuery();

		public PeriodCollection fPeriods;

		public PeriodCollection Periods
		{
			get
			{
				if (fPeriods == null)
				{
					PeriodFilter.OrderBy = AccPeriodManagementSchema.AM_Period.Name;
					fPeriods = new PeriodCollection(Factory, PeriodFilter, this);
					fPeriods.Load();
					RegisterEditableChildObject(fPeriods);
				}
				return fPeriods;
			}
		}

		#endregion

		#region Validation

		public void ValidateFinancialYear()
		{
			FinancialYearInfo.ClearAllNotifications();

			if (FinancialYear < 2000 || FinancialYear > 2200)
			{
				FinancialYearInfo.AddError(Res.GetString("27e771a5-6dec-43e0-b395-e7ff99fb5326", "Please enter a valid financial year"));
			}
		}

		public bool DoesTransactionExistInCurrentCompany => DoesTransactionExistInAccTransactionHeader || DoesTransactionExistInAccTransactionLines;

		internal bool DoesTransactionExistInAccTransactionHeader
		{
			get
			{
				DynamicBusinessObjectCollection readOnlyDynamicBusinessObjectCollection = new DynamicBusinessObjectCollection(Factory);
				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add(ZSqlParameter.New("@CurrentCompanyPK", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC));

				string sQLString = "IF EXISTS(SELECT AH_PK FROM " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + " WHERE " + AccTransactionHeaderSchema.Constants.AH_GC + " = @CurrentCompanyPK) SELECT 1 AS IsExists ELSE SELECT 0 AS IsExists";

				readOnlyDynamicBusinessObjectCollection.Load(sQLString, @params);

				return (ZInt)readOnlyDynamicBusinessObjectCollection[0]["IsExists"] == 1;
			}
		}

		internal bool DoesTransactionExistInAccTransactionLines
		{
			get
			{
				DynamicBusinessObjectCollection readOnlyDynamicBusinessObjectCollection = new DynamicBusinessObjectCollection(Factory);
				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add(ZSqlParameter.New("@CurrentCompanyPK", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC));

				string sQLString = @"IF EXISTS(SELECT AL_PK FROM " + AccTransactionLinesSchema.Constants.SqlSchemaName + "." + AccTransactionLinesSchema.Constants.TableName + " INNER JOIN " + GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName + " ON " + GlbBranchSchema.Constants.PK + " = " + AccTransactionLinesSchema.Constants.AL_GB + " AND " + GlbBranchSchema.Constants.GB_GC + " = @CurrentCompanyPK) SELECT 1 AS IsExists ELSE SELECT 0 AS IsExists";

				readOnlyDynamicBusinessObjectCollection.Load(sQLString, @params);

				return (ZInt)readOnlyDynamicBusinessObjectCollection[0]["IsExists"] == 1;
			}
		}

		bool IsEnabledPostDateGLJournal => AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		void UpdateTransactionHeaderWithNewPeriodEndDate()
		{
			foreach (Period period in Periods)
			{
				if (!period.AM_IsGeneralLedgerClosed && !period.AM_IsSubLedgerClosed)
				{
					if (period.AM_EndDate.Date != ((ZDateTime)period.AM_EndDateInfo.OriginalValue).Date)
					{
						UpdateTransactionHeaderPostDates(period);
						UpdateTransactionHeaderDueDate(period, TransactionTypes.GLAutoJournal);

						if (period.FirstPeriodOfNextYear != null && period.LastPeriodOfCurrentYear.PK == period.PK)
						{
							UpdateTransactionHeaderDueDate(period.FirstPeriodOfNextYear, TransactionTypes.GLReversingJournal);
						}
					}
					if (period.AM_StartDate.Date != ((ZDateTime)period.AM_StartDateInfo.OriginalValue).Date)
					{
						UpdateTransactionHeaderDueDate(period, TransactionTypes.GLReversingJournal);
					}
				}
			}
		}

		void UpdateTransactionHeaderPostDates(Period period)
		{
			var sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, period.AM_StartDateInfo.OriginalValue);
			sQLFilter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, period.AM_EndDateInfo.OriginalValue);
			if (IsEnabledPostDateGLJournal)
			{
				sQLFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual,
					new string[] { TransactionTypes.GLStandardJournal, TransactionTypes.GLNoteJournal, TransactionTypes.GLReversingJournal });
			}

			var journals = new GLJournalCollection(Factory, sQLFilter);
			journals.Load();

			foreach (GLJournal journal in journals)
			{
				if (!journal.AH_PostDateInfo.HasChanges)
				{
					journal.AH_PostDate = period.AM_EndDate;

					foreach (GLJournalLine journalLine in journal.GLJournalLines)
					{
						journalLine.AL_PostDate = period.AM_EndDate;
					}
				}
			}
		}

		void UpdateTransactionHeaderDueDate(Period period, string journalType)
		{
			bool isAuto = journalType == TransactionTypes.GLAutoJournal;

			if (!isAuto && IsEnabledPostDateGLJournal)
			{
				return;
			}

			var sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(AccTransactionHeaderSchema.AH_DueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, period.AM_StartDateInfo.OriginalValue);
			sQLFilter.AddToFilter(AccTransactionHeaderSchema.AH_DueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, period.AM_EndDateInfo.OriginalValue);
			sQLFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, journalType);

			var journals = new GLJournalCollection(Factory, sQLFilter);
			journals.Load();

			foreach (GLJournal journal in journals)
			{
				if (!journal.AH_DueDateInfo.HasChanges)
				{
					journal.AH_DueDate = isAuto ? period.AM_EndDate : period.AM_StartDate;

					foreach (GLJournalLine journalLine in journal.GLJournalLines)
					{
						journalLine.AL_ReverseDate = isAuto ? period.AM_EndDate : period.AM_StartDate;
					}
				}
			}
		}

		protected override void OnFactorySaving()
		{
			UpdateTransactionHeaderWithNewPeriodEndDate();
			base.OnFactorySaving();
		}

		#endregion

		#region Implementation

		AccountingPeriodCalculator fPeriodCalculator;
		protected AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}

				return fPeriodCalculator;
			}
		}

		protected void UpdatePeriodsCollection()
		{
			PeriodFilter = new ZQuery(AccPeriodManagementSchema.AM_Year, SQLComparisonOperator.Equal, FinancialYear);
			PeriodFilter.ReLoadExistingRows = true;
			PeriodFilter.OrderBy = AccPeriodManagementSchema.AM_Period.Name;
			Periods.RemoveAll();
			Periods.Load(PeriodFilter);
		}
		#endregion
	}
}
