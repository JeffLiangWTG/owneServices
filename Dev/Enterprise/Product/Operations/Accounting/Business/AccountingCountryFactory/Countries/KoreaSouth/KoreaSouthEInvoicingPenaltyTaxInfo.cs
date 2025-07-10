using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public sealed class KoreaSouthEInvoicingPenaltyTaxInfo : NonPersistentBusinessObject
	{
		public KoreaSouthEInvoicingPenaltyTaxInfo()
		{
			PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices = new KoreaSouthEInvoicingPenaltyTaxDetailCollection();
			PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices.Add(
				new KoreaSouthEInvoicingPenaltyTaxDetail(
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyType.NotIssued,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTypeExplanation.NotIssued,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.TwoPercent,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.NonDeductible));
			PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices.Add(
				new KoreaSouthEInvoicingPenaltyTaxDetail(
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyType.DelayedIssued,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTypeExplanation.DelayedIssued,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.OnePercent,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.HalfOfOnePercent));
			PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices.Add(
				new KoreaSouthEInvoicingPenaltyTaxDetail(
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyType.IssuedByPaper,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTypeExplanation.IssuedByPaper,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.OnePercent,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.NotApplicable));

			PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices = new KoreaSouthEInvoicingPenaltyTaxDetailCollection();
			PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices.Add(
				new KoreaSouthEInvoicingPenaltyTaxDetail(
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyType.NotTransmitted,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTypeExplanation.NotTransmitted,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.OnePercent,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.NotApplicable));
			PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices.Add(
				new KoreaSouthEInvoicingPenaltyTaxDetail(
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyType.DelayedTransmitted,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTypeExplanation.DelayedTransmitted,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.HalfOfOnePercent,
					KoreaSouthEInvoicingPenaltyTaxInfoConstants.PenaltyTax.NotApplicable));
		}

		public KoreaSouthEInvoicingPenaltyTaxDetailCollection PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices { get; }
		public KoreaSouthEInvoicingPenaltyTaxDetailCollection PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices { get; }
	}
}
