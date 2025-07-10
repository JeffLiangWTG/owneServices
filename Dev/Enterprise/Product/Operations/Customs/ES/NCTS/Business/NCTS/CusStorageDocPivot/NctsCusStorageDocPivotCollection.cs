using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsCusStorageDocPivotCollection : CusStorageDocPivotCollection<NctsCusStorageDocPivot, BusinessObject>
	{
		public NctsCusStorageDocPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new INctsCusStorageDocPivotParent Master => (INctsCusStorageDocPivotParent)base.Master;

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			var nctsChild = (NctsCusStorageDocPivot)child;
			nctsChild.Parent = Master as BusinessObject;
		}
	}
}
