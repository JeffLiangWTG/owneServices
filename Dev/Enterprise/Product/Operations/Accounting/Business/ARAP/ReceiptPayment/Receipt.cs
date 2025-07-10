using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	[UniversalDataContext(DataContextType.AccountingReceipt)]
	public abstract partial class Receipt : ReceiptPaymentBase, IDepositBatch
	{
		public Receipt(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Saving
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (!HasErrors)
			{
				string devErrorMessage = CreateDeveloperExceptionIfFractionAmountIsLongerThanAllowed();
				if (!string.IsNullOrWhiteSpace(devErrorMessage))
				{
					devErrorMessage = string.Format((NoResString)"Following field(s) have decimal places longer than allowed. [Transaction Number: {0}]\n{1}", !IsInDatabase ? (ZString)"<NEW>" : AH_TransactionNum, devErrorMessage);
					ExceptionReporter.Instance.ReportDeveloperException((NoResString)"Receipt>IncorrectDecimalPlaces", devErrorMessage, new Exception(devErrorMessage));
				}
			}
		}

		public ZBool SkipCreateDepositBatch
		{
			get;
			set;
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (!IsInDatabase && !IsReverseTransaction) // only create a new deposit batch for new, non-reverse receipts
			{
				if (AH_TransactionType != TransactionTypes.OpeningPayment)
				{
					DepositBatch existingComPayDepositBatch = null;

					if (AH_ReceiptBatchNo.IsEmpty && AH_ReceiptType == ReceiptTypes.eNettDirectCredit &&
						CompayReceiptBatchDate.IsValid &&
						CompayReceiptBatchDate.Date >= ZDateTime.Today)
					{
						existingComPayDepositBatch = FindUnclearedCompayDepositBatch(CompayReceiptBatchDate);

						if (existingComPayDepositBatch != null)
						{
							existingComPayDepositBatch.AH_OSTotal -= AH_OSTotal;
							existingComPayDepositBatch.AH_InvoiceAmount -= AH_InvoiceAmount;
							existingComPayDepositBatch.SetOriginalReceipt(this);
							SetReceiptBatchNo(existingComPayDepositBatch);
						}
					}

					if (AH_ReceiptBatchNo.IsEmpty && IsDirectCreditAndNotOpeningReceipt && !SkipCreateDepositBatch)
					{
						DepositBatchCreator depBatCreator = new DepositBatchCreator(this);
						depBatCreator.CreateDepositBatch();
					}
				}

				if (Header != null && Header.OH_IsDebtor && AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					Header.CompanyData.OB_ARPreviousChequeDrawer = AH_ChequeDrawer;
					Header.CompanyData.OB_ARPreviousChequeDrawerBank = AH_DrawerBank;
					Header.CompanyData.OB_ARPreviousChequeDrawerBankBranch = AH_DrawerBranch;
				}
			}
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "AH_TransactionNum is currently 8 characters for batching")]
		void SetReceiptBatchNo(DepositBatch comPayDepositBatch)
		{
			AH_ReceiptBatchNo = comPayDepositBatch.AH_TransactionNum;
		}

		DepositBatch FindUnclearedCompayDepositBatch(ZDateTime batchDate)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DepositBatch));
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ReceiptBatch);
			query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.EqualToDatePartOnly, batchDate.Date);
			query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.EqualToDatePartOnly, batchDate.Date);
			query.AddToFilter(AccTransactionHeaderSchema.AH_AB, AH_AB);
			query.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, AH_RX_NKTransactionCurrency);
			query.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, ZBool.False);
			query.AddToFilter(AccTransactionHeaderSchema.AH_DateClearedInCashbook, null);
			query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceAmount, SQLComparisonOperator.GreaterThan, 0);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC);
			query.OrderBy = AccTransactionHeaderSchema.Constants.AH_TransactionNum + " DESC ";

			DepositBatch[] batches = Factory.Load<DepositBatch>(query);

			foreach (DepositBatch batch in batches)
			{
				foreach (DepositBatchTransactionLine receipt in batch.Transactions)
				{
					if (receipt.AH_ReceiptType == ReceiptTypes.eNettDirectCredit)
					{
						return batch;
					}
				}
			}

			return null;
		}

		public ZDateTime CompayReceiptBatchDate
		{
			get { return fCompayReceiptBatchDate; }
			set { fCompayReceiptBatchDate = value; }
		}
		ZDateTime fCompayReceiptBatchDate;

		#endregion

		string CreateDeveloperExceptionIfFractionAmountIsLongerThanAllowed()
		{
			ZStringBuilder msg = new ZStringBuilder();

			try
			{
				if (AH_InvoiceAmount.DecimalPlaces > LocalCurrencyDecimals)
				{
					msg.Append(CreateDeveloperErrorMessage(AH_InvoiceAmountInfo, true));
				}
				if (AH_OutstandingAmount.DecimalPlaces > LocalCurrencyDecimals)
				{
					msg.Append(CreateDeveloperErrorMessage(AH_OutstandingAmountInfo, true));
				}
				if (AH_OSTotal.DecimalPlaces > TransactionCurrency.Decimals)
				{
					msg.Append(CreateDeveloperErrorMessage(AH_OSTotalInfo, false));
				}
				if (AH_OSExTaxAmount.DecimalPlaces > TransactionCurrency.Decimals)
				{
					msg.Append(CreateDeveloperErrorMessage(AH_OSExTaxAmountInfo, false));
				}
				if (AH_LocalExTaxAmount.DecimalPlaces > LocalCurrencyDecimals)
				{
					msg.Append(CreateDeveloperErrorMessage(AH_LocalExTaxAmountInfo, true));
				}
			}
			catch (NullReferenceException ex)
			{
				ZStringBuilder errorMsg = new ZStringBuilder();
				errorMsg.AppendLine();
				errorMsg.Append((NoResString)"Message: " + ex.Message);
				errorMsg.Append(string.Format("TransactionCurrency: {0}", TransactionCurrency != null ? TransactionCurrency.ToString() : "NULL"));
				errorMsg.Append(string.Format("AH_InvoiceAmount: {0}", !AH_InvoiceAmount.IsEmpty ? AH_InvoiceAmount.ToString() : "NULL"));
				errorMsg.Append(string.Format("AH_OutstandingAmount: {0}", !AH_OutstandingAmount.IsEmpty ? AH_OutstandingAmount.ToString() : "NULL"));
				errorMsg.Append(string.Format("AH_OSTotal: {0}", !AH_OSTotal.IsEmpty ? AH_OSTotal.ToString() : "NULL"));
				errorMsg.Append(string.Format("AH_OSExTaxAmount: {0}", !AH_OSExTaxAmount.IsEmpty ? AH_OSExTaxAmount.ToString() : "NULL"));
				errorMsg.Append(string.Format("AH_LocalExTaxAmount: {0}", !AH_LocalExTaxAmount.IsEmpty ? AH_LocalExTaxAmount.ToString() : "NULL"));
				errorMsg.Append(string.Format((NoResString)"Company: {0}", Company != null ? Company.ToString() : "NULL"));
				errorMsg.Append(string.Format((NoResString)"Company.LocalCurrency: {0}", Company != null && Company.LocalCurrency != null ? Company.LocalCurrency.ToString() : "NULL"));
				errorMsg.AppendLine();
				ErrorReporter.ReportOnce("", errorMsg.ToStringWithNewLineBetweenAppends(), ex);
			}

			return msg.ToStringWithNewLineBetweenAppends();
		}

		string CreateDeveloperErrorMessage(ZPropertyInfo propInfo, bool isLocalCurrency)
		{
			var allowedDecimalLength = isLocalCurrency ? Company.LocalCurrency.Decimals : TransactionCurrency.Decimals;
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"Field Name: {0}. Value: {1}, Decimal Length: {2}, Transaction Currency: {3}, Local Currency:{4}, Allowed Decimal Place: {5}", propInfo.Name, propInfo.Value, ((ZDecimal)propInfo.Value).DecimalPlaces, TransactionCurrency.Code, Company.LocalCurrency.Code, allowedDecimalLength);
		}

		#region Reversing

		public DepositBatch RelatedDepositBatch
		{
			get
			{
				DepositBatch relatedDepositBatch = null;

				if (!AH_ReceiptBatchNo.IsEmpty)
				{
					ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, AH_ReceiptBatchNo);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

					relatedDepositBatch = Factory.LoadTop1<DepositBatch>(filter);
				}
				return relatedDepositBatch;
			}
		}

		public ZBool IsDirectCreditAndNotOpeningReceipt
		{
			get
			{
				return (AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.DirectCredit ||
							AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.eNettDirectCredit ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.BankDepositFee ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.BankDebitTax ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.InterestPaid ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.InterestReceived ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.PeriodicPayment ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.StampDuty ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.MiscellaneousReceipt ||
									 AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.MiscellaneousFees) &&
								AH_TransactionType != ZArchitecture.Core.TransactionTypes.OpeningReceipt;
			}
		}

		#endregion

		#region Properties

		#region Overriden Readonly Properties

		protected override ZGuid AH_OHCore
		{
			get
			{
				return base.AH_OHCore;
			}
			set
			{
				base.AH_OHCore = value;
				if (AH_ReceiptType == ReceiptTypes.Cheque && Header != null)
				{
					if (Header.CompanyData != null)
					{
						AH_ChequeDrawer = Header.CompanyData.OB_ARPreviousChequeDrawer;
						AH_DrawerBank = Header.CompanyData.OB_ARPreviousChequeDrawerBank;
						AH_DrawerBranch = Header.CompanyData.OB_ARPreviousChequeDrawerBankBranch;
					}
				}
			}
		}

		protected override bool AH_OH_ReadOnly
		{
			get { return IsPostWithMatching; }
		}

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get { return base.AH_RX_NKTransactionCurrency_ReadOnly || IsCashAccountType; }
			set { base.AH_RX_NKTransactionCurrency_ReadOnly = value; }
		}

		#endregion

		public bool IsPostWithMatching { get; set; }
		public bool UseReceiptValidation { get; set; }

		protected bool IsReceiptTypeCheque
		{
			get { return AH_ReceiptType != ZArchitecture.Core.ReceiptTypes.Cheque; }
		}

		protected bool AH_ChequeDrawer_ReadOnly
		{
			get { return IsReceiptTypeCheque; }
		}

		protected bool AH_DrawerBank_ReadOnly
		{
			get { return IsReceiptTypeCheque; }
		}

		protected bool AH_DrawerBranch_ReadOnly
		{
			get { return IsReceiptTypeCheque; }
		}

		#endregion

		public override ZString DepositBatchNumber
		{
			get { return AH_ReceiptBatchNo; }
		}

		[List("ReceiptMethods")]
		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set { base.AH_ReceiptType = value; }
		}

		[List("BankAccounts")]
		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				base.AH_AB = value;
				if (IsCashAccountType)
				{
					AH_ReceiptType = ReceiptTypes.Cash;
				}
			}
		}

		#region Implementation

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.Receipt; }
		}

		protected override ZString TransactionType
		{
			get { return ZArchitecture.Core.TransactionTypes.Receipt; }
		}

		public new static readonly TransactionHeaderTypeDecider TypeDecider = new TransactionHeaderTypeDecider();

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new ReceiptValidation(this);
		}

		protected override AccTransactionHeaderValidation GetNewMatchingValidation()
		{
			if (UseReceiptValidation)
			{
				return GetNewValidationCore();
			}
			else
			{
				return base.GetNewMatchingValidation();
			}
		}

		protected override void PrepareReceiptPaymentForMatching()
		{
			IsMatching = false;
			((IMatching)this).OSPartialPaymentAmountInfo.RefreshBinding();
		}

		void SaveChequeDetailsAsDefault()
		{
			if (!AH_ChequeDrawer.IsEmpty)
			{
				Default_AH_ChequeDrawer = AH_ChequeDrawer;
			}
			if (!AH_DrawerBank.IsEmpty)
			{
				Default_AH_DrawerBank = AH_DrawerBank;
			}
			if (!AH_DrawerBranch.IsEmpty)
			{
				Default_AH_DrawerBranch = AH_DrawerBranch;
			}
		}
		ZString Default_AH_ChequeDrawer;
		ZString Default_AH_DrawerBank;
		ZString Default_AH_DrawerBranch;

		#region SetReceiptType
		protected override void SetReceiptType()
		{
			base.SetReceiptType();
			switch (AH_ReceiptType)
			{
				case ZArchitecture.Core.ReceiptTypes.DirectCredit:
				case ZArchitecture.Core.ReceiptTypes.Cash:
				case ZArchitecture.Core.ReceiptTypes.CreditCard:
					SaveChequeDetailsAsDefault();
					AH_ChequeDrawer = ZString.Empty;
					AH_DrawerBank = ZString.Empty;
					AH_DrawerBranch = ZString.Empty;
					break;

				case ZArchitecture.Core.ReceiptTypes.Cheque:
					AH_ChequeDrawer = Default_AH_ChequeDrawer;
					AH_DrawerBank = Default_AH_DrawerBank;
					AH_DrawerBranch = Default_AH_DrawerBranch;
					break;
			}
			AH_ChequeDrawerInfo.RefreshBinding();
			AH_DrawerBankInfo.RefreshBinding();
			AH_DrawerBranchInfo.RefreshBinding();
		}

		protected override void SetReferenceNumberCore()
		{
			base.SetReferenceNumberCore();
			if (AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.DirectCredit)
			{
				AH_ChequeOrReference = ZString.Empty;
			}
		}

		#endregion

		#endregion
	}
}
