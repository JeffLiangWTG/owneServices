using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageContainer : CusCodeData
	{
		public CusTempStorageContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.TempStorageContainer;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusTempStorageDec));

		public new CusTempStorageContainerLookups Lookups => (CusTempStorageContainerLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups() => new CusTempStorageContainerLookups(this);

		[List(nameof(Lookups) + "." + nameof(CusTempStorageContainerLookups.CY_CodeList))]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}
	}
}
