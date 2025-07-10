using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderCollection : ActiveBusinessObjectCollection<AccCollectionOrder>
	{
		public AccCollectionOrderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public AccCollectionOrderCollection(BusinessObjectFactory factory)
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

