using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyGenPivotNode : ZNode<DummyBusinessObject>
	{
		public DummyGenPivotNode(ZTreeModel<DummyBusinessObject> treeModel, DummyBusinessObject bizObj)
			: base(treeModel, bizObj)
		{
		}

		protected override DummyBusinessObject LoadParentBizObj()
		{
			var pivot = BizObj.Factory.LoadTop1<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation2ID, BizObj.PK));
			return pivot != null ? BizObj.Factory.Load<DummyBusinessObject>(pivot.XX_Relation1ID) : null;
		}

		protected override IEnumerable<DummyBusinessObject> LoadChildBizObjs()
		{
			var pivots = BizObj.Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, BizObj.PK));
			return pivots.Select(x => BizObj.Factory.Load<DummyBusinessObject>(x.XX_Relation2ID)).Where(x => x != null);
		}

		public ChangeParentOnBizObjResult? ChangeParentOnBizObjOverrideForTesting;
		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(DummyBusinessObject previousParent, DummyBusinessObject newParent, bool checkValid)
		{
			var result = ChangeParentOnBizObjOverrideForTesting ?? new ChangeParentOnBizObjResult(true, newParent);

			if (result.Success)
			{
				foreach (var pivot in BizObj.Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation2ID, BizObj.PK)))
				{
					pivot.Delete();
				}

				if (result.NewParent != null)
				{
					var newPivot = BizObj.Factory.New<GenPivot>();
					newPivot.XX_Relation1ID = result.NewParent.PK;
					newPivot.XX_Relation2ID = BizObj.PK;
				}
			}

			return result;
		}

		public ZNode<DummyBusinessObject> GetParentNodeWithoutRefresh()
		{
			return parentNode;
		}

		public HashSet<ZNode<DummyBusinessObject>> GetChildNodeCollectionWithoutRefresh()
		{
			return childNodeCollection;
		}
	}
}
