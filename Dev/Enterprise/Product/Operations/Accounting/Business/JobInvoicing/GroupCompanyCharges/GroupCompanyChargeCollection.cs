using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Used only for binding to grid in DebtorsAcceptGroupChargesForm.
	/// User can't modify the collection or its elements.
	/// </summary>
	public class GroupCompanyChargeCollection : NonPersistentBusinessObjectCollection<GroupCompanyCharge>
	{
		public GroupCompanyChargeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		/// <summary>
		/// This collection is read only and should not be used to generate Group Company Charges
		/// </summary>
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
