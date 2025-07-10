using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class PostingChargeDistributor
	{
		public PostingChargeDistributor()
		{
			chargeSplitterDueToSingleTaxPerTransaction_constructorInitializedOnly = new ChargeSplitterDueToSingleTaxPerTransaction();
		}

		/// <summary>
		/// For each IReceiablesPostingCharge, create the appropriate key and build unique collections based on that key.
		/// </summary>
		/// <param name="receivableCharges">All receivable charges to distribute.</param>
		/// <returns>A collection of IReceivablesPostingChargeCollection, uniquely identified by the key.</returns>
		public PostingChargeCollection DistributeCharges(IReceivablesPostingChargeCollection receivableCharges)
		{
			PostingChargeCollection result = new PostingChargeCollection();

			foreach (IReceivablesPostingCharge charge in receivableCharges)
			{
				PostingChargeKey key = CreateKey(charge);

				charge.DebtorAddressPK = key.OrgAddress;
				charge.DebtorContactPK = key.OrgContact;
				if (!result.ContainsKey(key))
				{
					result.SetCharges(key, CreateNewChargesCollection(receivableCharges));
				}

				result.GetCharges(key).Add(charge);
			}

			result = SplitChargesByCount(result);

			result = ChargeSplitterDueToSingleTaxPerTransaction.GetSplitCharges(result);

			result.MergeCommentChargeKeysWithoutComparingPostingGroups();

			return result;
		}

		#region Implementation

		internal static IReceivablesPostingChargeCollection CreateNewChargesCollection(IReceivablesPostingChargeCollection allCharges)
		{
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			if (allCharges.JobIsSet)
			{
				charges.Job = allCharges.Job;
			}
			return charges;
		}

		#region Split Charges for China

		PostingChargeCollection SplitChargesByCount(PostingChargeCollection charges)
		{
			PostingChargeCollection result = ChargeSplitterByChargeCountAndValue.GetSplitCharges(charges);
			return result;
		}

		#endregion

		PostingChargeKey CreateKey(IReceivablesPostingCharge charge)
		{
			PostingChargeKey key = CreateBasicKey(charge);

			switch (charge.InvoiceType)
			{
				case InvoiceTypesList.Codes.InvoicePerTaxCode:
				case InvoiceTypesList.Codes.InvoicePerTaxCode_Batching:
					SetKeySellCurrencyForBillInInvoiceCurrency(charge, key);
					key.TaxRate = charge.TaxRate;
					break;

				case InvoiceTypesList.Codes.FreightInvoice:
				case InvoiceTypesList.Codes.FreightInvoice_Batching:
				case InvoiceTypesList.Codes.ForeignCurrencyInvoice:
				case InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching:
				case InvoiceTypesList.Codes.DisbursementInForeignCurrency:
				case InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignCollect:
				case AgencyInvoiceTypesList.Codes.ForeignCollect_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching:
				case InvoiceTypesList.Codes.SelfBillingInvoice:
					SetKeySellCurrencyForForeignCurrencyInvoices(charge, key);
					break;
				case InvoiceTypesList.Codes.FinalInvoice:
				case InvoiceTypesList.Codes.FinalInvoice_Batching:
				case InvoiceTypesList.Codes.DisbursementInvoice:
				case InvoiceTypesList.Codes.DisbursementInvoice_Batching:
				case InvoiceTypesList.Codes.DestinationChargesInvoice:
				case InvoiceTypesList.Codes.DestinationChargesInvoice_Batching:
				case AgencyInvoiceTypesList.Codes.LocalCollect:
				case AgencyInvoiceTypesList.Codes.LocalCollect_Batching:
				case AgencyInvoiceTypesList.Codes.LocalPrePaid:
				case AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching:
				case AgencyInvoiceTypesList.Codes.Misc:
				case AgencyInvoiceTypesList.Codes.Misc_Batching:
					SetKeySellCurrencyForBillInInvoiceCurrency(charge, key);
					break;
			}

			return key;
		}

		protected virtual void SetKeySellCurrencyForForeignCurrencyInvoices(IReceivablesPostingCharge charge, PostingChargeKey key)
		{
			if (charge.SellCurrency != null)
			{
				key.SellCurrency = charge.SellCurrency.RX_Code;
			}
		}

		protected virtual void SetKeySellCurrencyForBillInInvoiceCurrency(IReceivablesPostingCharge charge, PostingChargeKey key)
		{
			if (!charge.BillInLocalCurrency && charge.SellCurrency != null && charge.SellCurrency.RX_Code != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				key.SellCurrency = charge.SellCurrency.RX_Code;
			}
		}

		protected virtual PostingChargeKey CreateBasicKey(IReceivablesPostingCharge charge)
		{
			ZGuid debtorPK = charge.Debtor != null ? charge.Debtor.PK : ZGuid.Empty;
			var result = new PostingChargeKey(debtorPK, charge.InvoiceType, charge.DebtorAddressPK, charge.DebtorContactPK, charge.TaxRatePostingGroupId, charge.Branch, charge.SellPlaceOfSupply, charge.SellTaxBranch);
			result.SellReference = charge.SellReference;
			result.IsCommentChargeKey = charge.IsCommentChargeCode;
			return result;
		}

		#endregion

		IChargeSplitterDueToSingleTaxPerTransaction ChargeSplitterDueToSingleTaxPerTransaction => chargeSplitterDueToSingleTaxPerTransaction_constructorInitializedOnly;
		IChargeSplitterDueToSingleTaxPerTransaction chargeSplitterDueToSingleTaxPerTransaction_constructorInitializedOnly;

#if DEBUG

		public void SubstituteChargeSplitterDueToSingleTaxPerTransaction_ForTestOnly(IChargeSplitterDueToSingleTaxPerTransaction replacement) => chargeSplitterDueToSingleTaxPerTransaction_constructorInitializedOnly = replacement;
		public IChargeSplitterDueToSingleTaxPerTransaction ChargeSplitterDueToSingleTaxPerTransaction_ExposedForTestOnly => ChargeSplitterDueToSingleTaxPerTransaction;

#endif
	}
}
