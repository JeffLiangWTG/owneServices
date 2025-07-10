using System;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectFactoryStatisticCollection : NonPersistentBusinessObjectCollection<BusinessObjectFactoryStatistic>
	{
		public BusinessObjectFactoryStatisticCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
