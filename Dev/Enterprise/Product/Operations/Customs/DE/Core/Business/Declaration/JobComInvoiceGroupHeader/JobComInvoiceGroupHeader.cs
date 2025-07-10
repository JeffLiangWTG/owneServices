using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceGroupHeader : EU.Business.Declaration.JobComInvoiceGroupHeader
		, Integration.Customs.DE.IJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		public new GroupInvoiceChargeCollection<GroupInvoiceCharge> Charges => (GroupInvoiceChargeCollection<GroupInvoiceCharge>)base.Charges;

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection() => new GroupInvoiceChargeCollection<GroupInvoiceCharge>(this);

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();
	}
}
