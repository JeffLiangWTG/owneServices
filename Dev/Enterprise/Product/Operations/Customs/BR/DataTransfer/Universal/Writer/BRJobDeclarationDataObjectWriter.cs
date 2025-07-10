using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BRJobDeclarationDataObjectWriter : DeclarationDataObjectWriter
	{
		public BRJobDeclarationDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			return new BRDataObjectWriterHelper(declarationBO.Factory);
		}

		protected override CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(CusEntryHeader relatedEntry)
		{
			return new BRInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriter()
		{
			return new BREntryInstructionDataObjectWriter(writeManager, helper);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceHeaderRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoicePK = row.GetValue(JobComInvoiceHeaderSchema.PK);
			yield return new FetchHint(JobDocAddressSchema.E2_ParentID, invoicePK);
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
	}
}
