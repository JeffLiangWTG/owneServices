using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BlanketVesselChangeBillCollection : NonPersistentBusinessObjectCollection<BlanketVesselChangeBill>
	{
		public BlanketVesselChangeBillCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
