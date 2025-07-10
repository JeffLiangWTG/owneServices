using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeInvoiceLineCharge : AutoInvoiceLineCharge
	{
		protected TypeSafeInvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		InvoiceLineCharge InvoiceLineCharge => (InvoiceLineCharge)this;

		public new InvoiceLineChargeValidation Validation
		{
			get { return (InvoiceLineChargeValidation)base.Validation; }
		}

		public new InvoiceLineChargeLookups Lookups
		{
			get { return (InvoiceLineChargeLookups)base.Lookups; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineChargeValidation(InvoiceLineCharge);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineChargeLookups(InvoiceLineCharge);
		}
	}
}
