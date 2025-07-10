using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusStorageDocPivotCollection : CusStorageDocPivotCollection<CusStorageDocPivot, BusinessObject>
	{
		public CusStorageDocPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new ICusStorageDocPivotParent Master => (ICusStorageDocPivotParent)base.Master;

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			var child1 = (CusStorageDocPivot)child;
			child1.Parent = Master as BusinessObject;
		}
	}
}
