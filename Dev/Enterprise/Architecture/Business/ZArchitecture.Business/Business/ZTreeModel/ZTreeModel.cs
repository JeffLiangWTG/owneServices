using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class ZTreeModel<T> : NonPersistentBusinessObject, IObsoleteValidation
		where T : class, IBusiness
	{
		protected ZTreeModel(BusinessObjectFactory factory)
			: base(factory)
		{
			this.HasChanges = false;
		}

		#region Node

		protected internal ZNode<T> CreateNewNode(T bizObj)
		{
			return CreateNewNodeCore(this, bizObj);
		}

		protected abstract ZNode<T> CreateNewNodeCore(ZTreeModel<T> treeModel, T bizObj);

		public ZNode<T> FindNode(ZTreePath<T> treePath)
		{
			ZNode<T> node = null;
			foreach (ZNode<T> pathNode in treePath.FullPath)
			{
				if (pathNode == treePath.FirstNode)
				{
					node = RootNodes.FirstOrDefault(x => x == pathNode);
					if (node == null)
					{
						return null;
					}
				}
				else
				{
					node = node.ChildNodes.FirstOrDefault(child => child.BizObj == pathNode.BizObj);
					if (node == null)
					{
						return null;
					}
				}
			}

			return node;
		}

		public ZNode<T> FindNode(T bizObj)
		{
			if (RootNodes == null)
			{
				return null;
			}
			else
			{
				foreach (var rootNode in RootNodes)
				{
					var result = rootNode.FindDescendantNodeInclusive(bizObj);
					if (result != null)
					{
						return result;
					}
				}
			}

			return null;
		}

		#endregion

		#region RootNode

		public ZNodeCollection<T> RootNodes
		{
			get { return GetRootNodes(); }
		}

		protected abstract ZNodeCollection<T> GetRootNodes();

		#endregion

		#region GetChildren

		public IEnumerable<ZNode<T>> GetChildNodes(ZTreePath<T> treePath)
		{
			return GetChildNodesCore(treePath);
		}

		protected virtual IEnumerable<ZNode<T>> GetChildNodesCore(ZTreePath<T> treePath)
		{
			if (treePath.IsEmpty())
			{
				return RootNodes;
			}

			var node = FindNode(treePath);
			return node != null ? node.ChildNodes : Enumerable.Empty<ZNode<T>>();
		}

		#endregion

		#region IsLeaf

		public bool IsLeaf(ZTreePath<T> treePath)
		{
			return !GetChildNodesCore(treePath).Any();
		}

		#endregion

		#region Events

		protected void OnNodesChanged(ZTreeModelEventArgs<T> args)
		{
			if (NodesChanged != null)
			{
				NodesChanged(this, args);
			}
		}
		public event EventHandler<ZTreeModelEventArgs<T>> NodesChanged;

		protected void OnStructureChanged(ZTreePathEventArgs<T> args)
		{
			if (StructureChanging != null)
			{
				StructureChanging(this, args);
			}

			if (StructureChanged != null)
			{
				StructureChanged(this, args);
			}
		}
		public event EventHandler<ZTreePathEventArgs<T>> StructureChanging;
		public event EventHandler<ZTreePathEventArgs<T>> StructureChanged;

		protected void OnNodesInserted(ZTreeModelEventArgs<T> args)
		{
			if (NodesInserted != null)
			{
				NodesInserted(this, args);
			}
		}
		public event EventHandler<ZTreeModelEventArgs<T>> NodesInserted;

		protected void OnNodesRemoved(ZTreeModelEventArgs<T> args)
		{
			if (NodesRemoved != null)
			{
				NodesRemoved(this, args);
			}
		}
		public event EventHandler<ZTreeModelEventArgs<T>> NodesRemoved;

		#endregion

		#region NodeRefreshSuspender

		public bool IsNodeRefreshSuspended
		{
			get { return nodeRefreshSuspendersCount > 0; }
		}

		int nodeRefreshSuspendersCount;
		public IDisposable GetNodeRefreshSuspender()
		{
			return new NodeRefreshSuspender(this);
		}

		class NodeRefreshSuspender : IDisposable
		{
			internal NodeRefreshSuspender(ZTreeModel<T> treeModel)
			{
				this.treeModel = treeModel;
				this.treeModel.nodeRefreshSuspendersCount++;
			}

			readonly ZTreeModel<T> treeModel;

			public void Dispose()
			{
				treeModel.nodeRefreshSuspendersCount--;
			}
		}

		#endregion

		#region Notify

		protected internal void NotifyStructureChanged()
		{
			OnStructureChanged(new ZTreePathEventArgs<T>(ZTreePath<T>.Empty()));
		}

		#endregion

		#region ChangeNodeParentFailed

		internal void OnChangeNodeParentFailed(ZNode<T> node, ZNode<T>.ChangeParentOnBizObjResult changeResult)
		{
			if (ChangeNodeParentFailed != null)
			{
				ChangeNodeParentFailed(this, new ChangeNodeParentFailedArgs(node, changeResult));
			}
		}

		public event EventHandler<ChangeNodeParentFailedArgs> ChangeNodeParentFailed;

		public class ChangeNodeParentFailedArgs : EventArgs
		{
			public ChangeNodeParentFailedArgs(ZNode<T> node, ZNode<T>.ChangeParentOnBizObjResult changeResult)
			{
				Node = node;
				ChangeResult = changeResult;
			}

			public readonly ZNode<T> Node;
			public readonly ZNode<T>.ChangeParentOnBizObjResult ChangeResult;
		}

		#endregion
	}
}
