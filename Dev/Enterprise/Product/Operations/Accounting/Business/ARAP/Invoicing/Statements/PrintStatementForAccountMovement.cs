using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PrintStatementForAccountMovement : PrintStatement
	{
		public PrintStatementForAccountMovement(BusinessObjectFactory factory, GlbBranch branch, bool isMultipleCurrency, decimal openingBalance = -1M)
			: base(factory, branch)
		{
			this.isMultipleCurrency = isMultipleCurrency;
			this.openingBalance = openingBalance;
			IssueBySettlementGroup = false;
			IssueByTransactionBranch = false;
			IssueByTransactionDepartment = false;
			loadTransactionForDueBuckets = false;
		}

		#region Properties

		#region PostDateFrom
		public ZDateTime PostDateFrom
		{
			get { return fPostDateFrom; }
			set { fPostDateFrom = value; }
		}

		ZDateTime fPostDateFrom;
		#endregion

		#region PostDateTo
		public ZDateTime PostDateTo
		{
			get { return fPostDateTo; }
			set
			{
				fPostDateTo = value;
				EndOfPeriod = fPostDateTo.Date.AddDays(1);
			}
		}

		ZDateTime fPostDateTo;
		#endregion

		#region GroupBy
		public ZString GroupBy
		{
			get { return fGroupBy; }
			set { fGroupBy = value; }
		}

		ZString fGroupBy;
		#endregion

		#region IsMultipleCurrency
		public ZBool IsMultipleCurrency
		{
			get { return isMultipleCurrency; }
			set { isMultipleCurrency = value; }
		}
		#endregion		

		#region Transactions
		public override TransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = LoadTransactions<AccountMovement>();
					if (fTransactions != null && fTransactions.Count > 0)
					{
						foreach (AccountMovement transaction in fTransactions)
						{
							transaction.IsMultipleCurrency = IsMultipleCurrency;
							transaction.AccountMovementCurrency = CurrencyNK;
							transaction.OpeningBalance = OpeningBalance;
						}
						(fTransactions[0] as AccountMovement).IsFirstTransaction = true;
					}
				}
				return fTransactions;
			}
		}
		#endregion

		#region TransactionForDueBuckets
		public TransactionHeaderCollection TransactionForDueBuckets
		{
			get
			{
				if (fTransactionForDueBuckets == null)
				{
					loadTransactionForDueBuckets = true;
					fTransactionForDueBuckets = LoadTransactions<AccountMovement>();
					if (fTransactionForDueBuckets != null && fTransactionForDueBuckets.Count > 0)
					{
						foreach (AccountMovement transaction in fTransactionForDueBuckets)
						{
							transaction.IsMultipleCurrency = IsMultipleCurrency;
							transaction.AccountMovementCurrency = CurrencyNK;
							transaction.OpeningBalance = OpeningBalance;
						}
						(fTransactionForDueBuckets[0] as AccountMovement).IsFirstTransaction = true;
					}

					loadTransactionForDueBuckets = false;
				}
				return fTransactionForDueBuckets;
			}
		}
		TransactionHeaderCollection fTransactionForDueBuckets;
		#endregion

		#region OpeningBalance
		public ZDecimal OpeningBalance
		{
			get { return openingBalance; }
			set { openingBalance = value; }
		}
		#endregion

		#region ClosingBalance
		public ZDecimal ClosingBalance
		{
			get
			{
				fClosingBalance = OpeningBalance;
				if (Transactions != null)
				{
					foreach (TransactionHeader item in Transactions)
					{
						if (IsMultipleCurrency)
						{
							fClosingBalance += item.AH_LocalTotal;
						}
						else
						{
							fClosingBalance += item.AH_OSTotal;
						}
					}
				}
				return fClosingBalance;
			}
		}

		ZDecimal fClosingBalance;
		#endregion

		#endregion

		#region Function
		public override TransactionHeaderCollection GetInvoicesForAttachment()
		{
			var invoices = new TransactionHeaderCollection(Factory);
			foreach (AccountMovement accMovement in Transactions)
			{
				if (accMovement.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice
					|| accMovement.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote
					|| accMovement.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote)
				{
					invoices.Add(accMovement.Transaction);
				}
			}
			return invoices;
		}
		#endregion

		protected override void AddStandardSQLString(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			AddTransactionHeaderSelect(sqlString, sqlParams);
			sqlString.Append(string.Format((NoResString)"ORDER BY {0}.{1}, {2} , ", Constants.AccTransactionHeader, Constants.AH_PostDate, Constants.AH_TransTypeIndex));
			sqlString.Append(Constants.AccTransactionHeader + "." + Constants.AH_TransactionNum + " ASC");
		}

		void AddTransactionHeaderSelect(ZStringBuilder sqlString, ZSqlParameterCollection sqlParams)
		{
			ZString amountExpresion = isMultipleCurrency ? (ZString)$"{Constants.AccTransactionHeader}.{Constants.AH_LocalTotal} " : (ZString)Constants.AH_OSTotal;
			ZString currencyExpresion = isMultipleCurrency ? ZString.Format("{0}.{1} ", Constants.AccTransactionHeader, Constants.AH_RX_NKTransactionCurrency) : ZString.Format("'{0}' ", CurrencyNK);
			ZString transactionTypeExpresion = ZString.Format((NoResString)" AND {0}.{1} NOT IN ('INB' {2})", Constants.AccTransactionHeader, Constants.AH_TransactionType, (!isMultipleCurrency ? (NoResString)", 'EXX'" : string.Empty));
			ZString transactionTypes = isMultipleCurrency ? string.Format((NoResString)"'INV CRD ADJ CTR DSC EXX JNL OVP REC TRF'") : string.Format((NoResString)"'INV CRD ADJ CTR DSC JNL OVP REC TRF'");
			ZString fromDateExpresion = (loadTransactionForDueBuckets || !PostDateFrom.IsValid) ? ZString.Empty : ZString.Format((NoResString)" AND	{0}.{1} >= @fromDate ", Constants.AccTransactionHeader, Constants.AH_PostDate);

			sqlString.Append(ZString.Format(@"SELECT		{0}.{1}, 
															{0}.{2}, 															
															CHARINDEX({0}.{3}, @TransactionTypes) AS {4},
															{0}.{5},
															{6} AS {7},
															{8} AS {9},
															{10} AS {11} 
													FROM	{0}
															INNER JOIN {12} ON {12}.{13} = {0}.{14}
													WHERE	{0}.{15} = @Ledger 															
															AND {0}.{16} = @Organisation															
															AND {0}.{17} = {18}
															AND	{12}.{19} = @Company
															{22}
															AND {0}.{20} <= @toDate
															{21} ",
													Constants.AccTransactionHeader,	//0
													Constants.AH_PK,	//1
													Constants.AH_InvoiceDate,	//2
													Constants.AH_TransactionType,	//3
													Constants.AH_TransTypeIndex,	//4
													Constants.AH_TransactionNum,	//5
													amountExpresion,	//6
													Constants.CalculatedMatchedAmount,	//7
													Constants.AH_DueDate,	//8
													Constants.DueDate,	//9
													Constants.AH_Description,	//10
													Constants.Description,	//11
													Constants.GlbCompany,	//12
													Constants.GC_PK,	//13
													Constants.AH_GC,	//14
													Constants.AH_Ledger,	//15
													Constants.AH_OH,	//16
													Constants.AH_RX_NKTransactionCurrency,	//17
													currencyExpresion,	//18
													Constants.GC_PK,	//19
													Constants.AH_PostDate, //20
													transactionTypeExpresion, // 21
													fromDateExpresion)); //22

			AddParameter(sqlParams, ref TransactionTypesParam, delegate { return ZSqlParameter.New("@TransactionTypes", transactionTypes, CargoWise.Schema.Schema.GenericStringSchemaColumn); });
			AddParameter(sqlParams, ref LedgerParam, delegate { return ZSqlParameter.New("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger); });
			AddParameter(sqlParams, ref OrganisationParam, delegate { return ZSqlParameter.New("@Organisation", OrganisationPK, AccTransactionHeaderSchema.AH_OH); });
			AddParameter(sqlParams, ref CompanyParam, delegate { return ZSqlParameter.New("@Company", Company.PK, GlbCompanySchema.PK); });
			if (!loadTransactionForDueBuckets && PostDateFrom.IsValid)
			{
				AddParameter(sqlParams, ref PostDateFromParam, delegate { return ZSqlParameter.New("@fromDate", PostDateFrom.Date, AccTransactionHeaderSchema.AH_PostDate); });
			}
			AddParameter(sqlParams, ref PostDateToParam, delegate { return ZSqlParameter.New("@toDate", PostDateTo.IsValid ? PostDateTo.EndOfDay() : new ZDateTime(Env.Time.CurrentLocalDate.AddDays(1).AddMinutes(-1)), AccTransactionHeaderSchema.AH_PostDate); });
		}

		ZSqlParameter PostDateFromParam;
		ZSqlParameter PostDateToParam;
		bool isMultipleCurrency;
		ZDecimal openingBalance;
		bool loadTransactionForDueBuckets;
	}

	public class AccountMovement : TransactionHeader
	{
		public AccountMovement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			IsFirstTransaction = false;
		}

		public bool IsMultipleCurrency { get; set; }
		public ZString AccountMovementCurrency { get; set; }
		public bool IsFirstTransaction { get; set; }

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal OpeningBalance { get; set; }

		public override ZDateTime AH_DueDate
		{
			get
			{
				if (base.AH_DueDate.IsValid)
				{
					return base.AH_DueDate;
				}
				else
				{
					return base.AH_PostDate;
				}
			}
			set
			{
				base.AH_DueDate = value;
			}
		}

		public List<TransactionLine> TransactionLines
		{
			get
			{
				if (fLines == null && loadLines)
				{
					loadLines = false;
					if (Transaction is TransactionHeaderWithLines)
					{
						fLines = (Transaction as TransactionHeaderWithLines).Lines.Cast<TransactionLine>().ToList();
					}
				}
				return fLines;
			}
		}
		List<TransactionLine> fLines;
		bool loadLines = true;

		public TransactionHeader Transaction
		{
			get
			{
				return header = header ?? Factory.Load<TransactionHeader>(PK);
			}
		}
		TransactionHeader header;

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.Invoice; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.ARInvoiceNo; }
		}
	}
}
