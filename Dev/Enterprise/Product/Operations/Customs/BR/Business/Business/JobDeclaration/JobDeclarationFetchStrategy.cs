using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return hint;
			}

			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(CusLineTariffDetailSchema.BZ_ParentID, invoiceLinePK);
			yield return new FetchHint(JobDocAddressSchema.E2_ParentID, invoiceLinePK);
		}

		protected override IEnumerable<IFetchHint> GetCusEntryHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusEntryHeaderRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var entryHeaderPK = row.GetValue(CusEntryHeaderSchema.PK);
			yield return new FetchHint(EDIMessageSchema.EM_LinkUniqueID, entryHeaderPK);
		}

		protected override void AddMergeFetchHintsFor(BaseJobComInvoiceHeader invoice)
		{
			base.AddMergeFetchHintsFor(invoice);
			if (invoice.JobDeclaration.IsImport)
			{
				Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, invoice.PK);
			}
		}

		protected override void AddMergeFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			if (invoiceLine.Declaration.IsImport)
			{
				Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK);
				Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_ClusterKey, BusinessObject.JE_ClusterKey);
			}
		}
	}
}
