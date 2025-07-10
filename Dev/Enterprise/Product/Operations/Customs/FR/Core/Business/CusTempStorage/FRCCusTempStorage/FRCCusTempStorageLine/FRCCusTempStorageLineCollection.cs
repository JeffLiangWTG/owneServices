using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class FRCCusTempStorageLineCollection : CusTempStorageLineCollection<FRCCusTempStorageLine, FRCCusTempStorageDec>
	{
		public FRCCusTempStorageLineCollection(FRCCusTempStorageDec parentStorageDec)
			: base(parentStorageDec)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((FRCCusTempStorageLine)child).TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}
	}
}
