using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public abstract class TypeSafeInvoiceApportionCharge : AutoInvoiceApportionCharge
	{
		protected TypeSafeInvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new InvoiceApportionCharge Clone()
		{
			return (InvoiceApportionCharge)base.Clone();
		}

		public new InvoiceApportionChargeValidation Validation
		{
			get { return (InvoiceApportionChargeValidation)base.Validation; }
		}

		public new InvoiceApportionChargeLookups Lookups
		{
			get { return (InvoiceApportionChargeLookups)base.Lookups; }
		}

		#endregion
	}
}
