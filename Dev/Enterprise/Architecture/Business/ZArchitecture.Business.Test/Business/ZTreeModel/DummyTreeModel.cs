using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyTreeModel : ZTreeModel<DummyBusinessObject>
	{
		public DummyTreeModel(BusinessObjectFactory factory, IEnumerable<DummyBusinessObject> rootBizObjs)
			: base(factory)
		{
			if (!rootBizObjs.Any())
			{
				throw new ArgumentException("Must contain at least 1 root BizObj");
			}

			nodeCollection = new ZNodeCollection<DummyBusinessObject>();
			foreach (var bizObj in rootBizObjs)
			{
				nodeCollection.Add(CreateNewNode(bizObj));
			}
		}

		protected override ZNodeCollection<DummyBusinessObject> GetRootNodes()
		{
			return nodeCollection;
		}
		readonly ZNodeCollection<DummyBusinessObject> nodeCollection;

		protected override ZNode<DummyBusinessObject> CreateNewNodeCore(ZTreeModel<DummyBusinessObject> treeModel, DummyBusinessObject bizObj)
		{
			return new DummyGenPivotNode(treeModel, bizObj);
		}
	}
}
