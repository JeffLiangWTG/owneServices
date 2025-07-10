using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class LADTCusTempStorageLineCollection : CusTempStorageLineCollection<LADTCusTempStorageLine, LADTCusTempStorageDec>
	{
		public LADTCusTempStorageLineCollection(LADTCusTempStorageDec parentStorageDec) : base(parentStorageDec)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((LADTCusTempStorageLine)child).TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}
	}
}
