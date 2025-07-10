using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public abstract class TypeSafeInvoiceLineCharge : AutoInvoiceLineCharge
	{
		protected TypeSafeInvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new InvoiceLineCharge Clone()
		{
			return (InvoiceLineCharge)base.Clone();
		}

		public new InvoiceLineChargeValidation Validation
		{
			get { return (InvoiceLineChargeValidation)base.Validation; }
		}

		public new InvoiceLineChargeLookups Lookups
		{
			get { return (InvoiceLineChargeLookups)base.Lookups; }
		}

		#endregion
	}
}
