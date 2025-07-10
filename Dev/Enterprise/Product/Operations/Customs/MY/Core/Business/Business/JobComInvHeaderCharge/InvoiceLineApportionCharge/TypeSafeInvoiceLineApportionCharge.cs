using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public abstract class TypeSafeInvoiceLineApportionCharge : AutoInvoiceLineApportionCharge
	{
		protected TypeSafeInvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new InvoiceLineApportionCharge Clone()
		{
			return (InvoiceLineApportionCharge)base.Clone();
		}

		public new InvoiceLineApportionChargeValidation Validation
		{
			get { return (InvoiceLineApportionChargeValidation)base.Validation; }
		}

		public new InvoiceLineApportionChargeLookups Lookups
		{
			get { return (InvoiceLineApportionChargeLookups)base.Lookups; }
		}

		#endregion
	}
}
