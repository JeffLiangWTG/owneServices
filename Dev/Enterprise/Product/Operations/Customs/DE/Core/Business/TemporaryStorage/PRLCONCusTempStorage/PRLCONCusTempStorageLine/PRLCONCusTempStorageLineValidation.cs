using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public abstract class PRLCONCusTempStorageLineValidation : CusTempStorageLineValidation
	{
		protected PRLCONCusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		protected override void CheckTSL_PackageQty()
		{
			base.CheckTSL_PackageQty();
			CheckPackageQtyIsBetween1And99999();
		}
	}
}
