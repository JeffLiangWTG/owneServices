using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class C88CreationStrategy : EU.Business.Declaration.EntryCreationStrategy
	{
		public C88CreationStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
		{
			MergeKey result = base.GetKeyForLine(invoiceLine);
			JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;
			result.Add(line.JI_OA_SupervisingOffice);
			result.Add(line.JI_FecQV1_NettMass);
			result.Add(line.JI_FecQV2_Supp);
			result.Add(line.JI_FecORG);
			var fiscalReferences = line.FiscalReferences.Cast<CusFiscalReference>()
				.Select(fr => fr.CFR_Reference.ToUpper())
				.OrderBy(fr => fr)
				.ToArray();
			foreach (var fr in fiscalReferences)
			{
				result.Add(fr);
			}
			return result;
		}

		protected override string[] GetSupportingDocumentKeysCore()
		{
			return new string[]
			{
				SupportingDocument.Schema.CSI_Actions,
				SupportingDocument.Schema.CSI_Availability
			}.Concat(base.GetSupportingDocumentKeysCore()).ToArray();
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForHeaderCore(invoiceLine);
			var line = (JobComInvoiceLine)invoiceLine;
			result.Add(line.InvoiceHeader.ZG_HouseSplitReference);
			result.Add(line.InvoiceHeader.ZG_CustomsAuthorisationReferenceForExportFallback);
			return result;
		}
	}
}
