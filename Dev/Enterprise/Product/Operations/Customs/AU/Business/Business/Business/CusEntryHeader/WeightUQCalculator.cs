using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WeightUQCalculator : Customs.Business.WeightUQCalculator
	{
		public WeightUQCalculator(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected new CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)base.EntryHeader; }
		}

		protected override ZWeight CalculateWeightAndWeightUQCore()
		{
			if (EntryHeader.IsCMRNature30)
			{
				ZString? uq = null;
				var totalWeight = ZDecimal.Zero;
				foreach (JobComInvoiceLine invoiceLine in EntryHeader.InvoiceLines)
				{
					if (Core.Constants.Weight.ContainsCode(invoiceLine.JI_WeightUQ))
					{
						if (!uq.HasValue)
						{
							uq = invoiceLine.JI_WeightUQ;
							totalWeight = invoiceLine.JI_Weight;
						}
						else
						{
							totalWeight += Core.Constants.Weight.Convert(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ, uq);
						}
					}
				}
				return uq.HasValue ? new ZWeight(totalWeight, uq.Value) : ZWeight.Empty;
			}
			else
			{
				return base.CalculateWeightAndWeightUQCore();
			}
		}
		protected override decimal GetSplitNatureFactor(Customs.Business.BaseJobComInvoiceHeader baseInvoiceHeader)
		{
			decimal factor = 1m;//what percentage of the invoiceheader is on this customs header (for split nature)

			if (!EntryHeader.Declaration.IsImportCMR)//N10 & N20 can be declared in one entry header in CMR
			{
				JobComInvoiceHeader invoiceHeader = baseInvoiceHeader as JobComInvoiceHeader;
				if (invoiceHeader.JZ_Nature10PackCount > 0 && invoiceHeader.JZ_BondPackCount > 0)
				{
					if (EntryHeader.Nature.Contains("10"))
					{
						factor = (decimal)invoiceHeader.JZ_Nature10PackCount / (invoiceHeader.JZ_Nature10PackCount + invoiceHeader.JZ_BondPackCount);
					}
					else
					{
						factor = (decimal)invoiceHeader.JZ_BondPackCount / (invoiceHeader.JZ_Nature10PackCount + invoiceHeader.JZ_BondPackCount);
					}
				}
			}
			return factor;
		}
	}
}
