using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(FRCCusTempStorageDec), "CusTempStorageLines")]

	public class FRCCusTempStorageLine : CusTempStorageLine
	{
		public FRCCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Dec

		public new FRCCusTempStorageDec Dec => Factory.Load<FRCCusTempStorageDec>(TSL_STH);

		#endregion
	}
}
