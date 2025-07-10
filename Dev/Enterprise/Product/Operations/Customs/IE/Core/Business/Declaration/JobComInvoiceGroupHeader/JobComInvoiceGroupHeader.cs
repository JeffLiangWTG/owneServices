using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobComInvoiceGroupHeader : EU.Business.Declaration.JobComInvoiceGroupHeader
		, Integration.Customs.IE.IJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection() => new GroupInvoiceChargeCollection<GroupInvoiceCharge>(this);

		[ChildEditable(true)]
		public new IJobComInvChargeCollection<GroupInvoiceCharge> Charges => (IJobComInvChargeCollection<GroupInvoiceCharge>)base.Charges;

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();
	}
}
