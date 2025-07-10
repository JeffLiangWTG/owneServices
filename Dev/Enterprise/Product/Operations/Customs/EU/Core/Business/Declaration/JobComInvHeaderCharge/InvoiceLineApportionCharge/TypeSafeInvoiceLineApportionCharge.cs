using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeInvoiceLineApportionCharge : AutoInvoiceLineApportionCharge
	{
		protected TypeSafeInvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		InvoiceLineApportionCharge invoiceLineApportionCharge
		{
			get { return (InvoiceLineApportionCharge)this; }
		}

		public new InvoiceLineApportionChargeValidation Validation
		{
			get { return (InvoiceLineApportionChargeValidation)base.Validation; }
		}

		public new InvoiceLineApportionChargeLookups Lookups
		{
			get { return (InvoiceLineApportionChargeLookups)base.Lookups; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineApportionChargeValidation(invoiceLineApportionCharge);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineApportionChargeLookups(invoiceLineApportionCharge);
		}
	}
}
