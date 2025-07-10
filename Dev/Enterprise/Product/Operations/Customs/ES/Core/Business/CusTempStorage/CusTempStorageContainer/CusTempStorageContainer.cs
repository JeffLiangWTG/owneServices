using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageContainer : CusCodeData
	{
		public CusTempStorageContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		const string TempStorageContainerCode = "TSC";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = TempStorageContainerCode;
		}

		public new CusTempStorageDec Parent => (CusTempStorageDec)base.Parent;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusTempStorageDec));
	}
}
