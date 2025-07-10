using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusTempStorageSelectableRegLineCollection : NonPersistentBusinessObjectCollection<CusTempStorageSelectableRegLine>
	{
		public CusTempStorageSelectableRegLineCollection()
		{
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("This method should not be called.");
		}
	}
}
