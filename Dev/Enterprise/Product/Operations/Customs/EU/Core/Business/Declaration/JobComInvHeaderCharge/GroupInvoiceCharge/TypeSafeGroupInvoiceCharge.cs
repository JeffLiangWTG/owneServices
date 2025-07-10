using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeGroupInvoiceCharge : AutoGroupInvoiceCharge
	{
		#region Constructor

		protected TypeSafeGroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new GroupInvoiceChargeLookups Lookups
		{
			get { return (GroupInvoiceChargeLookups)base.Lookups; }
		}

		public new GroupInvoiceChargeValidation Validation
		{
			get { return (GroupInvoiceChargeValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		GroupInvoiceCharge groupInvoiceCharge
		{
			get { return (GroupInvoiceCharge)this; }
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new GroupInvoiceChargeLookups(groupInvoiceCharge);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new GroupInvoiceChargeValidation(groupInvoiceCharge);
		}

		#endregion

		#endregion
	}
}
