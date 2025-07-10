using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class ISTCusTempStorageLineCollection : CusTempStorageLineCollection<ISTCusTempStorageLine, ISTCusTempStorageDec>
	{
		public ISTCusTempStorageLineCollection(ISTCusTempStorageDec parentStorageDec) : base(parentStorageDec)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((ISTCusTempStorageLine)child).TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}
	}
}
