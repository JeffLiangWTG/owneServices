using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositBatch : TransactionHeader, IDocumentSupportable, IDocManagerSupport, IEDocsParsingSupport
	{
		public DepositBatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString DepositBatchNum
		{
			get { return AH_TransactionNum; }
		}

		#region Related Business Objects

		[ChildEditable(true)]
		public DepositBatchTransactionLineCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					LoadTransactions(ZGuid.Empty);
				}
				return fTransactions;
			}
		}
		DepositBatchTransactionLineCollection fTransactions;

		public void LoadTransactions(List<ZGuid> receiptPKs)
		{
			fTransactions = new DepositBatchTransactionLineCollection(Factory, AH_AB, GlbBranch.CurrentBranch.PK, this, ZGuid.Empty);
			fTransactions.Load(new ZQuery(AccTransactionHeaderSchema.PK, receiptPKs.ToArray()));
		}

		public void LoadTransactions(ZGuid branchFilterPK)
		{
			ZGuid originalReceiptPK = (OriginalReceipt == null) ? ZGuid.Empty : ((TransactionHeader)OriginalReceipt).PK;
			fTransactions = new DepositBatchTransactionLineCollection(Factory, AH_AB, branchFilterPK, this, originalReceiptPK);
			RegisterEditableChildObject(fTransactions);
			if (RelatedTransactionPK.IsEmpty)
			{
				fTransactions.Load();
			}
			else
			{
				fTransactions.Load(new ZQuery(AccTransactionHeaderSchema.PK, RelatedTransactionPK));
			}
		}

		public ZGuid RelatedTransactionPK
		{
			get { return fRelatedTransactionPK; }
			set { fRelatedTransactionPK = value; }
		}
		ZGuid fRelatedTransactionPK;

		public IReversing ReversingReceipt
		{
			get { return fReversingReceipt; }
			set { fReversingReceipt = value; }
		}

		IReversing fReversingReceipt;

		public void SetOriginalReceipt(IDepositBatch originalReceipt)
		{
			this.OriginalReceipt = originalReceipt;
		}
		IDepositBatch OriginalReceipt;

		#endregion

		#region Saving

		public override bool IsSavedByFactory
		{
			get
			{
				bool result = base.IsSavedByFactory;
				if (ReversingReceipt == null)
				{
					result = result && (IsSelected || IsInDatabase);
				}
				return result;
			}
		}

		protected override bool NeedToUpdateTransactionNumberFromFountain
		{
			get { return base.NeedToUpdateTransactionNumberFromFountain && ReversingReceipt == null; }
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (!IsInDatabase)
			{
				if (ReversingReceipt == null)
				{
					SetReceiptBatchNo(this);
					AH_InvoiceAmount = TotalDepositInvoiceAmount + TotalDepositGSTAmount;
					AH_OSTotal = TotalDepositOSAmount;
					AH_DueDate = AH_InvoiceDate;
					AH_ChequeDrawer = AH_ReceiptBatchNoConst;

					foreach (DepositBatchTransactionLine line in Transactions)
					{
						if (line.IsSelected)
						{
							SetReceiptBatchNo(line);
						}
					}
				}
				else if (ReversingReceipt.ReverseTransaction != null)
				{
					AH_PostDate = ((TransactionHeader)ReversingReceipt.ReverseTransaction).AH_PostDate;
				}
			}
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "AH_TransactionNum is currently 8 characters for batching")]
		void SetReceiptBatchNo(TransactionHeader transactionToSetReceiptBatchNo)
		{
			transactionToSetReceiptBatchNo.AH_ReceiptBatchNo = AH_TransactionNum;
		}

		static string AH_ReceiptBatchNoConst
		{
			get { return Res.GetString("2d5caee5-e1fa-4de8-ad07-365f40cb50fd", "Deposit Batch"); }
		}

		#endregion

		#region Validation

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new DepositBatchValidation(this);
		}

		#endregion

		#region Lookups

		protected override AccTransactionHeaderLookups GetNewLookups()
		{
			return new DepositBatchLookups(this);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Core.Constants.DocManagerCodes.DepositBatch)); }
		}
		AccountingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region Transaction Header Implementation

		protected override List<string> GetWritableProperties()
		{
			return new List<string>();
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("09daeb59-9e57-49f4-a9fb-f955896f5da9", "Deposit Batch"); }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return Enterprise.ZArchitecture.Core.LedgerTypes.CashBook; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.BatchReceiptNo; }
		}

		protected override ZString TransactionType
		{
			get { return Enterprise.ZArchitecture.Core.TransactionTypes.ReceiptBatch; }
		}

		public override ZDecimal Debit
		{
			get { return fDebit; }
		}

		public override ZDecimal Credit
		{
			get { return fCredit; }
		}

		protected override bool AH_PostDate_ReadOnly
		{
			get { return false; }
		}

		[ResourceStringData("AH_PostDate", Caption = "Deposit Post Date")]
		public override ZDateTime AH_PostDate
		{
			get { return base.AH_PostDate; }
			set
			{
				base.AH_PostDate = value;
			}
		}

		#endregion

		#region Properties

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		public ZBool IsCancelledCalc
		{
			get
			{
				ZBool result = AH_IsCancelled;
				if (!AH_IsCancelled && Transactions != null && Transactions.Count == 1)
				{
					if (
						(Transactions[0].AH_TransactionType == TransactionTypes.DirectReceipt && Factory.Load<DirectReceipt.DirectReceipt>(Transactions[0].PK).IsDirectCredit)
						||
						(Transactions[0].AH_TransactionType == TransactionTypes.Receipt && Factory.Load<APReceipt>(Transactions[0].PK).IsDirectCreditAndNotOpeningReceipt)
						)
					{
						result = Transactions[0].AH_IsCancelled;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo IsCancelledCalcInfo
		{
			get { return GetZPropertyInfo(nameof(IsCancelledCalc)); }
		}

		public ZBool IsSelected
		{
			get
			{
				ZBool result = false;

				if (Transactions != null)
				{
					foreach (DepositBatchTransactionLine line in Transactions)
					{
						if (line.IsSelected)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
			set
			{
				foreach (DepositBatchTransactionLine line in Transactions)
				{
					line.IsSelected = value;
				}
				IsSelectedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSelectedInfo
		{
			get { return GetZPropertyInfo(nameof(IsSelected)); }
		}

		protected bool IsSelected_ReadOnly
		{
			get { return IsInDatabase; }
		}

		public ZString BankCode
		{
			get { return BankAccount != null ? BankAccount.AB_Code : ZString.Empty; }
		}

		public ZPropertyInfo BankCodeInfo
		{
			get { return GetZPropertyInfo(nameof(BankCode)); }
		}

		public ZString BankAddress
		{
			get { return BankAccount != null ? BankAccount.AB_BankAddress : ZString.Empty; }
		}

		public ZPropertyInfo BankAddressInfo
		{
			get { return GetZPropertyInfo(nameof(BankAddress)); }
		}

		public ZString BankBSBNumber
		{
			get { return BankAccount != null ? BankAccount.AB_BSB : ZString.Empty; }
		}

		public ZPropertyInfo BankBSBNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BankBSBNumber)); }
		}

		public ZString BankAccountNumber
		{
			get { return BankAccount != null ? BankAccount.AB_AccountNum : ZString.Empty; }
		}

		public ZPropertyInfo BankAccountNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BankAccountNumber)); }
		}

		public ZString BankName
		{
			get { return BankAccount != null ? BankAccount.AB_Desc : ZString.Empty; }
		}

		public ZPropertyInfo BankNameInfo
		{
			get { return GetZPropertyInfo(nameof(BankName)); }
		}

		public ZString CurrencyCode
		{
			get { return BankAccount != null ? BankAccount.AB_RX_NKAccountCurrency : ZString.Empty; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		#region Total

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal TotalDepositInvoiceAmount
		{
			get
			{
				ZDecimal result = 0;

				foreach (DepositBatchTransactionLine line in Transactions)
				{
					if (line.IsSelected)
					{
						result += CorrectSigns(line.AH_TransactionType, line.AH_InvoiceAmount);
					}
				}
				return result;
			}
		}

		public ZPropertyInfo TotalDepositInvoiceAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDepositInvoiceAmount)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal TotalDepositGSTAmount
		{
			get
			{
				return (from DepositBatchTransactionLine line in Transactions where line.IsSelected select line).Sum(line => CorrectSigns(line.AH_TransactionType, line.AH_GSTAmount));
			}
		}

		public ZPropertyInfo TotalDepositGSTAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDepositGSTAmount)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal TotalDepositOSAmount
		{
			get { return CashSelectedAmount + ChequeSelectedAmount + CreditCardSelectedAmount + DirectCreditSelectedAmount; }
		}

		public ZPropertyInfo TotalDepositOSAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDepositOSAmount)); }
		}

		public ZInt TotalDepositCount
		{
			get { return CashTransactionSelectedCount + ChequeTransactionSelectedCount + CreditCardTransactionSelectedCount + DirectCreditTransactionSelectedCount; }
		}

		public ZPropertyInfo TotalDepositCountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDepositCount)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal TotalDepositNotSelectedAmount
		{
			get { return CashNotSelectedAmount + ChequeNotSelectedAmount + CreditCardNotSelectedAmount + DirectCreditNotSelectedAmount; }
		}

		public ZPropertyInfo TotalDepositNotSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDepositNotSelectedAmount)); }
		}

		public ZInt TotalDepositNotSelectedCount
		{
			get { return CashTransactionNotSelectedCount + ChequeTransactionNotSelectedCount + CreditCardTransactionNotSelectedCount + DirectCreditTransactionNotSelectedCount; }
		}

		public ZPropertyInfo TotalDepositNotSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDepositNotSelectedCount)); }
		}

		#endregion

		#region Cash

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal CashSelectedAmount
		{
			get { return GetSelectedAmountByType(true, ReceiptTypes.Cash); }
		}

		public ZPropertyInfo CashSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(CashSelectedAmount)); }
		}

		public ZInt CashTransactionSelectedCount
		{
			get { return GetSelectedTransactionCountByType(true, ReceiptTypes.Cash); }
		}

		public ZPropertyInfo CashTransactionSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(CashTransactionSelectedCount)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal CashNotSelectedAmount
		{
			get { return GetSelectedAmountByType(false, ReceiptTypes.Cash); }
		}

		public ZPropertyInfo CashNotSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(CashNotSelectedAmount)); }
		}

		public ZInt CashTransactionNotSelectedCount
		{
			get { return GetSelectedTransactionCountByType(false, ReceiptTypes.Cash); }
		}

		public ZPropertyInfo CashTransactionNotSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(CashTransactionNotSelectedCount)); }
		}

		#endregion

		#region Cheque

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal ChequeSelectedAmount
		{
			get { return GetSelectedAmountByType(true, ReceiptTypes.Cheque); }
		}

		public ZPropertyInfo ChequeSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeSelectedAmount)); }
		}

		public ZInt ChequeTransactionSelectedCount
		{
			get { return GetSelectedTransactionCountByType(true, ReceiptTypes.Cheque); }
		}

		public ZPropertyInfo ChequeTransactionSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeTransactionSelectedCount)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal ChequeNotSelectedAmount
		{
			get { return GetSelectedAmountByType(false, ReceiptTypes.Cheque); }
		}

		public ZPropertyInfo ChequeNotSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeNotSelectedAmount)); }
		}

		public ZInt ChequeTransactionNotSelectedCount
		{
			get { return GetSelectedTransactionCountByType(false, ReceiptTypes.Cheque); }
		}

		public ZPropertyInfo ChequeTransactionNotSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeTransactionNotSelectedCount)); }
		}

		#endregion Cheque

		#region CreditCard

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal CreditCardSelectedAmount
		{
			get { return GetSelectedAmountByType(true, ReceiptTypes.CreditCard); }
		}

		public ZPropertyInfo CreditCardSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(CreditCardSelectedAmount)); }
		}

		public ZInt CreditCardTransactionSelectedCount
		{
			get { return GetSelectedTransactionCountByType(true, ReceiptTypes.CreditCard); }
		}

		public ZPropertyInfo CreditCardTransactionSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(CreditCardTransactionSelectedCount)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal CreditCardNotSelectedAmount
		{
			get { return GetSelectedAmountByType(false, ReceiptTypes.CreditCard); }
		}

		public ZPropertyInfo CreditCardNotSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(CreditCardNotSelectedAmount)); }
		}

		public ZInt CreditCardTransactionNotSelectedCount
		{
			get { return GetSelectedTransactionCountByType(false, ReceiptTypes.CreditCard); }
		}

		public ZPropertyInfo CreditCardTransactionNotSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(CreditCardTransactionNotSelectedCount)); }
		}

		#endregion

		#region DirectCredit

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal DirectCreditSelectedAmount
		{
			get
			{
				return GetSelectedAmountByType(true, ReceiptTypes.DirectCredit, ReceiptTypes.eNettDirectCredit);
			}
		}

		public ZPropertyInfo DirectCreditSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(DirectCreditSelectedAmount)); }
		}

		public ZInt DirectCreditTransactionSelectedCount
		{
			get
			{
				return GetSelectedTransactionCountByType(true, ReceiptTypes.DirectCredit, ReceiptTypes.eNettDirectCredit);
			}
		}

		public ZPropertyInfo DirectCreditTransactionSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(DirectCreditTransactionSelectedCount)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal DirectCreditNotSelectedAmount
		{
			get
			{
				return GetSelectedAmountByType(false, ReceiptTypes.DirectCredit, ReceiptTypes.eNettDirectCredit);
			}
		}

		public ZPropertyInfo DirectCreditNotSelectedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(DirectCreditNotSelectedAmount)); }
		}

		public ZInt DirectCreditTransactionNotSelectedCount
		{
			get
			{
				return GetSelectedTransactionCountByType(false, ReceiptTypes.DirectCredit, ReceiptTypes.eNettDirectCredit);
			}
		}

		public ZPropertyInfo DirectCreditTransactionNotSelectedCountInfo
		{
			get { return GetZPropertyInfo(nameof(DirectCreditTransactionNotSelectedCount)); }
		}

		#endregion

		ZDecimal GetSelectedAmountByType(bool selectedTransactions, params string[] transactionTypes)
		{
			ZDecimal result = 0;

			foreach (DepositBatchTransactionLine line in Transactions)
			{
				if (ShouldConsiderReceiptType(line.AH_ReceiptType, transactionTypes) && line.IsSelected == selectedTransactions)
				{
					var totalDepositAmount = line.IsBankCurrencyLocal ? line.AH_LocalTotal : line.AH_OSTotal;
					result += CorrectSigns(line.AH_TransactionType, totalDepositAmount);
				}
			}
			return result;
		}

		ZInt GetSelectedTransactionCountByType(bool selectedTransactions, params string[] transactionTypes)
		{
			int result = 0;

			foreach (DepositBatchTransactionLine line in Transactions)
			{
				if (ShouldConsiderReceiptType(line.AH_ReceiptType, transactionTypes) && line.IsSelected == selectedTransactions)
				{
					result++;
				}
			}

			return result;
		}

		bool ShouldConsiderReceiptType(string receiptType, params string[] transactionTypes)
		{
			List<string> transactionTypesList = new List<string>(transactionTypes);
			bool result = transactionTypesList.Contains(receiptType);

			if (!result && (transactionTypesList.Contains(ReceiptTypes.DirectCredit) || transactionTypesList.Contains(ReceiptTypes.eNettDirectCredit)))
			{
				result = BankChargeTypes_List.ContainsCode(receiptType);
			}

			return result;
		}

		#region BankChargeTypes_List

		CodeDescriptionPairList fBankChargeTypes_List;
		CodeDescriptionPairList BankChargeTypes_List
		{
			get
			{
				if (fBankChargeTypes_List == null)
				{
					fBankChargeTypes_List = new CodeDescriptionPairList(OLookUpEditType.BankChargeTypes);
				}
				return fBankChargeTypes_List;
			}
		}

		#endregion

		decimal CorrectSigns(string transactionType, decimal oSTotal)
		{
			return oSTotal * (transactionType == TransactionTypes.Receipt ? -1 : 1);
		}

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return new DepositBatchDocumentSupporter(this); }
		}

		#endregion
	}

	#region Document Supporter

	public class DepositBatchDocumentSupporter : TransactionHeader.TransactionHeaderDocumentSupporter
	{
		public DepositBatchDocumentSupporter(DepositBatch depositBatch)
			: base(depositBatch)
		{
		}

		protected DepositBatch DepositBatch
		{
			get { return (DepositBatch)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.DepositBatch, Core.Constants.DataContext.GenericFreightJob };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DepositBatch; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, DepositBatch);
			}
			else if (dataContext == Core.Constants.DataContext.DepositBatch)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.DepositBatch, DepositBatch) };
			}
			return null;
		}

		#endregion
	}

	#endregion
}
