using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NormalisationConditionChecker
	{
		public NormalisationConditionChecker(CusEntryHeader entryHeader)
		{
			this.EntryHeader = entryHeader;
		}

		public bool IsPreConditionInvoicesNormalisationMet
		{
			get { return EntryHeader.Declaration != null && EntryHeader.Declaration.IsImportCMR; }
		}

		public bool ShouldEntryBeNormalised
		{
			get
			{
				return IsPreConditionInvoicesNormalisationMet &&
						(ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice ||
							ShouldInvoiceLinesBeNormalised);
			}
		}

		internal bool ShouldInvoiceLinesBeNormalised
		{
			get
			{
				return HaveInvoiceLineLevelCharges || HaveChargesDistributedByOtherThanValue;
			}
		}

		internal bool ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice
		{
			get
			{
				return EntryHeader.InvoiceHeaders.Length > 1
					&&
					(
						HaveDifferentCurrency
						|| HaveDifferentIncoTermOrZeroAmountMandatoryCharges
						|| HaveInvoiceLevelChargeOrSubGroupCharges
					);
			}
		}

		public void NotifyNormalisationConditionIsDirty()
		{
			needToRecalculate_DifferentCurrencies = true;
			needToRecalculate_DifferentIncoTermOrZeroAmountMandatoryCharges = true;
			needToRecalculate_InvoiceLevelChargeOrSubGroupCharges = true;
			needToRecalculate_HaveChargesDistributedByOtherThanValue = true;
			needToRecalculate_HaveInvoiceLineCharges = true;
		}

		#region Implementation

		protected internal bool HaveDifferentCurrency
		{
			get
			{
				if (needToRecalculate_DifferentCurrencies)
				{
					ArrayList currencies = new ArrayList();

					foreach (JobComInvoiceHeader invoice in EntryHeader.InvoiceHeaders)
					{
						if (!invoice.IsDeleted && invoice.Invoice_Currency != null && !invoice.Invoice_Currency.IsDeleted && !currencies.Contains(invoice.Invoice_Currency.RX_Code))
						{
							currencies.Add(invoice.Invoice_Currency.RX_Code);
							fHaveDifferentCurrency = currencies.Count > 1;
							if (fHaveDifferentCurrency)
							{
								break;
							}
						}
					}

					needToRecalculate_DifferentCurrencies = false;
				}
				return fHaveDifferentCurrency;
			}
		}

		protected internal bool HaveDifferentIncoTermOrZeroAmountMandatoryCharges
		{
			get
			{
				if (needToRecalculate_DifferentIncoTermOrZeroAmountMandatoryCharges)
				{
					var incoterms = new List<ZString>();

					foreach (JobComInvoiceHeader invoice in EntryHeader.InvoiceHeaders)
					{
						var itotIncoTerm = invoice.ITOTIncoTerm;
						if (!itotIncoTerm.IsEmpty && !incoterms.Contains(itotIncoTerm))
						{
							incoterms.Add(itotIncoTerm);
						}

						fHaveDifferentIncoTermOrZeroAmountMandatoryCharges = incoterms.Count > 1
							|| (invoice.IncoTermAndChargeFactory?.MissingMandatoryCharges(invoice.IncoTerm, invoice).Length ?? 0) > 0;

						if (fHaveDifferentIncoTermOrZeroAmountMandatoryCharges)
						{
							break;
						}
					}

					needToRecalculate_DifferentIncoTermOrZeroAmountMandatoryCharges = false;
				}
				return fHaveDifferentIncoTermOrZeroAmountMandatoryCharges;
			}
		}

		protected internal bool HaveInvoiceLevelChargeOrSubGroupCharges
		{
			get
			{
				if (needToRecalculate_InvoiceLevelChargeOrSubGroupCharges)
				{
					JobComInvoiceHeader[] invoices = EntryHeader.InvoiceHeaders;

					if (invoices.Length > 1)
					{
						foreach (JobComInvoiceHeader invoice in invoices)
						{
							fHaveInvoiceLevelChargeOrSubGroupCharges = invoice.Charges.Count > 0 && invoice.Charges.HasAnElementWithValidCharges();
							if (fHaveInvoiceLevelChargeOrSubGroupCharges)
							{
								break;
							}
						}
					}

					if (!fHaveInvoiceLevelChargeOrSubGroupCharges)
					{
						foreach (JobComInvoiceHeader invoice in invoices)
						{
							fHaveInvoiceLevelChargeOrSubGroupCharges = IsThisGroupSubGroupHeaderWithValidCharge(invoice.GroupHeader);
							if (fHaveInvoiceLevelChargeOrSubGroupCharges)
							{
								break;
							}
						}
					}

					needToRecalculate_InvoiceLevelChargeOrSubGroupCharges = false;
				}
				return fHaveInvoiceLevelChargeOrSubGroupCharges;
			}
		}

		bool IsThisGroupSubGroupHeaderWithValidCharge(JobComInvoiceGroupHeader groupHeader)
		{
			if (groupHeader == null || groupHeader == EntryHeader.Declaration.JobComInvoiceGroupHeaders[0])
			{
				return false;
			}
			else if (groupHeader.Charges.HasAnElementWithValidCharges())
			{
				return true;
			}
			else
			{
				return IsThisGroupSubGroupHeaderWithValidCharge((JobComInvoiceGroupHeader)groupHeader.GroupHeader);
			}
		}

		internal bool HaveChargesDistributedByOtherThanValue
		{
			get
			{
				if (needToRecalculate_HaveChargesDistributedByOtherThanValue)
				{
					JobComInvoiceHeader[] invoices = EntryHeader.InvoiceHeaders;

					foreach (JobComInvoiceHeader invoice in invoices)
					{
						fHaveChargesDistributedByOtherThanValue = invoice.Charges.HasChargesDistributedByOtherThanValue() || DoesThisGroupHeaderHaveChargesDistributedByOtherThanValue(invoice.GroupHeader);
						if (fHaveChargesDistributedByOtherThanValue)
						{
							break;
						}
					}
					needToRecalculate_HaveChargesDistributedByOtherThanValue = false;
				}
				return fHaveChargesDistributedByOtherThanValue;
			}
		}

		bool DoesThisGroupHeaderHaveChargesDistributedByOtherThanValue(JobComInvoiceGroupHeader groupHeader)
		{
			if (groupHeader == null)
			{
				return false;
			}
			else if (groupHeader.Charges.HasChargesDistributedByOtherThanValue())
			{
				return true;
			}
			else
			{
				return DoesThisGroupHeaderHaveChargesDistributedByOtherThanValue((JobComInvoiceGroupHeader)groupHeader.GroupHeader);
			}
		}

		#endregion

		#region HaveInvoiceLineLevelCharges

		bool HaveInvoiceLineLevelCharges
		{
			get
			{
				if (needToRecalculate_HaveInvoiceLineCharges)
				{
					JobComInvoiceHeader[] invoices = EntryHeader.InvoiceHeaders;

					foreach (JobComInvoiceHeader invoice in invoices)
					{
						foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
						{
							fHaveInvoiceLineLevelCharges = invoiceLine.Charges.HasAnElementWithValidCharges();
							if (fHaveInvoiceLineLevelCharges)
							{
								break;
							}
						}
						if (fHaveInvoiceLineLevelCharges)
						{
							break;
						}
					}
					needToRecalculate_HaveInvoiceLineCharges = false;
				}
				return fHaveInvoiceLineLevelCharges;
			}
		}

		#endregion

		#region Variables

		protected readonly CusEntryHeader EntryHeader;

		internal bool needToRecalculate_DifferentCurrencies = true;
		bool fHaveDifferentCurrency;

		internal bool needToRecalculate_DifferentIncoTermOrZeroAmountMandatoryCharges = true;
		bool fHaveDifferentIncoTermOrZeroAmountMandatoryCharges;

		internal bool needToRecalculate_InvoiceLevelChargeOrSubGroupCharges = true;
		bool fHaveInvoiceLevelChargeOrSubGroupCharges;

		bool fHaveChargesDistributedByOtherThanValue;
		bool needToRecalculate_HaveChargesDistributedByOtherThanValue = true;

		bool fHaveInvoiceLineLevelCharges;
		internal bool needToRecalculate_HaveInvoiceLineCharges = true;

		#endregion
	}
}
