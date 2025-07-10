using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class InvoiceLineCharge : TypeSafeInvoiceLineCharge, Integration.Customs.MY.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineChargeLookups(this);
		}

		#endregion

		#endregion
	}
}
