using System.Collections.Generic;
using System.Data;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryLine : EU.Business.Declaration.CusEntryLine, Integration.Customs.NL.ICusEntryLine
{
	public CusEntryLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobComInvoiceLine RandomLine => (JobComInvoiceLine)base.RandomLine;

	public new JobComInvoiceLine FirstLine => (JobComInvoiceLine)base.FirstLine;

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees => (CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees;

	protected override Customs.Business.ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);

	protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetPreviousDocumentsToProcess()
	{
		foreach (var invoiceLine in InvoiceLines.Cast<JobComInvoiceLine>())
		{
			foreach (var document in invoiceLine.PreviousDocuments)
			{
				yield return document;
			}
		}
	}

	public new IEnumerable<AdditionalInfo> AdditionalInfos => base.AdditionalInfos.Cast<AdditionalInfo>();

	protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> AdditionalInfosCore
	{
		get
		{
			if (Declaration != null)
			{
				var addInfosToReturn = Declaration.Invoices.Cast<JobComInvoiceHeader>()
					.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>())
					.Union(InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.EffectiveAdditionalInfos()));
				return addInfosToReturn.DistinctBy(GetAdditionalInfoDistinctKey);
			}
			else
			{
				return base.AdditionalInfosCore;
			}
		}
	}
}
