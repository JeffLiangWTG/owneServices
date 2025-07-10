using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class InvoiceLineApportionCharge : TypeSafeInvoiceLineApportionCharge, Integration.Customs.MY.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineApportionChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineApportionChargeLookups(this);
		}

		#endregion

		#endregion
	}
}
