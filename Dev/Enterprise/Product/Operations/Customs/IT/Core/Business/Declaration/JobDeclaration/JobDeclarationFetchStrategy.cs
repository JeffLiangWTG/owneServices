using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
{
	public JobDeclarationFetchStrategy(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	protected override void FetchForMergeCore()
	{
		try
		{
			isImport = BusinessObject.IsImport;
			base.FetchForMergeCore();
		}
		finally
		{
			isImport = false;
		}
	}
	bool isImport;

	protected override void AddMergeFetchHintsFor(BaseJobComInvoiceHeader invoice)
	{
		base.AddMergeFetchHintsFor(invoice);
		Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, invoice.PK);
	}

	protected override void AddMergeFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
	{
		base.AddMergeFetchHintsFor(invoiceLine);
		Factory.AddFetchHint(CusHouseContPackInvoiceLinePivotSchema.CHC_JI, invoiceLine.PK);
		Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, invoiceLine.PK);
		Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK);
		Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_JI, invoiceLine.PK);
		if (isImport)
		{
			Factory.AddFetchHint(CusAuthorizationUsageSchema.AGC_ParentID, invoiceLine.PK);
			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, invoiceLine.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, invoiceLine.PK);
		}
	}
}
