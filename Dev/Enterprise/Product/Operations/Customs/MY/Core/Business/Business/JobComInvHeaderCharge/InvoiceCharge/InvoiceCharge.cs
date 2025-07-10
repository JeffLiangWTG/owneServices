using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class InvoiceCharge : TypeSafeInvoiceCharge, Integration.Customs.MY.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceChargeValidation(this);
		}

		#endregion

		#endregion
	}
}
