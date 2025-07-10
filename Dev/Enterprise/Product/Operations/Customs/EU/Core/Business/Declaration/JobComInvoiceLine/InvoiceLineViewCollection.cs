using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceLineViewCollection<T> : Customs.Business.InvoiceLineViewCollection<T> where T : JobComInvoiceLine
	{
		public InvoiceLineViewCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new FetchStrategies.InvoiceLineViewCollectionFetchStrategy(this);
		}

		protected override void CopyLastLineDetailsToNewLinesIfEnabled(T newLine, T previousLine)
		{
			base.CopyLastLineDetailsToNewLinesIfEnabled(newLine, previousLine);
			if (CopyLastLineDetailsToNewLines)
			{
				foreach (MultiLineAddInfos.SupportingDocument supportingDocument in previousLine.SupportingDocuments.ToArray())
				{
					newLine.SupportingDocuments.Add(supportingDocument.Clone());
				}
				foreach (MultiLineAddInfos.AdditionalInfo additionalInfo in previousLine.AdditionalInfos.ToArray())
				{
					newLine.AdditionalInfos.Add(additionalInfo.Clone());
				}
				foreach (MultiLineAddInfos.PreviousDocument previousDocument in previousLine.PreviousDocuments.ToArray())
				{
					newLine.PreviousDocuments.Add(previousDocument.Clone());
				}

				foreach (JobComInvoiceLineTax tax in previousLine.Taxes.ToArray())
				{
					newLine.Taxes.Add(tax.Clone());
				}
			}
		}
	}
}
