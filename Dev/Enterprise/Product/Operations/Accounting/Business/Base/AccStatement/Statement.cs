using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Auto;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.AccStatement
{
	#region StatementEventArgs

	public class StatementEventArgs : EventArgs
	{
		public StatementEventArgs(Statement statement)
		{
			this.Statement = statement;
		}

		public readonly Statement Statement;
		public bool Result;
	}

	public delegate void StatementEventHandler(object sender, StatementEventArgs e);

	#endregion

	public class Statement : AutoAccStatement, IBankReconMergedTransaction, ISupportDataImporting
	{
		public static readonly string DEBIT = "DR";
		public static readonly string CREDIT = "CR";

		public Statement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region BankCurrencyDecimals

		public ZInt BankCurrencyDecimals
		{
			get { return (BankAccount != null && BankAccount.AccountCurrency != null) ? BankAccount.AccountCurrency.Decimals : 2; }
		}

		public ZPropertyInfo BankCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(BankCurrencyDecimals)); }
		}

		#endregion

		#region Overriden Readonly Properties

		protected bool AS_StatementDate_ReadOnly
		{
			get { return !AccountingConfigurationRegistry.Instance.AllowManualEntryOfStatementDateWhenEnteringBankStatement.Value; }
		}

		#endregion

		#region Statement Debit

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal StatementDebit
		{
			get
			{
				return (AS_DebitCredit == DEBIT ? AS_Amount : new ZDecimal(0m));
			}
		}

		public ZPropertyInfo StatementDebitInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(StatementDebit));
			}
		}

		#endregion

		#region Statement Credit

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal StatementCredit
		{
			get
			{
				return (AS_DebitCredit == CREDIT ? AS_Amount : new ZDecimal(0m));
			}
		}

		public ZPropertyInfo StatementCreditInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(StatementCredit));
			}
		}

		#endregion

		#region AS_Type

		[List("Lookups.AS_Type_List")]
		public override ZString AS_Type
		{
			get { return base.AS_Type; }
			set
			{
				base.AS_Type = value;
				HandleDirectTransaction();
			}
		}

		#endregion

		#region AS_PageNumber

		public override ZShort AS_PageNumber
		{
			get { return base.AS_PageNumber; }
			set
			{
				base.AS_PageNumber = value;
				if (Master != null)
				{
					((BankStatement)Master).SetMaxPageNo(value);
				}
			}
		}

		#endregion

		#region OSCurrencyDecimals

		public int OSCurrencyDecimals => ((BankStatement)Master).OSCurrencyDecimals;

		#endregion

		#region AS_Amount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AS_Amount
		{
			get { return base.AS_Amount; }
			set
			{
				ZDecimal originalValue = base.AS_Amount;
				base.AS_Amount = value;

				if (AS_DebitCredit == Constants.DebitCredit.Credit)
				{
					if (Master != null)
					{
						((BankStatement)Master).CalculatePageCreditTotal();
						((BankStatement)Master).CalculateCreditTotal(originalValue, value);
					}
				}
				else
				{
					if (Master != null)
					{
						((BankStatement)Master).CalculatePageDebitTotal();
						((BankStatement)Master).CalculateDebitTotal(originalValue, value);
					}
				}

				if (Master != null)
				{
					((BankStatement)Master).CalculateBalanceAmount();
					((BankStatement)Master).CalculatePageBalanceAmount();
				}

				if (DirectTransaction != null && DirectTransaction.Lines.Count > 0)
				{
					DirectTransaction.Lines[0].AL_OSExTaxAmount = AS_Amount;
				}
			}
		}

		#endregion

		#region AS_DebitCredit

		[List("Lookups.AS_DebitCredit_List")]
		public override ZString AS_DebitCredit
		{
			get
			{
				return base.AS_DebitCredit;
			}
			set
			{
				ZString originalValue = base.AS_DebitCredit;
				base.AS_DebitCredit = value;

				if (Master != null)
				{
					((BankStatement)Master).CalculateBalanceAmount();
				}

				if ((originalValue != "") && (originalValue != base.AS_DebitCredit))
				{
					if (AS_DebitCredit == Constants.DebitCredit.Credit)
					{
						if (Master != null)
						{
							((BankStatement)Master).CalculateDebitTotal(0.0m, -AS_Amount);
							((BankStatement)Master).CalculateCreditTotal(0.0m, AS_Amount);
							((BankStatement)Master).CalculatePageCreditTotal();
						}
					}
					else
					{
						if (Master != null)
						{
							((BankStatement)Master).CalculateCreditTotal(0.0m, -AS_Amount);
							((BankStatement)Master).CalculateDebitTotal(0.0m, AS_Amount);
							((BankStatement)Master).CalculatePageDebitTotal();
						}
					}
				}
			}
		}

		#endregion

		#region AS_ChequeOrReference

		public override ZString AS_ChequeOrReference
		{
			get { return base.AS_ChequeOrReference; }
			set
			{
				base.AS_ChequeOrReference = value;
				if (DirectTransaction != null)
				{
					DirectTransaction.AH_ChequeOrReference = value;
				}
			}
		}

		#endregion

		#endregion

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			CreateCustomLog();
		}

		public override void Delete()
		{
			if (DirectTransaction != null)
			{
				DirectTransaction.Delete();
				fDirectTransaction = null;
			}

			base.Delete();
		}

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#endregion

		#region Master

		protected BankStatement fMaster;
		public BusinessObject Master
		{
			get
			{
				if (fMaster == null)
				{
					fMaster = Factory.Load(typeof(BankStatement), AS_AB) as BankStatement;
				}
				return fMaster;
			}
		}

		#endregion

		#region Direct Transaction Stuff

		#region HandleDirectTransaction

		public void HandleDirectTransaction()
		{
			if (DirectTransaction == null)
			{
				if (Lookups.BankChargeTypes_List.ContainsCode(AS_Type) && Master != null)
				{
					if (IsImportingData || ShouldCreateDirectTransaction)
					{
						if (AS_DebitCredit == Statement.CREDIT)
						{
							fDirectTransaction = Factory.New<BankReconDirectReceipt>();
						}
						else
						{
							fDirectTransaction = Factory.New<BankReconDirectPayment>();
						}
						fDirectTransaction.RelatedStatementPK = PK;
						fDirectTransaction.AH_InvoiceDate = AS_StatementDate;
						fDirectTransaction.AH_AB = AS_AB;
						fDirectTransaction.AH_ReceiptType = AS_Type;
						fDirectTransaction.AH_ChequeOrReference = AS_ChequeOrReference;
						fDirectTransaction.AH_PostDate = AS_StatementDate;
						fDirectTransaction.AH_ChequeDrawer = (fDirectTransaction is BankReconDirectPayment) ? fMaster.AB_BankName : ZString.Empty;

						fDirectTransaction.AH_RX_NKTransactionCurrency_ReadOnly = !fDirectTransaction.AH_RX_NKTransactionCurrency.IsEmpty;
						fDirectTransaction.CheckCurrencyOnEqualToCurrectCompany = true;

						DirectTransactionLineBase line = (DirectTransactionLineBase)fDirectTransaction.Lines.AddNew();
						line.AL_AG = AccountingConfigurationRegistry.Instance.BankTransactionGLAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
						line.ReadOnlyIfAL_AGIsValidAndNotMisc = true;
						line.IsParentReceiptTypeMisc = IsMiscType(AS_Type);
						line.AL_OSExTaxAmount = AS_Amount;

						ZQuery filter = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
						filter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
						filter.AddToFilter(AccTaxRateSchema.AT_Code, "NOTREPORT");
						AccTaxRate taxRate = Factory.LoadTop1<AccTaxRate>(filter);
						line.AL_AT = (taxRate != null) ? taxRate.PK : ZGuid.Empty;

						if (fDirectTransaction is BankReconDirectReceipt)
						{
							((BankReconDirectReceipt)fDirectTransaction).MakeDepositBatch();
						}

						fMaster.DirectTransactions.Headers.Add(fDirectTransaction);

						if (!IsImportingData)
						{
							ShowDirectTransaction();
						}
					}
				}
			}
			else
			{
				if (Lookups.BankChargeTypes_List.ContainsCode(AS_Type))
				{
					DirectTransaction.AH_ReceiptType = AS_Type;
				}
				else
				{
					DirectTransaction.Lines.RemoveAndDeleteAll();
					DirectTransaction.Delete();
					fDirectTransaction = null;
					HandleDirectTransaction();
				}
			}
		}

		#endregion

		#region DirectTransaction

		public DirectTransactionHeaderBase DirectTransaction
		{
			get
			{
				return fDirectTransaction;
			}
		}
		DirectTransactionHeaderBase fDirectTransaction;

		#endregion

		#region IsImporting

		public bool IsImportingData
		{
			get { return fIsImporting; }
			set { fIsImporting = value; }
		}
		bool fIsImporting;

		#endregion

		#region IsMiscType

		bool IsMiscType(ZString type)
		{
			return (AS_Type == ReceiptTypes.MiscellaneousFees || AS_Type == ReceiptTypes.MiscellaneousReceipt);
		}

		#endregion

		#region ShouldCreateDirectTransaction

		public bool ShouldCreateDirectTransaction
		{
			get { return (Master != null) && fMaster.ShouldCreateDirectTransactionForStatement(this); }
		}

		#endregion

		#region ShowDirectTransaction

		public void ShowDirectTransaction()
		{
			if (Master != null)
			{
				fMaster.ShowStatementDirectTransaction(this);
			}
		}

		#endregion

		#endregion

		#region IBankReconMergedTransaction Members

		public ZDateTime TransactionDate
		{
			get
			{
				return AS_StatementDate;
			}
		}

		public ZPropertyInfo TransactionDateInfo
		{
			get
			{
				return AS_StatementDateInfo;
			}
		}

		protected ZDateTime fInvoiceDate;

		public ZDateTime InvoiceDate
		{
			get
			{
				return AS_StatementDate;
			}
		}

		public ZPropertyInfo InvoiceDateInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(InvoiceDate));
			}
		}

		public ZString Type
		{
			get { return "STM"; }
		}

		public ZPropertyInfo TypeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Type));
			}
		}

		public ZString Method
		{
			get
			{
				return AS_Type;
			}
		}

		public ZPropertyInfo MethodInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Method));
			}
		}

		public ZString ChequeRef
		{
			get
			{
				return AS_ChequeOrReference;
			}
		}

		public ZPropertyInfo ChequeRefInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChequeRef));
			}
		}

		public ZString BatchNo
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZPropertyInfo BatchNoInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(BatchNo));
			}
		}

		public ZString Payee
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZPropertyInfo PayeeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Payee));
			}
		}

		public ZBool IsCleared
		{
			get
			{
				return AS_IsCleared;
			}
			set
			{
				AS_IsCleared = value;
				if (AS_IsCleared)
				{
					fClearedDate = AS_StatementDate;

					if (MasterBankRecon != null)
					{
						MasterBankRecon.SetTransactionIdCleared(PK);
					}
				}
				else
				{
					fClearedDate = ZDateTime.Empty;

					if (MasterBankRecon != null)
					{
						AutoReconciler reconciler = new AutoReconciler();
						reconciler.Unmatch(MasterBankRecon.MergedTransactions, this);

						MasterBankRecon.SetTransactionIdUncleared(PK);
					}
				}

				if (MasterBankRecon != null)
				{
					MasterBankRecon.CashbookTotalInfo.RefreshBinding();
					MasterBankRecon.TotalDifferenceInfo.RefreshBinding();
					MasterBankRecon.ValidateTotalDifference();
				}
				IsClearedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsClearedInfo
		{
			get
			{
				return AS_IsClearedInfo;
			}
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal Debit
		{
			get
			{
				return StatementCredit;
			}
		}

		public ZPropertyInfo DebitInfo
		{
			get
			{
				return StatementDebitInfo;
			}
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal Credit
		{
			get
			{
				return StatementDebit;
			}
		}

		public ZPropertyInfo CreditInfo
		{
			get
			{
				return StatementCreditInfo;
			}
		}

		protected BankReconciliation fMasterBankRecon;
		public BankReconciliation MasterBankRecon
		{
			get
			{
				return fMasterBankRecon;
			}

			set
			{
				fMasterBankRecon = value;
			}
		}

		public ZString LineType => Res.GetString("eee99409-8f62-42aa-91fe-08a5b47484c7", "Statement");

		public ZPropertyInfo LineTypeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(LineType));
			}
		}

		public void CreateCustomLog()
		{
			if (AS_IsCleared)
			{
				Logs.AddNew(Events.CashbookItemTickedOff, "");
			}
			else
			{
				Logs.AddNew(Events.CashbookItemUnTicked, "");
			}
		}

		ZDateTime fClearedDate;
		[ReadOnly(true)]
		public ZDateTime ClearedDate
		{
			get
			{
				if (fClearedDate.IsEmpty)
				{
					if (AS_IsCleared)
					{
						fClearedDate = AS_StatementDate;
					}
				}
				return fClearedDate;
			}
			set { SetNonPersistentPropertyValue(ClearedDateInfo, ref fClearedDate, value); }
		}

		public ZPropertyInfo ClearedDateInfo
		{
			get { return GetZPropertyInfo(nameof(ClearedDate)); }
		}

		#endregion
	}
}
