namespace Enterprise.ZArchitecture.Business.Internal
{
	/// <summary>
	/// On the right panel of Layout Manager form, it shows layout details on a tree view.
	/// </summary>
	public interface ILayoutDetailTreeNode
	{
		/// <summary>
		/// A text that appears on a tree view node. filter strip description or column name
		/// </summary>
		string UniqueID { get; }

		/// <summary>
		/// A text that appears on a parent node. Category description for filters or null for grids. 
		/// For filter layouts, the tree view shows two levels. Categories and then filters under each category
		/// </summary>
		string ParentUniqueID { get; }
	}
}
