using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EdificeEntryCreationStrategy : EntryCreationStrategyBase
	{
		public EdificeEntryCreationStrategy(LineMerger lineMerger)
			: base(lineMerger)
		{
			haveInvoicesWithValidCharges = ((JobDeclaration)lineMerger.Declaration).Invoices.HasAnElementWithValidCharges();
		}

		protected override Customs.Business.MergeKey GetKeyForHeaderCore(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			JobComInvoiceHeader invoiceHeader = invoiceLine.InvoiceHeader;

			Customs.Business.MergeKey result = base.GetKeyForHeaderCore(baseInvoiceLine);

			if (invoiceHeader != null)
			{
				if (invoiceHeader.JobDeclaration.Invoices.Count > 0 && invoiceHeader.Charges.HasAnElementWithValidCharges())
				{
					result.Add(invoiceHeader.PK);
				}
				else
				{
					result.Add(invoiceHeader.Supplier == null ? ZString.Empty : invoiceHeader.Supplier.OH_Code);
					result.Add(invoiceHeader.ITOTIncoTerm);
					result.Add(invoiceHeader.Invoice_Currency == null ? ZString.Empty : invoiceHeader.Invoice_Currency.RX_Code);
					result.Add(invoiceHeader.AddInfo.ZA_ValuationBasis_Hidden);
					result.Add(invoiceLine.Nature);
					result.Add(invoiceHeader.AddInfo.ZA_EFD);
				}
			}
			return result;
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			Customs.Business.MergeKey result = base.GetKeyForLine(baseInvoiceLine);

			if (invoiceLine.Declaration != null && invoiceLine.Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMerge)
			{
				result.Add(DefaultCPAnswers(invoiceLine));
				result.Add(invoiceLine.AddInfo.ZA_RelatedLinePK_Hidden);
			}
			return result;
		}

		protected bool haveInvoicesWithValidCharges;

		protected ZString DefaultCPAnswers(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.Pivot?.EffectiveAddInfo.ZA_CPDecDefaultAnswerTrue_Hidden ?? ZString.Empty;
		}
	}
}
