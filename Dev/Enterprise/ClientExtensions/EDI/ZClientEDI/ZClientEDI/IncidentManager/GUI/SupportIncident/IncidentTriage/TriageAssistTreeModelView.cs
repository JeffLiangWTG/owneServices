using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class TriageAssistTreeModelView : ZTreeModelView<TriageAssistTreeBizObjWrapper>
	{
		public TriageAssistTreeModelView(TriageAssistBusinessObject triageAssist) : base(new TriageAssistTreeModel(triageAssist))
		{
		}

		public void Rebuild()
		{
			InnerModel.Rebuild();
			RefreshView();
		}

		new TriageAssistTreeModel InnerModel => (TriageAssistTreeModel)base.InnerModel;

		public class TriageAssistTreeModel : ZTreeModel<TriageAssistTreeBizObjWrapper>
		{
			public TriageAssistTreeModel(TriageAssistBusinessObject triageAssist) : base(triageAssist.Factory)
			{
				Parent = triageAssist;
			}

			public void Rebuild()
			{
				TopNodes.Clear();
				foreach (var wrapper in Parent.FilteredTriageAssistTreeWrapperCollection.OfType<TriageAssistTreeBizObjWrapper>())
				{
					var node = new TriageAssistTreeNode(this, wrapper);
					TopNodes.Add(node);
				}
			}

			public TriageAssistBusinessObject Parent { get; }

			readonly ZNodeCollection<TriageAssistTreeBizObjWrapper> TopNodes = new ZNodeCollection<TriageAssistTreeBizObjWrapper>();

			protected override ZNode<TriageAssistTreeBizObjWrapper> CreateNewNodeCore(ZTreeModel<TriageAssistTreeBizObjWrapper> treeModel, TriageAssistTreeBizObjWrapper bizObj)
				=> new TriageAssistTreeNode(treeModel, bizObj);

			protected override ZNodeCollection<TriageAssistTreeBizObjWrapper> GetRootNodes() => TopNodes;
		}

		class TriageAssistTreeNode : ZNode<TriageAssistTreeBizObjWrapper>
		{
			public TriageAssistTreeNode(ZTreeModel<TriageAssistTreeBizObjWrapper> treeModel, TriageAssistTreeBizObjWrapper bizObj) : base(treeModel, bizObj)
			{
			}

			protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(TriageAssistTreeBizObjWrapper previousParent, TriageAssistTreeBizObjWrapper newParent, bool checkValid)
			{
				throw new NotImplementedException();
			}

			protected override IEnumerable<TriageAssistTreeBizObjWrapper> LoadChildBizObjs() => BizObj.Children;

			protected override TriageAssistTreeBizObjWrapper LoadParentBizObj()
			{
				throw new NotImplementedException();
			}

			public ZString Description => BizObj.Description;
			public ZString Type => BizObj.Type;
			public ZString Status => BizObj.Status;
			public ZString Product => BizObj.Product;
			public ZString Area => BizObj.Area;
			public ZString Section => BizObj.Section;
			public bool IsFocused => BizObj.IsFocused;
		}
	}
}
