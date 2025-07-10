using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class EntryCreationStrategyBase : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategyBase(LineMerger lineMerger)
			: base(lineMerger.Declaration)
		{
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			Customs.Business.MergeKey result = base.GetKeyForLine(baseInvoiceLine);

			if (invoiceLine.Declaration != null && invoiceLine.Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMerge)
			{
				result.Add(invoiceLine.JI_ConcessionOrder);
				result.Add(invoiceLine.AddInfo.AggregatedZA_ValuationBasis_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_TreatmentCode_Hidden);
				result.Add(invoiceLine.AddInfo.MergeAddInfoString);
			}
			return result;
		}

		protected override string GetNonAmendableLineDetails(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			JobComInvoiceLine auInvoiceLine = (JobComInvoiceLine)invoiceLine;
			return auInvoiceLine.NatureLegOrCMR;
		}
	}
}
