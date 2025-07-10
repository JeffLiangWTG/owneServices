
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobComInvoiceGroupHeader
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceGroupHeader Clone()
		{
			return (JobComInvoiceGroupHeader)base.Clone();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public new JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.GroupHeader; }
		}

		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders
		{
			get { return (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders; }
		}

		public new JobComInvoiceGroupHeaderValidation Validation
		{
			get { return (JobComInvoiceGroupHeaderValidation)base.Validation; }
		}

		public new JobComInvoiceGroupHeaderLookups Lookups
		{
			get { return (JobComInvoiceGroupHeaderLookups)base.Lookups; }
		}

		[ChildEditable(true)]
		public new JobComInvChargeCollection<GroupInvoiceCharge> Charges
		{
			get { return (JobComInvChargeCollection<GroupInvoiceCharge>)base.Charges; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this, true);
		}

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection()
		{
			return new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			return new JobComInvoiceGroupHeaderValidation(this);
		}

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceGroupHeaderLookups(this);
		}

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection()
		{
			return new JobComInvChargeCollection<GroupInvoiceCharge>(this);
		}

		#endregion

		#endregion
	}
}
