using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A helper for binding the elements of a collection to a collection of TreeNodeS.
	/// </summary>
	public class TreeNodeCollectionBinder : ITreeNodeCollectionBinder
	{
		/// <summary>
		/// Constructor to create a binding between a tree view and a data source.
		/// </summary>
		/// <param name="treeView">The tree view whose nodes will be bound.</param>
		/// <param name="dataSource">The data source that contains the list for binding.</param>
		/// <param name="listMember">The member on the data source that contains the list for binding or an empty string if dataSource is the list.</param>
		public TreeNodeCollectionBinder(
			KTreeView treeView, object dataSource, string listMember)
			: this(treeView, treeView.Nodes, dataSource, listMember, "")
		{
		}

		/// <summary>
		/// Constructor to create a binding between a tree view and a data source.
		/// </summary>
		/// <param name="treeView">The tree view whose nodes will be bound.</param>
		/// <param name="dataSource">The data source that contains the list for binding.</param>
		/// <param name="listMember">The member on the data source that contains the list for binding or an empty string if dataSource is the list.</param>
		/// <param name="displayPropertyName">
		/// The property on elements of the list that are shown as the Text of each TreeNode.
		/// Set to an empty string to use the ToString() method of each element of the list.
		/// Set to null to allow the Text property of each TreeNode to be set programatically.
		/// </param>
		public TreeNodeCollectionBinder(
			KTreeView treeView, object dataSource, string listMember, string displayPropertyName)
			: this(treeView, treeView.Nodes, dataSource, listMember, displayPropertyName)
		{
		}

		/// <summary>
		/// Constructor to create a binding between a tree view and a data source.
		/// </summary>
		/// <param name="treeView">The tree view.</param>
		/// <param name="nodes">The TreeNode objects that will be bound.</param>
		/// <param name="dataSource">The data source that contains the list for binding.</param>
		/// <param name="listMember">The member on the data source that contains the list for binding or an empty string if dataSource is the list.</param>
		public TreeNodeCollectionBinder(
			KTreeView treeView, TreeNodeCollection nodes, object dataSource, string listMember)
			: this(treeView, nodes, dataSource, listMember, "")
		{
		}

		/// <summary>
		/// Constructor to create a binding between a tree view and a data source.
		/// </summary>
		/// <param name="treeView">The tree view.</param>
		/// <param name="nodes">The TreeNode objects that will be bound.</param>
		/// <param name="dataSource">The data source that contains the list for binding.</param>
		/// <param name="listMember">The member on the data source that contains the list for binding or an empty string if dataSource is the list.</param>
		/// <param name="displayPropertyName">
		/// The property on elements of the list that are shown as the Text of each TreeNode.
		/// Set to an empty string to use the ToString() method of each element of the list.
		/// Set to null to allow the Text property of each TreeNode to be set programatically.
		/// </param>
		public TreeNodeCollectionBinder(
			KTreeView treeView, TreeNodeCollection nodes, object dataSource, string listMember, string displayPropertyName)
		{
			this.TreeView = treeView;
			this.Nodes = nodes;
			this.dataSource = dataSource;
			this.listMember = listMember;
			this.displayPropertyName = displayPropertyName;
		}

		/// <summary>
		/// Get the TreeView that contains the nodes in the NodeCollection.
		/// </summary>
		public KTreeView TreeView { get; private set; }

		/// <summary>
		/// Get the PropertyDescriptor of the display member.
		/// </summary>
		public PropertyDescriptor DisplayProperty
		{
			get
			{
				if (displayProperty == null &&
					!string.IsNullOrEmpty(displayPropertyName) &&
					ListManager != null)
				{
					displayProperty = ListManager.GetItemProperties()[displayPropertyName];
					if (displayProperty == null)
					{
						throw new InvalidOperationException("Could not find display member " + displayPropertyName);
					}
				}
				return displayProperty;
			}
		}
		PropertyDescriptor displayProperty;
		readonly string displayPropertyName;

		/// <summary>
		/// The CurrencyManager that contains the data the the nodes come from.
		/// </summary>
		public CurrencyManager ListManager
		{
			get
			{
				if (listManager == null && dataSource != null)
				{
					listManager = (CurrencyManager)TreeView.BindingContext[dataSource, listMember];
				}
				return listManager;
			}
		}
		CurrencyManager listManager;

		#region FirstNode / LastNode / AppendNodesTo

		public TreeNode FirstNode
		{ get { return LastNode == null ? null : Nodes[FirstNodeIndex]; } }

		public TreeNode LastNode
		{ get { return lastNode; } }

		public int FirstNodeIndex
		{
			get
			{
				int result = firstNodeIndexOverride;
				if (binderAppendedTo != null)
				{
					result = binderAppendedTo.LastNode == null ? 0 : (binderAppendedTo.LastNode.Index + 1);
				}
				return result;
			}
			set
			{
				binderAppendedTo = null;
				firstNodeIndexOverride = value;
			}
		}

		/// <summary>
		/// Make the FirstNodeIndex 1 after the last node of the given binder. Effectively the nodes of this binder will appear
		/// immediately after the given binder.
		/// </summary>
		public void AppendNodesTo(ITreeNodeCollectionBinder binder)
		{
			this.binderAppendedTo = binder;
			firstNodeIndexOverride = 0;
		}

		#endregion

		#region BoundNodes

		TreeNode[] BoundNodes
		{
			get
			{
				ArrayList result = new ArrayList();
				if (LastNode != null)
				{
					for (int i = FirstNodeIndex; i <= LastNode.Index; i++)
					{
						result.Add(Nodes[i]);
					}
				}
				return (TreeNode[])result.ToArray(typeof(TreeNode));
			}
		}

		int BoundNodeCount
		{ get { return LastNode == null ? 0 : LastNode.Index - FirstNodeIndex + 1; } }

		TreeNode GetBoundNode(int i)
		{ return Nodes[FirstNodeIndex + i]; }

		void SetBoundNode(int i, TreeNode value)
		{ Nodes[FirstNodeIndex + i] = value; }

		#endregion

		#region RestoreViewState

		public void RestoreViewState(ITreeNodeCollectionBinder oldBinder)
		{
			TreeNodeCollectionBinder old_binder = (TreeNodeCollectionBinder)oldBinder;
			RestoreViewState(old_binder.BoundNodes, old_binder.childBinders.ToArray());
		}

		void RestoreViewState(IList oldNodes, ITreeNodeCollectionBinder[][] oldChildBinders)
		{
			if (!IsBinding)
			{
				throw new InvalidOperationException("Must be binding for this operation to be valid.");
			}

			if (oldNodes.Count == BoundNodeCount)
			{
				if (oldNodes.Count != oldChildBinders.Length)
				{
					throw new InvalidOperationException(
						"There should be the same number of elements in the old child binders list as the old nodes list");
				}

				for (int i = 0; i < oldNodes.Count; i++)
				{
					TreeNode old_node = (TreeNode)oldNodes[i];
					TreeNode new_node = GetBoundNode(i);
					RestoreViewState(old_node, new_node, oldChildBinders[i], childBinders[i]);
				}
			}
		}

		static void RestoreViewState(TreeNode oldNode, TreeNode newNode, ITreeNodeCollectionBinder[] oldChildBinders, ITreeNodeCollectionBinder[] newChildBinders)
		{
			if (oldNode.Text == newNode.Text && oldChildBinders.Length == newChildBinders.Length)
			{
				for (int i = 0; i < oldChildBinders.Length; i++)
				{
					RestoreViewState(oldNode, newNode, oldChildBinders[i], newChildBinders[i]);
				}
			}
		}

		static void RestoreViewState(TreeNode oldNode, TreeNode newNode, ITreeNodeCollectionBinder oldChildBinder, ITreeNodeCollectionBinder newChildBinder)
		{
			if (oldChildBinder.GetType() == newChildBinder.GetType())
			{
				if (oldNode.IsExpanded)
				{
					newNode.Expand();
				}
				else
				{
					newNode.Collapse();
				}

				if (newChildBinder.IsBinding)
				{
					newChildBinder.RestoreViewState(oldChildBinder);
				}
			}
		}

		#endregion

		#region ITreeNodeCollectionBinder Members

		public TreeNodeCollection Nodes { get; private set; }

		public bool IsBinding
		{ get { return isBinding; } }
		bool isBinding;

		public void StartBinding()
		{
			if (!IsBinding && ListManager != null)
			{
				this.ListManager.ItemChanged += new ItemChangedEventHandler(ListManager_ItemChanged);
				this.ListManager.PositionChanged += new EventHandler(ListManager_PositionChanged);
				this.TreeView.AfterSelect += new TreeViewEventHandler(TreeView_AfterSelect);
				this.TreeView.BeforeExpand += new TreeViewCancelEventHandler(TreeView_BeforeExpand);
				this.TreeView.BeforeLabelEdit += new NodeLabelEditEventHandler(TreeView_BeforeLabelEdit);
				this.TreeView.AfterLabelEdit += new NodeLabelEditEventHandler(TreeView_AfterLabelEdit);
				isBinding = true;
				UpdateList();
			}
		}

		public void StopBinding()
		{
			if (IsBinding)
			{
				this.ListManager.ItemChanged -= new ItemChangedEventHandler(ListManager_ItemChanged);
				this.ListManager.PositionChanged -= new EventHandler(ListManager_PositionChanged);
				this.TreeView.AfterSelect -= new TreeViewEventHandler(TreeView_AfterSelect);
				this.TreeView.BeforeExpand -= new TreeViewCancelEventHandler(TreeView_BeforeExpand);
				this.TreeView.AfterLabelEdit -= new NodeLabelEditEventHandler(TreeView_BeforeLabelEdit);
				this.TreeView.AfterLabelEdit -= new NodeLabelEditEventHandler(TreeView_AfterLabelEdit);
				isBinding = false;
				UpdateList();
			}
		}

		public TreeNode GetNodeFromItem(object item)
		{
			TreeNode result = null;
			int index = this.ListManager.List.IndexOf(item);
			if (index != -1)
			{
				result = GetBoundNode(index);
			}
			if (result == null)
			{
				foreach (ITreeNodeCollectionBinder[] childBinderList in childBinders)
				{
					foreach (ITreeNodeCollectionBinder childBinder in childBinderList)
					{
						result = childBinder.GetNodeFromItem(item);
						if (result != null)
						{
							break;
						}
					}
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		public object GetItemFromNode(TreeNode node)
		{
			object result = null;
			int nodeIndex = Nodes.IndexOf(node);
			int index = nodeIndex - FirstNodeIndex;
			if (ListManager != null && index >= 0 && index < ListManager.List.Count)
			{
				result = ListManager.List[index];
			}
			if (result == null)
			{
				foreach (ITreeNodeCollectionBinder[] binders in childBinders)
				{
					foreach (ITreeNodeCollectionBinder childBinder in binders)
					{
						result = childBinder.GetItemFromNode(node);
						if (result != null)
						{
							break;
						}
					}
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region Implementation

		// config
		ITreeNodeCollectionBinder binderAppendedTo;
		int firstNodeIndexOverride;
		TreeNode lastNode;

		// state
		readonly object dataSource;
		readonly string listMember;
		IList currentList;
		readonly List<ITreeNodeCollectionBinder[]> childBinders = new List<ITreeNodeCollectionBinder[]>();

		/// <summary>
		/// Get a suitable TreeNode.Text value from an element in the data source.
		/// </summary>
		string TextFromElement(object element)
		{
			if (displayPropertyName == null)
			{
				throw new InvalidOperationException("This should not be called when not using a display property.");
			}
			object result = DisplayProperty != null ? DisplayProperty.GetValue(element) : element;
			return result == null ? "" : result.ToString();
		}

		void UpdateList()
		{
			if (!IsBinding || currentList != ListManager.List)
			{
				if (IsBinding && TreeView.BindingContext == null)
				{
					throw new InvalidOperationException(
						"TreeView.BindingContext not yet available, try calling TreeView.CreateControl()");
				}

				if (currentList != null)
				{
					IBindingList list = currentList as IBindingList;
					if (list != null)
					{
						list.ListChanged -= new ListChangedEventHandler(List_Changed);
					}
				}
				currentList = isBinding ? ListManager.List : null;
				if (currentList != null)
				{
					IBindingList list = currentList as IBindingList;
					if (list != null)
					{
						list.ListChanged += new ListChangedEventHandler(List_Changed);
					}
				}
				UpdateAllNodes(false);
			}
		}

		TreeNode NewTreeNode(object item, out ITreeNodeCollectionBinder[] newChildBinders)
		{
			QueryNewTreeNodeEventArgs e = new QueryNewTreeNodeEventArgs(item);
			TreeView.OnQueryNewTreeNode(e);
			TreeNode result = e.NewTreeNode;

			newChildBinders = TreeView.GetNodeCollectionBinders(result.Nodes, item);
			if (newChildBinders.Length > 0)
			{
				result.Nodes.Add(new PlaceHolderTreeNode());
			}
			return result;
		}

		class PlaceHolderTreeNode : TreeNode
		{
			public PlaceHolderTreeNode()
				: base("placeholder_for_children")
			{
			}
		}

		void UpdateNodeText(TreeNode node, object item)
		{
			if (displayPropertyName != null)
			{
				string text = TextFromElement(item);
				node.Text = text;
				if (node.Text != text)
				{
					throw new InvalidOperationException("Text should be set");
				}
			}
		}

		#endregion

		#region Event Handlers

		void List_Changed(object sender, ListChangedEventArgs e)
		{
			object item;
			TreeNode newNode;
			ITreeNodeCollectionBinder[] binders;

			switch (e.ListChangedType)
			{
				case ListChangedType.ItemAdded:
				case ListChangedType.ItemChanged:
					if (e.ListChangedType == ListChangedType.ItemAdded && this.BoundNodeCount < currentList.Count)
					{
						item = currentList[e.NewIndex];
						newNode = NewTreeNode(item, out binders);
						UpdateNodeText(newNode, item);

						childBinders.Insert(e.NewIndex, binders);
						Nodes.Insert(FirstNodeIndex + e.NewIndex, newNode);
					}
					else // ItemChanged or 'ItemAdded and committing item'
					{
						item = currentList[e.NewIndex];
						UpdateNodeText(GetBoundNode(e.NewIndex), item);
					}
					break;
				case ListChangedType.ItemDeleted:
					Nodes.RemoveAt(FirstNodeIndex + e.NewIndex);
					StopBindingForChildBindings(childBinders[e.NewIndex]);
					childBinders.RemoveAt(e.NewIndex);
					break;
				case ListChangedType.ItemMoved:
					TreeNode selected = TreeView.SelectedNode;

					ITreeNodeCollectionBinder[] binderToSwap = childBinders[e.NewIndex];
					childBinders[e.NewIndex] = childBinders[e.OldIndex];
					childBinders[e.OldIndex] = binderToSwap;

					TreeNode nodeToSwap = GetBoundNode(e.OldIndex);
					SetBoundNode(e.OldIndex, GetBoundNode(e.NewIndex));
					SetBoundNode(e.NewIndex, nodeToSwap);

					if (TreeView.SelectedNode != selected)
					{
						TreeView.SelectedNode = selected;
					}
					break;
				case ListChangedType.Reset:
					UpdateAllNodes(true);
					break;
			}

			lastNode = (currentList.Count == 0) ? null : this.GetBoundNode(currentList.Count - 1);
			CheckInvariant();
		}

		void UpdateAllNodes(bool fromListChanged)
		{
			TreeView.BeginUpdate();
			try
			{
				object oldSelected = null;
				ITreeNodeCollectionBinder[][] allOldChildBinders = childBinders.ToArray();
				ArrayList oldNodes = null;
				if (fromListChanged)
				{
					oldSelected = GetItemFromNode(TreeView.SelectedNode);
					oldNodes = new ArrayList(Nodes);
				}

				RemoveAllBoundNodes();
				childBinders.Clear();
				if (currentList != null)
				{
					for (int i = 0; i < currentList.Count; i++)
					{
						object item = currentList[i];

						ITreeNodeCollectionBinder[] binders;
						TreeNode newNode = NewTreeNode(item, out binders);
						UpdateNodeText(newNode, item);

						Nodes.Insert(LastNode == null ? FirstNodeIndex : LastNode.Index + 1, newNode);
						lastNode = newNode;
						childBinders.Add(binders);
					}
				}

				if (fromListChanged)
				{
					RestoreViewState(oldNodes, allOldChildBinders);
					if (oldSelected != null)
					{
						TreeView.SelectedObject = oldSelected;
					}
				}
				StopBindingForChildBindings(allOldChildBinders);
			}
			finally
			{
				TreeView.EndUpdate();
			}
		}

		bool IsPositionWithinNodeRange(int position)
		{ return position >= 0 && position < BoundNodeCount; }

		static void StopBindingForChildBindings(IList<ITreeNodeCollectionBinder[]> bindings)
		{
			foreach (ITreeNodeCollectionBinder[] bindingSet in bindings)
			{
				StopBindingForChildBindings(bindingSet);
			}
		}

		static void StopBindingForChildBindings(ITreeNodeCollectionBinder[] bindings)
		{
			foreach (ITreeNodeCollectionBinder binding in bindings)
			{
				binding.StopBinding();
			}
		}

		void RemoveAllBoundNodes()
		{
			if (LastNode != null)
			{
				for (int i = LastNode.Index; i >= FirstNodeIndex; i--)
				{
					Nodes.RemoveAt(i);
				}
			}
			if (Nodes.Count == 1 && Nodes[0] is PlaceHolderTreeNode)
			{
				Nodes.RemoveAt(0);
			}
			lastNode = null;
		}

		void ListManager_ItemChanged(object sender, ItemChangedEventArgs e)
		{
			if (e.Index == -1)
			{
				UpdateList();
			}
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			if (listManagerPositionChangedSuspended == 0 && IsPositionWithinNodeRange(ListManager.Position))
			{
				TreeNode new_node = GetBoundNode(ListManager.Position);
				if (this.TreeView.SelectedNode != new_node)
				{
					afterSelectSuspended++;
					try
					{
						this.TreeView.SelectedNode = GetBoundNode(ListManager.Position);
					}
					finally
					{
						afterSelectSuspended--;
					}
				}
			}
		}
		[ThreadStatic]
		static int listManagerPositionChangedSuspended;

		void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (afterSelectSuspended == 0)
			{
				TreeNode current = e.Node;
				int newPosition = -1;
				while (current != null && !IsPositionWithinNodeRange(newPosition))
				{
					newPosition = Nodes.IndexOf(current) - FirstNodeIndex;
					current = current.Parent;
				}

				if (IsPositionWithinNodeRange(newPosition) && this.ListManager.Position != newPosition)
				{
					listManagerPositionChangedSuspended++;
					try
					{
						this.ListManager.Position = newPosition;
					}
					finally
					{
						listManagerPositionChangedSuspended--;
					}
				}
			}
		}
		[ThreadStatic]
		static int afterSelectSuspended;

		void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			int index = Nodes.IndexOf(e.Node) - FirstNodeIndex;
			if (index >= 0 && index < childBinders.Count)
			{
				ITreeNodeCollectionBinder[] immediateChildBinders = childBinders[index];
				foreach (ITreeNodeCollectionBinder childBinder in immediateChildBinders)
				{
					if (!childBinder.IsBinding)
					{
						childBinder.StartBinding();
					}
				}
			}
		}

		void TreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (!e.CancelEdit && Nodes.Contains(e.Node))
			{
				if (DisplayProperty == null)
				{
					e.CancelEdit = true;
				}
				else
				{
					object selectedItem = GetItemFromNode(e.Node);
					if (selectedItem != null)
					{
						bool readOnly =
							DisplayProperty.IsReadOnly ||
							(DisplayReadOnlyProperty != null && (bool)DisplayReadOnlyProperty.GetValue(selectedItem));
						if (readOnly)
						{
							e.CancelEdit = true;
						}
					}
				}
			}
		}

		void TreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (!e.CancelEdit && Nodes.Contains(e.Node))
			{
				object item = GetItemFromNode(e.Node);
				if (e.Label != null &&
					item != null &&
					DisplayProperty != null)
				{
					DisplayProperty.SetValue(item, e.Label);
					IEditableObject editable = item as IEditableObject;
					if (editable != null)
					{
						editable.EndEdit();
					}
				}
			}
		}

		#endregion

		#region Implementation

		PropertyDescriptor DisplayReadOnlyProperty
		{
			get
			{
				if (displayReadOnlyProperty == null && DisplayProperty != null)
				{
					displayReadOnlyProperty = MetaData.GetMetaDataProperty(DisplayProperty.ComponentType, DisplayProperty, MetaDataTypes.ReadOnly);
				}
				return displayReadOnlyProperty;
			}
		}
		PropertyDescriptor displayReadOnlyProperty;

		[Conditional("DEBUG")]
		void CheckInvariant()
		{
			if (ListManager != null && IsBinding)
			{
				if (BoundNodeCount != ListManager.List.Count)
				{
					throw new InvalidOperationException(
						"Should have the same number of nodes as elements in the data source");
				}
			}
		}

		#endregion
	}
}
