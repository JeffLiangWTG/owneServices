using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	public class DirectTransactionsBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DirectTransactionsBusinessObject(BusinessObjectFactory factory, ZGuid reconciliationAccountPK, ZDateTime statementDate)
			: base(factory)
		{
			ReconciliationBankAccountPK = reconciliationAccountPK;
			this.StatementDate = statementDate;
		}

		public readonly ZGuid ReconciliationBankAccountPK;
		public readonly ZDateTime StatementDate;

		#region Collections

		public DirectTransactionHeaderBaseCollection Headers
		{
			get
			{
				if (fHeaders == null)
				{
					fHeaders = new DirectTransactionHeaderBaseCollection(Factory, StatementDate);
					RegisterEditableChildObject(fHeaders);
				}
				return fHeaders;
			}
		}
		DirectTransactionHeaderBaseCollection fHeaders;

		public BankReconTransCollection BankReconTransactions
		{
			get { return BankReconTransactions_innerValue ?? (BankReconTransactions_innerValue = new BankReconTransCollection(Factory, ZGuid.Empty)); }
		}
		BankReconTransCollection BankReconTransactions_innerValue;

		public BankReconDirectPaymentCollection DirectPayments
		{
			get
			{
				if (fDirectPayments == null)
				{
					fDirectPayments = new BankReconDirectPaymentCollection(Factory);
					fDirectPayments.StatementDate = StatementDate;
					RegisterEditableChildObject(fDirectPayments);
				}
				return fDirectPayments;
			}
		}
		BankReconDirectPaymentCollection fDirectPayments;

		public BankReconDirectReceiptCollection DirectReceipts
		{
			get
			{
				if (fDirectReceipts == null)
				{
					fDirectReceipts = new BankReconDirectReceiptCollection(Factory);
					fDirectReceipts.StatementDate = StatementDate;
					RegisterEditableChildObject(fDirectReceipts);
				}
				return fDirectReceipts;
			}
		}
		BankReconDirectReceiptCollection fDirectReceipts;

		#endregion

		#region Direct Receipt to Batch PK Mapping

		readonly Dictionary<ZGuid, ZGuid> directReceiptToBatchPKMapping = new Dictionary<ZGuid, ZGuid>();

		public void AddToBatchToDirectReceiptPKMapping(ZGuid batchPK, ZGuid receiptPK)
		{
			if (directReceiptToBatchPKMapping.ContainsKey(batchPK))
			{
				if (directReceiptToBatchPKMapping[batchPK] != receiptPK)
				{
					throw new ApplicationException("Trying to add a different direct receipt to same batch. Expecting one to one mapping here");
				}
			}
			else
			{
				directReceiptToBatchPKMapping.Add(batchPK, receiptPK);
			}
		}

		public ZGuid GetDirectReceiptForBatch(ZGuid batchPK)
		{
			ZGuid value;
			return directReceiptToBatchPKMapping.TryGetValue(batchPK, out value) ? value : ZGuid.Empty;
		}

		public IEnumerable<ZGuid> DirectReceiptBatchPKs
		{
			get
			{
				foreach (var pk in directReceiptToBatchPKMapping.Keys)
				{
					yield return pk;
				}
			}
		}

		#endregion

		public void MoveDirectReceiptsAndPaymentsToBaseCollection()
		{
			Headers.AddRange(DirectReceipts);
			Headers.AddRange(DirectPayments);
		}
	}
}