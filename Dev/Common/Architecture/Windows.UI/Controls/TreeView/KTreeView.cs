using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KTreeView"/></summary>
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KTreeView : TreeView, IDataBoundControl, IEndCurrentEdit, IContextMenuNotify
	{
		public KTreeView()
		{ VisibleChanged += new EventHandler(VisibleChanged_ForHorizontalScrollbarHack); }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object DataSource
		{
			get { return dataSource; }
			set { SetDataBinding(value, DataMember); }
		}
		object dataSource;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string DataMember
		{
			get { return dataMember; }
			set { SetDataBinding(DataSource, value); }
		}
		string dataMember;

		public TreeNode GetNodeFromItem(object item)
		{
			foreach (ITreeNodeCollectionBinder binder in rootBinders)
			{
				TreeNode result = binder.GetNodeFromItem(item);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		public object GetItemFromNode(TreeNode node)
		{
			foreach (ITreeNodeCollectionBinder binder in rootBinders)
			{
				object result = binder.GetItemFromNode(node);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object SelectedObject
		{
			get { return SelectedNode == null ? null : GetItemFromNode(SelectedNode); }
			set { SelectedNode = (value == null) ? null : GetNodeFromItem(value); }
		}

		/// <summary>
		/// Event to create a new TreeNode for the given data item.
		/// </summary>
		[Category(DesignerConstants.Category)]
		public event EventHandler<QueryNewTreeNodeEventArgs> QueryNewTreeNode;

		/// <summary>
		/// Fires the QueryNewTreeNode event. You should override this method instead of using the event if you're sub-classing
		/// the TreeView to provide this functionality.
		/// </summary>
		protected internal virtual void OnQueryNewTreeNode(QueryNewTreeNodeEventArgs e)
		{
			if (QueryNewTreeNode != null)
			{
				QueryNewTreeNode(this, e);
			}
			if (e.NewTreeNode == null)
			{
				e.NewTreeNode = new TreeNode((e.Item ?? "null").ToString());
			}
		}

		/// <summary>
		/// Query the ITreeNodeCollectionBinder for child nodes of this DTreeView, or the child nodes of another TreeNode.
		/// </summary>
		[Category(DesignerConstants.Category)]
		[SmartTagVisible]
		public event EventHandler<QueryTreeNodeDataBindingsEventArgs> QueryNodeDataBindings;

		/// <summary>
		/// Fires the QueryNodeDataBindings event. You should override this method instead of
		/// using the event if you're sub-classing the TreeView to provide this functionality.
		/// </summary>
		protected virtual void OnQueryNodeDataBindings(QueryTreeNodeDataBindingsEventArgs e)
		{
			if (QueryNodeDataBindings != null)
			{
				QueryNodeDataBindings(this, e);
			}
			else if (e.ParentItem == null)
			{
				e.NodeBinders.Add(new TreeNodeCollectionBinder(this, Nodes, DataSource, DataMember, null));
			}
		}

		/// <summary>
		/// Get the CurrencyManager and display member for the children items of a given item.
		/// </summary>
		internal virtual ITreeNodeCollectionBinder[] GetNodeCollectionBinders(TreeNodeCollection nodes, object parentItem)
		{
			QueryTreeNodeDataBindingsEventArgs e = new QueryTreeNodeDataBindingsEventArgs(this, nodes, parentItem);
			OnQueryNodeDataBindings(e);
			return e.NodeBinders.ToArray();
		}

		public void PerformSelect(TreeNode node)
		{
			var treeViewCancel = new TreeViewCancelEventArgs(node, false, TreeViewAction.Unknown);
			OnBeforeSelect(treeViewCancel);
			if (!treeViewCancel.Cancel)
			{
				SelectedNode = node;
				OnAfterSelect(new TreeViewEventArgs(node, TreeViewAction.Unknown));
			}
		}

		#region Fix for Horizonal Scrollbar

		void VisibleChanged_ForHorizontalScrollbarHack(object sender, EventArgs e)
		{
			if (Visible)
			{
				// required to defeat the horizontal scroll bar showing unnecessarily
				int old_width = Width;
				ControlDpiScalingHelper.SetWidth(this, 0, true);
				ControlDpiScalingHelper.SetWidth(this, old_width, false);
			}
		}

		#endregion

		#region IDataBoundControl Members

		Type IDataBoundControl.DataSourceType
		{ get { return typeof(IList); } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "dataSource"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "dataMember")]
		public void SetDataBinding(object dataSource, string dataMember)
		{
			foreach (ITreeNodeCollectionBinder binder in rootBinders)
			{
				binder.StopBinding();
			}
			rootBinders = Array.Empty<ITreeNodeCollectionBinder>();

			Nodes.Clear();

			this.dataMember = dataMember;
			if (dataSource != null)
			{
				this.dataSource = dataSource;
				TryCreateRootNodeBinderAndStartBinding();
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			TryCreateRootNodeBinderAndStartBinding();
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			TryCreateRootNodeBinderAndStartBinding();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			inOnHandleCreated = true;
			try
			{
				base.OnHandleCreated(e);
			}
			finally
			{
				inOnHandleCreated = false;
			}
		}
		bool inOnHandleCreated;

		void TryCreateRootNodeBinderAndStartBinding()
		{
			if (Visible && !inOnHandleCreated && BindingContext != null && rootBinders.Length == 0 && DataSource != null)
			{
				CreateRootNodeBinderAndStartBinding();
			}
		}

		void CreateRootNodeBinderAndStartBinding()
		{
			rootBinders = GetNodeCollectionBinders(this.Nodes, null);
			if (rootBinders.Length == 0)
			{
				throw new InvalidOperationException("You must add items to e.NodeBinder on the QueryNodeDataBindings event when e.ParentItem is null to specify the entity list that populates the root nodes");
			}
			foreach (ITreeNodeCollectionBinder binder in rootBinders)
			{
				binder.StartBinding();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (ITreeNodeCollectionBinder binder in rootBinders)
				{
					binder.StopBinding();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region IEndCurrentEdit Members

		void IEndCurrentEdit.EndCurrentEdit()
		{
			if (SelectedNode != null)
			{
				SelectedNode.EndEdit(false);
			}
		}

		#endregion

		#region OnActionDelete / OnActionOpen

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (this.SelectedNode != null &&
				!this.SelectedNode.IsEditing &&
				e.KeyCode == Keys.Delete)
			{
				OnActionDelete(new TreeViewEventArgs(SelectedNode, TreeViewAction.ByKeyboard));
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (this.SelectedNode != null &&
				!this.SelectedNode.IsEditing &&
				e.KeyChar == '\r')
			{
				OnActionOpen(new TreeViewEventArgs(SelectedNode, TreeViewAction.ByKeyboard));
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			OnActionOpen(new TreeViewEventArgs(SelectedNode, TreeViewAction.ByMouse));
		}

		protected virtual void OnActionDelete(TreeViewEventArgs e)
		{
		}

		protected virtual void OnActionOpen(TreeViewEventArgs e)
		{
		}

		#endregion

		#region IContextMenuNotify Members

		void IContextMenuNotify.OnContextMenuOpening(CancelEventArgs e)
		{
			if (Focused)
			{
				Point clientMousePosition = PointToClient(Form.MousePosition);
				TreeViewHitTestInfo hit = HitTest(clientMousePosition);
				if (hit.Node != null)
				{
					SelectedNode = hit.Node;
				}
			}
		}

		#endregion

		ITreeNodeCollectionBinder[] rootBinders = Array.Empty<ITreeNodeCollectionBinder>();
	}

	#region QueryTreeNodesDataBindingsEventArgs

	/// <summary>
	/// Arguments to the DTreeView.QueryNodeDataBindings event.
	/// </summary>
	public class QueryTreeNodeDataBindingsEventArgs : EventArgs
	{
		public QueryTreeNodeDataBindingsEventArgs(KTreeView treeView, TreeNodeCollection nodes, object parentItem)
		{
			this.TreeView = treeView;
			this.Nodes = nodes;
			this.ParentItem = parentItem;
		}

		/// <summary>
		/// Get the DTreeView object the nodes will belong to.
		/// </summary>
		public KTreeView TreeView { get; private set; }

		/// <summary>
		/// Get the TreeNodeCollection that the resultant ITreeNodeCollectionBinder will populate with nodes.
		/// </summary>
		public TreeNodeCollection Nodes { get; private set; }

		/// <summary>
		/// Get the parent business entity that represents the 'parent tree node'. Null if you're populating nodes
		/// at the root level of the tree.
		/// </summary>
		public object ParentItem { get; private set; }

		/// <summary>
		/// Add ITreeNodeCollectionBinder objects here in the DTreeNode.QueryNodeDataBindings event.
		/// </summary>
		public TreeNodeCollectionBinderCollection NodeBinders
		{
			get
			{
				if (nodeBinders == null)
				{
					nodeBinders = new TreeNodeCollectionBinderCollection(TreeView, Nodes);
				}
				return nodeBinders;
			}
		}
		TreeNodeCollectionBinderCollection nodeBinders;
	}

	#endregion

	#region QueryNewTreeNodeEventArgs

	/// <summary>
	/// Arguments to the KTreeView.QueryNewTreeNode event.
	/// </summary>
	public class QueryNewTreeNodeEventArgs : EventArgs
	{
		public QueryNewTreeNodeEventArgs(object item)
		{ this.Item = item; }

		/// <summary>
		/// Get the item that is represented by the tree node.
		/// </summary>
		public object Item { get; private set; }

		/// <summary>
		/// Get or set the newly created TreeNode.
		/// </summary>
		public TreeNode NewTreeNode { get; set; }
	}

	#endregion
}
