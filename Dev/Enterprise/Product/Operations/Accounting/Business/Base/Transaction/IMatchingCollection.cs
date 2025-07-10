using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public enum MatchingCollectionTypes
	{
		UnmatchedTransactions,
		MatchedTransactions
	}

	public partial class IMatchingCollection : BusinessObjectCollection<BusinessObject>
	{
		public IMatchingCollection(BusinessObjectFactory factory)
			: this(factory, MatchingCollectionTypes.MatchedTransactions)
		{
		}

		public IMatchingCollection(BusinessObjectFactory factory, MatchingCollectionTypes collectionType)
			: base(factory)
		{
			MatchingCollectionType = collectionType;
		}

		public readonly MatchingCollectionTypes MatchingCollectionType;

		public new IMatching this[int index]
		{
			get { return (IMatching)Elements[index]; }
		}

		public void Add(IMatching transaction)
		{
			Add((BusinessObject)transaction);
		}

		public override void Add(BusinessObject businessObject)
		{
			IMatching matchingRow = businessObject as IMatching;

			if (matchingRow != null)
			{
				if (IsSettingChequeNumReadOnlyOnAdd)
				{
					matchingRow.ChequeOrReference_ReadOnly = true;
				}

				matchingRow.OSPartialPaymentAmountInfo.ValueChanged += new EventHandler(OSPartialPaymentAmountInfo_ValueChanged);

				base.Add(businessObject);
			}
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ARContraRow);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			if (elementToRemove != null && !MustBeMatched.Contains(elementToRemove) && this.Contains(elementToRemove))
			{
				base.Remove(elementToRemove);
			}
			IMatching matchingRow = elementToRemove as IMatching;
			if (matchingRow != null)
			{
				matchingRow.OSPartialPaymentAmountInfo.ValueChanged -= new EventHandler(OSPartialPaymentAmountInfo_ValueChanged);
			}
		}

		public void RemoveAllFromAllCollections()
		{
			MustBeMatched.RemoveAll();
			RemoveAll();
		}

		public ZBool IsSettingChequeNumReadOnlyOnAdd
		{
			get { return fIsSettingChequeNumReadOnlyOnAdd; }
			set { fIsSettingChequeNumReadOnlyOnAdd = value; }
		}

		ZBool fIsSettingChequeNumReadOnlyOnAdd;

#if DEBUG

		// this should only be used for testing purposes
		public void SetPartialPaidAmount()
		{
			foreach (IMatching matchingBizO in this)
			{
				matchingBizO.OSPartialPaymentAmount = matchingBizO.OSOutstandingAmount;
			}
		}

