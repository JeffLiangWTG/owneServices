using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class InvoiceSplitArgs : EventArgs
	{
		public ARInvoice OldInvoice;
		public ARInvoice NewInvoice;
		public IEnumerable<ARInvoiceLine> InvoiceLines;
	}

	public interface IInvoiceUpdateNotifier
	{
		void OnInvoiceChanged(ARInvoice newInvoice);
	}

	public class EDIARInvoiceTaxProcessor
	{
		public IEnumerable<ARInvoice> Process(ARInvoice invoice)
		{
			if (invoice != null && EDIDataRegistry.Instance.EnableTaxProcessorForBilling.Value
					&& (invoice.Company?.IsEnabledForTaxFrameworkConfiguration(invoice.Factory) ?? false))
			{
				return ProcessCore(invoice);
			}
			else
			{
				return new[] { invoice };
			}
		}

		public event EventHandler<InvoiceSplitArgs> OnInvoiceSplit;

		IEnumerable<ARInvoice> ProcessCore(ARInvoice invoice)
		{
			var invoices = new List<ARInvoice>() { invoice };
			if (invoice.Lines.Count > 1)
			{
				var charges = new IReceivablesPostingChargeCollection();
				charges.AddRange(invoice.Lines.OfType<ARInvoiceLine>().Select(x => new EDIARInvoiceCharge(x)).ToArray());
				var chargeCollection = GetPostingChargeDistributor().DistributeCharges(charges);

				if (chargeCollection.Keys.Count > 1)
				{
					invoices.Clear();
					invoices.AddRange(SplitInvoice(invoice, chargeCollection));
				}
			}

			invoices.ForEach(x => ProcessTaxesOnPosting(x));
			return invoices;
		}

		IEnumerable<ARInvoice> SplitInvoice(ARInvoice invoice, PostingChargeCollection chargeCollection)
		{
			var result = new List<ARInvoice>(chargeCollection.Count);
			var invoiceDesc = invoice.AH_Desc;
			var groups = chargeCollection.OfType<IReceivablesPostingChargeCollection>()
				.Select(x => x.OfType<EDIARInvoiceCharge>().Select(y => y.InvoiceLine))
				.OrderBy(x => x.Min(y => y.AL_Sequence))
				.ToArray();

			for (var index = 0; index < groups.Length; index++)
			{
				ARInvoice currentInvoice = null;
				if (index == 0)
				{
					var linesToRemove = invoice.Lines.OfType<ARInvoiceLine>().Except(groups[index]).ToList();
					linesToRemove.ForEach(x => invoice.Lines.Remove(x));
					currentInvoice = invoice;
				}
				else
				{
					currentInvoice = invoice.Factory.New<ARInvoice>();
					currentInvoice.CopyPersistentValuesFrom(invoice);
					var lines = groups[index];
					currentInvoice.Lines.AddRange(lines);
					OnInvoiceSplit?.Invoke(this, new InvoiceSplitArgs() { OldInvoice = invoice, NewInvoice = currentInvoice, InvoiceLines = lines });
				}

				currentInvoice.AH_Desc = $"{invoiceDesc} - {index + 1} of {groups.Length}";
				result.Add(currentInvoice);
			}

			return result;
		}

		void ProcessTaxesOnPosting(ARInvoice invoice)
		{
			var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			var errorMessage =  ObjectFactory.Get<ITaxProcessor>().ProcessTaxesOnPosting(taxParent);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				throw new InvalidOperationException(errorMessage);
			}
		}

		protected virtual PostingChargeDistributor GetPostingChargeDistributor() => new PostingChargeDistributor();
	}
}
