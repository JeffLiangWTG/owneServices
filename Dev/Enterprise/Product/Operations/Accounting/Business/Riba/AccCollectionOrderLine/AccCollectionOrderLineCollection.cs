using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderLineCollection : ActiveBusinessObjectCollection<AccCollectionOrderLine>
	{
		public AccCollectionOrderLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public AccCollectionOrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}
	}
}