#endif

		public void Pay(ZDateTime fullyPayDate)
		{
			foreach (IMatching transaction in this)
			{
				if (transaction.LocalPartialPaymentAmount == transaction.OriginalOutstandingAmount ||
					transaction.TransactionType == ZArchitecture.Core.TransactionTypes.Discount ||
					transaction.TransactionType == ZArchitecture.Core.TransactionTypes.ExchangeDifference ||
					transaction.TransactionType == ZArchitecture.Core.TransactionTypes.Overpayment)
				{
					transaction.FullyPay(fullyPayDate);
				}
				else if (transaction.LocalPartialPaymentAmount != 0)
				{
					transaction.PartiallyPay();
				}
			}
		}

		void OSPartialPaymentAmountInfo_ValueChanged(object sender, EventArgs e)
		{
				ValidatePaymentApproval();
		}

		public void ValidatePaymentApproval()
		{
			foreach (IMatching transaction in this)
			{
				if (transaction is PaymentApprovalBase)
				{
					PaymentApprovalBase paymentApprovalBase = transaction as PaymentApprovalBase;
					paymentApprovalBase.Validation.ValidateAV_Amount();
					break;
				}
			}
		}

		// IMPORTANT: this must clear the MatchLinks collection in the IMatching object
		#region GenerateMatchLinkRows

		public TransactionMatchLink[] GenerateMatchLinkRows()
		{
			var matchLinks = new List<TransactionMatchLink>();
			foreach (IMatching transaction in this)
			{
				transaction.CurrentMatchGroup.RemoveAll();
				transaction.GenerateMatchLinks();
				matchLinks.AddRange(transaction.CurrentMatchGroup.ToArray<TransactionMatchLink>());
				transaction.CurrentMatchGroup.RemoveAll();
			}
			return matchLinks.ToArray();
		}

		#endregion

		#region	GeneratePaymentApprovalItems

		public PaymentApprovalItemCollection GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
			PaymentApprovalItemCollection paymentApprovalItems = new PaymentApprovalItemCollection(Factory);

			foreach (IMatching transaction in this)
			{
				transaction.GeneratePaymentApprovalItems(approval);
				paymentApprovalItems.AddRange(transaction.PaymentApprovalItems);
			}

			return paymentApprovalItems;
		}

		#endregion
		#region ContainsBusinessObjects

		public ZBool ContainsBusinessObjects(BusinessObject[] bizOs)
		{
			foreach (BusinessObject bizO in bizOs)
			{
				if (!this.Contains(bizO))
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#region Balance

		public IDisposable SuspendBalanceCalculation()
		{
			return new BalanceCalculationSuspender(this);
			//IsBalanceCalculationSuspended = true;
		}

		class BalanceCalculationSuspender : Disposable
		{
			public BalanceCalculationSuspender(IMatchingCollection parent)
			{
				this.parent = parent;
				parent.BalanceCalculationSuspendedCount++;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					parent.BalanceCalculationSuspendedCount--;
					if (parent.BalanceCalculationSuspendedCount < 0)
					{
						ErrorReporter.ReportOnce("IMatchingCollection.BalanceCalculationSuspender.Dispose", "Suspend Count is below zero.");
					}
				}
			}

			readonly IMatchingCollection parent;
		}

		int BalanceCalculationSuspendedCount;

		public ZDecimal Balance
		{
			get
			{
				ZDecimal balance = 0.0M;
				if (BalanceCalculationSuspendedCount == 0)
				{
					foreach (IMatching transaction in this)
					{
						if (!(transaction is Contra) && !(transaction is Transfer))
						{
							balance += transaction.LocalPartialPaymentAmount;
						}
					}
				}
				return balance;
			}
		}

		#endregion

		#region SuspendHeaderAmountsRecalculation

		public IDisposable SuspendHeaderAmountsRecalculation()
		{
			return new HeaderAmountsRecalculationSuspender(this);
		}

		class HeaderAmountsRecalculationSuspender : Disposable
		{
			public HeaderAmountsRecalculationSuspender(IMatchingCollection parent)
			{
				this.parent = parent;
				suspenders = new List<IDisposable>();
				foreach (IMatching matching in parent)
				{
					InvoicingBase invoice = matching as InvoicingBase;
					if (invoice != null)
					{
						suspenders.Add(invoice.SuspendHeaderAmountsRecalculation());
					}
				}
				parent.HeaderAmountsRecalculationSuspendedCount++;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					parent.HeaderAmountsRecalculationSuspendedCount--;
					if (parent.HeaderAmountsRecalculationSuspendedCount < 0)
					{
						ErrorReporter.ReportOnce("IMatchingCollection.BalanceCalculationSuspender.Dispose", "Suspend Count is below zero.");
					}
					foreach (IDisposable suspender in suspenders)
					{
						suspender.Dispose();
					}
					suspenders.Clear();
				}
			}

			readonly IMatchingCollection parent;
			readonly List<IDisposable> suspenders;
		}

		int HeaderAmountsRecalculationSuspendedCount;

		#endregion

		#region ContainsFullyPaidTransaction

		public ZBool ContainsFullyPaidTransaction
		{
			get
			{
				foreach (IMatching transaction in this)
				{
					if (!(transaction is Contra) && !(transaction is Transfer) &&
						transaction.OutstandingAmount == 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region CountOfPayments

		public ZInt CountOfPayments
		{
			get
			{
				int result = 0;
				foreach (IMatching transaction in this)
				{
					if (transaction is Payment || transaction is PaymentApprovalBase)
					{
						result++;
					}
				}
				return result;
			}
		}

		#endregion

		#region ContainsTransactionFromSpecifiedOrg

		public ZBool ContainsTransactionFromSpecifiedOrg(OrgHeader org)
		{
			if (org != null)
			{
				foreach (IMatching transaction in this)
				{
					if (transaction.Organisation == org.PK)
					{
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		public bool ContainsTransactionWithSpecifiedCurrency(ZString currency)
		{
			bool result = false;
			if (!currency.IsEmpty)
			{
				foreach (IMatching transaction in this)
				{
					if (transaction.CurrencyCode == currency)
					{
						result = true;
					}
				}
			}
			return result;
		}

		public ZDecimal TotalOSAmountForSpecifiedCurrency(ZString currency)
		{
			ZDecimal amount = 0m;
			if (!currency.IsEmpty)
			{
				foreach (IMatching matchingObj in this)
				{
					if (matchingObj.CurrencyCode == currency)
					{
						amount += matchingObj.OSPartialPaymentAmount;
					}
				}
			}
			return amount;
		}

		public ZDecimal TotalLocalAmountForSpecifiedCurrency(ZString currency)
		{
			ZDecimal result = 0m;
			if (!currency.IsEmpty)
			{
				foreach (IMatching matchingObj in this)
				{
					if (matchingObj.CurrencyCode == currency)
					{
						result += matchingObj.LocalPartialPaymentAmount;
					}
				}
			}
			return result;
		}

		public ZDecimal TotalOSAmountForSpecifiedCurrencyAndTransactionType(ZString currency, string type)
		{
			ZDecimal result = 0m;
			if (!currency.IsEmpty)
			{
				foreach (IMatching transaction in this)
				{
					if (transaction.CurrencyCode == currency &&
						transaction.TransactionType == type)
					{
						result += transaction.OSPartialPaymentAmount;
					}
				}
			}
			return result;
		}

		// Returns the sub-total of AH_InvoiceAmount over all transactions belonging to the 
		// specified Organization and from the specified ledger
		#region GetOrganizationBalanceAmount

		public ZDecimal GetOrganizationBalanceAmount(ZGuid organization, ZString ledger)
		{
			ZDecimal subBalance = 0M;
			// check that Ledger is AR or AP
			if (ledger == LedgerTypes.AccountsReceivable || ledger == LedgerTypes.AccountsPayable)
			{
				foreach (IMatching transaction in this)
				{
					if (transaction.Organisation == organization && transaction.Ledger == ledger)
					{
						subBalance += transaction.LocalPartialPaymentAmount;
					}
					else if (transaction is Transfer && transaction.Ledger == ledger)
					{
						// check if either of the TransferRows is from the Organization
						Transfer currentTransfer = (Transfer)transaction;
						if (currentTransfer.TransferFrom.AH_OH == organization)
						{
							subBalance += currentTransfer.TransferFrom.AH_InvoiceAmount;
						}
						else if (currentTransfer.TransferTo.AH_OH == organization)
						{
							subBalance += currentTransfer.TransferTo.AH_InvoiceAmount;
						}
					}
					else if (transaction is Contra)
					{
						Contra currentContra = (Contra)transaction;
						if (ledger == LedgerTypes.AccountsPayable &&
							currentContra.APRow.AH_OH == organization)
						{
							subBalance += currentContra.APRow.AH_InvoiceAmount;
						}
						else if (ledger == LedgerTypes.AccountsReceivable &&
							currentContra.ARRow.AH_OH == organization)
						{
							subBalance += currentContra.ARRow.AH_InvoiceAmount;
						}
					}
				}
			}
			return subBalance;
		}

		#endregion

		// Returns the sub-total of AH_Invoice amount over all transactions from the 
		// specified ledger
		#region GetLedgerBalanceAmount

		public ZDecimal GetLedgerBalanceAmount(ZString ledger)
		{
			ZDecimal subBalance = 0;
			foreach (IMatching transaction in this)
			{
				if (transaction is Contra)
				{
					Contra currentContra = (Contra)transaction;
					if (ledger == LedgerTypes.AccountsReceivable)
					{
						subBalance += currentContra.ARRow.AH_InvoiceAmount;
					}
					else if (ledger == LedgerTypes.AccountsPayable)
					{
						subBalance += currentContra.APRow.AH_InvoiceAmount;
					}
				}
				else if (transaction is Transfer)
				{
					// do nothing since Transfer does not affect balance of ledger
				}
				else if (transaction.Ledger == ledger)
				{
					subBalance += transaction.OutstandingAmount;
				}
			}
			return subBalance;
		}

		#endregion

		// Returns the first Transfer with the values specified in the TransferFilter object 
		// passed in, or null if no such Transfer exists
		#region GetMatchingTransfer

		public Transfer GetMatchingTransfer(TransferFilter filter)
		{
			foreach (IMatching transaction in this)
			{
				if (transaction is Transfer)
				{
					Transfer currentTransfer = (Transfer)transaction;
					if (currentTransfer.TransferFrom.AH_OH == filter.TransferFromOrg
						&& currentTransfer.TransferTo.AH_OH == filter.TransferToOrg
						&& currentTransfer.TransferFrom.AH_InvoiceAmount == filter.TransferFromAmount
						&& currentTransfer.TransferTo.AH_InvoiceAmount == filter.TransferToAmount
						&& currentTransfer.AH_Ledger == filter.TransferLedger)
					{
						return currentTransfer;
					}
				}
			}
			return null;
		}

		#endregion

		// Returns the first Contra with the values specified in the ContraFilter object
		#region GetMatchingContra

		public Contra GetMatchingContra(ContraFilter filter)
		{
			foreach (IMatching transaction in this)
			{
				if (transaction is Contra)
				{
					Contra currentContra = (Contra)transaction;
					if (currentContra.APRow.AH_OH == filter.APOrg &&
						currentContra.APRow.AH_InvoiceAmount == filter.APAmount &&
						currentContra.ARRow.AH_OH == filter.AROrg &&
						currentContra.ARRow.AH_InvoiceAmount == filter.ARAmount)
					{
						return currentContra;
					}
				}
			}
			return null;
		}

		#endregion

		public void AddTransactionThatMustBeMatched(IMatching transaction)
		{
			MustBeMatched.Add(transaction);
			this.Add(transaction);
		}

		public ZBool MustTransactionBeMatched(BusinessObject transaction)
		{
			return MustBeMatched.Contains(transaction);
		}

		internal IMatchingCollection MustBeMatched
		{
			get
			{
				if (fMustBeMatched == null)
				{
					fMustBeMatched = new IMatchingCollection(Factory);
				}
				return fMustBeMatched;
			}
		}

		IMatchingCollection fMustBeMatched;

		#region FetchStrategy

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new IMatchingCollectionFetchStrategy(this);
		}

		#endregion
	}

	#region Helper Classes

	#region TransferFilter

	/// <summary>
	/// This is a helper class used for test cases - for checking whether transfers with certain values 
	/// exist in collections.  Note: all values on this object must be set
	/// </summary>
	public class TransferFilter
	{
		public TransferFilter()
		{
		}

		#region TransferFromOrg

		public ZGuid TransferFromOrg
		{
			get
			{
				return fTransferFromOrg;
			}
			set
			{
				fTransferFromOrg = value;
			}
		}
		protected ZGuid fTransferFromOrg;

		#endregion

		#region TransferToOrg

		public ZGuid TransferToOrg
		{
			get
			{
				return fTransferToOrg;
			}
			set
			{
				fTransferToOrg = value;
			}
		}
		protected ZGuid fTransferToOrg;

		#endregion

		public ZDecimal TransferFromAmount;

		public ZDecimal TransferToAmount;

		public ZString TransferLedger;
	}

	#endregion

	#region ContraFilter

	/// <summary>
	/// This is also a helper class used only in test cases - for checking whether contras with certain values
	/// exist in collections
	/// </summary>
	public class ContraFilter
	{
		public ContraFilter()
		{
		}

		public ZGuid APOrg;
		public ZGuid AROrg;
		public ZDecimal APAmount;
		public ZDecimal ARAmount;
	}

	#endregion

	#endregion
}
