using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeJobComInvoiceHeader : AutoJobComInvoiceHeader
	{
		#region Constructor

		protected TypeSafeJobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public new JobComInvoiceHeaderLookups Lookups
		{
			get { return (JobComInvoiceHeaderLookups)base.Lookups; }
		}

		public new JobComInvoiceHeaderValidation Validation
		{
			get { return (JobComInvoiceHeaderValidation)base.Validation; }
		}

		public new JobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
		}

		[ChildEditable(true)]
		public new IJobComInvChargeCollection<InvoiceCharge> Charges
		{
			get { return (IJobComInvChargeCollection<InvoiceCharge>)base.Charges; }
		}

		[ChildEditable(true)]
		public new IJobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (IJobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

		public new JobComInvoiceGroupHeader Master
		{
			get { return (JobComInvoiceGroupHeader)base.Master; }
		}

		public new JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.GroupHeader; }
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)this; }
		}

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceHeaderLookups(InvoiceHeader);
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			if (JobDeclaration != null)
			{
				return new JobComInvoiceLineViewCollection(InvoiceHeader, JobDeclaration.InvoiceLines);
			}
			return null;
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new InvoiceChargeCollection<InvoiceCharge>(InvoiceHeader);
		}

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(InvoiceHeader);
		}

		#endregion

		#endregion
	}
}
