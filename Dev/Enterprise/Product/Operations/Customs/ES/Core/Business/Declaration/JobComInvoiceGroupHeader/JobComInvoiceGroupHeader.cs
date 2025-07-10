using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public partial class JobComInvoiceGroupHeader : EU.Business.Declaration.JobComInvoiceGroupHeader, Integration.Customs.ES.IJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[LightValidationTestExempt]
		public override ZBool JZ_GroupInvoice { get => base.JZ_GroupInvoice; set => base.JZ_GroupInvoice = value; }

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		public new GroupInvoiceChargeCollection<GroupInvoiceCharge> Charges => (GroupInvoiceChargeCollection<GroupInvoiceCharge>)base.Charges;

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection() => new GroupInvoiceChargeCollection<GroupInvoiceCharge>(this);

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();
		}
	}
}
