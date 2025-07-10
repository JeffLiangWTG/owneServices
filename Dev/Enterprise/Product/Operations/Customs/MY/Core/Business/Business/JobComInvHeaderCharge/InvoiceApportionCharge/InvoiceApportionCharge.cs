using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class InvoiceApportionCharge : TypeSafeInvoiceApportionCharge, Integration.Customs.MY.IInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceApportionChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceApportionChargeLookups(this);
		}

		#endregion

		#endregion
	}
}
