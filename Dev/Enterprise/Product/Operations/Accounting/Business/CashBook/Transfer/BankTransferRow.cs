using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public enum TransferType { TransferFrom, TransferTo, Undefined }

	public abstract class BankTransferRow : TransactionHeader, IDocumentSupportable, ICashBook
	{
		public BankTransferRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ExchangeRate.Currency_ReadOnly = true;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_ReceiptType = ReceiptTypes.EFT;
		}

		public override bool IsSavedByFactory
		{
			get
			{
				var isParentNotInDB = !BankTransferParent?.IsInDatabase ?? true;
				return isParentNotInDB && base.IsSavedByFactory;
			}
		}

		protected override bool InvertSigns
		{
			get { return TransferType == TransferType.TransferFrom; }
		}

		public TransferType TransferType
		{
			get { return fTransferType; }
			set { fTransferType = value; }
		}
		TransferType fTransferType = TransferType.Undefined;

		public BankTransfer BankTransferParent
		{
			get { return fParent; }
			set { fParent = value; }
		}
		BankTransfer fParent;

		protected override ZString Ledger
		{
			get { return LedgerTypes.CashBook; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.Transfer; }
		}

		public override ZDecimal Debit
		{
			get { return DebitForDirectReceiptPayment; }
		}

		public override ZDecimal Credit
		{
			get { return CreditForDirectReceiptPayment; }
		}

		protected override ZDecimal LocalCreditCore
		{
			get { return AH_InvoiceAmount < 0m ? -AH_InvoiceAmount : 0m; }
		}

		protected override ZDecimal LocalDebitCore
		{
			get { return AH_InvoiceAmount >= 0m ? AH_InvoiceAmount : (ZDecimal)0m; }
		}

		public override TransactionHeaderCollection RelatedTransactions
		{
			get
			{
				if (fRelatedTransactions == null)
				{
					ZQuery transferRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, AH_Ledger);
					transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
					transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, AH_TransactionNum);
					transferRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
					fRelatedTransactions = new TransactionHeaderCollection(Factory, transferRowFilter);
					fRelatedTransactions.Load();
				}

				return fRelatedTransactions;
			}
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return new BankTransferDocumentSupporter(this); }
		}

		protected override List<string> GetWritableProperties()
		{
			return new List<string>();
		}

		public abstract bool IsReversal
		{
			get;
		}

		protected bool AH_ExchangeRate_ReadOnly
		{
			get { return BankTransferParent == null || BankTransferParent.BuySellAmountsAndRatesReadOnly; }
		}

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get { return base.AH_RX_NKTransactionCurrency_ReadOnly || AH_ExchangeRate_ReadOnly; }
			set { base.AH_RX_NKTransactionCurrency_ReadOnly = value; }
		}

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
				BankTransferFromRow bankTransferFromRow = null;
				BankTransferToRow bankTransferToRow = null;
				AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
				if (this is BankTransferToRow)
				{
					bankTransferToRow = (BankTransferToRow)this;
					bankTransferFromRow = Factory.New<BankTransferFromRow>();
					bankTransferFromRow.AH_TransactionBelongsToGroup = AH_TransactionBelongsToGroup;
				}
				else if (this is BankTransferFromRow)
				{
					bankTransferFromRow = (BankTransferFromRow)this;
					bankTransferToRow = Factory.New<BankTransferToRow>();
					bankTransferToRow.AH_TransactionBelongsToGroup = AH_TransactionBelongsToGroup;
				}
				else
				{
					throw new System.ArgumentException(string.Format("object must be BankTransferToRow or BankTransferFromRow, instread of {0}", this.GetType().Name));
				}
			}
		}

#endif
		#endregion
	}

	public class BankTransferDocumentSupporter : TransactionHeader.TransactionHeaderDocumentSupporter
	{
		public BankTransferDocumentSupporter(BankTransferRow bankTransferRow)
			: base(bankTransferRow)
		{
		}

		protected BankTransferRow BankTransferRow
		{
			get { return (BankTransferRow)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.APTransaction; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			List<Core.Constants.DataContext> result = new List<Core.Constants.DataContext>(base.GetSupportedDataContexts());
			result.Add(Core.Constants.DataContext.GenericFreightJob);
			return result.ToArray();
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, BankTransferRow);
			}
			else if (dataContext == Core.Constants.DataContext.AccountingVoucher)
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
			else
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.MasterTransferRecord, BankTransferRow) };
			}
		}
	}
}
