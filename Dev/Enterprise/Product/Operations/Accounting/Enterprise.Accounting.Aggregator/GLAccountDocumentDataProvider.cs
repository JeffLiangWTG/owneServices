using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AccGLHeader = Enterprise.MasterFiles.Business.AccGLHeader;
using AccPeriodManagement = Enterprise.MasterFiles.Business.AccPeriodManagement;

namespace Enterprise.Accounting.ReportTableProviders
{
	public partial class GLAccountDocumentDataProvider : ParameterisedTableProvider
	{
		#region Construction

		protected GLAccountDocumentDataProvider()
		{
		}

		public static GLAccountDocumentDataProvider New()
		{
			GLAccountDocumentDataProvider result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden == null)
			{
				result = new GLAccountDocumentDataProvider();
			}
			else
			{
				result = overridden();
			}
			return result;
		}

		protected delegate GLAccountDocumentDataProvider NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Document Provider Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Parameter Name Does Not Need To Be Localised")]
		protected override Parameter[] ExpectedParameters()
		{
			return new Parameter[]
			{
				new Parameter("Start Period", typeof(ZInt)),
				new Parameter("End Period", typeof(ZInt)),
				new Parameter("Start GL Account", typeof(Guid)),
				new Parameter("End GL Account", typeof(Guid)),
				new Parameter("CurrentCompany", typeof(Guid)),
				new Parameter("BranchPK", typeof(Guid)),
				new Parameter("DepartmentPK", typeof(Guid)),
				new Parameter("Description Display", typeof(string)),
				new Parameter("Period Start Date", typeof(ZDateTime)),
				new Parameter("Period End Date", typeof(ZDateTime)),
			};
		}

		public const string HeaderDescOption = "HDR";
		public const string LineDescOption = "LDR";

		protected override void SetupParameterValues(object[] values)
		{
			StartPeriod = Convert.ToInt32(values[0].ToString());
			EndPeriod = Convert.ToInt32(values[1].ToString());

			if (StartPeriod > EndPeriod)
			{
				EndPeriod = StartPeriod;
				StartPeriod = Convert.ToInt32(values[1].ToString());
			}

			FromGLAccount = (Guid)values[2];
			ToGLAccount = (Guid)values[3];
			StartAccount = new Account(FromGLAccount);
			EndAccount = new Account(ToGLAccount);
			Company = (Guid)values[4];
			BranchPK = (Guid)values[5];
			DepartmentPK = (Guid)values[6];
			fShowHeaderDescription = (string)values[7] == "HDR";
			StartDate = (ZDateTime)values[8];
			EndDate = (ZDateTime)values[9];
			if (EndDate.IsValid)
			{
				EndDate = EndDate.AddDays(1).AddMinutes(-1);
			}

			ExcludeZeroLines = ZBool.True;

			SetDatesFromPeriods();
			SetPeriodRange(StartPeriod, EndPeriod);
			SetAccountRange();
		}

		protected string Language = "";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		protected override DataTable GetDataTable()
		{
			DataTable resultTable = null;
			WhereClause = "";
			using (GLAccountListDocumentDataSet dataSetToReturn = new GLAccountListDocumentDataSet())
			{
				GetControlAccount();
				AddControlAccountToList();
				fGLTransactionContainsControlAccount = SetGLTransactionContainsControlAccount();
				SetWhereClause();
				SetDatesFromPeriods();

				FillDataSet(dataSetToReturn, StartDate.ToDateTime(), EndDate.ToDateTime());
				AddControlToDataSet(fARControlList, dataSetToReturn, LedgerTypes.AccountsReceivable);
				AddControlToDataSet(fAPControlList, dataSetToReturn, LedgerTypes.AccountsPayable);

				string filter = " GLAccount >= '" + StartAccount.AccountNo + "' AND GLAccount <= '" + EndAccount.AccountNo + "' ";

				if (BranchPK != Guid.Empty)
				{
					filter += " AND BranchPK = '" + BranchPK + "'";
				}

				if (DepartmentPK != Guid.Empty)
				{
					filter += " AND DepartmentPK = '" + DepartmentPK + "'";
				}

				using (GLAccountListDocumentDataSet dataSetToReturnFiltered = DoFilter(filter, dataSetToReturn))
				{
					SetDebitCredit(dataSetToReturnFiltered);
					SetOpeningBalance(dataSetToReturnFiltered);

					using (GLAccountListDocumentDataSet dataSetToReturnSorted = Sort("GLAccount, Period, InvoiceDate, TransactionDesc, ChargeCode", dataSetToReturnFiltered))
					{
						resultTable = dataSetToReturnSorted.Tables[0].Copy();
					}
				}
			}

			return resultTable;
		}

		#endregion

		#region Instance Variables

		protected int StartPeriod, EndPeriod;
		protected ZDateTime StartDate, EndDate;
		protected Guid FromGLAccount, ToGLAccount;
		protected Guid LocalAccountStart, LocalAccounEnd;
		protected Guid Company;
		protected Guid BranchPK;
		protected Guid DepartmentPK;
		protected bool ExcludeZeroLines;

		protected Account StartAccount, EndAccount;

		protected Account fARControl;
		protected Account fAPControl;
		protected Account fExchangeDifference;
		protected Account fDiscount;
		protected Account fOverpayment;
		protected Account fGSTInput;
		protected Account fGSTOutput;
		protected Account fPendingGSTInput;
		protected Account fPendingGSTOutput;
		protected Account fWIPAccount;
		protected Account fACRAccount;
		protected Account fRetainedEarningsPreviousYear;
		protected Account fJobRevenueJournalControl;

		protected ArrayList fARControlList = new ArrayList();
		protected ArrayList fAPControlList = new ArrayList();
		protected ArrayList fControlAccountList = new ArrayList();

		protected string GLRangeClause;
		protected bool fGLTransactionContainsControlAccount;

		protected int[] PeriodRange;
		protected AccGLHeader[] GLAccountList;
		protected AccGLAccountDescriptor[] AccountList;
		protected BusinessObjectFactory fFactory;

		protected string fLanguage = "";

		#endregion

		#region Factory

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		#endregion

		#region Control Accounts

		Account GetARControl(int period)
		{
			foreach (Account control in fARControlList)
			{
				if (control.PostPeriod == period)
				{
					return control;
				}
			}

			return null;
		}

		Account GetAPControl(int period)
		{
			foreach (Account control in fAPControlList)
			{
				if (control.PostPeriod == period)
				{
					return control;
				}
			}

			return null;
		}

