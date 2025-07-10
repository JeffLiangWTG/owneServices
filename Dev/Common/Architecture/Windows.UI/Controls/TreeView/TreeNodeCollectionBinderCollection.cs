using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A collection of ITreeNodeCollectionBinder objects.
	/// </summary>
	public class TreeNodeCollectionBinderCollection : Collection<ITreeNodeCollectionBinder>
	{
		public TreeNodeCollectionBinderCollection(KTreeView treeView, TreeNodeCollection nodes)
		{
			this.treeView = treeView;
			this.nodes = nodes;
		}

		/// <summary>
		/// Method to create a binding between a tree view and a data source.
		/// </summary>
		/// <param name="dataSource">The data source that contains the list for binding.</param>
		/// <param name="listMember">The member on the data source that contains the list for binding or an empty string if dataSource is the list.</param>
		public ITreeNodeCollectionBinder Add(object dataSource, string listMember)
		{ return Add(dataSource, listMember, ""); }

		/// <summary>
		/// Method to create a binding between a tree view and a data source.
		/// </summary>
		/// <param name="dataSource">The data source that contains the list for binding.</param>
		/// <param name="listMember">The member on the data source that contains the list for binding or an empty string if dataSource is the list.</param>
		/// <param name="displayPropertyName">
		/// The property on elements of the list that are shown as the Text of each TreeNode.
		/// Set to an empty string to use the ToString() method of each element of the list.
		/// Set to null to allow the Text property of each TreeNode to be set programatically.
		/// </param>
		public ITreeNodeCollectionBinder Add(object dataSource, string listMember, string displayPropertyName)
		{
			TreeNodeCollectionBinder binder = new TreeNodeCollectionBinder(treeView, nodes, dataSource, listMember, displayPropertyName);
			Add(binder);
			return binder;
		}

		public ITreeNodeCollectionBinder[] ToArray()
		{
			ITreeNodeCollectionBinder[] result = new ITreeNodeCollectionBinder[Count];
			CopyTo(result, 0);
			return result;
		}

		readonly KTreeView treeView;
		readonly TreeNodeCollection nodes;
	}
}
