using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobComInvoiceHeaderDocAddressDependentCollection : JobDocAddressDependentCollection
{
	public JobComInvoiceHeaderDocAddressDependentCollection(JobComInvoiceHeader invoice) : base(invoice)
	{
	}

	public new JobComInvoiceHeaderDocAddress this[int index] => (JobComInvoiceHeaderDocAddress)base[index];

	public new JobComInvoiceHeaderDocAddress AddNew() => (JobComInvoiceHeaderDocAddress)base.AddNew();

	public new JobComInvoiceHeaderDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement) => (JobComInvoiceHeaderDocAddress)base.FindOrCreateWithRequirement(requirement);
}
