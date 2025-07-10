using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobComInvoiceGroupHeader : EU.Business.Declaration.JobComInvoiceGroupHeader, Integration.Customs.IT.IJobComInvoiceGroupHeader
{
	public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);
}
