using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageDecLookups : EU.Business.CusTempStorage.CusTempStorageDecLookups
	{
		public CusTempStorageDecLookups(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		public IBusinessObjectCollection CusTempStorageRegLineCollection
		{
			get
			{
				var customsOffice = Parent?.StorageHeader?.SJH_CustomsOffice ?? ZString.Empty;
				return TemporaryStorageHelper.GetCusTempStorageRegLineCollection(customsOffice, Factory);
			}
		}

		new CusTempStorageDec Parent => (CusTempStorageDec)base.Parent;
	}
}
