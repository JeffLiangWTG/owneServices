using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.DataTransfer;

class DeclarationDataObjectWriter : Customs.DataTransfer.Universal.DeclarationDataObjectWriter
{
	public DeclarationDataObjectWriter(IDataWritingManager manager) : base(manager)
	{
	}

	protected override Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriter()
	{
		return new CustomsEntryInstructionDataObjectWriter(writeManager, helper);
	}

	protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
	{
		return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
	}

	protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
	{
		foreach (var fetchHint in base.GetJobComInvoiceLineRelatedFetchHints(row))
		{
			yield return fetchHint;
		}

		var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
		yield return new FetchHint(CusLineTariffDetailSchema.BZ_ParentID, invoiceLinePK);
	}
}
