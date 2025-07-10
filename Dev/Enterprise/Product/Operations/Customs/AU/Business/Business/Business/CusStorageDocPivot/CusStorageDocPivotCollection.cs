using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusStorageDocPivotCollection : CusStorageDocPivotCollection<CusStorageDocPivot, BusinessObject>
	{
		public CusStorageDocPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new ICusStorageDocPivotParent Master => (ICusStorageDocPivotParent)base.Master;

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (CusStorageDocPivot)dependent;
			child.Parent = Master as BusinessObject;
		}
	}
}
