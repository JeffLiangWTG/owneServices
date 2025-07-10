using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DK.Business.Declaration
{
	public class JobComInvoiceGroupHeader : EU.Business.Declaration.JobComInvoiceGroupHeader
		, Integration.Customs.DK.IJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);
	}
}
