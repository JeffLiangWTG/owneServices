using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CusTempStorageDec")]

	public class LADTCusTempStorageDec : CusTempStorageDec
	{
		public LADTCusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = FRConstants.TemporaryStorage.AppCodeLAD;
		}

		public new LADTCusTempStorageDecValidation Validation => (LADTCusTempStorageDecValidation)base.Validation;

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation()
		{
			return new LADTCusTempStorageDecValidation(this);
		}

		#region Properties

		#endregion

		#region CusTempStorageLines

		public new LADTCusTempStorageLineCollection CusTempStorageLines => (LADTCusTempStorageLineCollection)base.CusTempStorageLines;

		protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new LADTCusTempStorageLineCollection(this);

		#endregion

		public static LADTCusTempStorageDec New(CusTempStorageJobHeader parent)
		{
			var result = parent.Factory.New<LADTCusTempStorageDec>();
			using (result.SuspendSettingHasChanges())
			{
				result.STH_SJH = parent.PK;
			}
			return result;
		}

		public static LADTCusTempStorageDec Load(CusTempStorageJobHeader parent)
		{
			return CusTempStorageDec.Load<LADTCusTempStorageDec>(parent);
		}
	}
}
