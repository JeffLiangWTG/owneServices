using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented on an object that manages the binding of a TreeNodeCollection.
	/// </summary>
	public interface ITreeNodeCollectionBinder
	{
		/// <summary>
		/// Get the TreeNodeCollection that is being bound.
		/// </summary>
		TreeNodeCollection Nodes { get; }

		/// <summary>
		/// Get whether this binder is binding or not.
		/// </summary>
		bool IsBinding { get; }

		/// <summary>
		/// Start binding the nodes, adding whatever nodes are required then updating them subsequently.
		/// </summary>
		void StartBinding();

		/// <summary>
		/// Stop binding the nodes. If there are still nodes on the collection, just leave them there.
		/// </summary>
		void StopBinding();

		/// <summary>
		/// Get the TreeNode given the bound item. Null if none could be found.
		/// </summary>
		TreeNode GetNodeFromItem(object item);

		/// <summary>
		/// Get the item given the TreeNode it is bound to. Null if none could be found.
		/// </summary>
		object GetItemFromNode(TreeNode node);

		/// <summary>
		/// Restore the visual state of the currently expanded nodes with the state that was about in the
		/// given binder. This may not be a perfect implementation, for example just the text may be examined
		/// to match the original nodes.
		/// </summary>
		void RestoreViewState(ITreeNodeCollectionBinder oldBinder);

		/// <summary>
		/// Get the first node in the collection of bound nodes. Null if the bound collection is of zero length.
		/// </summary>
		TreeNode FirstNode { get; }

		/// <summary>
		/// Get the last node in the collection of bound nodes. Null if the bound collection is of zero length.
		/// </summary>
		TreeNode LastNode { get; }

		/// <summary>
		/// Get or set the index of the first node.
		/// </summary>
		int FirstNodeIndex { get; set; }
	}
}
