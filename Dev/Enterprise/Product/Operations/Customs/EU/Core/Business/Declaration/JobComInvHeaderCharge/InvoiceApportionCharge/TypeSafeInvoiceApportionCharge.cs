using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeInvoiceApportionCharge : AutoInvoiceApportionCharge
	{
		protected TypeSafeInvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		InvoiceApportionCharge invoiceApportionCharge
		{
			get { return (InvoiceApportionCharge)this; }
		}

		public new InvoiceApportionChargeValidation Validation
		{
			get { return (InvoiceApportionChargeValidation)base.Validation; }
		}

		public new InvoiceApportionChargeLookups Lookups
		{
			get { return (InvoiceApportionChargeLookups)base.Lookups; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceApportionChargeValidation(invoiceApportionCharge);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceApportionChargeLookups(invoiceApportionCharge);
		}
	}
}
