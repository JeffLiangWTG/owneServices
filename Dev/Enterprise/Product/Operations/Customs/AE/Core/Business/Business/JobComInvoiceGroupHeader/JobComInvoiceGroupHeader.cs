using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class JobComInvoiceGroupHeader : BaseJobComInvoiceGroupHeader, Integration.Customs.AE.IJobComInvoiceGroupHeader
{
	public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

	public new JobComInvoiceGroupHeaderLookups Lookups
	{
		get
		{
			if (fLookups == null)
			{
				fLookups = new JobComInvoiceGroupHeaderLookups(this);
			}
			return fLookups;
		}
	}

	JobComInvoiceGroupHeaderLookups fLookups;

	public new JobComInvoiceGroupHeaderValidation Validation => new JobComInvoiceGroupHeaderValidation(this);
}
