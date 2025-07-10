using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconTransaction : AccTransactionHeader, IBankReconMergedTransaction
	{
		public BankReconTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_DateClearedInCashbook), ConcurrencyPolicy.Observe);
		}

		#region Deposit Batches Collection

		[ChildEditable(false)]
		public BatchTransactionCollection DepositBatchTransactions
		{
			get
			{
				if (fDepositBatchTransactions == null)
				{
					ZQuery childQuery = new ZQuery(AccTransactionHeaderSchema.AH_ReceiptBatchNo, AH_ReceiptBatchNo);
					childQuery.AddToFilter(PKSchemaColumn, SQLComparisonOperator.NotEqual, PK);
					childQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK);
					if (BankAccount != null)
					{
						childQuery.AddToFilter(AccTransactionHeaderSchema.AH_AB, BankAccount.PK);
					}
					childQuery.AddToFilter(GetChildTransactionsTypeFilter(TransactionTypes.ReceiptBatch));

					if (!AH_ReceiptBatchNo.IsEmpty)
					{
						fDepositBatchTransactions = new BatchTransactionCollection(Factory, childQuery);
						fDepositBatchTransactions.Load();
					}
					else
					{
						fDepositBatchTransactions = new BatchTransactionCollection(Factory);

						if (!IsInDatabase && fMasterBankRecon != null)
						{
							// Not in DB? Then this direct receipt must have been created on the BankTransactionForm, and so we 
							// know that we can look this one up using GetDirectReceiptForBatch because that form added the PKs
							// to the lookup used by GetDirectReceiptForBatch.
							var directPaymentPK = fMasterBankRecon.AdditionalTransactions.GetDirectReceiptForBatch(PK);

							if (directPaymentPK.IsValid)
							{
								fDepositBatchTransactions.Add(Factory.Load<BatchTransaction>(directPaymentPK));
							}
						}
					}

					RegisterEditableChildObject(fDepositBatchTransactions);
				}

				return fDepositBatchTransactions;
			}
		}
		protected BatchTransactionCollection fDepositBatchTransactions;

		#endregion

		#region Direct Debit Batches Collection

		protected BatchTransactionCollection fDirectDebitBatchTransactions;

		public BatchTransactionCollection DirectDebitBatchTransactions
		{
			get
			{
				if (fDirectDebitBatchTransactions == null)
				{
					ZQuery childQuery = new ZQuery(AccTransactionHeaderSchema.AH_ReceiptBatchNo, AH_ReceiptBatchNo);
					childQuery.AddToFilter(PKSchemaColumn, SQLComparisonOperator.NotEqual, PK);
					childQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK);
					if (BankAccount != null)
					{
						childQuery.AddToFilter(AccTransactionHeaderSchema.AH_AB, BankAccount.PK);
					}
					childQuery.AddToFilter(GetChildTransactionsTypeFilter(TransactionTypes.DDRBatch));
					fDirectDebitBatchTransactions = new BatchTransactionCollection(Factory, childQuery);
					fDirectDebitBatchTransactions.Load();
					RegisterEditableChildObject(fDepositBatchTransactions);
				}

				return fDirectDebitBatchTransactions;
			}
		}

		#endregion

		#region Debit

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal Debit
		{
			get { return fDebit; }
		}

		public ZPropertyInfo DebitInfo
		{
			get { return GetZPropertyInfo(nameof(Debit)); }
		}

		#endregion

		#region Credit

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal Credit
		{
			get { return fCredit; }
		}

		public ZPropertyInfo CreditInfo
		{
			get { return GetZPropertyInfo(nameof(Credit)); }
		}

		#endregion

		#region DrawerOrPayeeName

		public ZString DrawerOrPayeeName
		{
			get { return fDrawerOrPayeeName; }
		}

		public ZPropertyInfo DrawerOrPayeeNameInfo
		{
			get { return GetZPropertyInfo(nameof(DrawerOrPayeeName)); }
		}

		#endregion

		#region RelatedStatementPK

		ZGuid fRelatedStatementPK;
		public ZGuid RelatedStatementPK
		{
			get { return fRelatedStatementPK; }
			set { fRelatedStatementPK = value; }
		}

		#endregion

		#region Public Override

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetDebitCredit();
			SetDrawerPayeeName();
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			CreateCustomLog();
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				throw new NotSupportedException("Cannot Delete Bank Reconciliation Transaction Object");
			}
			else
			{
				base.Delete();
			}
		}

		#endregion

		#region Implementation

		protected ZDecimal fCredit;
		protected ZDecimal fDebit;
		protected ZString fDrawerOrPayeeName;

		protected ZQuery GetChildTransactionsTypeFilter(ZString type)
		{
			ZQuery filter = new ZQuery();
			if (type == TransactionTypes.ReceiptBatch)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType,TransactionTypes.Receipt);
				filter.AddToFilter(JoinCondition.Or,AccTransactionHeaderSchema.AH_TransactionType,TransactionTypes.DirectReceipt);
			}
			else if (type == TransactionTypes.DDRBatch)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
				filter.AddToFilter(JoinCondition.Or,AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DirectPayment);
			}
			return filter;
		}

		[ReadOnly(true)]
		public override ZDateTime AH_InvoiceDate
		{
			get { return base.AH_InvoiceDate; }
			set { base.AH_InvoiceDate = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime AH_PostDate
		{
			get { return base.AH_PostDate; }
			set { base.AH_PostDate = value; }
		}

		[ReadOnly(true)]
		public override ZString AH_TransactionType
		{
			get { return base.AH_TransactionType; }
			set { base.AH_TransactionType = value; }
		}

		[ReadOnly(true)]
		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set { base.AH_ReceiptType = value; }
		}

		[ReadOnly(true)]
		public override ZString AH_ChequeOrReference
		{
			get { return base.AH_ChequeOrReference; }
			set { base.AH_ChequeOrReference = value; }
		}

		[ReadOnly(true)]
		public override ZString AH_ReceiptBatchNo
		{
			get { return base.AH_ReceiptBatchNo; }
			set { base.AH_ReceiptBatchNo = value; }
		}

		protected ZDecimal Amount
		{
			get
			{
				ZDecimal result = 0m;
				if (AH_TransactionType == TransactionTypes.ReceiptBatch)
				{
					result = AH_OSTotal;
				}
				else
				{
					if (BankAccount != null)
					{
						if (BankAccount.AB_RX_NKAccountCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							result = AH_LocalTotal;
						}
						else
						{
							result = AH_OSTotal;
						}
					}
					else
					{
						result = AH_OSTotal;
					}
				}
				return result;
			}
		}

		protected void SetDebitCredit()
		{
			fDebit = 0m;
			fCredit = 0m;

			switch (AH_TransactionType)
			{
				case TransactionTypes.ReceiptBatch:
				case TransactionTypes.DirectPayment:
				case TransactionTypes.Transfer:
					if (Amount >= 0)
					{
						fDebit = Amount;
					}
					else
					{
						fCredit = -Amount;
					}
					break;

				case TransactionTypes.Payment:
				case TransactionTypes.DDRBatch:
				case TransactionTypes.OpeningPayment:
				case TransactionTypes.OpeningReceipt:
					if (Amount < 0)
					{
						fDebit = -Amount;
					}
					else
					{
						fCredit = Amount;
					}
					break;
			}
		}

		protected void CalculateDebitCreditForDepositBatch()
		{
			ZDecimal fTotal = 0.0m;
			ZDecimal fDebitSubTotal = 0.0m;
			ZDecimal fCreditSubTotal = 0.0m;

			foreach (BatchTransaction transaction in DepositBatchTransactions)
			{
				fDebitSubTotal += transaction.Debit;
				fCreditSubTotal += transaction.Credit;
			}

			fTotal = fDebitSubTotal - fCreditSubTotal;

			if (fTotal > 0m)
			{
				fDebit = fTotal;
				fCredit = 0m;
			}
			else
			{
				fCredit = -fTotal;
				fDebit = 0m;
			}
		}

		protected void SetDrawerPayeeName()
		{
			switch (AH_TransactionType)
			{
				case TransactionTypes.Transfer:
					fDrawerOrPayeeName = (NoResString)"Bank Transfer";
					break;
				case TransactionTypes.DirectPayment:
					fDrawerOrPayeeName = AH_ChequeDrawer;
					break;
				case TransactionTypes.ReceiptBatch:
					fDrawerOrPayeeName = !AH_Desc.IsEmpty ? AH_Desc : new ZString((NoResString)"Deposit Batch No. " + AH_TransactionNum);
					break;
				case TransactionTypes.DDRBatch:
					if (AH_ReceiptType == ReceiptTypes.eNettDirectDebit)
					{
						fDrawerOrPayeeName = AH_Desc;
					}
					else
					{
						fDrawerOrPayeeName = (NoResString)"DDR Batch No. " + AH_TransactionNum;
					}
					break;
				case TransactionTypes.Payment:
				case TransactionTypes.OpeningPayment:
					OrgHeader org = Factory.Load(typeof(OrgHeader), AH_OH) as OrgHeader;
					if (org != null)
					{
						fDrawerOrPayeeName = org.OH_FullNameTruncated;
					}

					break;
				case TransactionTypes.OpeningReceipt:
					fDrawerOrPayeeName = fDrawerOrPayeeName = (NoResString)"Opening Receipt";
					break;
			}
		}

		bool IsRCBWithSingleDCR
		{
			get { return AH_TransactionType == TransactionTypes.ReceiptBatch && DepositBatchTransactions.Count == 1 && (DepositBatchTransactions[0].AH_TransactionType == TransactionTypes.Receipt || DepositBatchTransactions[0].AH_TransactionType == TransactionTypes.DirectReceipt)  && DepositBatchTransactions[0].AH_ReceiptType == ReceiptTypes.DirectCredit; }
		}

		#endregion

		#region IBankReconMergedTransaction Members

		public ZDateTime TransactionDate
		{
			get { return AH_PostDate; }
		}

		public ZPropertyInfo TransactionDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TransactionDate), x => AH_PostDateInfo); }
		}

		public ZDateTime InvoiceDate
		{
			get { return AH_InvoiceDate; }
		}

		public ZPropertyInfo InvoiceDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(InvoiceDate), x => AH_InvoiceDateInfo); }
		}

		public ZString Type
		{
			get
			{
				if ((AH_TransactionType == TransactionTypes.Payment || AH_TransactionType == TransactionTypes.DirectPayment) && AH_ReceiptType == ReceiptTypes.DirectDebit)
				{
					return TransactionTypes.DDRBatch;
				}
				else
				{
					return AH_TransactionType;
				}
			}
		}

		public ZPropertyInfo TypeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Type), x => AH_TransactionTypeInfo); }
		}

		public ZString Method
		{
			get
			{
				if (AH_TransactionType == TransactionTypes.DDRBatch && AH_ReceiptType != ReceiptTypes.eNettDirectDebit)
				{
					return ReceiptTypes.DirectDebit;
				}
				else
				{
					return IsRCBWithSingleDCR ? (ZString)ReceiptTypes.DirectCredit : AH_ReceiptType;
				}
			}
		}

		public ZPropertyInfo MethodInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Method), x => AH_ReceiptTypeInfo); }
		}

		public ZString ChequeRef
		{
			get
			{
				if (AH_TransactionType == TransactionTypes.ReceiptBatch)
				{
					return IsRCBWithSingleDCR ? DepositBatchTransactions[0].AH_ChequeOrReference : AH_ReceiptBatchNo;
				}
				else
				{
					return AH_ChequeOrReference;
				}
			}
		}

		public ZPropertyInfo ChequeRefInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ChequeRef), x => AH_ChequeOrReferenceInfo); }
		}

		public ZString BatchNo
		{
			get { return AH_ReceiptBatchNo; }
		}

		public ZPropertyInfo BatchNoInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BatchNo), x => AH_ReceiptBatchNoInfo); }
		}

		public ZString Payee
		{
			get { return DrawerOrPayeeName; }
		}

		public ZPropertyInfo PayeeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Payee), x => DrawerOrPayeeNameInfo); }
		}

		[BusinessObjectTestExclude]
		public ZBool IsCleared
		{
			get { return !ClearedDate.IsEmpty; }
			set
			{
				if (MasterBankRecon != null)
				{
					if (value)
					{
						ClearedDate = MasterBankRecon.StatementDate;

						MasterBankRecon.SetTransactionIdCleared(PK);
					}
					else
					{
						ClearedDate = ZDateTime.Empty;

						AutoReconciler reconciler = new AutoReconciler();
						reconciler.Unmatch(MasterBankRecon.MergedTransactions, this);

						MasterBankRecon.SetTransactionIdUncleared(PK);
					}

					MasterBankRecon.RefreshBinding();
					MasterBankRecon.ValidateTotalDifference();

					if (Type == TransactionTypes.ReceiptBatch)
					{
						foreach (BatchTransaction transaction in DepositBatchTransactions)
						{
							transaction.AH_DateClearedInCashbook = ClearedDate;
						}
					}
					else if (AH_TransactionType == TransactionTypes.DDRBatch)
					{
						foreach (BatchTransaction transaction in DirectDebitBatchTransactions)
						{
							transaction.AH_DateClearedInCashbook = ClearedDate;
						}
					}
				}
				else
				{
					IsClearedValueShouldBeSet = true;
					IsClearedValue = value;
				}
				IsClearedInfo.RefreshBinding();
			}
		}
		bool IsClearedValue;
		bool IsClearedValueShouldBeSet;

		public ZPropertyInfo IsClearedInfo
		{
			get { return GetZPropertyInfo(nameof(IsCleared)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal StatementDebit
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo StatementDebitInfo
		{
			get { return GetZPropertyInfo(nameof(StatementDebit)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal StatementCredit
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo StatementCreditInfo
		{
			get { return GetZPropertyInfo(nameof(StatementCredit)); }
		}

		protected BankReconciliation fMasterBankRecon;
		public BankReconciliation MasterBankRecon
		{
			get { return fMasterBankRecon; }
			set
			{
				fMasterBankRecon = value;
				if (fMasterBankRecon != null && IsClearedValueShouldBeSet)
				{
					IsCleared = IsClearedValue;
					IsClearedValueShouldBeSet = false;
					if (IsClearedValue)
					{
						MasterBankRecon.SetTransactionIdCleared(PK);
					}
					else
					{
						MasterBankRecon.SetTransactionIdUncleared(PK);
					}
				}
			}
		}

		public ZString LineType => Res.GetString("2d343843-2cee-41f1-a273-18c1d5cf93c3", "Cashbook");

		[ReadOnly(true)]
		public ZDateTime ClearedDate
		{
			get { return AH_DateClearedInCashbook; }
			set
			{
				AH_DateClearedInCashbook = value;
				ClearedDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ClearedDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ClearedDate), x => AH_DateClearedInCashbookInfo); }
		}

		public ZPropertyInfo LineTypeInfo
		{
			get { return GetZPropertyInfo(nameof(LineType)); }
		}

		public void CreateCustomLog()
		{
			if (IsCleared)
			{
				Logs.AddNew(Events.CashbookItemTickedOff, "");
			}
			else
			{
				Logs.AddNew(Events.CashbookItemUnTicked, "");
			}
		}

		#endregion

	}
}
