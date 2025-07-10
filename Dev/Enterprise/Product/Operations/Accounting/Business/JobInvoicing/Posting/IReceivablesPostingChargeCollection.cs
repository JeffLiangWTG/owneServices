using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class IReceivablesPostingChargeCollection : List<IReceivablesPostingCharge>, ITransactionBranchCalculationDataProviderFromJobCharge
	{
		#region Posted Invoice

		/// <summary>
		/// The invoice that is created as a result of posting all charges in this collection.
		/// </summary>
		public InvoicingBase PostedInvoice
		{
			get { return fPostedInvoice; }
			set { fPostedInvoice = value; }
		}

		#endregion

		#region CFX Journal

		/// <summary>
		/// The CFX Journal Header that relates to this collection of charges and invoice.
		/// Only applicable if CFX Journalling is enabled.
		/// </summary>
		public JCJournalHeader CFXJournal
		{
			get { return fCFXJournal; }
			set { fCFXJournal = value; }
		}

		public bool ShouldCreateCFXJournal
		{
			get
			{
				bool result = false;
				foreach (IReceivablesPostingCharge charge in this)
				{
					if (charge.CFXAmount != 0)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		JCJournalHeader fCFXJournal;

		#endregion

		#region Posting Key

		/// <summary>
		/// The Unique key used to identify these charges.
		/// </summary>
		public PostingChargeKey Key
		{
			get { return fKey; }
			set { fKey = value; }
		}

		/// <summary>
		/// Is the Invoice Type specified on the Key "Disbursement Invoice".
		/// </summary>
		public bool IsDisbursementChargeCollection
		{
			get
			{
				return InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(Key.InvoiceType);
			}
		}

		PostingChargeKey fKey;

		#endregion

		#region Total Value
		public ZDecimal TotalExclTaxValueInLocalCurrency
		{
			get
			{
				ZDecimal result = 0m;
				foreach (IReceivablesPostingCharge charge in this)
				{
					result += (charge.LocalSellAmount);
				}

				return result;
			}
		}

		/// <summary>
		/// The total value of all charges in this collection, expressed in Local Currency.
		/// </summary>
		public ZDecimal TotalValueInLocalCurrency
		{
			get
			{
				ZDecimal result = 0m;
				foreach (IReceivablesPostingCharge charge in this)
				{
					result += (charge.LocalSellAmount + charge.LocalSellTaxAmount);
				}

				return result;
			}
		}

		/// <summary>
		/// The total value of all charges in this collection, expressed in Foreign Currency.
		/// </summary>
		public ZDecimal TotalValueInForeignCurrency
		{
			get
			{
				ZDecimal result = 0m;
				foreach (IReceivablesPostingCharge charge in this)
				{
					result += (charge.OSSellAmount + charge.OSSellTaxAmount);
				}

				return result;
			}
		}

		#endregion

		public RefCurrency[] GetAllChargeCurrencies()
		{
			ArrayList currencies = new ArrayList();
			foreach (IReceivablesPostingCharge charge in this)
			{
				if (!currencies.Contains(charge.SellCurrency))
				{
					currencies.Add(charge.SellCurrency);
				}
			}
			return (RefCurrency[])currencies.ToArray(typeof(RefCurrency));
		}

		public ZString PostingCurrency
		{
			get
			{
				ZString result = fPostingCurrency;
				if (result.IsEmpty)
				{
					result = FirstCharge.SellCurrency.RX_Code;
				}
				return result;
			}
			set { fPostingCurrency = value; }
		}

		public void SetAllChargesInvoiceType(ZString invoiceType)
		{
			foreach (IReceivablesPostingCharge charge in this)
			{
				charge.InvoiceType = invoiceType;
			}
		}

		public ZDecimal PostingCurrencyExchangeRate
		{
			get
			{
				ZDecimal result = fPostingCurrencyExchangeRate;
				if (result == 0m)
				{
					result = FirstCharge.SellExchangeRate;
				}
				return result;
			}
			set { fPostingCurrencyExchangeRate = value; }
		}

		public virtual ZGuid InvoicePostingDepartment
		{
			get { return Job.Department; }
		}

		public virtual ZGuid InvoicePostingBranch
		{
			get { return Job.Branch; }
		}

		public virtual ZGuid JobPK
		{
			get { return Job.PK; }
		}

		public virtual ZGuid Debtor
		{
			get { return FirstCharge.Debtor.PK; }
		}

		public OrgHeader DebtorBizO
		{
			get { return FirstCharge.Debtor; }
		}

		public OrgAddress DebtorAddressBizO
		{
			get { return FirstCharge?.Debtor?.Factory?.Load<OrgAddress>(DebtorAddress); }
		}

		public virtual ZGuid DebtorAddress
		{
			get { return FirstCharge.DebtorAddressPK; }
		}

		public virtual ZGuid DebtorContact
		{
			get { return FirstCharge.DebtorContactPK; }
		}

		public virtual ZString JobNumber
		{
			get { return Job.JobNumber; }
		}

		public virtual ZBool IsBillInLocalCurrency
		{
			get { return FirstCharge.BillInLocalCurrency; }
		}

		public IJobInvoicingPlugIn JobInvoicingPlugIn
		{
			get;
			set;
		}

		public virtual IPostingJob Job
		{
			get { return JobIsSet ? fJob : FirstCharge.Job; }
			internal set
			{
				fJob = value;
				JobIsSet = true;
			}
		}

		internal bool JobIsSet;

		IPostingJob fJob;

		IReceivablesPostingCharge FirstCharge
		{
			get { return this[0]; }
		}

		internal bool? IsPositiveCost
		{
			get { return fIsPositiveCost; }
		}

		ZGuid ITransactionBranchCalculationDataProviderFromJobCharge.JobBranchPK => FirstCharge.Job.Branch;

		HashSet<ZGuid> ITransactionBranchCalculationDataProviderFromJobCharge.LineBranchPKs => this.Select(x => x.Branch).ToHashSet();

		bool ITransactionBranchCalculationDataProviderFromJobCharge.AnyCharges => this.Any();

		internal void SetIsPositiveCost(bool isPositiveCost)
		{
			fIsPositiveCost = isPositiveCost;
		}

		#region Implementation

		ZDecimal fPostingCurrencyExchangeRate;
		ZString fPostingCurrency;
		InvoicingBase fPostedInvoice;
		bool? fIsPositiveCost;

		#endregion
	}
}
