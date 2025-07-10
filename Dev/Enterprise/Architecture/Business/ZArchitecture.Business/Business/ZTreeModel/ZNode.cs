using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IZNode
	{
		void RemoveParent();
		IBusiness BizObjForBinding { get; }
	}

	public abstract class ZNode<T> : IZNode
		where T : class, IBusiness
	{
		protected ZNode(ZTreeModel<T> treeModel, T bizObj)
		{
			TreeModel = treeModel;
			BizObj = bizObj;
		}

		public readonly ZTreeModel<T> TreeModel;
		public readonly T BizObj;

		public virtual T BizObjForBinding
		{
			get { return BizObj; }
		}

		#region ParentNode

		public ZNode<T> ParentNode
		{
			get
			{
				if (!TreeModel.IsNodeRefreshSuspended && parentNodeRequiresRefresh)
				{
					RefreshParentNode();
				}
				return parentNode;
			}
			set
			{
				SetParentNode(value, true);
			}
		}
		bool parentNodeRequiresRefresh = true;
		protected ZNode<T> parentNode;

		void RefreshParentNode()
		{
			parentNodeRequiresRefresh = false;

			var originalParentNode = parentNode;
			var loadedParentBizOj = LoadParentBizObj();
			if (loadedParentBizOj != null)
			{
				using (TreeModel.GetNodeRefreshSuspender())
				{
					// Do not add parent node if there is already a child node for same BizObj (to prevent infinite loop)
					if (FindDescendantNode(loadedParentBizOj) != null)
					{
						loadedParentBizOj = null;
					}
				}
			}

			if (loadedParentBizOj == null)
			{
				if (originalParentNode != null)
				{
					originalParentNode.childNodeCollection.Remove(this);

					parentNode = null;
				}
			}
			else if (originalParentNode == null || originalParentNode.BizObj != loadedParentBizOj)
			{
				if (originalParentNode != null)
				{
					originalParentNode.childNodeCollection.Remove(this);
				}

				parentNode = TreeModel.CreateNewNode(loadedParentBizOj);
				parentNode.childNodeCollection.Add(this);
			}
			else if (!originalParentNode.childNodeCollection.Contains(this))
			{
				originalParentNode.childNodeCollection.Add(this);
			}
		}

		public bool SetParentNode(ZNode<T> node, bool checkValid)
		{
			var originalParentNode = parentNode;
			if (originalParentNode == node)
			{
				return true;
			}

			var previousParentBizObj = (originalParentNode != null) ? originalParentNode.BizObj : null;
			var newParentBizObj = (node != null) ? node.BizObj : null;

			var changed = false;
			using (TreeModel.GetNodeRefreshSuspender())
			{
				var changeParentOnBizObjResult = ChangeParentOnBizObj(previousParentBizObj, newParentBizObj, checkValid);
				if (changeParentOnBizObjResult.Success)
				{
					if (originalParentNode != null)
					{
						originalParentNode.childNodeCollection.Remove(this);
					}

					if (newParentBizObj != changeParentOnBizObjResult.NewParent)
					{
						node = changeParentOnBizObjResult.NewParent != null ? TreeModel.FindNode(changeParentOnBizObjResult.NewParent) : null;
					}

					parentNode = node;
					parentNodeRequiresRefresh = false;

					if (node != null)
					{
						node.childNodeCollection.Add(this);
					}

					changed = true;
				}
				else
				{
					TreeModel.OnChangeNodeParentFailed(this, changeParentOnBizObjResult);
				}
			}

			if (changed)
			{
				TreeModel.HasChanges = true;
				TreeModel.NotifyStructureChanged();
			}

			return changed;
		}

		protected abstract T LoadParentBizObj();
		protected abstract ChangeParentOnBizObjResult ChangeParentOnBizObj(T previousParent, T newParent, bool checkValid);

		public struct ChangeParentOnBizObjResult
		{
			public ChangeParentOnBizObjResult(ZBool success, T newParent)
				: this(success, "", newParent)
			{
			}

			public ChangeParentOnBizObjResult(ZBool success, ZString reason, T newParent)
			{
				Success = success;
				Reason = reason;
				NewParent = newParent;
			}

			public readonly ZBool Success;
			public readonly ZString Reason;
			public readonly T NewParent;
		}

		#endregion

		#region ChildNodes

		public IEnumerable<ZNode<T>> ChildNodes
		{
			get
			{
				if (!TreeModel.IsNodeRefreshSuspended && childNodesRequiresRefresh)
				{
					RefreshChildNodes();
				}
				return childNodeCollection;
			}
		}
		bool childNodesRequiresRefresh = true;
		protected HashSet<ZNode<T>> childNodeCollection = new HashSet<ZNode<T>>();

		void RefreshChildNodes()
		{
			childNodesRequiresRefresh = false;

			var childBizObjs = LoadChildBizObjs();
			Dictionary<ZGuid, T> childBizObjsToAddByPk;
			using (TreeModel.GetNodeRefreshSuspender())
			{
				// Do not add child node if there is already a parent node for same BizObj (to prevent inifinite loop)
				childBizObjsToAddByPk = childBizObjs.Where(x => FindAncestorNode(x) == null).ToDictionary(x => ((BusinessObject)(IBusiness)x).PK);
			}

			foreach (var childNode in childNodeCollection.ToArray())
			{
				var child = (BusinessObject)(IBusiness)childNode.BizObj;
				if (!childBizObjsToAddByPk.ContainsKey(child.PK))
				{
					childNodeCollection.Remove(childNode);
				}
				else
				{
					childBizObjsToAddByPk.Remove(child.PK);
					childNode.parentNode = this;
					childNode.parentNodeRequiresRefresh = false;
				}
			}

			if (childBizObjsToAddByPk.Count > 0)
			{
				foreach (var child in childBizObjsToAddByPk.Values)
				{
					var newNode = TreeModel.CreateNewNode(child);
					newNode.parentNode = this;
					newNode.parentNodeRequiresRefresh = false;
					childNodeCollection.Add(newNode);
				}
			}
		}

		protected abstract IEnumerable<T> LoadChildBizObjs();

		public ZNode<T> AddNewChild(T childBizObj)
		{
			var newNode = TreeModel.CreateNewNode(childBizObj);
			if (newNode.SetParentNode(this, true))
			{
				return newNode;
			}
			return null;
		}

		#endregion

		public ZTreePath<T> ToPath()
		{
			var node = this;
			var stack = new Stack<ZNode<T>>();
			while (node != null)
			{
				stack.Push(node);
				node = node.ParentNode;
			}
			return new ZTreePath<T>(stack.ToArray());
		}

		public ZNode<T> FindDescendantNodeInclusive(T descendantBizObj)
		{
			if (BizObjEquals(descendantBizObj))
			{
				return this;
			}

			foreach (var childNode in ChildNodes)
			{
				var descendantNode = childNode.FindDescendantNodeInclusive(descendantBizObj);
				if (descendantNode != null)
				{
					return descendantNode;
				}
			}

			return null;
		}

		public ZNode<T> FindDescendantNode(T descendantBizObj)
		{
			foreach (var childNode in ChildNodes)
			{
				var descendantNode = childNode.FindDescendantNodeInclusive(descendantBizObj);
				if (descendantNode != null)
				{
					return descendantNode;
				}
			}

			return null;
		}

		public ZNode<T> FindAncestorNode(T ancestorBizObj)
		{
			var parentNode = ParentNode;
			if (parentNode != null)
			{
				if (parentNode.BizObjEquals(ancestorBizObj))
				{
					return parentNode;
				}
				else
				{
					return parentNode.FindAncestorNode(ancestorBizObj);
				}
			}

			return null;
		}

		protected void NotifyChildNodesChanged()
		{
			childNodesRequiresRefresh = true;
			if (!TreeModel.IsNodeRefreshSuspended)
			{
				TreeModel.NotifyStructureChanged();
			}
		}

		protected void NotifyParentNodesChanged()
		{
			parentNodeRequiresRefresh = true;
			if (!TreeModel.IsNodeRefreshSuspended)
			{
				TreeModel.NotifyStructureChanged();
			}
		}

		public bool BizObjEquals(IBusiness bizObj)
		{
			return (((BusinessObject)(IBusiness)BizObj).PK == ((BusinessObject)bizObj).PK);
		}

		#region IZNode Members

		public void RemoveParent()
		{
			ParentNode = null;
		}

		IBusiness IZNode.BizObjForBinding
		{
			get { return BizObjForBinding; }
		}

		#endregion
	}
}
