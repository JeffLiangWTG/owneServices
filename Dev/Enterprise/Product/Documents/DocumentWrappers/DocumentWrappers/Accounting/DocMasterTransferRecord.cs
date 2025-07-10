using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	/// <summary>
	/// Used for CashBook TRF Remittance Advice and Payment Voucher to calculate the To and From Party.
	/// </summary>
	public class DocMasterTransferRecord : DocBaseWrapper, IGenericTransactionHeaderPlugIn
	{
		DocMasterTransferRecord(BankTransferRow bankTransferRow, BusinessObjectFactory factoryToWrap)
			: base(bankTransferRow, factoryToWrap)
		{
			this.BankTransferRow = bankTransferRow;
			SetFromAndToParty();
		}

		public override string ToString()
		{
			return BankTransferRow.AH_TransactionNum;
		}

		public static DocMasterTransferRecord New(BankTransferRow bankTransferRow, BusinessObjectFactory factoryToWrap)
		{
			if (bankTransferRow == null)
			{
				return null;
			}
			else
			{
				return new DocMasterTransferRecord(bankTransferRow, factoryToWrap);
			}
		}

		#region IGenericTransactionPlugIn members

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocMasterTransferRecordGenericTransactionSupporter(this)); }
		}
		DocMasterTransferRecordGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		public class DocMasterTransferRecordGenericTransactionSupporter : GenericTransactionHeaderSupporter
		{
			public DocMasterTransferRecordGenericTransactionSupporter(DocMasterTransferRecord parent)
			{
				this.Parent = parent;
			}
			protected readonly DocMasterTransferRecord Parent;

			protected internal override ZString GetTransactionType()
			{
				return Parent.TransferFrom.TransactionType;
			}

			protected internal override ZDecimal GetSummaryTotalForRemittanceAdvice()
			{
				return Parent.TransferFrom.InvoiceAmount;
			}

			protected internal override ZDateTime GetCreatedDate()
			{
				return Parent.TransferFrom.CreatedDate;
			}

			protected internal override ZDateTime GetPostDate()
			{
				return Parent.TransferFrom.PostDate;
			}

			protected internal override ZString GetCreatingUser()
			{
				return Parent.TransferFrom.CreatingUser;
			}

			protected internal override ZString GetReceiptTypeDescription()
			{
				return Parent.TransferFrom.ReceiptTypeDescription;
			}

			protected internal override ZString GetReceiptType()
			{
				return Parent.TransferFrom.ReceiptType;
			}

			protected internal override ZString GetChequeOrReference()
			{
				return Parent.TransferFrom.ChequeOrReference;
			}

			protected internal override ZDecimal GetInvoiceAmount()
			{
				return Parent.TransferFrom.InvoiceAmount;
			}

			protected internal override ZString GetTransactionNumber()
			{
				return Parent.TransferFrom.TransactionNumber;
			}

			protected internal override ZString GetDesc()
			{
				return Parent.TransferFrom.Desc;
			}

			protected internal override ZString GetBankAccountCode()
			{
				return Parent.TransferFrom.BankAccount == null ? ZString.Empty : Parent.TransferFrom.BankAccount.Code;
			}

			protected internal override ZDecimal GetOSTotal()
			{
				return Parent.TransferFrom.OSTotal;
			}

			protected internal override ZString GetCurrencyCode()
			{
				return Parent.TransferFrom.Currency.Code;
			}

			protected internal override ZString GetCurrentCompanyCurrencyCode()
			{
				return Parent.CurrentCompany == null || Parent.CurrentCompany.Currency == null ? ZString.Empty : Parent.CurrentCompany.Currency.Code;
			}

			protected internal override ZDecimal GetExchangeRate()
			{
				return Parent.TransferFrom.ExchangeRate;
			}

			protected internal override ZString GetSecondaryTransactionType()
			{
				return Parent.TransferTo == null ? ZString.Empty : Parent.TransferTo.TransactionType;
			}

			protected internal override ZString GetSecondaryTransactionNumber()
			{
				return Parent.TransferTo == null ? ZString.Empty : Parent.TransferTo.TransactionNumber;
			}

			protected internal override ZString GetSecondaryBankAccountCode()
			{
				return Parent.TransferTo == null || Parent.TransferTo.BankAccount == null ? ZString.Empty : Parent.TransferTo.BankAccount.Code;
			}

			protected internal override ZDecimal GetSecondaryInvoiceAmount()
			{
				return Parent.TransferTo == null ? ZDecimal.Zero : Parent.TransferTo.InvoiceAmount;
			}

			protected internal override ZDecimal GetSecondaryOSTotal()
			{
				return Parent.TransferTo == null ? ZDecimal.Zero : Parent.TransferTo.OSTotal;
			}

			protected internal override ZString GetSecondaryCurrencyCode()
			{
				return Parent.TransferTo == null ? ZString.Empty : Parent.TransferTo.Currency.Code;
			}

			protected internal override ZDecimal GetSecondaryExchangeRate()
			{
				return Parent.TransferTo == null ? ZDecimal.Zero : Parent.TransferTo.ExchangeRate;
			}

			protected internal override ZString GetChargesTransactionType()
			{
				return Parent.Charges == null ? ZString.Empty : Parent.Charges.TransactionType;
			}

			protected internal override ZString GetChargesTransactionNumber()
			{
				return Parent.Charges == null ? ZString.Empty : Parent.Charges.TransactionNumber;
			}

			protected internal override ZString GetChargesBankAccountCode()
			{
				return Parent.Charges == null || Parent.Charges.BankAccount == null ? ZString.Empty : Parent.Charges.BankAccount.Code;
			}

			protected internal override ZDecimal GetChargesInvoiceAmountAndTax()
			{
				return Parent.Charges == null ? ZDecimal.Zero : Parent.Charges.InvoiceAmountAndTax;
			}

			protected internal override ZString GetChargesDesc()
			{
				return Parent.Charges == null ? ZString.Empty : Parent.Charges.Desc;
			}

			protected internal override ZDecimal GetChargesOSTotal()
			{
				return Parent.Charges == null ? ZDecimal.Zero : Parent.Charges.OSTotal;
			}

			protected internal override ZString GetChargesCurrencyCode()
			{
				return Parent.Charges == null ? ZString.Empty : Parent.Charges.Currency.Code;
			}

			protected internal override ZDecimal GetChargesExchangeRate()
			{
				return Parent.Charges == null ? ZDecimal.Zero : Parent.Charges.ExchangeRate;
			}

			protected internal override ZString GetBarcode()
			{
				return Parent.Barcode;
			}

			protected internal override ZDecimal GetTotalPaidAsForPaymentVoucher()
			{
				return Parent.TransferFrom == null ? ZDecimal.Zero : (ZDecimal)Math.Abs(Parent.TransferFrom.OSTotal);
			}

			protected internal override ZDecimal GetTotalPaymentForPaymentVoucher()
			{
				return Parent.TransferFrom == null ? ZDecimal.Zero : (ZDecimal)Math.Abs(Parent.TransferFrom.InvoiceAmount);
			}

			protected internal override ZDecimal GetExchangeRateForPaymentVoucher()
			{
				return Parent.TransferFrom == null ? ZDecimal.Zero : Parent.TransferFrom.ExchangeRate;
			}

			protected internal override ZDecimal GetPaymentVoucherOSTotal()
			{
				return Parent.TransferFrom == null ? ZDecimal.Zero : (ZDecimal)Math.Abs(Parent.TransferFrom.OSTotal);
			}
		}

		protected DocCashBookTransfer fTransferTo;
		public DocCashBookTransfer TransferTo
		{
			get { return fTransferTo; }
		}

		protected DocCashBookTransfer fTransferFrom;
		public DocCashBookTransfer TransferFrom
		{
			get { return fTransferFrom; }
		}

		public DocDirectPayment Charges
		{
			get
			{
				ZQuery filter = GetRelatedCashBookTransferFilter(ZArchitecture.Core.TransactionTypes.DirectPayment);
				if (BankTransferRow.AH_TransactionCount > 3)
				{
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, SQLComparisonOperator.GreaterThan, (ZByte)3);
				}
				else
				{
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, SQLComparisonOperator.LessThanOrEqualTo, (ZByte)3);
				}

				return DocDirectPayment.New(Factory.LoadTop1<DirectPayment>(filter), Factory);
			}
		}

		public ZString TransactionType
		{
			get { return BankTransferRow.AH_TransactionType; }
		}

		public ZBool ShowOSTotal
		{
			get
			{
				if (TransferTo != null && TransferFrom != null)
				{
					if (TransferTo.Currency != null && TransferFrom.Currency != null)
					{
						return (TransferTo.Currency.Code != TransferFrom.Currency.Code);
					}
				}
				return ZBool.False;
			}
		}

		public ZString Barcode
		{
			get { return ""; }
		}

		#region Implementation
		readonly BankTransferRow BankTransferRow;
		protected void SetFromAndToParty()
		{
			if (BankTransferRow.GetType() == typeof(BankTransferFromRow))
			{
				fTransferFrom = DocCashBookTransfer.New((BankTransferFromRow)BankTransferRow, Factory);

				ZQuery filter = GetRelatedCashBookTransferFilter(ZArchitecture.Core.TransactionTypes.Transfer);
				fTransferTo = DocCashBookTransfer.New(Factory.LoadTop1<BankTransferToRow>(filter), Factory);
			}
			else
			{
				fTransferTo = DocCashBookTransfer.New((BankTransferToRow)BankTransferRow, Factory);

				ZQuery filter = GetRelatedCashBookTransferFilter(ZArchitecture.Core.TransactionTypes.Transfer);
				fTransferFrom = DocCashBookTransfer.New(Factory.LoadTop1<BankTransferFromRow>(filter), Factory);
			}
		}

		protected ZQuery GetRelatedCashBookTransferFilter(ZString transactionType)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, BankTransferRow.AH_TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, BankTransferRow.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionType);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.CashBook);
			if (transactionType == ZArchitecture.Core.TransactionTypes.Transfer)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, BankTransferRow.AH_TransactionNum);
			}
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, BankTransferRow.AH_GC);
			return filter;
		}
		#endregion
	}
}
