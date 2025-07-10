using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public class DocLandedCostingExchangeRateCollection : NonPersistentBusinessObjectCollection<DocLandedCostingExchangeRate>
	{
		public DocLandedCostingExchangeRateCollection(BusinessObjectFactory factory)
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