		internal void AddControlAccountToList()
		{
			fControlAccountList.Add(fARControl.PK);
			fControlAccountList.Add(fAPControl.PK);
			fControlAccountList.Add(fExchangeDifference.PK);
			fControlAccountList.Add(fDiscount.PK);
			fControlAccountList.Add(fOverpayment.PK);
			fControlAccountList.Add(fGSTInput.PK);
			fControlAccountList.Add(fGSTOutput.PK);
			fControlAccountList.Add(fWIPAccount.PK);
			fControlAccountList.Add(fACRAccount.PK);
			fControlAccountList.Add(fPendingGSTInput.PK);
			fControlAccountList.Add(fPendingGSTOutput.PK);
			fControlAccountList.Add(fJobRevenueJournalControl.PK);
		}

		protected virtual void GetControlAccount()
		{
			fARControl = new Account(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			fAPControl = new Account(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			fExchangeDifference = new Account(AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value);
			fDiscount = new Account(AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value);
			fOverpayment = new Account(AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value);
			fGSTInput = new Account(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value);
			fGSTOutput = new Account(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value);
			fWIPAccount = new Account(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);
			fACRAccount = new Account(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);
			fPendingGSTInput = new Account(AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value);
			fPendingGSTOutput = new Account(AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value);
			fRetainedEarningsPreviousYear = new Account((Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			fJobRevenueJournalControl = new Account(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value);
		}

		protected virtual bool SetGLTransactionContainsControlAccount()
		{
			foreach (AccGLHeader gLHeader in GLAccountList)
			{
				if (fControlAccountList.Contains(gLHeader.PK.ToGuid()))
				{
					return true;
				}
			}
			return false;
		}

		void AddToARAPControlAccount(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd)
		{
			Account controlAccount = new Account();
			Account controlAccountToCopy = (rowToAdd.Ledger == LedgerTypes.AccountsReceivable ? fARControl : fAPControl);

			if (rowToAdd.Ledger == LedgerTypes.AccountsReceivable)
			{
				controlAccount = GetARControl(rowToAdd.Period);
			}
			else
			{
				controlAccount = GetAPControl(rowToAdd.Period);
			}

			if (controlAccount == null)
			{
				controlAccount = new Account();

				controlAccount.AccountNo = controlAccountToCopy.AccountNo;
				controlAccount.AccountType = controlAccountToCopy.AccountType;
				controlAccount.DebitCreditType = controlAccountToCopy.DebitCreditType;
				controlAccount.Description = controlAccountToCopy.Description;

				controlAccount.Amount = GetARAPAmount(rowToAdd);
				controlAccount.PostPeriod = rowToAdd.Period;

				if (rowToAdd.Ledger == LedgerTypes.AccountsReceivable)
				{
					fARControlList.Add(controlAccount);
				}
				else
				{
					fAPControlList.Add(controlAccount);
				}
			}
			else
			{
				controlAccount.Amount += GetARAPAmount(rowToAdd);
			}
		}

		#endregion

		#region Period Dates

		void SetDatesFromPeriods()
		{
			if (StartPeriod == 0 && EndPeriod == 0 && StartDate.IsValid && EndDate.IsValid)
			{
				StartPeriod = PeriodCalculator.GetPeriodFromDate(StartDate, Company);
				EndPeriod = PeriodCalculator.GetPeriodFromDate(EndDate, Company);
			}

			if (!StartDate.IsValid || !EndDate.IsValid)
			{
				StartDate = PeriodCalculator.GetFirstDayForPeriod(StartPeriod);
				EndDate = PeriodCalculator.GetLastDayForPeriod(EndPeriod);
			}
		}

		bool IsReportStartDateEqualToStartPeriodStartDate
		{
			get
			{
				bool result = true;
				if (PeriodCalculator.GetFirstDayForPeriod(StartPeriod) != StartDate)
				{
					result = false;
				}
				return result;
			}
		}

		AccountingPeriodCalculator PeriodCalculator
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
		AccountingPeriodCalculator fPeriodCalculator;

		#endregion

		#region Opening Balance Related

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		decimal GetGLAccountOpeningBalance(string accountNum)
		{
			Decimal result = 0;

			if (!IsReportStartDateEqualToStartPeriodStartDate)
			{
				using (GLAccountListDocumentDataSet openingBalanceDataSet = new GLAccountListDocumentDataSet())
				{
					FillDataSet(openingBalanceDataSet, PeriodCalculator.GetFirstDayForPeriod(StartPeriod).ToDateTime(), StartDate.AddSeconds(-1).ToDateTime());
					AddControlToDataSet(fARControlList, openingBalanceDataSet, LedgerTypes.AccountsReceivable);
					AddControlToDataSet(fAPControlList, openingBalanceDataSet, LedgerTypes.AccountsPayable);

					int period = 0;
					if (PeriodRange.Length > 0)
					{
						period = PeriodRange[0];
					}

					string filter = "Period = " + period + " AND GLAccount = '" + accountNum + "'";
					if (BranchPK != Guid.Empty)
					{
						filter += " AND BranchPK = '" + BranchPK + "'";
					}

					if (DepartmentPK != Guid.Empty)
					{
						filter += " AND DepartmentPK = '" + DepartmentPK + "'";
					}

					object openingBalance = openingBalanceDataSet.GLAccountListDataSet.Compute("SUM(Amount)", filter);

					result = openingBalance == DBNull.Value ? 0 : Convert.ToDecimal(openingBalance);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual decimal GetOpeningBalance(string accountNum, int period, string companyFilter)
		{
			int startingPeriod = 0;
			decimal result = 0;
			string accountType = GetGLAccountType(accountNum);

			string additionalFilter = "";

			if (accountType != "HDR")
			{
				if (accountType == "P&L")
				{
					startingPeriod = GetStartPeriodOfFinancialYear(period);
				}
				else if (accountType == "BSH")
				{
					startingPeriod = 0;
				}

				if (BranchPK != Guid.Empty)
				{
					additionalFilter += " AND AA_GB = '" + BranchPK + "'";
				}

				if (DepartmentPK != Guid.Empty)
				{
					additionalFilter += " AND AA_GE = '" + DepartmentPK + "'";
				}

				string sQL = "";
				DbCommand command;

				if (accountNum == fRetainedEarningsPreviousYear.AccountNo)
				{
					sQL = @"Select sum(AA_Amount) From dbo.accglaggregate inner join dbo.accglheader on AG_PK = AA_AG 
						Where AA_Period < " + period + " AND AG_AccountNum = '" + accountNum + "'" +
						companyFilter + additionalFilter;

					command = ((IDbConnected)Factory).Connection.Command(sQL);
					result = Utilities.ConvertToDecimal(command.ExecuteScalar());

					int firstPeriod = GetStartPeriodOfFinancialYear(period);

					sQL = @"SELECT SUM(AA_Amount) FROM dbo.AccGLAggregate" +
						" INNER JOIN dbo.AccGLHeader ON AA_AG = AG_PK" +
						" WHERE AG_AccountType = 'P&L'" +
						" AND AA_Period < " + firstPeriod + companyFilter + additionalFilter;

					command = ((IDbConnected)Factory).Connection.Command(sQL);

					result += Utilities.ConvertToDecimal(command.ExecuteScalar());
				}
				else
				{
					sQL = @"Select sum(AA_Amount) From dbo.accglaggregate inner join dbo.accglheader on AG_PK = AA_AG 
						Where (AA_Period >= @StartingPeriod AND AA_Period < @Period) AND AG_AccountNum = @AccountNum " +
						companyFilter + additionalFilter;

					command = ((IDbConnected)Factory).Connection.Command(sQL);
					command.AddParameter("@Period", SqlDbType.Int, period);
					command.AddParameter("@StartingPeriod", SqlDbType.Int, startingPeriod);
					command.AddParameterBasedOnDbColumn("@AccountNum", accountNum, AccGLHeaderSchema.AG_AccountNum);

					result = Utilities.ConvertToDecimal(command.ExecuteScalar());
				}

				result += GetGLAccountOpeningBalance(accountNum);
			}

			return result;
		}

		string GetGLAccountType(string accountNum)
		{
			string accountType = "";

			AccGLHeader gLHeader = Factory.LoadFromNaturalKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, accountNum) as AccGLHeader;

			if (gLHeader != null)
			{
				accountType = gLHeader.AG_AccountType;
			}

			return accountType;
		}

		protected int GetStartPeriodOfFinancialYear(int period)
		{
			int startPeriod = 0;

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccPeriodManagement), AccPeriodManagementSchema.AM_Year);
			subQuery.AddToFilter(AccPeriodManagementSchema.AM_Period, period);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccPeriodManagement));
			query.AddSubQuery(AccPeriodManagementSchema.AM_Year, subQuery, JoinCondition.And);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(query);
			filter.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate;

			AccPeriodManagement[] periodManagement =
				(AccPeriodManagement[])Factory.Load(typeof(AccPeriodManagement), filter);

			if (periodManagement.Length > 0)
			{
				startPeriod = periodManagement[0].AM_Period;
			}

			return startPeriod;
		}

		void InsertEmptyRow(GLAccountListDocumentDataSet dataSetToProcess, int period, AccGLHeader gLAccount, decimal openingBalance)
		{
			GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd = dataSetToProcess.GLAccountListDataSet.NewGLAccountListDataSetRow();

			rowToAdd.Period = period;
			rowToAdd.GLAccount = gLAccount.AG_AccountNum;
			rowToAdd.GLAccountDesc = gLAccount.AG_DescriptionMultilingual;
			rowToAdd.Amount = 0;
			SetOpeningBalance(rowToAdd, openingBalance);

			dataSetToProcess.GLAccountListDataSet.AddGLAccountListDataSetRow(rowToAdd);
		}

		protected void SetOpeningBalance(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToProcess, decimal openingBalance)
		{
			string balanceDebitCredit = "";
			if (openingBalance > 0)
			{
				balanceDebitCredit = Res.GetString("a2f4c926-4f9d-4b3c-826d-654f6bdc1b51", "(Debit)");
			}
			else if (openingBalance < 0)
			{
				balanceDebitCredit = Res.GetString("a90b7c3d-3ef8-4834-9d47-73f91cfbc48f", "(Credit)");
			}

			if (rowToProcess.IsOpeningBalanceNull())
			{
				rowToProcess.OpeningBalance = 0;
			}

			rowToProcess.OpeningBalance = rowToProcess.OpeningBalance + openingBalance;
			rowToProcess.OpeningBalanceAbsolute = Math.Abs(openingBalance);
			rowToProcess.OpeningBalanceDebitCredit = balanceDebitCredit;
			rowToProcess.OpeningPeriodDate = StartDate.ToDateTime();
			rowToProcess.ClosingPeriodDate = EndDate.AddDays(-1).ToDateTime();
		}

		protected void SetOpeningBalance(DataRow[] rows, decimal openingBalance)
		{
			foreach (GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToProcess in rows)
			{
				SetOpeningBalance(rowToProcess, openingBalance);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		protected virtual void SetOpeningBalance(GLAccountListDocumentDataSet dataSetToProcess)
		{
			decimal openingBalance = 0;
			string filter = " AND (" + Business.AccountingUtils.GenerateCompanyFilter(AccGLAggregateSchema.AA_GB, new BusinessObjectFactory()).LiteralTextSqlFormatted + ")";
			string dataRowFilter = "";

			if (PeriodRange.Length > 0)
			{
				int period = PeriodRange[0];

				foreach (AccGLHeader gLAccount in GLAccountList)
				{
					openingBalance = GetOpeningBalance(gLAccount.AG_AccountNum, period, filter);

					dataRowFilter = "GLAccount = '" + gLAccount.AG_AccountNum + "'";
					if (BranchPK != Guid.Empty)
					{
						dataRowFilter += " AND BranchPK = '" + BranchPK + "'";
					}
					if (DepartmentPK != Guid.Empty)
					{
						dataRowFilter += " AND DepartmentPK = '" + DepartmentPK + "'";
					}

					DataRow[] rows = dataSetToProcess.GLAccountListDataSet.Select(dataRowFilter);
					if (rows.Length == 0)
					{
						if (!ExcludeZeroLines)
						{
							InsertEmptyRow(dataSetToProcess, period, gLAccount, openingBalance);
						}
					}
					else
					{
						SetOpeningBalance(rows, openingBalance);
					}
				}
			}
		}

		#endregion

		#region Row Processing

		void ProcessRow(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			switch (rowToAdd.Ledger)
			{
				case LedgerTypes.General:
					switch (rowToAdd.Type)
					{
						case TransactionTypes.GLStandardJournal:
							Process_GL_GJL(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.GLAutoJournal:
							Process_GL_AJL(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.GLReversingJournal:
							Process_GL_RJL(rowToAdd, dataSetToProcess);
							break;
						default:
							break;
					}
					break;
				case LedgerTypes.JobCosting:
					switch (rowToAdd.Type)
					{
						case TransactionLineTypes.Accrual:
						case TransactionLineTypes.WIP:
							Process_WIP_ACR(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.Journal:
							Process_JC_JNL(rowToAdd, dataSetToProcess);
							break;
					}
					break;
				default:
					switch (rowToAdd.Type)
					{
						case TransactionTypes.Journal:
						case TransactionTypes.Payment:
						case TransactionTypes.Receipt:
							Process_JNL_PAY_REC(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.Contra:
							ProcessContra(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.ExchangeDifference:
							ProcessExchangeDiff(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.Discount:
						case TransactionTypes.Overpayment:
							Process_DSC_OVP(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.DirectReceipt:
						case TransactionTypes.DirectPayment:
							ProcessDirectTransactions(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.Invoice:
						case TransactionTypes.AdjustmentNote:
						case TransactionTypes.CreditNote:
							Process_INV_CRD_ADJ(rowToAdd, dataSetToProcess);
							break;
						case TransactionTypes.Transfer:
							ProcessTransfer(rowToAdd, dataSetToProcess);
							break;
						default:
							break;
					}
					break;
			}
		}

		protected virtual DataRow AddRow(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			DataRow result = dataSetToProcess.GLAccountListDataSet.Rows.Add(rowToAdd.ItemArray);
			if (dataSetToProcess.GLAccountListDataSet.Rows.Count >= MaximumRowsBeforeException)
			{
				throw new TooManyRowsException();
			}
			return result;
		}

		[Serializable]
		internal class TooManyRowsException : Exception
		{
			protected internal TooManyRowsException()
				: base()
			{
			}

#if NETFRAMEWORK
			protected TooManyRowsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		DataRow Process_GL_GJL(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			return AddRow(rowToAdd, dataSetToProcess);
		}

		void Process_GL_RJL(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			if (rowToAdd.Period >= StartPeriod && rowToAdd.Period <= EndPeriod)
			{
				AddRow(rowToAdd, dataSetToProcess);
			}

			if (rowToAdd.ReversePeriod >= StartPeriod && rowToAdd.ReversePeriod <= EndPeriod)
			{
				rowToAdd.Period = rowToAdd.ReversePeriod;
				rowToAdd.Amount = -rowToAdd.Amount;
				AddRow(rowToAdd, dataSetToProcess);
			}
		}

		void Process_GL_AJL(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			int offset = FindBeginningPeriodOffsetForTheAutoJournal(rowToAdd.Period);

			int periodEnd = rowToAdd.ReversePeriod > EndPeriod ? EndPeriod : rowToAdd.ReversePeriod;
			int endPeriodIndex = FindBeginningPeriodOffsetForTheAutoJournal(periodEnd);

			for (int i = offset; i <= endPeriodIndex; i++)
			{
				rowToAdd.Period = PeriodRange[i];
				AddRow(rowToAdd, dataSetToProcess);
			}
		}

		int FindBeginningPeriodOffsetForTheAutoJournal(int autoJournalPeriod)
		{
			int index = 0;

			if (autoJournalPeriod <= PeriodRange[PeriodRange.Length - 1] && autoJournalPeriod >= PeriodRange[0])
			{
				while (index < PeriodRange.Length && autoJournalPeriod != PeriodRange[index])
				{
					index++;
				}
			}
			else if (autoJournalPeriod > PeriodRange[PeriodRange.Length - 1])
			{
				index = -1;
			}

			return index;
		}

		void Process_JC_JNL(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			if (rowToAdd.ExtraInfo == "LINE")
			{
				AddRow(rowToAdd, dataSetToProcess);
			}
			else
			{
				rowToAdd.Amount = -rowToAdd.Amount;
				AddRow(rowToAdd, dataSetToProcess);
			}
		}

		void ProcessTransfer(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			if (rowToAdd.Ledger == LedgerTypes.AccountsPayable || rowToAdd.Ledger == LedgerTypes.AccountsReceivable)
			{
				AddToARAPControlAccount(rowToAdd);
			}
			else if (rowToAdd.Ledger == LedgerTypes.CashBook)
			{
				AddRow(rowToAdd, dataSetToProcess);
			}
		}

		void Process_WIP_ACR(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			AddRow(rowToAdd, dataSetToProcess);
			AddToDataSet(rowToAdd, dataSetToProcess, -rowToAdd.Amount, (rowToAdd.ExtraInfo == TransactionLineTypes.WIP ? fWIPAccount : fACRAccount));
		}

		void Process_INV_CRD_ADJ(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			AddToARAPControlAccount(rowToAdd);

			rowToAdd.Amount = -rowToAdd.Amount;
			rowToAdd.GST = -rowToAdd.GST;

			if (rowToAdd.Ledger == LedgerTypes.AccountsPayable)
			{
				AddToDataSet(rowToAdd, dataSetToProcess, rowToAdd.GST, fGSTInput);
			}
			else
			{
				AddToDataSet(rowToAdd, dataSetToProcess, rowToAdd.GST, fGSTOutput);
			}

			AddRow(rowToAdd, dataSetToProcess);
		}

		void ProcessDirectTransactions(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			if (rowToAdd.ExtraInfo == "LINE")
			{
				rowToAdd.Amount = -rowToAdd.Amount;
				rowToAdd.GST = -rowToAdd.GST;

				if (rowToAdd.Type == TransactionTypes.DirectReceipt && rowToAdd.Ledger == LedgerTypes.CashBook)
				{
					AddToDataSet(rowToAdd, dataSetToProcess, rowToAdd.GST, fGSTOutput);
				}
				else if (rowToAdd.Type == TransactionTypes.DirectPayment && rowToAdd.Ledger == LedgerTypes.CashBook)
				{
					AddToDataSet(rowToAdd, dataSetToProcess, rowToAdd.GST, fGSTInput);
				}
			}
			else
			{
				rowToAdd.Amount = rowToAdd.Amount + rowToAdd.GST;
			}

			AddRow(rowToAdd, dataSetToProcess);
		}

		void Process_DSC_OVP(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			AddToARAPControlAccount(rowToAdd);

			Account controlAccountToCopy = (rowToAdd.Type == TransactionTypes.Discount ? fDiscount : fOverpayment);

			rowToAdd.Amount = -rowToAdd.Amount;
			rowToAdd.GLAccount = controlAccountToCopy.AccountNo;
			rowToAdd.GLAccountDesc = controlAccountToCopy.Description;
			AddRow(rowToAdd, dataSetToProcess);
		}

		void Process_JNL_PAY_REC(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			AddToARAPControlAccount(rowToAdd);
			rowToAdd.Amount = -rowToAdd.Amount;
			AddRow(rowToAdd, dataSetToProcess);
		}

		void ProcessContra(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			AddToARAPControlAccount(rowToAdd);
		}

		void ProcessExchangeDiff(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess)
		{
			if (rowToAdd.Ledger == LedgerTypes.CashBook)
			{
				AddRow(rowToAdd, dataSetToProcess);
			}
			else
			{
				AddToARAPControlAccount(rowToAdd);
			}

			rowToAdd.Amount = -rowToAdd.Amount;
			rowToAdd.GLAccount = fExchangeDifference.AccountNo;
			rowToAdd.GLAccountDesc = fExchangeDifference.Description;
			AddRow(rowToAdd, dataSetToProcess);
		}

		#endregion

		#region DataSet_related

#if DEBUG
		internal void FillDataSetForTestOnly(GLAccountListDocumentDataSet dataSet)
		{
			FillDataSet(dataSet, StartDate.ToDateTime(), EndDate.ToDateTime());
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal void FillDataSet(GLAccountListDocumentDataSet dataSet, DateTime tranStartDate, DateTime tranEndDate)
		{
			string sQL = GetSQL();
			DbCommand sqlCmd = ((IDbConnected)Factory).Connection.Command(sQL);
			sqlCmd.AddParameter("@CurrentCompany", SqlDbType.UniqueIdentifier, Company);
			sqlCmd.AddParameter("@StartDate", SqlDbType.DateTime, tranStartDate);
			sqlCmd.AddParameter("@EndDate", SqlDbType.DateTime, tranEndDate);

			using (var reader = sqlCmd.ExecuteReader())
			{
				do
				{
					while (reader.Read())
					{
						GLAccountListDocumentDataSet.GLAccountListDataSetRow newRow = (GLAccountListDocumentDataSet.GLAccountListDataSetRow)dataSet.GLAccountListDataSet.NewRow();

						for (int i = 0; i < reader.FieldCount; i++)
						{
							string columnName = reader.GetName(i);
							newRow[columnName] = reader[i];
						}
						ProcessRow(newRow, dataSet);
					}
				}
				while (reader.NextResult());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public GLAccountListDocumentDataSet DoFilter(string filter, GLAccountListDocumentDataSet dataSetToSort)
		{
			DataSet temporaryDataSet = null;
			temporaryDataSet = dataSetToSort.Clone();
			DataRow newRow = null;
			DataRow[] rows = dataSetToSort.GLAccountListDataSet.Select(filter);

			foreach (DataRow row in rows)
			{
				newRow = temporaryDataSet.Tables[0].NewRow();
				for (int i = 0; i < row.Table.Columns.Count; i++)
				{
					newRow[i] = row[i];
				}
				temporaryDataSet.Tables[0].Rows.Add(newRow);
			}

			return (GLAccountListDocumentDataSet)temporaryDataSet;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public GLAccountListDocumentDataSet Sort(string column, GLAccountListDocumentDataSet dataSetToSort)
		{
			DataSet temporaryDataSet = null;
			temporaryDataSet = dataSetToSort.Clone();
			DataRow newRow = null;
			DataRow[] rows = dataSetToSort.GLAccountListDataSet.Select("", column);

			foreach (DataRow row in rows)
			{
				newRow = temporaryDataSet.Tables[0].NewRow();
				for (int i = 0; i < row.Table.Columns.Count; i++)
				{
					newRow[i] = row[i];
				}
				temporaryDataSet.Tables[0].Rows.Add(newRow);
			}

			return (GLAccountListDocumentDataSet)temporaryDataSet;
		}

		void AddToDataSet(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess, decimal amount, Account accountToAdd)
		{
			GLAccountListDocumentDataSet.GLAccountListDataSetRow addedRow = AddRow(rowToAdd, dataSetToProcess) as GLAccountListDocumentDataSet.GLAccountListDataSetRow;
			addedRow.GLAccount = accountToAdd.AccountNo;
			addedRow.GLAccountDesc = accountToAdd.Description;
			addedRow.Amount = amount;
		}

		void AddControlToDataSet(ArrayList controlAccountList, GLAccountListDocumentDataSet dataSetToProcess, string ledger)
		{
			foreach (Account control in controlAccountList)
			{
				GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd = dataSetToProcess.GLAccountListDataSet.NewGLAccountListDataSetRow();

				rowToAdd.Amount = Convert.ToDecimal(control.Amount);
				rowToAdd.GLAccount = control.AccountNo;
				rowToAdd.GLAccountDesc = control.Description;
				rowToAdd.Period = control.PostPeriod;
				rowToAdd.Ledger = ledger;
				rowToAdd.TransactionDesc = Res.GetString("5b741bfb-8205-4157-aec9-a71293336dbc", "{0} Control Account", ledger) + " ";

				SetDebitCredit(rowToAdd);

				AddRow(rowToAdd, dataSetToProcess);
			}
		}

		#endregion

		#region Implementation

		bool fShowHeaderDescription;
		bool ShowHeaderDescription
		{
			get { return fShowHeaderDescription; }
		}

		protected void SetPeriodRange(int startPeriod, int endPeriod)
		{
			int period = startPeriod;

			PeriodRange = new int[endPeriod - startPeriod + 1];

			for (int i = 0; i <= endPeriod - startPeriod; i++)
			{
				PeriodRange[i] = period;
				period = PeriodCalculator.GetNextPeriod(period);
			}
		}

		protected virtual void SetAccountRange()
		{
			ZQuery filter = new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.GreaterThanOrEqualTo, StartAccount.AccountNo);
			filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.LessThanOrEqualTo, EndAccount.AccountNo);
			filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.NotEqual, "HDR");
			filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.NotEqual, "ALT");
			filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.NotEqual, "TTL");
			GLAccountList = Factory.Load(typeof(AccGLHeader), filter) as AccGLHeader[];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		protected virtual void SetWhereClause()
		{
			if (!fGLTransactionContainsControlAccount)
			{
				WhereClause = " AND " + AccGLHeaderSchema.Constants.AG_AccountType + " NOT IN ('HDR', 'ALT', 'TTL')" +
					" AND " + AccGLHeaderSchema.Constants.AG_AccountNum + " >= '" + StartAccount.AccountNo + "' AND " + AccGLHeaderSchema.Constants.AG_AccountNum + " <= '" + EndAccount.AccountNo + "' ";
			}
			if (!string.IsNullOrEmpty(fLanguage))
			{
				WhereClause = WhereClause + " AND AJ_Language = '" + fLanguage + "'";
			}
		}

		protected virtual void SetDebitCredit(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd)
		{
			if (rowToAdd.Amount >= 0)
			{
				rowToAdd.Debit = rowToAdd.Amount;
			}
			else
			{
				rowToAdd.Credit = -1.0m * rowToAdd.Amount;
			}
		}

		protected virtual void SetDebitCredit(GLAccountListDocumentDataSet dataSetToProcess)
		{
			foreach (GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToProcess in dataSetToProcess.GLAccountListDataSet)
			{
				SetDebitCredit(rowToProcess);
			}
		}

		bool Is_INV_CRD_ADJ(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd)
		{
			return (rowToAdd.Type == TransactionTypes.Invoice || rowToAdd.Type == TransactionTypes.CreditNote || rowToAdd.Type == TransactionTypes.AdjustmentNote);
		}

		double GetARAPAmount(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd)
		{
			return (double)(Is_INV_CRD_ADJ(rowToAdd) ? rowToAdd.Amount + rowToAdd.GST : rowToAdd.Amount);
		}

		protected virtual string GetSQL()
		{
			string sQL = "";

			sQL += SetGLFromHeader();
			sQL += SetGetGLFromHeaderBank();
			if (fGLTransactionContainsControlAccount)
			{
				sQL += SetGetGLFromControlAccount();
			}
			sQL += SetDirectReceiptPaymentLine();
			sQL += SetDirectReceiptPaymentBank();
			sQL += SetInvCrdAdj();
			sQL += SetWIPSJournal();
			sQL += SetWIPSJournalReverse();
			sQL += SetCFX();
			sQL += SetGLJournal();
			sQL += SetGLAutoJournal();
			sQL += SetGLReverseJournal();

			return sQL;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void SaveAsXML(string filename, DataSet result)
		{
			//Create the FileStream to write with.
			using (FileStream fs = new FileStream(filename, FileMode.Create))
			{
				//Create an XmlTextWriter for the FileStream.
				XmlTextWriter xtw = new XmlTextWriter(fs, Encoding.Unicode);

				//Add processing instructions to the beginning of the XML file, one of which indicates a style sheet.
				xtw.WriteProcessingInstruction((NoResString)"xml", (NoResString)"version='1.0'");
																		//xtw.WriteProcessingInstruction("xml-stylesheet", "type='text/xsl' href='TrialBalance.xsl'");

				//Write the XML from the dataset to the file.
				result.WriteXml(xtw);
				xtw.Close();
			}
		}

		protected virtual int MaximumRowsBeforeException
		{
			get { return fMaximumRowsBeforeException; }
		}
		const int fMaximumRowsBeforeException = 600000;

		#endregion

		#region SQL

		protected virtual string SetGLFromHeader()
		{
			return @" SELECT			AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate,
										AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AH_InvoiceAmount as Amount,
										AH_GSTAmount as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AH_TransactionNum as VoucherNumber,
										AH_ExchangeRate as ExchangeRate,
										RX_Code " + ExtraFieldsString + @"

						FROM			dbo.AccTransactionHeader 
										INNER JOIN		dbo.AccGLHeader ON AH_AG = AG_PK
										INNER JOIN		dbo.GlbBranch ON AH_GB = GB_PK
										INNER JOIN		dbo.GlbDepartment ON AH_GE = GE_PK
										INNER JOIN		dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
										LEFT OUTER JOIN	dbo.OrgHeader ON AH_OH = OH_PK
										" + ExtraTablesString + @"

						WHERE 			AH_Ledger in ('AR', 'AP') 
										AND AH_TransactionType = 'JNL'
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained from AH_AB
		/// Header Only
		/// AR & AP Payment and Receipt, CB Transfer and CB Exchange Differences
		/// </summary>
		/// <returns></returns>
		protected virtual string SetGetGLFromHeaderBank()
		{
			return @"	SELECT			AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate,
										AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AH_InvoiceAmount as Amount,
										AH_GSTAmount as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"

						FROM			dbo.AccTransactionHeader 
										INNER JOIN		dbo.AccBankAccount ON AH_AB = AB_PK
										INNER JOIN		dbo.AccGLHeader ON AB_AG = AG_PK
										INNER JOIN		dbo.GlbBranch ON AH_GB = GB_PK
										INNER JOIN		dbo.GlbDepartment ON AH_GE = GE_PK
										INNER JOIN		dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
										LEFT OUTER JOIN	dbo.OrgHeader ON AH_OH = OH_PK
										" + ExtraTablesString + @"

						WHERE 			((AH_Ledger IN ('AR', 'AP') AND AH_TransactionType IN ('PAY', 'REC')) OR
										(AH_Ledger = 'CB' AND AH_TransactionType in ('TRF', 'EXX')))
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generate Transactions which GL is obtained from their respected Control Account 
		/// Header Only
		/// Overpayment, Discount, AR & AP Exchange Diff, Contra and Transfer
		/// </summary>
		/// <returns></returns>
		protected virtual string SetGetGLFromControlAccount()
		{
			return @" SELECT			'' as GLAccount,
										'' as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate,
										AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AH_InvoiceAmount as Amount,
										AH_GSTAmount as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"

						FROM 			dbo.AccTransactionHeader 
										INNER JOIN		dbo.GlbBranch ON AH_GB = GB_PK
										INNER JOIN		dbo.GlbDepartment ON AH_GE = GE_PK
										INNER JOIN		dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										LEFT OUTER JOIN	dbo.OrgHeader ON AH_OH = OH_PK
										LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
										" + ExtraTablesString + @"

						WHERE			(AH_TransactionType in ('OVP', 'DSC') OR
										(AH_TransactionType in ('EXX', 'CTR', 'TRF') AND AH_Ledger in ('AR', 'AP')))
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained From Transaction Line
		/// Header and Lines
		/// Direct Receipt & Payment
		/// </summary>
		/// <returns></returns>
		protected virtual string SetDirectReceiptPaymentLine()
		{
			return @"	SELECT			AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate,
										AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										" + GetTransactionDescriptionTruncated() + @" as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										'LINE' AS ExtraInfo,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"
						
						FROM 			dbo.AccTransactionHeader 
										INNER JOIN		dbo.AccTransactionLines ON AH_PK = AL_AH
										INNER JOIN		dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN		dbo.GlbDepartment ON AL_GE = GE_PK					
										INNER JOIN		dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										INNER JOIN		dbo.AccGLHeader ON AL_AG = AG_PK 
										LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
										LEFT OUTER JOIN	dbo.OrgHeader ON AL_OH = OH_PK
										LEFT OUTER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
										" + ExtraTablesString + @"

						WHERE 			AH_TransactionType in ('DRC', 'DPY')
										AND AH_Ledger = 'CB'
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained from the Header's Bank
		/// Header Only
		/// Direct Receipt & Payment
		/// </summary>
		/// <returns></returns>
		protected virtual string SetDirectReceiptPaymentBank()
		{
			return @"	SELECT			AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AH_InvoiceAmount as Amount,
										AH_GSTAmount as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										'BANK' AS ExtraInfo,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"

						FROM 			dbo.AccTransactionHeader 
										INNER JOIN		dbo.AccBankAccount ON AH_AB = AB_PK 
										INNER JOIN		dbo.GlbBranch ON AH_GB = GB_PK
										INNER JOIN		dbo.GlbDepartment ON AH_GE = GE_PK
										INNER JOIN		dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										INNER JOIN		dbo.AccGLHeader ON AB_AG = AG_PK
										LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
										LEFT OUTER JOIN	dbo.OrgHeader ON AH_OH = OH_PK
										" + ExtraTablesString + @"

						WHERE 			AH_TransactionType in ('DRC', 'DPY')
										AND AH_Ledger = 'CB'
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained either directly from the Lines or Line Charge
		/// Header and Lines
		/// Invoice, Credit Notes and Adjustment
		/// </summary>
		/// <returns></returns>
		protected virtual string SetInvCrdAdj()
		{
			return @"	SELECT			AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										" + GetTransactionDescriptionTruncated() + @" as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										CASE WHEN AH_Ledger = 'AP' THEN AH_TransactionReference
											ELSE AH_TransactionNum
										END as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"

						FROM 			dbo.AccTransactionHeader 
										INNER JOIN		dbo.AccTransactionLines ON AH_PK = AL_AH 
										LEFT OUTER JOIN	dbo.AccChargeCode ON AL_AC = AC_PK 
										INNER JOIN		dbo.AccGLHeader ON CASE WHEN AL_AC is null THEN AL_AG ELSE 
															(CASE	WHEN AL_LineType = 'REV' THEN AC_AG_RevenueAccount
																	WHEN AL_LineType = 'CST' THEN AC_AG_CostAccount
																	ELSE AL_AG END) END = AG_PK
										INNER JOIN		dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN		dbo.GlbDepartment ON AL_GE = GE_PK
										INNER JOIN		dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
										LEFT OUTER JOIN dbo.OrgHeader ON AH_OH = OH_PK
										" + ExtraTablesString + @"
						
						WHERE 			AH_Ledger IN ('AP', 'AR') 
										AND AH_TransactionType IN ('INV', 'CRD', 'ADJ')
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained from the Line Charges
		/// Header and Lines
		/// WIPS and Accruals
		/// </summary>
		/// <returns></returns>
		protected virtual string SetWIPSJournal()
		{
			return @"	SELECT			AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AL_PostDate as InvoiceDate, AL_PostDate as PostDate,
										dbo.GetPeriodFromDate (AL_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										'JC' as Ledger,
										AL_LineType as Type,
										'' as TransactionNumber,
										" + GetAL_DescTruncated() + @" as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AL_ReverseDate, @CurrentCompany) as ReversePeriod,
										'' as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"
						
						FROM 			dbo.AccTransactionLines
										INNER JOIN 		dbo.AccChargeCode ON AL_AC = AC_PK 
										INNER JOIN 		dbo.AccGLHeader ON CASE WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount ELSE AC_AG_AccrualAccount END = AG_PK
										INNER JOIN 		dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN 		dbo.GlbDepartment ON AL_GE = GE_PK
										INNER JOIN		dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
										LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
										LEFT OUTER JOIN dbo.OrgHeader ON AL_OH = OH_PK
										" + ExtraTablesStringForWIPs + @"

						WHERE 			AL_PostToGL = 'Y'
										AND AL_LineType in ('WIP', 'ACR')
										AND GB_GC = @CurrentCompany
										AND AL_PostDate Between @StartDate AND @EndDate "
				+ WhereClause;
		}

		protected virtual string SetWIPSJournalReverse()
		{
			return @"	SELECT			AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AL_PostDate as InvoiceDate, AL_PostDate as PostDate,
										dbo.GetPeriodFromDate (AL_ReverseDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										'JC' as Ledger,
										AL_LineType as Type,
										'' as TransactionNumber,
										" + GetAL_DescTruncated() + @" as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										-AL_LineAmount as Amount,
										-AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AL_ReverseDate, @CurrentCompany) as ReversePeriod,
										'' as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"

						FROM 			dbo.AccTransactionLines
										INNER JOIN 		dbo.AccChargeCode ON AL_AC = AC_PK 
										INNER JOIN 		dbo.AccGLHeader ON CASE WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount ELSE AC_AG_AccrualAccount END = AG_PK
										INNER JOIN 		dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN 		dbo.GlbDepartment ON AL_GE = GE_PK
										INNER JOIN		dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
										LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
										LEFT OUTER JOIN dbo.OrgHeader ON AL_OH = OH_PK
										" + ExtraTablesStringForWIPs + @"

						WHERE 			AL_ReverseToGL = 'Y'
										AND AL_LineType in ('WIP', 'ACR')
										AND GB_GC = @CurrentCompany
										AND AL_ReverseDate Between @StartDate AND @EndDate  "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained both Charges and Lines
		/// Headers and Lines
		/// CFX (JobCosting Ledger Type)
		/// </summary>
		/// <returns></returns>
		protected virtual string SetCFX()
		{
			return @"SELECT			AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										" + GetTransactionDescriptionTruncated() + @" as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										'CHARGE' as ExtraInfo,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"
						
						FROM 			dbo.AccTransactionHeader 
										INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH 
										INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK 
										INNER JOIN dbo.AccGLHeader ON CASE 	WHEN AL_LineType = 'REV' THEN AC_AG_RevenueAccount 
														WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount
														WHEN AL_LineType = 'CST' THEN AC_AG_CostAccount
														ELSE AC_AG_AccrualAccount END = AG_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
										INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
										LEFT OUTER JOIN dbo.OrgHeader ON AH_OH = OH_PK
										" + ExtraTablesString + @"

						WHERE 			AH_Ledger IN ('JC') 
										AND AH_TransactionType IN ('JNL')
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause +
				@" SELECT		AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										" + GetAL_DescTruncated() + @" as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										'LINE' as ExtraInfo,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"
						
						FROM 			dbo.AccTransactionHeader 
										INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH 
										INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
										INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
										LEFT OUTER JOIN dbo.OrgHeader ON AH_OH = OH_PK
										" + ExtraTablesString + @"

						WHERE 			AH_Ledger IN ('JC') 
										AND AH_TransactionType IN ('JNL')
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		protected virtual string SetGLJournal()
		{
			return @"SELECT				AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										" + GetTransactionDescriptionTruncated() + @" as TransactionDesc,
										'' as Job,
										'' as ChargeCode,
										'' as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AH_DueDate, @CurrentCompany) as ReversePeriod,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"
					
					FROM				dbo.AccTransactionHeader
										INNER JOIN dbo.AccTransactionLines on AH_PK = AL_AH
										INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
										INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										" + ExtraTablesString + @"

					WHERE				AH_Ledger  = 'GL' 
										AND AH_TransactionType in ('GJL')
										AND GB_GC = @CurrentCompany 
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		protected virtual string SetGLAutoJournal()
		{
			return @"SELECT				AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										" + GetTransactionDescriptionTruncated() + @" as TransactionDesc,
										'' as Job,
										'' as ChargeCode,
										'' as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AH_DueDate, @CurrentCompany) as ReversePeriod,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"

					FROM				dbo.AccTransactionHeader
										INNER JOIN dbo.AccTransactionLines on AH_PK = AL_AH
										INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
										INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										" + ExtraTablesString + @"

					WHERE				AH_Ledger  = 'GL' 
										AND AH_TransactionType in ('AJL')
										AND GB_GC = @CurrentCompany 
										AND NOT (AH_DueDate < @StartDate OR AH_PostDate > @EndDate) "
				+ WhereClause;
		}

		protected virtual string SetGLReverseJournal()
		{
			return @"SELECT				AG_AccountNum as GLAccount,
										AG_Description as GLAccountDesc,
										AH_DueDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										" + GetTransactionDescriptionTruncated() + @" as TransactionDesc,
										'' as Job,
										'' as ChargeCode,
										'' as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AH_DueDate, @CurrentCompany) as ReversePeriod,
										AH_TransactionNum as VoucherNumber,
										RX_Code " + ExtraFieldsString + @"

					FROM				dbo.AccTransactionHeader
										INNER JOIN dbo.AccTransactionLines on AH_PK = AL_AH
										INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
										INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
										" + ExtraTablesString + @"

					WHERE				AH_Ledger  = 'GL' 
										AND AH_TransactionType in ('RJL')
										AND GB_GC = @CurrentCompany 
										AND ((AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) 
										OR (AH_DueDate >= @StartDate AND AH_DueDate <= @EndDate)) "
				+ WhereClause;
		}

		protected virtual string ExtraFieldsString
		{
			get { return ""; }
		}

		protected virtual string ExtraTablesString
		{
			get { return ""; }
		}

		protected virtual string ExtraTablesStringForWIPs
		{
			get { return ""; }
		}

		protected string GetAL_DescTruncated()
		{
			return "SUBSTRING(AL_Desc, 1, 60)";
		}

		string GetTransactionDescriptionTruncated()
		{
			string result = "";
			if (ShowHeaderDescription)
			{
				result = "SUBSTRING(AH_DESC, 1, 60)";
			}
			else
			{
				result = "SUBSTRING(AL_Desc, 1, 60)";
			}
			return result;
		}

		#endregion

#if DEBUG
		#region Test Class

		public class MockGLAccountDocumentDataProvider : GLAccountDocumentDataProvider
		{
			public MockGLAccountDocumentDataProvider(Guid startGL, Guid endGL)
				: this(startGL, endGL, 1, 201001)
			{
			}

			public MockGLAccountDocumentDataProvider(Guid startGL, Guid endGL, int fStartPeriod, int fEndPeriod)
			{
				StartPeriod = fStartPeriod;
				EndPeriod = fEndPeriod;

				FromGLAccount = startGL;
				ToGLAccount = endGL;

				Company = GlbCompany.CurrentCompany.PK.ToGuid();
				base.SetDatesFromPeriods();
			}

			public string GetGLAccountType_Exposed(string accountNum)
			{ return base.GetGLAccountType(accountNum); }

			public void GetControlAccountExposed()
			{ GetControlAccount(); }

			public void SetPeriodRange_Exposed(int startPeriod, int endPeriod)
			{
				base.SetPeriodRange(startPeriod, endPeriod);
			}

			public int GetStartPeriodOfFinancialYear_Exposed(int year)
			{
				return base.GetStartPeriodOfFinancialYear(year);
			}

			public BusinessObjectFactory Factory_Exposed
			{
				get
				{
					return base.fFactory;
				}

				set
				{
					base.fFactory = value;
				}
			}

			protected override void GetControlAccount()
			{
				fARControl = new Account(Guid.NewGuid());
				fARControl.AccountNo = "ARControl";

				fAPControl = new Account(Guid.NewGuid());
				fAPControl.AccountNo = "APControl";

				fExchangeDifference = new Account(Guid.NewGuid());
				fExchangeDifference.AccountNo = "ExchangeDifference";

				fDiscount = new Account(Guid.NewGuid());
				fDiscount.AccountNo = "Discount";

				fOverpayment = new Account(Guid.NewGuid());
				fOverpayment.AccountNo = "Overpayment";

				fGSTInput = new Account(Guid.NewGuid());
				fGSTInput.AccountNo = "GSTInput";

				fGSTOutput = new Account(Guid.NewGuid());
				fGSTOutput.AccountNo = "GSTOutput";

				fWIPAccount = new Account(Guid.NewGuid());
				fWIPAccount.AccountNo = "WIPAccount";

				fACRAccount = new Account(Guid.NewGuid());
				fACRAccount.AccountNo = "ACRAccount";

				fPendingGSTInput = new Account(Guid.NewGuid());
				fPendingGSTInput.AccountNo = "PendingGSTInput";

				fPendingGSTOutput = new Account(Guid.NewGuid());
				fPendingGSTOutput.AccountNo = "PendingGSTOutput";

				fJobRevenueJournalControl = new Account(Guid.NewGuid());
				fJobRevenueJournalControl.AccountNo = "JobRevenueJournalControl";
			}

			#region Properties

			public Account ARControl
			{
				get
				{
					return fARControl;
				}
			}

			public Account APControl
			{
				get
				{
					return fAPControl;
				}
			}

			public Account ExchangeDifference
			{
				get
				{
					return fExchangeDifference;
				}
			}

			public Account Discount
			{
				get
				{
					return fDiscount;
				}
			}

			public Account Overpayment
			{
				get
				{
					return fOverpayment;
				}
			}

			public Account GSTInput
			{
				get
				{
					return fGSTInput;
				}
			}

			public Account GSTOutput
			{
				get
				{
					return fGSTOutput;
				}
			}

			public Account WIPAccount
			{
				get
				{
					return fWIPAccount;
				}
			}

			public Account ACRAccount
			{
				get
				{
					return fACRAccount;
				}
			}

			public Account PendingGSTInput
			{
				get
				{
					return fPendingGSTInput;
				}
			}

			public Account PendingGSTOutput
			{
				get
				{
					return fPendingGSTOutput;
				}
			}

			public Account JobRevenueJournalControl
			{
				get
				{
					return fJobRevenueJournalControl;
				}
			}

			public Account GetARControlExposed(int period)
			{
				return base.GetARControl(period);
			}

			public Account GetAPControlExposed(int period)
			{
				return base.GetAPControl(period);
			}

			public int[] PeriodRangeExposed
			{
				get
				{
					return PeriodRange;
				}
			}

			public int StartPeriodExposed
			{
				get
				{
					return StartPeriod;
				}

				set
				{
					StartPeriod = value;
				}
			}

			public int EndPeriodExposed
			{
				get
				{
					return EndPeriod;
				}

				set
				{
					EndPeriod = value;
				}
			}

			public bool fGLTransactionContainsControlAccount_Exposed
			{
				get
				{
					return base.fGLTransactionContainsControlAccount;
				}

				set
				{
					base.fGLTransactionContainsControlAccount = value;
				}
			}

			#endregion

		}

		internal class ClientGLAccountDocumentDataProvider_ForTestOnly : GLAccountDocumentDataProvider
		{
			ClientGLAccountDocumentDataProvider_ForTestOnly()
			{
			}

			public static void Register()
			{
				OverridableNewDelegate.Value = new NewDelegate(GetInstance);
			}

			static GLAccountDocumentDataProvider GetInstance()
			{
				return new ClientGLAccountDocumentDataProvider_ForTestOnly();
			}

			protected override int MaximumRowsBeforeException
			{
				get { return 1000000; }
			}
		}

		#endregion
#endif
	}
}
