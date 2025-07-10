using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aga.Business.Tree;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IZTreeModelView : ITreeModel
	{
		Tuple<string, System.Windows.Forms.SortOrder>[] SortProperties { get; set; }
		SecurityCheckpoint SecurityCheckpointForEdit { get; }
	}

	public abstract class ZTreeModelView<T> : NonPersistentBusinessObject, IZTreeModelView
		where T : class, IBusiness
	{
		public ZTreeModelView(ZTreeModel<T> inner)
		{
			this.InnerModel = inner;
			InnerModel.NodesChanged += innerModel_NodesChanged;
			InnerModel.NodesInserted += innerModel_NodesInserted;
			InnerModel.NodesRemoved += innerModel_NodesRemoved;
			InnerModel.StructureChanged += innerModel_StructureChanged;
		}

		protected readonly ZTreeModel<T> InnerModel;

		public void RefreshView()
		{
			OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
		}

		#region Sort

		public Tuple<string, System.Windows.Forms.SortOrder>[] SortProperties
		{
			get { return Comparer.SortProperties; }
			set
			{
				Comparer.SortProperties = value;
				RefreshView();
			}
		}
		readonly ZNodeComparer Comparer = new ZNodeComparer();

		#endregion

		#region ITreeModel Members

		#region GetChildren

		public IEnumerable GetChildren(TreePath treePath)
		{
			IEnumerable<ZNode<T>> result = GetChildrenCore(treePath).ToArray();
			result = GetFiltedChildren(result);

			if (result != null && Comparer.SortProperties.Length > 0)
			{
				result = result.OrderBy(x => x, Comparer);
			}

			return result;
		}

		protected virtual IEnumerable<ZNode<T>> GetChildrenCore(TreePath treePath)
		{
			return InnerModel.GetChildNodes(new ZTreePath<T>(treePath.FullPath.Cast<ZNode<T>>().ToArray()));
		}

		protected virtual IEnumerable<ZNode<T>> GetFiltedChildren(IEnumerable<ZNode<T>> children)
		{
			return children;
		}

		#endregion

		#region IsLeaf

		public bool IsLeaf(TreePath treePath)
		{
			return !GetChildrenCore(treePath).Any();
		}

		#endregion

		#region Events

		protected void OnNodesChanged(TreeModelEventArgs args)
		{
			if (NodesChanged != null)
			{
				NodesChanged(this, args);
			}
		}
		public event EventHandler<TreeModelEventArgs> NodesChanged;

		protected void OnStructureChanged(TreePathEventArgs args)
		{
			if (StructureChanged != null)
			{
				StructureChanged(this, args);
			}
		}
		public event EventHandler<TreePathEventArgs> StructureChanged;

		protected void OnNodesInserted(TreeModelEventArgs args)
		{
			if (NodesInserted != null)
			{
				NodesInserted(this, args);
			}
		}
		public event EventHandler<TreeModelEventArgs> NodesInserted;

		protected void OnNodesRemoved(TreeModelEventArgs args)
		{
			if (NodesRemoved != null)
			{
				NodesRemoved(this, args);
			}
		}
		public event EventHandler<TreeModelEventArgs> NodesRemoved;

		#endregion

		#endregion

		#region InnerModel Events

		void innerModel_NodesChanged(object sender, ZTreeModelEventArgs<T> e)
		{
			OnNodesChanged(new TreeModelEventArgs(new TreePath(e.Path.FullPath), e.Indices, e.Children));
		}

		void innerModel_NodesInserted(object sender, ZTreeModelEventArgs<T> e)
		{
			OnNodesInserted(new TreeModelEventArgs(new TreePath(e.Path.FullPath), e.Indices, e.Children));
		}

		void innerModel_NodesRemoved(object sender, ZTreeModelEventArgs<T> e)
		{
			OnNodesRemoved(new TreeModelEventArgs(new TreePath(e.Path.FullPath), e.Indices, e.Children));
		}

		void innerModel_StructureChanged(object sender, ZTreePathEventArgs<T> e)
		{
			OnStructureChanged(new TreePathEventArgs(new TreePath(e.Path.FullPath)));
		}

		#endregion

		#region Security

		public virtual SecurityCheckpoint SecurityCheckpointForEdit
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region Implementation

		class ZNodeComparer : IComparer<ZNode<T>>
		{
			public Tuple<string, System.Windows.Forms.SortOrder>[] SortProperties = Array.Empty<Tuple<string, System.Windows.Forms.SortOrder>>();

			public int Compare(ZNode<T> x, ZNode<T> y)
			{
				if (SortProperties.Length > 0)
				{
					var xProperties = ZCustomTypeDescriptor.GetProperties(x.GetType());
					var yProperties = ZCustomTypeDescriptor.GetProperties(y.GetType());
					foreach (var property in SortProperties)
					{
						var result = ((IComparable)xProperties[property.Item1].GetValue(x)).CompareTo((IComparable)yProperties[property.Item1].GetValue(y));
						if (result != 0)
						{
							return property.Item2 == System.Windows.Forms.SortOrder.Ascending ? result : -result;
						}
					}

					return 0;
				}
				else
				{
					return x.GetHashCode().CompareTo(y.GetHashCode());
				}
			}
		}

		#endregion
	}
}
