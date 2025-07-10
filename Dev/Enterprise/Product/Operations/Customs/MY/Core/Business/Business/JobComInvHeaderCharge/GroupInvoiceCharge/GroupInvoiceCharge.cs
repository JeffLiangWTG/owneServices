using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class GroupInvoiceCharge : TypeSafeGroupInvoiceCharge, Integration.Customs.MY.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new GroupInvoiceChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new GroupInvoiceChargeValidation(this);
		}

		#endregion

		#endregion
	}
}
