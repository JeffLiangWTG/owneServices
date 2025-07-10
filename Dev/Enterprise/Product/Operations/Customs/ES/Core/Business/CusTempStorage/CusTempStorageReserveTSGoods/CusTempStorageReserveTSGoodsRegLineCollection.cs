using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageReserveTSGoodsRegLineCollection : NonPersistentBusinessObjectCollection<CusTempStorageReserveTSGoodsRegLine>
{
	protected override bool AllowNewCore => false;
	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException("This method should not be called.");
}
