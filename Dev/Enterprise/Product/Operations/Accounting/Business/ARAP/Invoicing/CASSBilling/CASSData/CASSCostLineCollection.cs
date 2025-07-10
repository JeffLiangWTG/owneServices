using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class CASSCostLineCollection<T> : NonPersistentBusinessObjectCollection<T>
		where T : CASSCostLine
	{
		protected CASSCostLineCollection()
			: base()
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException("Adding new lines is not supported by this CASSCostLineCollection. Create new Line and use Add method.");
		}
	}
}
