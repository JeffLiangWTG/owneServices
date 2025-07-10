using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeJobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader
	{
		#region Constructor

		protected TypeSafeJobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

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
		public new IJobComInvChargeCollection<GroupInvoiceCharge> Charges
		{
			get { return (IJobComInvChargeCollection<GroupInvoiceCharge>)base.Charges; }
		}

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		JobComInvoiceGroupHeader ThisInvoiceGroupHeader
		{
			get { return (JobComInvoiceGroupHeader)this; }
		}

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection()
		{
			return new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(ThisInvoiceGroupHeader);
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(ThisInvoiceGroupHeader, true);
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			return new JobComInvoiceGroupHeaderValidation(ThisInvoiceGroupHeader);
		}

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceGroupHeaderLookups(ThisInvoiceGroupHeader);
		}

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection()
		{
			return new GroupInvoiceChargeCollection<GroupInvoiceCharge>(ThisInvoiceGroupHeader);
		}

		#endregion

		#endregion
	}
}
