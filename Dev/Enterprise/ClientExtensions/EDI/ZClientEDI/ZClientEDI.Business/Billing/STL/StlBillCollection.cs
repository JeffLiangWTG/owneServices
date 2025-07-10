using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlBillCollection : NonPersistentBusinessObjectCollection<StlBill>
	{
		public StlBillCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StlBill(Factory, GlbBranch.CurrentBranch, Factory.New<EDIOrgHeader>(), "", new ZDateTime(2015, 7, 1), ZDateTime.Today);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

