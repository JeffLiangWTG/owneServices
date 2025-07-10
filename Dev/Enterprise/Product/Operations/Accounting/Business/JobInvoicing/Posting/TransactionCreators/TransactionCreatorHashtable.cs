using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class TransactionCreatorHashtable : ICollection, IEnumerable
	{
		public TransactionCreatorHashtable()
		{
			transactions = new Dictionary<string, TransactionHeader>();
			matchLinks = new Dictionary<ZGuid, TransactionMatchLinkCollection>();
			paymentApprovals = new Dictionary<string, PaymentApprovalBase>();
			apInvoiceApprovalRequests = new HashSet<APInvoiceChargesApprovalRequest>();
		}

		readonly Dictionary<string, TransactionHeader> transactions;
		readonly Dictionary<ZGuid, TransactionMatchLinkCollection> matchLinks;
		readonly Dictionary<string, PaymentApprovalBase> paymentApprovals;
		readonly HashSet<APInvoiceChargesApprovalRequest> apInvoiceApprovalRequests;

		public GlbCompany Company => transactions.Values.FirstOrDefault()?.Company ?? GlbCompany.CurrentCompany;

		public void RemoveAndDeleteAll()
		{
			foreach (TransactionHeader transaction in transactions.Values)
			{
				transaction.Delete();
			}
			transactions.Clear();

			foreach (TransactionMatchLinkCollection matchLinkCollection in matchLinks.Values)
			{
				matchLinkCollection.RemoveAndDeleteAll();
			}
			matchLinks.Clear();

			foreach (PaymentApprovalBase paymentApproval in paymentApprovals.Values)
			{
				paymentApproval.Delete();
			}
			paymentApprovals.Clear();

			apInvoiceApprovalRequests.Clear();
		}

		#region AP Transactions Methods

		public void AddAPInvoice(APInvoice invoice, string clientCode, string aPInvoiceNumber)
		{
			var key = GetAPInvoiceKey(clientCode, aPInvoiceNumber);
			if (transactions.ContainsKey(key))
			{
				throw new DuplicatedInvoiceException(clientCode, aPInvoiceNumber);
			}
			transactions.Add(key, invoice);
		}

		public APInvoice RetrieveAPInvoice(OrgHeader client, string aPInvoiceNumber)
		{
			string clientCode = client != null ? client.OH_Code : ZString.Empty;
			return RetrieveAPInvoice(clientCode, aPInvoiceNumber);
		}

		TransactionHeader[] GetTransactions(string ledger)
		{
			return GetTransactions(ledger, Array.Empty<string>());
		}

		TransactionHeader[] GetTransactions(string ledger, string transactionType)
		{
			return GetTransactions(ledger, new[] { transactionType });
		}

		TransactionHeader[] GetTransactions(string ledger, string[] transactionTypes)
		{
			List<TransactionHeader> result = new List<TransactionHeader>();
			var hash = new HashSet<string>(transactionTypes);
			foreach (object entry in transactions.Values)
			{
				TransactionHeader header = entry as TransactionHeader;
				if (header != null
					&& header.AH_Ledger == ledger
					&& (hash.Count == 0 || hash.Contains(header.AH_TransactionType)))
				{
					result.Add(header);
				}
			}
			return result.ToArray();
		}

		public TransactionHeader[] GetAllAPCreditNotes()
		{
			return GetTransactions(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);
		}

		public TransactionHeader[] GetAllARCreditNotes()
		{
			return GetTransactions(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
		}

		public TransactionHeader[] GetAllARInvoices()
		{
			return GetTransactions(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
		}

		public TransactionHeader[] GetAllAPTransactions()
		{
			return GetTransactions(LedgerTypes.AccountsPayable);
		}

		public TransactionHeader[] GetAllUATransactions()
		{
			return GetTransactions(LedgerTypes.UnapprovedPayableTransactions);
		}

		public InvoicingBase[] GetAllAPInvoicesAndCreditNotes()
		{
			List<InvoicingBase> result = new List<InvoicingBase>();
			foreach (object header in transactions.Values)
			{
				if (header is APInvoice || header is APCreditNote)
				{
					result.Add((InvoicingBase)header);
				}
			}
			return result.ToArray();
		}

		public InvoicingBase[] GetAllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotes()
		{
			List<InvoicingBase> result = new List<InvoicingBase>();
			foreach (TransactionHeader header in transactions.Values)
			{
				if (header.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions && (header is APInvoice || header is APCreditNote))
				{
					result.Add((InvoicingBase)header);
				}
			}
			return result.ToArray();
		}

		public APInvoice RetrieveAPInvoice(string clientCode, string aPInvoiceNumber)
		{
			string key = GetAPInvoiceKey(clientCode, aPInvoiceNumber);
			return transactions.ContainsKey(key) ? (APInvoice)transactions[key] : null;
		}

		public void RemoveAPInvoice(string clientCode, string aPInvoiceNumber)
		{
			transactions.Remove(GetAPInvoiceKey(clientCode, aPInvoiceNumber));
		}

		public bool ContainsAPInvoice(string clientCode, string aPInvoiceNumber)
		{
			return transactions.ContainsKey(GetAPInvoiceKey(clientCode, aPInvoiceNumber));
		}

		protected string GetAPInvoiceKey(string clientCode, string aPInvoiceNumber)
		{
			return TransactionTypes.Invoice + ":" + clientCode + ":" + aPInvoiceNumber;
		}

		#endregion

		#region APInvoiceApprovalRequests methods

		public void AddAPInvoiceApprovalRequest(APInvoiceChargesApprovalRequest request)
		{
			if (request != null)
			{
				apInvoiceApprovalRequests.Add(request);
			}
		}

		public APInvoiceChargesApprovalRequest[] GetAllAPInvoiceApprovalRequests()
		{
			return apInvoiceApprovalRequests.ToArray();
		}

		#endregion

		#region AR Transactions Methods

		public TransactionHeader[] GetAllARTransactions()
		{
			return GetTransactions(LedgerTypes.AccountsReceivable);
		}

		public InvoicingBase[] GetAllARInvoicesAndCreditNotes()
		{
			List<InvoicingBase> result = new List<InvoicingBase>();
			foreach (object header in transactions.Values)
			{
				if (header is ARInvoice || header is ARCreditNote)
				{
					result.Add((InvoicingBase)header);
				}
			}
			return result.ToArray();
		}

		#endregion

		#region AP Credit Note Methods

		public void AddAPCreditNote(APCreditNote creditNote, string clientCode, string aPCreditNoteNumber)
		{
			transactions.Add(GetAPCreditNoteKey(clientCode, aPCreditNoteNumber), creditNote);
		}

		public APCreditNote RetrieveAPCreditNote(OrgHeader client, string aPCreditNoteNumber)
		{
			string clientCode = client != null ? client.OH_Code : ZString.Empty;
			return RetrieveAPCreditNote(clientCode, aPCreditNoteNumber);
		}

		public APCreditNote RetrieveAPCreditNote(string clientCode, string aPCreditNoteNumber)
		{
			string key = GetAPCreditNoteKey(clientCode, aPCreditNoteNumber);
			return transactions.ContainsKey(key) ? (APCreditNote)transactions[key] : null;
		}

		public void RemoveAPCreditNote(string clientCode, string aPCreditNoteNumber)
		{
			transactions.Remove(GetAPCreditNoteKey(clientCode, aPCreditNoteNumber));
		}

		public bool ContainsAPCreditNote(string clientCode, string aPCreditNoteNumber)
		{
			return transactions.ContainsKey(GetAPCreditNoteKey(clientCode, aPCreditNoteNumber));
		}

		protected string GetAPCreditNoteKey(string clientCode, string aPCreditNoteNumber)
		{
			return TransactionTypes.CreditNote + ":" + clientCode + ":" + aPCreditNoteNumber;
		}

		#endregion

#if DEBUG
		#region AP Payment Methods For Test Only

		/// <summary>
		/// Payments created only when payment approvals are saved.
		/// </summary>
		public APPayment RetrieveAPPayment_ForTestOnly(OrgHeader client, AccBankAccount bankAccount, string receiptType, string chequeOrReference, string jobNumber)
		{
			string clientCode = client != null ? client.OH_Code : ZString.Empty;
			string bankAccountCode = bankAccount != null ? bankAccount.AB_Code : ZString.Empty;
			return RetrieveAPPayment_ForTestOnly(clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber);
		}

		/// <summary>
		/// Payments created only when payment approvals are saved.
		/// </summary>
		public APPayment RetrieveAPPayment_ForTestOnly(string clientCode, string bankAccountCode, string receiptType, string chequeOrReference, string jobNumber)
		{
			APPayment result = null;
			var key = GetAPPaymentApprovalKey(clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber);
			if (paymentApprovals.ContainsKey(key))
			{
				result = paymentApprovals[key].NewPayment as APPayment;
			}

			return result;
		}

		#endregion
#endif

		#region AP Payment Approval Methods

		public void AddAPPaymentApproval(PaymentApprovalBase paymentApproval, string clientCode, string bankAccountCode, string receiptType, string chequeOrReference, string jobNumber)
		{
			paymentApprovals.Add(GetAPPaymentApprovalKey(clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber), paymentApproval);
		}

		public PaymentApprovalBase RetrieveAPPaymentApproval(OrgHeader client, AccBankAccount bankAccount, string receiptType, string chequeOrReference, string jobNumber)
		{
			string clientCode = client != null ? client.OH_Code : ZString.Empty;
			string bankAccountCode = bankAccount != null ? bankAccount.AB_Code : ZString.Empty;
			return RetrieveAPPaymentApproval(clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber);
		}

		public PaymentApprovalBase RetrieveAPPaymentApproval(string clientCode, string bankAccountCode, string receiptType, string chequeOrReference, string jobNumber)
		{
			string key = GetAPPaymentApprovalKey(clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber);
			return paymentApprovals.ContainsKey(key) ? paymentApprovals[key] : null;
		}

		public void RemoveAPPaymentApproval(string clientCode, string bankAccountCode, string receiptType, string chequeOrReference, string jobNumber)
		{
			paymentApprovals.Remove(GetAPPaymentApprovalKey(clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber));
		}

		public bool ContainsAPPaymentApproval(string clientCode, string bankAccountCode, string receiptType, string chequeOrReference, string jobNumber)
		{
			return paymentApprovals.ContainsKey(GetAPPaymentApprovalKey(clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber));
		}

		static string GetAPPaymentApprovalKey(string clientCode, string bankAccountCode, string receiptType, string chequeOrReference, string jobNumber)
		{
			return clientCode + ":" + bankAccountCode + ":" + receiptType + ":" + chequeOrReference + (jobNumber != ZString.Empty ? ":" + jobNumber : string.Empty);
		}

		public PaymentApprovalBase[] GetAllAPPaymentApprovals()
		{
			return new List<PaymentApprovalBase>(paymentApprovals.Values).ToArray();
		}

		/// <summary>
		/// Payments created only when payment approvals are saved.
		/// </summary>
		public APPayment[] GetAllAPPaymentsCreatedFormApprovalsOnSaving()
		{
			return paymentApprovals.Values.Select(x => x.NewPayment as APPayment).Where(x => x != null).ToArray();
		}

		#endregion

		#region Match Links

		public IDictionary<ZGuid, TransactionMatchLinkCollection> MatchLinks
		{
			get { return matchLinks; }
		}

		public void AddMatchLinks(IDictionary<ZGuid, TransactionMatchLinkCollection> matchLinks)
		{
			foreach (ZGuid matchKey in matchLinks.Keys)
			{
				this.matchLinks.Add(matchKey, matchLinks[matchKey]);
			}
		}

		#endregion

		#region AR Invoice methods

		public void AddARInvoice(InvoicingBase invoice)
		{
			transactions.Add(invoice.PK.ToString(), invoice);
		}

		public bool ContainsARTransaction(ZGuid pk)
		{
			return transactions.ContainsKey(pk.ToString());
		}

		#endregion

		#region Collection Interfaces

		#region ICollection Members

		public bool IsSynchronized
		{
			get { return ((ICollection)transactions).IsSynchronized; }
		}

		public int Count
		{
			get { return transactions.Count; }
		}

		public int APTransactionsCount
		{
			get { return GetAllAPTransactions().Length; }
		}

		public int UATransactionsCount
		{
			get { return GetAllUATransactions().Length; }
		}

		public int ARTransactionsCount
		{
			get { return GetAllARTransactions().Length; }
		}

		public void CopyTo(Array array, int index)
		{
			((ICollection)transactions).CopyTo(array, index);
		}

		public object SyncRoot
		{
			get { return ((ICollection)transactions).SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return transactions.GetEnumerator();
		}

		#endregion

		#region ISerializable Members

#if NETFRAMEWORK
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			transactions.GetObjectData(info, context);
		}
#endif

		#endregion

		#region IDeserializationCallback Members

		public void OnDeserialization(object sender)
		{
			transactions.OnDeserialization(sender);
		}

		#endregion

		public ICollection Keys
		{
			get { return transactions.Keys; }
		}

		public ICollection Values
		{
			get { return transactions.Values; }
		}

		public TransactionHeader this[string key]
		{
			get { return transactions[key]; }
		}

		public void Remove(string key)
		{
			transactions.Remove(key);
		}

		public void Clear()
		{
			transactions.Clear();
			matchLinks.Clear();
		}

		#endregion
	}

	[Serializable]
	public class DuplicatedInvoiceException : Exception
	{
		public DuplicatedInvoiceException(ZString creditor, ZString invoiceNumber)
		{
			this.creditor = creditor;
			this.invoiceNumber = invoiceNumber;
		}

		public DuplicatedInvoiceException(ZString creditor, ZString invoiceNumber, string message)
			: base(message)
		{
			this.creditor = creditor;
			this.invoiceNumber = invoiceNumber;
		}

		public DuplicatedInvoiceException(ZString creditor, ZString invoiceNumber, string message, Exception inner)
			: base(message, inner)
		{
			this.creditor = creditor;
			this.invoiceNumber = invoiceNumber;
		}

#if NETFRAMEWORK
		protected DuplicatedInvoiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			creditor = info.GetString(CreditorFieldName);
			invoiceNumber = info.GetString(InvoiceNumberFieldName);
		}
#endif

		public ZString Creditor
		{
			get { return creditor; }
		}
		readonly ZString creditor;

		public ZString InvoiceNumber
		{
			get { return invoiceNumber; }
		}
		readonly ZString invoiceNumber;

#if NETFRAMEWORK
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(CreditorFieldName, Creditor);
			info.AddValue(InvoiceNumberFieldName, InvoiceNumber);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Serialization constant")]
		const string CreditorFieldName = "Creditor";
		const string InvoiceNumberFieldName = "InvoiceNumber";
#endif
	}
}
