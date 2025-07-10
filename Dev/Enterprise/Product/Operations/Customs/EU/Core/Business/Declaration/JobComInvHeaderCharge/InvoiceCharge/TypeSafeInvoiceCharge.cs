using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeInvoiceCharge : AutoInvoiceCharge
	{
		#region Constructor

		protected TypeSafeInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new InvoiceChargeLookups Lookups
		{
			get { return (InvoiceChargeLookups)base.Lookups; }
		}

		public new InvoiceChargeValidation Validation
		{
			get { return (InvoiceChargeValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		InvoiceCharge InvoiceCharge
		{
			get { return (InvoiceCharge)this; }
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceChargeLookups(InvoiceCharge);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceChargeValidation(InvoiceCharge);
		}

		#endregion

		#endregion
	}
}
