using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(LADTCusTempStorageDec), "CusTempStorageLines")]

	public class LADTCusTempStorageLine : CusTempStorageLine
	{
		public LADTCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Dec

		public new LADTCusTempStorageDec Dec => Factory.Load<LADTCusTempStorageDec>(TSL_STH);

		#endregion

		public new LADTCusTempStorageLineValidation Validation => (LADTCusTempStorageLineValidation)base.Validation;

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation()
		{
			return new LADTCusTempStorageLineValidation(this);
		}
	}
}
