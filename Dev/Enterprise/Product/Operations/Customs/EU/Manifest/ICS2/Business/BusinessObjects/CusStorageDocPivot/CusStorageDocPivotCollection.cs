using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CusStorageDocPivotCollection : CusStorageDocPivotCollection<CusStorageDocPivot, BusinessObject>
	{
		public CusStorageDocPivotCollection(BusinessObject master) : base(master)
		{
		}
	}
}
