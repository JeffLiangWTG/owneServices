using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CusTempStorageDec")]

	public class FRCCusTempStorageDec : CusTempStorageDec
	{
		public FRCCusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = FRConstants.TemporaryStorage.AppCodeFRC;
		}

		public static FRCCusTempStorageDec New(CusTempStorageJobHeader parent)
		{
			var result = parent.Factory.New<FRCCusTempStorageDec>();
			using (result.SuspendSettingHasChanges())
			{
				result.STH_SJH = parent.PK;
			}
			return result;
		}

		public static FRCCusTempStorageDec Load(CusTempStorageJobHeader parent)
		{
			return CusTempStorageDec.Load<FRCCusTempStorageDec>(parent);
		}

		#region CusTempStorageLines

		public new FRCCusTempStorageLineCollection CusTempStorageLines => (FRCCusTempStorageLineCollection)base.CusTempStorageLines;

		protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new FRCCusTempStorageLineCollection(this);

		#endregion

	}
}
